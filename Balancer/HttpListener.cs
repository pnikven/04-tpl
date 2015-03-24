using System;
using System.Net;
using System.Threading.Tasks;
using log4net;
using Listeners;

namespace Balancer
{
	abstract class HttpListener
	{
		private const string suffix = "method";
		private readonly IPEndPoint address;
		private readonly Func<HttpListenerContext, Task> onContextAsync;
		protected static ILog log;
		private Listener listener;

		protected HttpListener(IPEndPoint address, Func<HttpListenerContext, Task> onContextAsync, ILog log)
		{
			this.address = address;
			this.onContextAsync = onContextAsync;
			HttpListener.log = log;
		}

		public void Start()
		{
			listener = new Listener(address.Port, suffix, onContextAsync, log);
			listener.Start();
			log.InfoFormat("{0} started!", Name);
		}

		protected abstract string Name { get; }
	}
}