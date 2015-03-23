using System.Net;
using log4net;

namespace Balancer
{
	class Replica
	{
		public int Id { get; private set; }
		private readonly ILog log;
		private readonly IPEndPoint Address;

		public Replica(int replicaId, IPEndPoint replicaAddress, ILog log)
		{
			this.log = log;
			Id = replicaId;
			Address = replicaAddress;
		}

		public void Start()
		{

		}
	}
}
