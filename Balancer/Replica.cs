using System;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace Balancer
{
	class Replica : HttpListener
	{
		public int Id { get; private set; }

		public Replica(int replicaId, IPEndPoint replicaAddress, ILog log)
			: base(replicaAddress, log)
		{
			Id = replicaId;
		}

		protected override async Task OnContextAsync(HttpListenerContext context)
		{
			var requestId = Guid.NewGuid();
			var query = context.Request.QueryString["query"];
			var remoteEndPoint = context.Request.RemoteEndPoint;
			log.InfoFormat("{0}: replica {1} received {2} from {3}",
				requestId, Id, query, remoteEndPoint);
			context.Request.InputStream.Close();
		}

		protected override string Name
		{
			get { return string.Format("Replica {0}", Id); }
		}
	}
}
