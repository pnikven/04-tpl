using System.Net;
using System.Threading;
using Listeners;

namespace Balancer
{
	class Balancer
	{
		private readonly IPEndPoint[] serverAddresses;
		private readonly Listener listener;

		public Balancer(IPEndPoint[] serverAddresses)
		{
			this.serverAddresses = serverAddresses;
		}

		public void Start()
		{
			
		}
	}
}
