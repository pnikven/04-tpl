using System;
using System.Collections.Generic;
using System.Linq;
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
		private string processedQuery;
		private Balancer balancer;
		private const int balancerRandomSeed = 0;
		private List<IPEndPoint> replicaAddresses;
		private List<Replica> replicas;
		private Random random;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			balancerAddress = new IPEndPoint(IPAddress.Loopback, 10000);
			query = "qqq=lalala";
			processedQuery = QueryProcessor.Process(query);
			replicaAddresses = new[]
			{
				new IPEndPoint(IPAddress.Loopback, 20000),
				new IPEndPoint(IPAddress.Loopback, 20001),
				new IPEndPoint(IPAddress.Loopback, 20002),
			}.ToList();
			random = new Random(balancerRandomSeed);
		}

		[SetUp]
		public void SetUp()
		{
			log = A.Fake<ILog>();
			balancer = new Balancer(balancerAddress, log, balancerRandomSeed);
			balancer.Start();
			replicas = replicaAddresses.Select(address => new Replica(address, log)).ToList();
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
			CreateTestHttpRequestToBalancerAndGetResponse();

			CheckBalancerReceivedQuery();
		}

		[Test]
		public void return_error_code_500_if_there_is_no_replicas()
		{
			var ex = Assert.Catch<WebException>(() => CreateTestHttpRequestToBalancerAndGetResponse());
			StringAssert.Contains("500", ex.Message);

			A.CallTo(() => log.InfoFormat("{0}: {1} can't proxy request: there is no any replica",
				A<Guid>.Ignored, balancer.Name)).MustHaveHappened();
		}

		[Test]
		public void proxy_client_request_to_replica_if_there_is_exactly_one_replica()
		{
			var replica = replicas[0];
			balancer.TryAddReplicaAddress(replica.Address);
			CreateTestHttpRequestToBalancerAndGetResponse();

			CheckBalancerReceivedQuery();
			CheckBalancerSentQueryToReplica(replica);
			CheckReplicaReceivedQuery(replica);
			CheckReplicaSentProcessedQuery(replica);
			CheckBalancerReceivedProcessedQueryFromReplica(replica);
			CheckBalancerSentProcessedQuery();
		}

		[Test]
		public void proxy_client_request_to_random_replica()
		{
			AddAllTestReplicaAddressesToBalancer();
			CreateTestHttpRequestToBalancerAndGetResponse();
			var chosenReplica = replicas.FirstOrDefault(x => x.Address.Equals(balancer.LastChosenReplicaAddress));

			CheckBalancerReceivedQuery();
			CheckBalancerSentQueryToReplica(chosenReplica);
			CheckReplicaReceivedQuery(chosenReplica);
			CheckReplicaSentProcessedQuery(chosenReplica);
			CheckBalancerReceivedProcessedQueryFromReplica(chosenReplica);
			CheckBalancerSentProcessedQuery();
		}

		private void AddAllTestReplicaAddressesToBalancer()
		{
			foreach (var replica in replicas)
				balancer.TryAddReplicaAddress(replica.Address);
		}

		[Test]
		public void repeat_query_to_other_replica_if_current_chosen_replica_fails()
		{
			AddAllTestReplicaAddressesToBalancer();
			var replica = replicas[random.Next(replicas.Count())];
			replica.Stop();

			TestBadReplica(replica);
		}

		[Test]
		public void repeat_query_to_other_replica_if_current_chosen_replica_returns_error_code_500()
		{
			AddAllTestReplicaAddressesToBalancer();
			var replica = replicas[random.Next(replicas.Count())];
			replica.ShouldReturn500Error = true;

			TestBadReplica(replica);
		}

		private void TestBadReplica(Replica replica)
		{
			CreateTestHttpRequestToBalancerAndGetResponse();

			CheckBalancerReceivedQuery();
			CheckBalancerSentQueryToReplica(replica);
			A.CallTo(() => log.InfoFormat("{0}: {1} can't proxy request to {2}: try next replica",
				A<Guid>.Ignored, balancer.Name, replica.Address)).MustHaveHappened();
			replicas.Remove(replica);
			replica.Stop();
			var nextReplica = replicas[random.Next(replicas.Count())];
			CheckBalancerSentQueryToReplica(nextReplica);
			CheckReplicaReceivedQuery(nextReplica);
			CheckReplicaSentProcessedQuery(nextReplica);
			CheckBalancerReceivedProcessedQueryFromReplica(nextReplica);
			CheckBalancerSentProcessedQuery();
		}

		private WebResponse CreateTestHttpRequestToBalancerAndGetResponse()
		{
			var request = WebRequest.CreateHttp(
				string.Format("http://{0}/method?{1}", balancerAddress, query));
			return request.GetResponse();
		}

		private void CheckBalancerReceivedQuery()
		{
			A.CallTo(() => log.InfoFormat("{0}: {1} received {2} from {3}",
				A<Guid>.Ignored, balancer.Name, query, A<IPEndPoint>.Ignored)).MustHaveHappened();
		}

		private void CheckBalancerSentQueryToReplica(Replica replica)
		{
			A.CallTo(() => log.InfoFormat("{0}: {1} sent {2} to {3}",
				A<Guid>.Ignored, balancer.Name, query, replica.Address)).MustHaveHappened();
		}

		private void CheckReplicaReceivedQuery(Replica replica)
		{
			A.CallTo(() => log.InfoFormat("{0}: {1} received {2} from {3}",
				A<Guid>.Ignored, replica.Name, query, A<IPEndPoint>.Ignored)).MustHaveHappened();
		}

		private void CheckReplicaSentProcessedQuery(Replica replica)
		{
			A.CallTo(() => log.InfoFormat("{0}: {1} sent {2} to {3}",
				A<Guid>.Ignored, replica.Name, processedQuery, A<IPEndPoint>.Ignored)).MustHaveHappened();
		}

		private void CheckBalancerReceivedProcessedQueryFromReplica(Replica chosenReplica)
		{
			A.CallTo(() => log.InfoFormat("{0}: {1} received {2} from {3}",
				A<Guid>.Ignored, balancer.Name, processedQuery, chosenReplica.Address)).MustHaveHappened();
		}

		private void CheckBalancerSentProcessedQuery()
		{
			A.CallTo(() => log.InfoFormat("{0}: {1} sent {2} to {3}",
				A<Guid>.Ignored, balancer.Name, processedQuery, A<IPEndPoint>.Ignored)).MustHaveHappened();
		}
	}
}