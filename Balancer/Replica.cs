using System;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace Balancer
{
	class Replica : HttpListener
	{
		public Replica(IPEndPoint replicaAddress, ILog log)
			: base(replicaAddress, log)
		{
		}

		public IPEndPoint Address { get { return address; } }

		protected override async Task OnContextAsync(HttpListenerContext context)
		{
			var requestId = Guid.NewGuid();
			var query = GetQuery(context.Request.RawUrl);
			var remoteEndPoint = context.Request.RemoteEndPoint;
			log.InfoFormat("{0}: {1} received {2} from {3}", requestId, Name, query, remoteEndPoint);
			context.Request.InputStream.Close();
		}

		public override string Name
		{
			get { return string.Format("Replica {0}", address); }
		}
	}
}
