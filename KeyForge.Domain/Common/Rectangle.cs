using System;

namespace KeyForge.Domain.Common
{
    /// <summary>
    /// 跨平台的矩形区域定义
    /// 原本实现：使用System.Drawing.Rectangle
    /// 简化实现：自定义跨平台Rectangle类型
    /// </summary>
    public struct Rectangle : IEquatable<Rectangle>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int Left => X;
        public int Top => Y;
        public int Right => X + Width;
        public int Bottom => Y + Height;

        public static readonly Rectangle Empty = new Rectangle(0, 0, 0, 0);

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool IntersectsWith(Rectangle rect)
        {
            return !(Left >= rect.Right || Right <= rect.Left || Top >= rect.Bottom || Bottom <= rect.Top);
        }

        public Rectangle Intersection(Rectangle rect)
        {
            if (!IntersectsWith(rect))
                return Empty;

            int x = Math.Max(Left, rect.Left);
            int y = Math.Max(Top, rect.Top);
            int width = Math.Min(Right, rect.Right) - x;
            int height = Math.Min(Bottom, rect.Bottom) - y;

            return new Rectangle(x, y, width, height);
        }

        public bool Contains(Rectangle rect)
        {
            return Left <= rect.Left && Right >= rect.Right && Top <= rect.Top && Bottom >= rect.Bottom;
        }

        public bool Contains(int x, int y)
        {
            return x >= Left && x <= Right && y >= Top && y <= Bottom;
        }

        public override bool Equals(object obj)
        {
            return obj is Rectangle rect && Equals(rect);
        }

        public bool Equals(Rectangle other)
        {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }

        public static bool operator ==(Rectangle left, Rectangle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rectangle left, Rectangle right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"X={X}, Y={Y}, Width={Width}, Height={Height}";
        }
    }
}