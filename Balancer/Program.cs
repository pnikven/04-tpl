using System;
using System.IO;
using System.Linq;
using System.Net;

namespace Balancer
{
	class Program
	{
		static void Main()
		{
			IPEndPoint[] servers;
			if (!TryReadTopologyServers("topologyServers.txt", out servers))
			{
				Console.WriteLine("Incorrect input file topologyServers.txt");
				Environment.Exit(0);
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
