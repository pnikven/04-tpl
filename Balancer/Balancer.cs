using System;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace Balancer
{
	class Balancer : HttpListener
	{
		private readonly IPEndPoint[] serverAddresses;

		public Balancer(IPEndPoint balancerAddress, IPEndPoint[] serverAddresses, ILog log)
			: base(balancerAddress, OnContextAsync, log)
		{
			this.serverAddresses = serverAddresses;
		}

		private static async Task OnContextAsync(HttpListenerContext context)
		{
			var requestId = Guid.NewGuid();
			var query = context.Request.QueryString["query"];
			var remoteEndPoint = context.Request.RemoteEndPoint;
			log.InfoFormat("{0}: received {1} from {2}", requestId, query, remoteEndPoint);
			context.Request.InputStream.Close();
		}

		protected override string Name
		{
			get { return "Balancer"; }
		}
	}
}
