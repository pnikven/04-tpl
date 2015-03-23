using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Listeners;

namespace Balancer
{
	class Balancer
	{
		private const string suffix = "method";
		private readonly IPEndPoint balancerAddress;
		private readonly IPEndPoint[] serverAddresses;
		private Listener listener;
		private readonly ILog log;

		public Balancer(IPEndPoint balancerAddress, IPEndPoint[] serverAddresses, ILog log)
		{
			this.balancerAddress = balancerAddress;
			this.serverAddresses = serverAddresses;
			this.log = log;
		}

		public void Start()
		{
			listener = new Listener(balancerAddress.Port, suffix, OnContextAsync, log);
			listener.Start();
			log.InfoFormat("Balancer started!");
		}

		private async Task OnContextAsync(HttpListenerContext context)
		{
			var requestId = Guid.NewGuid();
			var query = context.Request.QueryString["query"];
			var remoteEndPoint = context.Request.RemoteEndPoint;
			log.InfoFormat("{0}: received {1} from {2}", requestId, query, remoteEndPoint);
			context.Request.InputStream.Close();
		}

	}
}
