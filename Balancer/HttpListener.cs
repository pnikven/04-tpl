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
		protected static ILog log;
		private Listener listener;

		protected HttpListener(IPEndPoint address, ILog log)
		{
			this.address = address;
			HttpListener.log = log;
		}

		public void Start()
		{
			listener = new Listener(address.Port, suffix, OnContextAsync, log);
			listener.Start();
			log.InfoFormat("{0} started!", Name);
		}

		protected abstract Task OnContextAsync(HttpListenerContext context);

		protected abstract string Name { get; }
	}
}