using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace HashServer
{
	public class ListenerSync
	{
		private HttpListener listener;

		public ListenerSync(int port, string suffix, Action<HttpListenerContext> callback)
		{
			ThreadPool.SetMinThreads(8, 8);
			Callback = callback;
			listener = new HttpListener();
			listener.Prefixes.Add(string.Format("http://+:{0}{1}/", port, suffix != null ? "/" + suffix.TrimStart('/') : ""));
		}

		public void Start()
		{
			listener.Start();
			Task.Run(() => StartListen());
		}

		private void StartListen()
		{
			while(true)
			{
				try
				{
					var context = listener.GetContext();
					Task.Run(
						() =>
						{
							try
							{
								Callback(context);
							}
							catch(Exception e)
							{
								log.Error(e);
							}
							finally
							{
								context.Response.Close();
							}
						}
					);
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}

		private Action<HttpListenerContext> Callback { get; set; }

		private static readonly ILog log = LogManager.GetLogger(typeof(ListenerSync));
	}
}
