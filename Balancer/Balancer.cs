using System;
using System.Collections.Generic;
using System.IO;
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
		}

		public Balancer(IPEndPoint balancerAddress, ILog log, int randomSeed)
			: base(balancerAddress, log)
		{
			replicaAddresses = new List<IPEndPoint>();
			random = new Random(randomSeed);
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
			var remoteEndPoint = context.Request.RemoteEndPoint;
			log.InfoFormat("{0}: {1} received {2} from {3}", requestId, Name, query, remoteEndPoint);
			if (replicaAddresses.Count == 0)
			{
				log.InfoFormat("{0}: can't proxy request: there is no any replica", requestId);
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			}
			else
			{
				LastChosenReplicaAddress = GetRandomReplicaAddress();
				var replicaUrl = string.Format("http://{0}/{1}?{2}", LastChosenReplicaAddress, suffix, query);
				var replicaRequest = WebRequest.CreateHttp(replicaUrl);
				log.InfoFormat("{0}: {1} sent {2} to {3}", requestId, Name, query, LastChosenReplicaAddress);
				var replicaResponse = await replicaRequest.GetResponseAsync();
				using (var stream = replicaResponse.GetResponseStream())
				{
					var streamReader = new StreamReader(stream, Encoding.UTF8);
					var processedQuery = await streamReader.ReadToEndAsync();
					log.InfoFormat("{0}: {1} received {2} from {3}", requestId, Name, processedQuery, LastChosenReplicaAddress);
					stream.CopyToAsync(context.Response.OutputStream);
					log.InfoFormat("{0}: {1} sent {2} to {3}", requestId, Name, processedQuery, remoteEndPoint);

				}
			}
			context.Request.InputStream.Close();
			context.Response.OutputStream.Close();
		}

		private IPEndPoint GetRandomReplicaAddress()
		{
			return replicaAddresses[random.Next(replicaAddresses.Count)];
		}

		public override string Name
		{
			get { return "Balancer"; }
		}

		public IPEndPoint LastChosenReplicaAddress { get; private set; }
	}
}
