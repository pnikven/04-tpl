using System;
using System.Net;
using FakeItEasy;
using log4net;
using NUnit.Framework;

namespace Balancer
{
	[TestFixture]
	class Replica_should
	{
		private readonly IPEndPoint replicaAddress = new IPEndPoint(IPAddress.Loopback, 20000);

		[Test]
		public void listen_http_requests()
		{
			var log = A.Fake<ILog>();
			var replicaId = 0;
			var replica = new Replica(replicaId, replicaAddress, log);
			replica.Start();
			var uri = string.Format("http://{0}/method?query=1", replicaAddress);
			var request = WebRequest.CreateHttp(uri);

			request.GetResponse();

			A.CallTo(() => log.InfoFormat("{0}: replica {3} received {1} from {2}",
				A<Guid>.Ignored, "1", A<IPEndPoint>.Ignored, replicaId)).MustHaveHappened();
		}
	}
}