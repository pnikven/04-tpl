using System;
using System.Collections.Generic;
using System.Net;
using FakeItEasy;
using log4net;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Balancer
{
	[TestFixture]
	class Balancer_should
	{
		private ILog log;
		private IPEndPoint balancerAddress;
		private string query;
		private Balancer balancer;
		private List<IPEndPoint> replicaAddresses;
			
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			log = A.Fake<ILog>();
			balancerAddress = new IPEndPoint(IPAddress.Loopback, 10000);
			query = "333";
			balancer = new Balancer(balancerAddress, null, log);
			balancer.Start();
		}

		[SetUp]
		public void SetUp()
		{
			balancer.ClearReplicas();
		}

		[Test]
		public void listen_http_requests()
		{
			balancer.TryAddReplica(new IPEndPoint(IPAddress.Loopback, 20000));
			CreateHttpRequestAndGetResponse(
				string.Format("http://{0}/method?query={1}", balancerAddress, query));

			A.CallTo(() => log.InfoFormat("{0}: received {1} from {2}",
				A<Guid>.Ignored, query, A<IPEndPoint>.Ignored)).MustHaveHappened();
		}

		[Test]
		public void return_error_code_500_if_there_is_no_replicas()
		{
			var ex = Assert.Catch<WebException>(() => CreateHttpRequestAndGetResponse(
				string.Format("http://{0}/method?query={1}", balancerAddress, query)));
			StringAssert.Contains("500", ex.Message);

			A.CallTo(() => log.InfoFormat("{0}: can't proxy request: there is no any replica",
				A<Guid>.Ignored)).MustHaveHappened();
		}

		[Test]
		[Ignore]
		public void proxy_client_request_to_random_replica()
		{

		}

		private WebResponse CreateHttpRequestAndGetResponse(string uri)
		{
			var request = WebRequest.CreateHttp(uri);
			return request.GetResponse();
		}
	}
}