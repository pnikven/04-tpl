using System.Net;
using System.Threading.Tasks;
using log4net;
using Listeners;

namespace Balancer
{
	abstract class HttpListener
	{
		public IPEndPoint Address { get; private set; }
		protected const string suffix = "method";
		protected static ILog log;
		private Listener listener;

		protected HttpListener(IPEndPoint address, ILog log)
		{
			Address = address;
			HttpListener.log = log;
		}

		public void Start()
		{
			listener = new Listener(Address.Port, suffix, OnContextAsync, log);
			listener.Start();
			log.InfoFormat("{0} started!", Name);
		}

		public void Stop()
		{
			listener.Stop();
			log.InfoFormat("{0} stopped!", Name);
		}

		protected abstract Task OnContextAsync(HttpListenerContext context);

		public abstract string Name { get; }

		protected string GetQuery(string rawUrl)
		{
			return rawUrl.Split('?')[1];
		}
	}
}