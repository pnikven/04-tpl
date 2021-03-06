﻿using System.Collections.Generic;
using System.Linq;
using JapaneseCrossword.Solvers.Utils.Enums;

namespace JapaneseCrossword.Solvers.Utils
{
	public class Line
	{
		public LineType Type { get; private set; }
		public int Index { get; private set; }
		public bool NeedRefresh { get; private set; }
		public Block[] Blocks { get; private set; }
		public int BlockCount { get { return Blocks.Length; } }

		public Line(LineType type, int index, Block[] blocks)
		{
			Type = type;
			Index = index;
			NeedRefresh = true;
			Blocks = blocks;
		}

		public static Line Create(LineType type, int index, int[] blocks)
		{
			return blocks == null ?
				null :
				new Line(type, index, blocks
					.Select((blockLength, i) => new Block(blockLength, i))
					.ToArray());
		}

		public void Refresh()
		{
			NeedRefresh = false;
		}

		public void Invalidate()
		{
			NeedRefresh = true;
		}

		public bool IsRow
		{
			get { return Type == LineType.Row; }
		}

		public bool IsColumn
		{
			get { return Type == LineType.Column; }
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((Line)obj);
		}

		protected bool Equals(Line other)
		{
			return
				Type == other.Type &&
				Index == other.Index &&
				NeedRefresh == other.NeedRefresh &&
				BlockCount == other.BlockCount &&
				Blocks.All(b => b.Equals(other.Blocks.ElementAt(b.Index)));
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (int)Type;
				hashCode = (hashCode * 397) ^ Index;
				hashCode = (hashCode * 397) ^ NeedRefresh.GetHashCode();
				hashCode = (hashCode * 397) ^ (Blocks != null ? Blocks.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ BlockCount;
				return hashCode;
			}
		}
	}
}