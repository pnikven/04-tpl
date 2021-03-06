﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using log4net;
using log4net.Config;

namespace Balancer
{
	class Program
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));
		private static readonly IPEndPoint balancerAddress = new IPEndPoint(IPAddress.Loopback, 10000);
		private const int replicaTimeout = 1000;
		private const int greyListTimeout = 10 * 1000;
		private static int delayMs = 1000;
		private static readonly int changedReplicaId = 0;

		static void Main()
		{
			XmlConfigurator.Configure();
			IPEndPoint[] replicaAddresses;
			if (!TryReadTopologyServers("topologyServers.txt", out replicaAddresses))
			{
				Console.WriteLine("Incorrect input file topologyServers.txt");
				Environment.Exit(0);
			}
			try
			{
				var balancer = new Balancer(balancerAddress, log, replicaTimeout, greyListTimeout);
				var replicas = replicaAddresses.Select(address => new Replica(address, log)).ToList();
				foreach (var replica in replicas)
				{
					replica.Start();
					balancer.TryAddReplicaAddress(replica.Address);
				}
				#region for testing
				//replicas[0].Stop();
				//replicas[1].RequestProcessingTime = replicaTimeout * 10;
				//replicas[2].Stop();
				#endregion
				balancer.Start();


				Console.WriteLine("Enter replica #0 delay in ms");
				while (true)
				{
					var timeToSleepString = Console.ReadLine();
					int timeToSleep;
					if (int.TryParse(timeToSleepString, out timeToSleep))
					{
						delayMs = timeToSleep;
						replicas[changedReplicaId].RequestProcessingTime = timeToSleep;
						log.InfoFormat("Replica {0} processingTime set to {1}", changedReplicaId, delayMs);
					}
					else
						Console.WriteLine("Couldn't parse \"{0}\" as valid int.", timeToSleepString);
					Console.WriteLine("Delay is {0} ms", delayMs);
				}
			}
			catch (Exception e)
			{
				log.Fatal(e);
				throw;
			}
		}

		public static bool TryReadTopologyServers(string filePath, out IPEndPoint[] servers)
		{
			try
			{
				servers = File.ReadAllLines(filePath)
					.Select(x => x.Split(':'))
					.Select(x => new IPEndPoint(
						IPAddress.Parse(x[0].Replace("localhost", "127.0.0.1")), int.Parse(x[1])))
					.ToArray();
				return true;
			}
			catch
			{
				servers = null;
				return false;
			}
		}
	}
}
