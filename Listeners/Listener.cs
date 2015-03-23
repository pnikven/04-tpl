using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Listeners
{
	public class Listener
	{
		private readonly HttpListener listener;

		public Listener(int port, string suffix, Func<HttpListenerContext, Task> callbackAsync, ILog log)
		{
			ThreadPool.SetMinThreads(8, 8);
			CallbackAsync = callbackAsync;
			listener = new HttpListener();
			listener.Prefixes.Add(string.Format("http://+:{0}{1}/", port, suffix != null ? "/" + suffix.TrimStart('/') : ""));
			this.log = log;
		}

		public void Start()
		{
			listener.Start();
			StartListen();
		}

		private async void StartListen()
		{
			while (true)
			{
				try
				{
					var context = await listener.GetContextAsync();

					Task.Run(
						async () =>
						{
							var ctx = context;
							try
							{
								await CallbackAsync(ctx);
							}
							catch (Exception e)
							{
								log.Error(e);
							}
							finally
							{
								ctx.Response.Close();
							}
						}
					);
				}
				catch (Exception e)
				{
					log.Error(e);
				}
			}
		}

		private Func<HttpListenerContext, Task> CallbackAsync { get; set; }

		private readonly ILog log;
	}
}
