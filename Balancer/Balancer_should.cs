using System;
using System.Collections.Generic;
using System.Linq;
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
		private string processedQuery;
		private Balancer balancer;
		private List<IPEndPoint> replicaAddresses;
		private Replica[] replicas;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			balancerAddress = new IPEndPoint(IPAddress.Loopback, 10000);
			query = "query=333";
			processedQuery = QueryProcessor.Process(query);
			replicaAddresses = new[]
			{
				new IPEndPoint(IPAddress.Loopback, 20000),
				new IPEndPoint(IPAddress.Loopback, 20001),
				new IPEndPoint(IPAddress.Loopback, 20002),
			}.ToList();
		}

		[SetUp]
		public void SetUp()
		{
			log = A.Fake<ILog>();
			balancer = new Balancer(balancerAddress, null, log);
			balancer.Start();
			replicas = replicaAddresses.Select(address => new Replica(address, log)).ToArray();
			foreach (var replica in replicas)
				replica.Start();
		}

		[TearDown]
		public void TestFixtureTearDown()
		{
			foreach (var replica in replicas)
				replica.Stop();
			balancer.Stop();
		}

		[Test]
		public void listen_http_requests()
		{
			balancer.TryAddReplicaAddress(replicaAddresses[0]);
			CreateHttpRequestAndGetResponse(
				string.Format("http://{0}/method?{1}", balancerAddress, query));

			A.CallTo(() => log.InfoFormat("{0}: {1} received {2} from {3}",
				A<Guid>.Ignored, balancer.Name, query, A<IPEndPoint>.Ignored)).MustHaveHappened();
		}

		[Test]
		public void return_error_code_500_if_there_is_no_replicas()
		{
			var ex = Assert.Catch<WebException>(() => CreateHttpRequestAndGetResponse(
				string.Format("http://{0}/method?{1}", balancerAddress, query)));
			StringAssert.Contains("500", ex.Message);

			A.CallTo(() => log.InfoFormat("{0}: can't proxy request: there is no any replica",
				A<Guid>.Ignored)).MustHaveHappened();
		}

		[Test]
		public void proxy_client_request_to_replica_if_there_is_exactly_one_replica()
		{
			var replica = replicas[0];
			balancer.TryAddReplicaAddress(replica.Address);
			CreateHttpRequestAndGetResponse(
				string.Format("http://{0}/method?query={1}", balancerAddress, query));

			A.CallTo(() => log.InfoFormat("{0}: {1} sent {2} to {3}",
				A<Guid>.Ignored, balancer.Name, query, replica.Name)).MustHaveHappened();
			A.CallTo(() => log.InfoFormat("{0}: {1} received {2} from {3}",
				A<Guid>.Ignored, replica.Name, query, balancer.Name)).MustHaveHappened();
			A.CallTo(() => log.InfoFormat("{0}: {1} sent {2} to {3}",
				A<Guid>.Ignored, replica.Name, query, balancer.Name)).MustHaveHappened();
			A.CallTo(() => log.InfoFormat("{0}: {1} received {2} from {3}",
				A<Guid>.Ignored, balancer.Name, query, replica.Name)).MustHaveHappened();
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