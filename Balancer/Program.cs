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
			if (!TryReadServersTopology("serversTopology.txt", out servers))
			{
				Console.WriteLine("Incorrect input file serversTopology.txt");
				Environment.Exit(0);
			}

		}

		private static bool TryReadServersTopology(string filePath, out IPEndPoint[] servers)
		{
			try
			{
				servers = File.ReadAllLines("serversTopology.txt")
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
