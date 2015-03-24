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
			log = A.Fake<ILog>();
			balancerAddress = new IPEndPoint(IPAddress.Loopback, 10000);
			query = "333";
			processedQuery = QueryProcessor.Process(query);
			balancer = new Balancer(balancerAddress, null, log);
			balancer.Start();
			replicaAddresses = new[]
			{
				new IPEndPoint(IPAddress.Loopback, 20000),
				new IPEndPoint(IPAddress.Loopback, 20001),
				new IPEndPoint(IPAddress.Loopback, 20002),
			}.ToList();
			replicas = replicaAddresses.Select(address => new Replica(address, log)).ToArray();
			foreach (var replica in replicas)
				replica.Start();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			foreach (var replica in replicas)
				replica.Stop();
		}

		[SetUp]
		public void SetUp()
		{
			balancer.ClearReplicas();
		}

		[Test]
		public void listen_http_requests()
		{
			balancer.TryAddReplica(replicaAddresses[0]);
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
		public void proxy_client_request_to_replica_if_there_is_exactly_one_replica()
		{
			var replicaAddress = replicaAddresses[0];
			balancer.TryAddReplica(replicaAddress);
			CreateHttpRequestAndGetResponse(
				string.Format("http://{0}/method?query={1}", balancerAddress, query));

			A.CallTo(() => log.InfoFormat("{0}: sent {1} to replica {2}",
				A<Guid>.Ignored, query, replicaAddress)).MustHaveHappened();
			A.CallTo(() => log.InfoFormat("{0}: replica {1} received {2} from {3}",
				A<Guid>.Ignored, replicaAddress, query, balancerAddress)).MustHaveHappened();
			A.CallTo(() => log.InfoFormat("{0}: replica {1} sent {2} to {3}",
				A<Guid>.Ignored, replicaAddress, query, balancerAddress)).MustHaveHappened();
			A.CallTo(() => log.InfoFormat("{0}: received {1} from replica {2}",
				A<Guid>.Ignored, query, replicaAddress)).MustHaveHappened();
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