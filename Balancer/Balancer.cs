using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace Balancer
{
	class Balancer : HttpListener
	{
		private readonly List<IPEndPoint> replicas;

		public Balancer(IPEndPoint balancerAddress, IPEndPoint[] replicaAddresses, ILog log)
			: base(balancerAddress, log)
		{
			replicas = replicaAddresses == null ? new List<IPEndPoint>() : replicaAddresses.ToList();
		}

		public bool TryAddReplica(IPEndPoint replicaAddress)
		{
			if (replicas.Contains(replicaAddress))
				return false;
			replicas.Add(replicaAddress);
			return true;
		}

		public bool TryRemoveReplica(IPEndPoint replicaAddress)
		{
			if (!replicas.Contains(replicaAddress))
				return false;
			replicas.Remove(replicaAddress);
			return true;
		}

		public void ClearReplicas()
		{
			replicas.Clear();
		}

		protected override async Task OnContextAsync(HttpListenerContext context)
		{
			var requestId = Guid.NewGuid();
			var query = context.Request.QueryString["query"];
			var remoteEndPoint = context.Request.RemoteEndPoint;
			log.InfoFormat("{0}: received {1} from {2}", requestId, query, remoteEndPoint);
			if (replicas.Count == 0)
			{
				log.InfoFormat("{0}: can't proxy request: there is no any replica", requestId);
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			}
			context.Request.InputStream.Close();
			context.Response.OutputStream.Close();
		}

		protected override string Name
		{
			get { return "Balancer"; }
		}
	}
}
