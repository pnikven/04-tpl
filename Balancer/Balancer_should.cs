using System;
using System.Collections.Generic;
using System.Net;
using FakeItEasy;
using log4net;
using NUnit.Framework;

namespace Balancer
{
	[TestFixture]
	class Balancer_should
	{
		private ILog log;
		private IPEndPoint balancerAddress;
		private string query;
		private Balancer balancer;

		[SetUp]
		public void SetUp()
		{
			log = A.Fake<ILog>();
			balancerAddress = new IPEndPoint(IPAddress.Loopback, 10000);
			query = "333";
			balancer = new Balancer(balancerAddress, null, log);
			balancer.Start();
		}

		[Test]
		public void listen_http_requests()
		{
			var uri = string.Format("http://{0}/method?query={1}", balancerAddress, query);
			var request = WebRequest.CreateHttp(uri);

			request.GetResponse();

			A.CallTo(() => log.InfoFormat("{0}: received {1} from {2}",
				A<Guid>.Ignored, query, A<IPEndPoint>.Ignored)).MustHaveHappened();
		}

		[Test]
		public void return_error_code_500_if_there_is_no_replicas()
		{
			var balancer = new Balancer(balancerAddress, null, log);
			balancer.Start();
			var uri = string.Format("http://{0}/method?query={1}", balancerAddress, query);
			var request = WebRequest.CreateHttp(uri);

			request.GetResponse();

			A.CallTo(() => log.InfoFormat("{0}: received {1} from {2}",
				A<Guid>.Ignored, query, A<IPEndPoint>.Ignored)).MustHaveHappened();


		}

		[Test]
		public void proxy_client_request_to_random_replica()
		{

		}

	}
}