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
		private const int balancerTimeoutForReplica = 1000;
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
		}

		[SetUp]
		public void SetUp()
		{
			log = A.Fake<ILog>();
			balancer = new Balancer(balancerAddress, log, balancerRandomSeed, balancerTimeoutForReplica);
			balancer.Start();
			replicas = replicaAddresses.Select(address => new Replica(address, log)).ToList();
			foreach (var replica in replicas)
				replica.Start();
			random = new Random(balancerRandomSeed);
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
			CheckGoodReplica(replica);
			CheckBalancerSentProcessedQuery();
		}

		[Test]
		public void proxy_client_request_to_random_replica()
		{
			AddAllTestReplicaAddressesToBalancer();
			CreateTestHttpRequestToBalancerAndGetResponse();
			var chosenReplica = replicas.FirstOrDefault(x => x.Address.Equals(balancer.LastChosenReplicaAddress));

			CheckBalancerReceivedQuery();
			CheckGoodReplica(chosenReplica);
			CheckBalancerSentProcessedQuery();
		}

		private void AddAllTestReplicaAddressesToBalancer()
		{
			foreach (var replica in replicas)
				balancer.TryAddReplicaAddress(replica.Address);
		}

		[TestCase(1)]
		[TestCase(2)]
		[TestCase(3)]
		public void repeat_query_to_other_replica_if_current_chosen_replica_fails(int testCaseId)
		{
			AddAllTestReplicaAddressesToBalancer();

			var leftReplicas = replicas.ToList();
			var replica = leftReplicas[random.Next(leftReplicas.Count())];
			switch (testCaseId)
			{
				case 1: replica.Stop();
					break;
				case 2: replica.ShouldReturn500Error = true;
					break;
				case 3: replica.RequestProcessingTime = balancerTimeoutForReplica + 1;
					break;
				default:
					throw new NotImplementedException();
			}

			CreateTestHttpRequestToBalancerAndGetResponse();
			CheckBalancerReceivedQuery();

			CheckBadReplica(replica);

			leftReplicas.Remove(replica);
			var nextReplica = leftReplicas[random.Next(leftReplicas.Count())];

			CheckGoodReplica(nextReplica);

			CheckBalancerSentProcessedQuery();
		}

		private WebResponse CreateTestHttpRequestToBalancerAndGetResponse()
		{
			var request = WebRequest.CreateHttp(
				string.Format("http://{0}/method?{1}", balancerAddress, query));
			return request.GetResponse();
		}

		private void CheckGoodReplica(Replica replica)
		{
			CheckBalancerSentQueryToReplica(replica);
			CheckReplicaReceivedQuery(replica);
			CheckReplicaSentProcessedQuery(replica);
			CheckBalancerReceivedProcessedQueryFromReplica(replica);
		}

		private void CheckBadReplica(Replica replica)
		{
			CheckBalancerSentQueryToReplica(replica);
			A.CallTo(() => log.InfoFormat("{0}: {1} can't proxy request to {2}: try next replica",
				A<Guid>.Ignored, balancer.Name, replica.Address)).MustHaveHappened();
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