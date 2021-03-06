﻿
namespace BK.Plugins.MouseHook.Core
{
	public readonly struct MousePoint
	{
		public readonly int X;
		public readonly int Y;

		internal MousePoint(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static bool operator ==(MousePoint p1, MousePoint p2) => p1.X == p2.X && p1.Y == p2.Y;
		public static bool operator !=(MousePoint p1, MousePoint p2) => p1.X != p2.X && p1.Y != p2.Y;
		
		public bool Equals(MousePoint other) => X == other.X && Y == other.Y;
		public override bool Equals(object obj) => obj is MousePoint other && Equals(other);

		public override int GetHashCode() { unchecked { return (X * 397) ^ Y; } }

		public override string ToString() => $"X/Y: {X}/{Y}";
	}
}
