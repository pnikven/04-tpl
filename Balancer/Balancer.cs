using System.Net;
using System.Threading;
using log4net;
using Listeners;

namespace Balancer
{
	class Balancer
	{
		private readonly IPEndPoint[] serverAddresses;
		private readonly Listener listener;
		private readonly ILog log;

		public Balancer(IPEndPoint balancerAddress, IPEndPoint[] serverAddresses, ILog log)
		{
			this.serverAddresses = serverAddresses;
			this.log = log;
		}

		public void Start()
		{
			
		}
	}
}
