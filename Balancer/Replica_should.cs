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
			var replica = new Replica(replicaAddress, log);
			replica.Start();
			var query = "query=1";
			var uri = string.Format("http://{0}/method?{1}", replicaAddress, query);
			var request = WebRequest.CreateHttp(uri);

			request.GetResponse();

			A.CallTo(() => log.InfoFormat("{0}: replica {1} received {2} from {3}",
				A<Guid>.Ignored, replica.Id, query, A<IPEndPoint>.Ignored)).MustHaveHappened();
		}
	}
}