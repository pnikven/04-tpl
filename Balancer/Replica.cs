using System.Net;
using System.Threading.Tasks;
using log4net;

namespace Balancer
{
	class Replica : HttpListener
	{
		public int Id { get; private set; }

		public Replica(int replicaId, IPEndPoint replicaAddress, ILog log)
			: base(replicaAddress, OnContextAsync, log)
		{
			Id = replicaId;
		}

		private static async Task OnContextAsync(HttpListenerContext context)
		{
		}

		protected override string Name
		{
			get { return string.Format("Replica {0}", Id); }
		}
	}
}
