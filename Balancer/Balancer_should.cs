using System;
using System.Net;
using FakeItEasy;
using log4net;
using NUnit.Framework;

namespace Balancer
{
	[TestFixture]
	class BalancerTests
	{
		[Test]
		public void balancer_should_listen_http_requests()
		{
			var log = A.Fake<ILog>();
			var balancerAddress = new IPEndPoint(IPAddress.Loopback, 10000);
			var balancer = new Balancer(balancerAddress, null, log);
			balancer.Start();
			var uri = string.Format("http://{0}/method?query=1", balancerAddress);
			var request = WebRequest.CreateHttp(uri);
			request.GetResponse();
			A.CallTo(() => log.InfoFormat("{0}: received {1} from {2}",
				A.Dummy<Guid>(), "1", A.Dummy<IPEndPoint>())).MustHaveHappened();
		}

	}
}