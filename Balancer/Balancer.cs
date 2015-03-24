﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace Balancer
{
	class Balancer : HttpListener
	{
		private readonly List<IPEndPoint> replicas;

		public Balancer(IPEndPoint balancerAddress, IPEndPoint[] replicaAddresses, ILog log)
			: base(balancerAddress, log)
		{
			replicas = replicaAddresses == null ? new List<IPEndPoint>() : replicaAddresses.ToList();
		}

		public bool TryAddReplicaAddress(IPEndPoint replicaAddress)
		{
			if (replicas.Contains(replicaAddress))
				return false;
			replicas.Add(replicaAddress);
			return true;
		}

		public bool TryRemoveReplicaAddress(IPEndPoint replicaAddress)
		{
			if (!replicas.Contains(replicaAddress))
				return false;
			replicas.Remove(replicaAddress);
			return true;
		}

		public void ClearReplicaAddresses()
		{
			replicas.Clear();
		}

		protected override async Task OnContextAsync(HttpListenerContext context)
		{
			var requestId = Guid.NewGuid();
			var query = GetQuery(context.Request.RawUrl);
			var remoteEndPoint = context.Request.RemoteEndPoint;
			log.InfoFormat("{0}: {1} received {2} from {3}", requestId, Name, query, remoteEndPoint);
			if (replicas.Count == 0)
			{
				log.InfoFormat("{0}: can't proxy request: there is no any replica", requestId);
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			}
			context.Request.InputStream.Close();
			context.Response.OutputStream.Close();
		}

		public override string Name
		{
			get { return "Balancer"; }
		}
	}
}
