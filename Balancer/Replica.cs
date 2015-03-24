using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Balancer
{
	class Replica : HttpListener
	{
		public Replica(IPEndPoint replicaAddress, ILog log)
			: base(replicaAddress, log)
		{
		}

		public bool ShouldReturn500Error { get; set; }
		public int RequestProcessingTime { get; set; }

		protected override async Task OnContextAsync(HttpListenerContext context)
		{
			var requestId = Guid.NewGuid();
			var query = GetQuery(context.Request.RawUrl);
			var remoteEndPoint = context.Request.RemoteEndPoint;
			log.InfoFormat("{0}: {1} received {2} from {3}", requestId, Name, query, remoteEndPoint);
			context.Request.InputStream.Close();
			if (ShouldReturn500Error)
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			else
			{
				if (RequestProcessingTime > 0)
					await Task.Delay(RequestProcessingTime);
				var processedQuery = QueryProcessor.Process(query);
				var encryptedBytes = Encoding.UTF8.GetBytes(processedQuery);
				await context.Response.OutputStream.WriteAsync(encryptedBytes, 0, encryptedBytes.Length);
				log.InfoFormat("{0}: {1} sent {2} to {3}", requestId, Name, processedQuery, remoteEndPoint);
			}
			context.Response.OutputStream.Close();
		}

		public override string Name
		{
			get { return string.Format("Replica {0}", Address); }
		}
	}
}
