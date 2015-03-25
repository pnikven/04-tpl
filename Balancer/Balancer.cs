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
			var remoteEndPoint = context.Request.RemoteEndPoint;
			log.InfoFormat("{0}: {1} received {2} from {3}", requestId, Name, query, remoteEndPoint);

			WebResponse replicaResponse = null;
			while (replicaResponse == null)
			{
				if (replicaAddresses.Count == 0)
				{
					log.InfoFormat("{0}: {1} can't proxy request: there is no any replica", requestId, Name);
					context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
					break;
				}
				var replicaAddress = GetRandomReplicaAddress();
				try
				{
					var replicaUrl = string.Format("http://{0}/{1}?{2}", replicaAddress, suffix, query);
					var replicaRequest = WebRequest.CreateHttp(replicaUrl);
					replicaRequest.Timeout = ReplicaTimeout;
					log.InfoFormat("{0}: {1} sent {2} to {3}", requestId, Name, query, replicaAddress);
					replicaResponse = replicaRequest.GetResponse();
					using (var stream = replicaResponse.GetResponseStream())
					{
						log.InfoFormat("{0}: {1} received processed query from {2}", requestId, Name, replicaAddress);
						await stream.CopyToAsync(context.Response.OutputStream);
						log.InfoFormat("{0}: {1} sent processed query to {2}", requestId, Name, remoteEndPoint);
					}
				}
				catch (Exception e)
				{
					log.Error(e);
					log.InfoFormat("{0}: {1} can't proxy request to {2}: try next replica",
						requestId, Name, replicaAddress);
					replicaAddresses.Remove(replicaAddress);
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

		private int ReplicaTimeout { get; set; }
	}
}
