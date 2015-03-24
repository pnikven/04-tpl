using System;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace Balancer
{
	class Replica : HttpListener
	{
		public string Id
		{
			get { return address.ToString(); }
		}

		public Replica(IPEndPoint replicaAddress, ILog log)
			: base(replicaAddress, log)
		{
		}

		protected override async Task OnContextAsync(HttpListenerContext context)
		{
			var requestId = Guid.NewGuid();
			var query = context.Request.RawUrl.Split('?')[1];
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
