using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace Balancer
{
	class Balancer : HttpListener
	{
		private readonly List<IPEndPoint> replicaAddresses;
		private readonly List<IPEndPoint> replicaGreyAddresses = new List<IPEndPoint>();
		private readonly Random random;
		private readonly object locker = new object();

		public Balancer(IPEndPoint balancerAddress, IPEndPoint[] replicaAddresses, ILog log)
			: base(balancerAddress, log)
		{
			this.replicaAddresses = replicaAddresses == null ? new List<IPEndPoint>() : replicaAddresses.ToList();
			random = new Random();
			ReplicaTimeout = 100 * 1000;
			GreyListTimeout = 10 * 1000;
		}

		public Balancer(IPEndPoint balancerAddress, ILog log, int replicaTimeout, int greyListTimeout, int? randomSeed = null)
			: base(balancerAddress, log)
		{
			replicaAddresses = new List<IPEndPoint>();
			random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
			ReplicaTimeout = replicaTimeout;
			GreyListTimeout = greyListTimeout;
		}

		public bool TryAddReplicaAddress(IPEndPoint replicaAddress)
		{
			lock (locker)
			{
				if (replicaAddresses.Contains(replicaAddress) || replicaGreyAddresses.Contains(replicaAddress))
					return false;
				replicaAddresses.Add(replicaAddress);
				return true;
			}
		}

		public bool TryRemoveReplicaAddress(IPEndPoint replicaAddress)
		{
			lock (locker)
				return replicaAddresses.Remove(replicaAddress) || replicaGreyAddresses.Remove(replicaAddress);
		}

		public void ClearReplicaAddresses()
		{
			lock (locker)
			{
				replicaAddresses.Clear();
				replicaGreyAddresses.Clear();
			}
		}

		protected override async Task OnContextAsync(HttpListenerContext context)
		{
			var requestId = Guid.NewGuid();
			var query = GetQuery(context.Request.RawUrl);
			var acceptEncoding = context.Request.Headers.GetValues("Accept-Encoding");
			var remoteEndPoint = context.Request.RemoteEndPoint;
			log.InfoFormat("{0}: {1} received {2} from {3}", requestId, Name, query, remoteEndPoint);

			var replicaResponse = await GetResponseFromRandomReplica(requestId, query);
			if (replicaResponse != null)
			{
				var deflate = acceptEncoding != null && acceptEncoding.Contains("deflate");
				if (deflate)
					context.Response.AddHeader("Content-Encoding", "deflate");
				await CopyStream(replicaResponse.GetResponseStream(), context.Response.OutputStream, deflate);
				log.InfoFormat("{0}: {1} sent processed query to {2}", requestId, Name, remoteEndPoint);
			}
			else
			{
				log.InfoFormat("{0}: {1} can't proxy request to any replica", requestId, Name);
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			}

			context.Request.InputStream.Close();
			context.Response.OutputStream.Close();
		}

		private async Task CopyStream(Stream source, Stream destination, bool deflate)
		{
			using (source)
				if (deflate)
					using (var compressionStream = new DeflateStream(destination, CompressionMode.Compress))
						await source.CopyToAsync(compressionStream);
				else
					await source.CopyToAsync(destination);
		}

		private async Task<WebResponse> GetResponseFromRandomReplica(Guid requestId, string query)
		{
			var replicaAddressFromGreyListUsed = false;
			while (true)
			{
				IPEndPoint replicaAddress;
				lock (locker)
					if (replicaAddresses.Count > 0)
						replicaAddress = replicaAddresses[random.Next(replicaAddresses.Count)];
					else if (replicaGreyAddresses.Count > 0)
					{
						log.InfoFormat("{0}: there is no any active replica, try proxy request to some replica address from grey list",
							requestId);
						replicaAddress = replicaGreyAddresses[random.Next(replicaGreyAddresses.Count)];
						replicaAddressFromGreyListUsed = true;
					}
					else
						break;
				var replicaResponse = await GetResponseFromReplica(requestId, replicaAddress, query);
				if (replicaResponse != null)
					return replicaResponse;
				if (replicaAddressFromGreyListUsed)
					break;
				log.InfoFormat("{0}: {1} can't proxy request to {2}", requestId, Name, replicaAddress);
				lock (locker)
					if (replicaAddresses.Remove(replicaAddress))
					{
						replicaGreyAddresses.Add(replicaAddress);
						log.InfoFormat("{0}: {1} move {2} to grey list", requestId, Name, replicaAddress);
						ReturnReplicaAddressFromGreyListAfterTimeoutAsync(requestId, replicaAddress);
					}
			}
			return null;
		}

		private async void ReturnReplicaAddressFromGreyListAfterTimeoutAsync(Guid requestId, IPEndPoint replicaAddress)
		{
			var sw = new Stopwatch();
			sw.Start();
			await Task.Delay(GreyListTimeout);
			sw.Stop();
			lock (locker)
			{
				log.InfoFormat("{0}: returned {1} from grey list after {2} ms", requestId, replicaAddress, sw.ElapsedMilliseconds);
				if (replicaGreyAddresses.Remove(replicaAddress))
					replicaAddresses.Add(replicaAddress);
			}
		}

		private async Task<WebResponse> GetResponseFromReplica(Guid requestId, IPEndPoint replicaAddress, string query)
		{
			var replicaRequest = CreateRequestToReplica(replicaAddress, query);
			log.InfoFormat("{0}: {1} sent {2} to {3}", requestId, Name, query, replicaAddress);
			try
			{
				var replicaResponse = await replicaRequest.GetResponseAsync().TimeoutAfter(ReplicaTimeout);
				log.InfoFormat("{0}: {1} received processed query from {2}", requestId, Name, replicaAddress);
				return replicaResponse;
			}
			catch (Exception e)
			{
				log.ErrorFormat("Can't get response from {0}: {1}", replicaAddress, e.Message);
				return null;
			}
		}

		private HttpWebRequest CreateRequestToReplica(IPEndPoint replicaAddress, string query)
		{
			var uriStr = string.Format("http://{0}/{1}?{2}", replicaAddress, suffix, query);
			var request = WebRequest.CreateHttp(uriStr);
			request.Proxy = null;
			request.KeepAlive = true;
			request.ServicePoint.UseNagleAlgorithm = false;
			request.ServicePoint.ConnectionLimit = 4;
			return request;
		}

		public override string Name
		{
			get { return "Balancer"; }
		}

		private int ReplicaTimeout { get; set; }
		private int GreyListTimeout { get; set; }
	}
}
