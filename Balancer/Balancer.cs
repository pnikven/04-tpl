using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Balancer
{
	class Balancer : HttpListener
	{
		private readonly List<IPEndPoint> replicaAddresses;
		private readonly Random random;

		public Balancer(IPEndPoint balancerAddress, IPEndPoint[] replicaAddresses, ILog log)
			: base(balancerAddress, log)
		{
			this.replicaAddresses = replicaAddresses == null ? new List<IPEndPoint>() : replicaAddresses.ToList();
			random = new Random();
			ReplicaTimeout = 100 * 1000;
		}

		public Balancer(IPEndPoint balancerAddress, ILog log, int randomSeed, int replicaTimeout)
			: base(balancerAddress, log)
		{
			replicaAddresses = new List<IPEndPoint>();
			random = new Random(randomSeed);
			ReplicaTimeout = replicaTimeout;
		}

		public bool TryAddReplicaAddress(IPEndPoint replicaAddress)
		{
			if (replicaAddresses.Contains(replicaAddress))
				return false;
			replicaAddresses.Add(replicaAddress);
			return true;
		}

		public bool TryRemoveReplicaAddress(IPEndPoint replicaAddress)
		{
			if (!replicaAddresses.Contains(replicaAddress))
				return false;
			replicaAddresses.Remove(replicaAddress);
			return true;
		}

		public void ClearReplicaAddresses()
		{
			replicaAddresses.Clear();
		}

		protected override async Task OnContextAsync(HttpListenerContext context)
		{
			var requestId = Guid.NewGuid();
			var query = GetQuery(context.Request.RawUrl);
			var acceptEncoding = context.Request.Headers.GetValues("Accept-Encoding");
			var remoteEndPoint = context.Request.RemoteEndPoint;
			log.InfoFormat("{0}: {1} received {2} from {3}", requestId, Name, query, remoteEndPoint);

			WebResponse replicaResponse;
			if (TryProxyRequestToRandomReplica(requestId, query, out replicaResponse))
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

		private bool TryProxyRequestToRandomReplica(Guid requestId, string query, out WebResponse replicaResponse)
		{
			while (replicaAddresses.Count > 0)
			{
				var replicaAddress = GetRandomReplicaAddress();
				if (TryGetResponseFromReplica(requestId, replicaAddress, query, out replicaResponse))
					return true;
				replicaAddresses.Remove(replicaAddress);
				log.InfoFormat("{0}: {1} can't proxy request to {2}", requestId, Name, replicaAddress);
			}
			replicaResponse = null;
			return false;
		}

		private bool TryGetResponseFromReplica(Guid requestId, IPEndPoint replicaAddress, string query, out WebResponse replicaResponse)
		{
			var replicaRequest = CreateRequestToReplica(replicaAddress, query);
			log.InfoFormat("{0}: {1} sent {2} to {3}", requestId, Name, query, replicaAddress);
			try
			{
				replicaResponse = replicaRequest.GetResponse();
				log.InfoFormat("{0}: {1} received processed query from {2}", requestId, Name, replicaAddress);
				return true;
			}
			catch (Exception e)
			{
				log.Error(e);
				replicaResponse = null;
				return false;
			}
		}

		private HttpWebRequest CreateRequestToReplica(IPEndPoint replicaAddress, string query)
		{
			var uriStr = string.Format("http://{0}/{1}?{2}", replicaAddress, suffix, query);
			var request = WebRequest.CreateHttp(uriStr);
			request.Timeout = ReplicaTimeout;
			return request;
		}

		private IPEndPoint GetRandomReplicaAddress()
		{
			return replicaAddresses[random.Next(replicaAddresses.Count)];
		}

		public override string Name
		{
			get { return "Balancer"; }
		}

		private int ReplicaTimeout { get; set; }
	}
}
