using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using FakeItEasy;
using log4net;
using NUnit.Framework;

namespace Balancer
{
	enum ReplicaBehaviour
	{
		Stopped,
		Failed,
		Timeouted
	}

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
		private const int balancerTimeoutForGreyList = 10 * 1000;
		private List<Replica> replicas;
		private Random random;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			balancerAddress = new IPEndPoint(IPAddress.Loopback, 10000);
			query = "qqq=lalala";
			processedQuery = QueryProcessor.Process(query);
		}

		[SetUp]
		public void SetUp()
		{
			log = A.Fake<ILog>();
			balancer = new Balancer(balancerAddress, log, balancerTimeoutForReplica, balancerTimeoutForGreyList, balancerRandomSeed);
			balancer.Start();
			var replicaAddresses = new[]
			{
				new IPEndPoint(IPAddress.Loopback, 20000),
				new IPEndPoint(IPAddress.Loopback, 20001),
				new IPEndPoint(IPAddress.Loopback, 20002),
			}.ToList();
			replicas = replicaAddresses.Select(address => new Replica(address, log)).ToList();
			foreach (var replica in replicas)
				replica.Start();
			ResetRandom();
		}

		[TearDown]
		public void TestFixtureTearDown()
		{
			StopAllReplicas();
			balancer.Stop();
		}

		private void StopAllReplicas()
		{
			foreach (var replica in replicas)
				replica.Stop();
		}

		[Test]
		public void listen_http_requests()
		{
			CreateTestHttpRequestToBalancerAndCheckResponseIgnoringExceptions();

			CheckBalancerReceivedQuery();
		}

		[Test]
		public void return_error_code_500_if_there_is_no_replicas()
		{
			var ex = Assert.Catch<WebException>(() => CreateTestHttpRequestToBalancerAndCheckResponse());
			StringAssert.Contains("500", ex.Message);
			CheckBalancerCannotProxyRequestToAnyReplica();
		}

		[Test]
		public void proxy_client_request_to_replica_if_there_is_exactly_one_replica()
		{
			var replica = replicas[0];
			balancer.TryAddReplicaAddress(replica.Address);
			CreateTestHttpRequestToBalancerAndCheckResponse();

			CheckBalancerReceivedQuery();
			CheckGoodReplica(replica);
			CheckBalancerSentProcessedQuery();
		}

		[Test]
		public void proxy_client_request_to_random_replica()
		{
			AddAllTestReplicaAddressesToBalancer();
			CreateTestHttpRequestToBalancerAndCheckResponse();
			var chosenReplica = replicas[random.Next(replicas.Count())];

			CheckBalancerReceivedQuery();
			CheckGoodReplica(chosenReplica);
			CheckBalancerSentProcessedQuery();
		}

		private void AddAllTestReplicaAddressesToBalancer()
		{
			foreach (var replica in replicas)
				balancer.TryAddReplicaAddress(replica.Address);
		}

		[TestCase(ReplicaBehaviour.Stopped)]
		[TestCase(ReplicaBehaviour.Failed)]
		[TestCase(ReplicaBehaviour.Timeouted)]
		public void repeat_query_to_other_replica_if_current_chosen_replica_fails(ReplicaBehaviour replicaBehaviour)
		{
			AddAllTestReplicaAddressesToBalancer();

			var leftReplicas = replicas.ToList();
			var replica = leftReplicas[random.Next(leftReplicas.Count())];
			switch (replicaBehaviour)
			{
				case ReplicaBehaviour.Stopped: replica.Stop();
					break;
				case ReplicaBehaviour.Failed: replica.ShouldReturn500Error = true;
					break;
				case ReplicaBehaviour.Timeouted: replica.RequestProcessingTime = balancerTimeoutForReplica + 1;
					break;
				default:
					throw new NotImplementedException();
			}

			CreateTestHttpRequestToBalancerAndCheckResponse();
			CheckBalancerReceivedQuery();

			CheckBadReplica(replica);
			if (replicaBehaviour == ReplicaBehaviour.Timeouted)
				CheckTimeoutedReplica(replica);

			leftReplicas.Remove(replica);
			var nextReplica = leftReplicas[random.Next(leftReplicas.Count())];

			CheckGoodReplica(nextReplica);

			CheckBalancerSentProcessedQuery();
		}

		[Test]
		public void repeat_query_to_left_replicas_while_active_replica_will_not_be_found()
		{
			AddAllTestReplicaAddressesToBalancer();
			var replicasToBeStoppedCount = replicas.Count - 1;
			StopReplicasInPredefinedRandomOrder(replicasToBeStoppedCount);

			CreateTestHttpRequestToBalancerAndCheckResponse();
			CheckBalancerReceivedQuery();

			var leftReplicas = replicas.ToList();
			Replica replica;
			while (replicasToBeStoppedCount > 0)
			{
				replica = leftReplicas[random.Next(leftReplicas.Count())];
				CheckBadReplica(replica);
				leftReplicas.Remove(replica);
				replicasToBeStoppedCount--;
			}
			replica = leftReplicas[random.Next(leftReplicas.Count())];
			CheckGoodReplica(replica);
			CheckBalancerSentProcessedQuery();
		}

		[Test]
		public void return_error_code_500_if_all_replicas_fail()
		{
			AddAllTestReplicaAddressesToBalancer();
			StopAllReplicas();

			var ex = Assert.Catch<WebException>(() => CreateTestHttpRequestToBalancerAndCheckResponse());
			StringAssert.Contains("500", ex.Message);
			CheckBalancerCannotProxyRequestToAnyReplica();

			CheckBalancerReceivedQuery();
			var leftReplicas = replicas.ToList();
			while (leftReplicas.Count > 0)
			{
				var replica = leftReplicas[random.Next(leftReplicas.Count())];
				CheckBadReplica(replica);
				leftReplicas.Remove(replica);
			}
		}

		private void StopReplicasInPredefinedRandomOrder(int replicasToBeStoppedCount)
		{
			var leftReplicas = replicas.ToList();
			while (replicasToBeStoppedCount > 0)
			{
				var replica = leftReplicas[random.Next(leftReplicas.Count())];
				replica.Stop();
				leftReplicas.Remove(replica);
				replicasToBeStoppedCount--;
			}
			ResetRandom();
		}

		private void ResetRandom()
		{
			random = new Random(balancerRandomSeed);
		}

		private void CreateTestHttpRequestToBalancerAndCheckResponseIgnoringExceptions(bool clientSopportsCompressing = false, int? balancerTimeout = null)
		{
			try
			{
				CreateTestHttpRequestToBalancerAndCheckResponse(clientSopportsCompressing, balancerTimeout);
			}
			catch
			{
				// ignored
			}
		}

		private void CreateTestHttpRequestToBalancerAndCheckResponse(bool clientSopportsCompressing = false, int? balancerTimeout = null)
		{
			var request = CreateTestHttpRequestToBalancer(balancerTimeout);

			if (clientSopportsCompressing)
				CheckBalancerDeflatedResponse(request);
			else
				CheckBalancerResponse(request);
		}

		private HttpWebRequest CreateTestHttpRequestToBalancer(int? balancerTimeout = null)
		{
			var request = WebRequest.CreateHttp(
				string.Format("http://{0}/method?{1}", balancerAddress, query));
			if (balancerTimeout.HasValue)
				request.Timeout = balancerTimeout.Value;
			request.Proxy = null;
			request.KeepAlive = true;
			request.ServicePoint.UseNagleAlgorithm = false;
			request.ServicePoint.ConnectionLimit = 4;
			return request;
		}

		private void CheckBalancerResponse(HttpWebRequest balancerRequest)
		{
			var balancerResponse = balancerRequest.GetResponse();
			using (var stream = balancerResponse.GetResponseStream())
				CheckReturnedContent(stream);
		}

		private void CheckBalancerDeflatedResponse(HttpWebRequest balancerRequest)
		{
			balancerRequest.Headers.Add("Accept-Encoding", "deflate");
			balancerRequest.Headers.Add("Accept-Encoding", "gzip");
			var balancerResponse = balancerRequest.GetResponse();
			using (var stream = balancerResponse.GetResponseStream())
			{
				using (var decompressedStream = new DeflateStream(stream, CompressionMode.Decompress))
					CheckReturnedContent(decompressedStream);
				Assert.AreEqual("deflate", balancerResponse.Headers.Get("Content-Encoding"));
			}
		}

		private void CheckReturnedContent(Stream stream)
		{
			using (var streamReader = new StreamReader(stream, Encoding.UTF8))
			{
				var actualProcessedQuery = streamReader.ReadToEnd();
				Assert.AreEqual(processedQuery, actualProcessedQuery);
			}
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
			A.CallTo(() => log.InfoFormat("{0}: {1} can't proxy request to {2}",
				A<Guid>.Ignored, balancer.Name, replica.Address)).MustHaveHappened();
		}

		private void CheckTimeoutedReplica(Replica replica)
		{
			CheckBalancerSentQueryToReplica(replica);
			A.CallTo(() => log.ErrorFormat("Can't get response from {0}: {1}", replica.Address,
				new TimeoutException("The operation has timed out.").Message)).MustHaveHappened();
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
			A.CallTo(() => log.InfoFormat("{0}: {1} received processed query from {2}",
				A<Guid>.Ignored, balancer.Name, chosenReplica.Address)).MustHaveHappened();
		}

		private void CheckBalancerSentProcessedQuery()
		{
			A.CallTo(() => log.InfoFormat("{0}: {1} sent processed query to {2}",
				A<Guid>.Ignored, balancer.Name, A<IPEndPoint>.Ignored)).MustHaveHappened();
		}

		private void CheckBalancerCannotProxyRequestToAnyReplica()
		{
			A.CallTo(() => log.InfoFormat("{0}: {1} can't proxy request to any replica",
				A<Guid>.Ignored, balancer.Name)).MustHaveHappened();
		}

		[Test]
		public void compress_replica_reply_if_client_supports_deflate()
		{
			balancer.TryAddReplicaAddress(GetSomeReplica().Address);
			CreateTestHttpRequestToBalancerAndCheckResponse(true);
		}

		[Test]
		public void move_failed_replica_to_grey_list()
		{
			var replica = GetSomeReplica();
			balancer.TryAddReplicaAddress(replica.Address);
			replica.Stop();
			CreateTestHttpRequestToBalancerAndCheckResponseIgnoringExceptions();

			CheckBalancerReceivedQuery();
			CheckBalancerSentQueryToReplica(replica);
			CheckBadReplica(replica);
			CheckBalancerMoveReplicaToGreyList(replica);
		}

		[Test]
		public void return_replica_from_grey_list_after_timeout()
		{
			var greyListTimeout = 10;
			RecreateBalancer(greyListTimeout);

			var replica = GetSomeReplica();
			balancer.TryAddReplicaAddress(replica.Address);
			replica.Stop();
			CreateTestHttpRequestToBalancerAndCheckResponseIgnoringExceptions(false, 1000);

			CheckBalancerReturnedReplicaFromGreyListAfterTimeout(replica, greyListTimeout);
		}

		private void RecreateBalancer(int greyListTimeout)
		{
			balancer.Stop();
			balancer = new Balancer(balancerAddress, log, balancerTimeoutForReplica, greyListTimeout, balancerRandomSeed);
			balancer.Start();
		}

		private void CheckBalancerReturnedReplicaFromGreyListAfterTimeout(Replica replica, int greyListTimeout)
		{
			Thread.Sleep(greyListTimeout * 10);
			A.CallTo(() => log.InfoFormat("{0}: returned {1} from grey list after {2} ms",
				A<Guid>.Ignored, replica.Address, A<long>.Ignored)).MustHaveHappened();
		}

		private Replica GetSomeReplica()
		{
			return replicas.First();
		}

		private void CheckBalancerMoveReplicaToGreyList(Replica replica)
		{
			A.CallTo(() => log.InfoFormat("{0}: {1} move {2} to grey list",
				A<Guid>.Ignored, balancer.Name, replica.Address)).MustHaveHappened();
		}

		[Test]
		public void try_get_response_from_some_replica_in_grey_list_if_there_is_no_active_replicas()
		{
			AddAllTestReplicaAddressesToBalancer();
			StopAllReplicas();

			CreateTestHttpRequestToBalancerAndCheckResponseIgnoringExceptions();	// now all replicas are in grey list
			CreateTestHttpRequestToBalancerAndCheckResponseIgnoringExceptions();	// new client request

			CheckBalancerSentQueryToReplicaFromGreyList();
		}

		private void CheckBalancerSentQueryToReplicaFromGreyList()
		{
			A.CallTo(() => log.InfoFormat("{0}: there is no any active replica, try proxy request to some replica address from grey list",
				A<Guid>.Ignored)).MustHaveHappened();
		}
	}
}