using System;

namespace KeyForge.Domain.ValueObjects
{
    /// <summary>
    /// 屏幕区域值对象
    /// 
    /// 原本实现：支持多种坐标系和区域计算
    /// 简化实现：基本的矩形区域定义
    /// </summary>
    public readonly struct ScreenRegion : IEquatable<ScreenRegion>
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        
        public ScreenRegion(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        
        public static ScreenRegion From(int x, int y, int width, int height) => new ScreenRegion(x, y, width, height);
        public static ScreenRegion Empty => new ScreenRegion(0, 0, 0, 0);
        
        public bool Contains(int x, int y) => 
            x >= X && x <= X + Width && y >= Y && y <= Y + Height;
        
        public bool Equals(ScreenRegion other) => 
            X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        
        public override bool Equals(object obj) => obj is ScreenRegion other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
        
        public static bool operator ==(ScreenRegion left, ScreenRegion right) => left.Equals(right);
        public static bool operator !=(ScreenRegion left, ScreenRegion right) => !left.Equals(right);
    }
    
    /// <summary>
    /// 识别参数值对象
    /// 
    /// 原本实现：复杂的识别参数配置，支持多种算法
    /// 简化实现：基本的识别参数
    /// </summary>
    public readonly struct RecognitionParameters : IEquatable<RecognitionParameters>
    {
        public double ConfidenceThreshold { get; }
        public int TimeoutMs { get; }
        public bool UseGrayscale { get; }
        public bool EnableImageEnhancement { get; }
        public int MaxAttempts { get; }
        
        public RecognitionParameters(
            double confidenceThreshold = 0.8,
            int timeoutMs = 5000,
            bool useGrayscale = true,
            bool enableImageEnhancement = false,
            int maxAttempts = 3)
        {
            ConfidenceThreshold = confidenceThreshold;
            TimeoutMs = timeoutMs;
            UseGrayscale = useGrayscale;
            EnableImageEnhancement = enableImageEnhancement;
            MaxAttempts = maxAttempts;
        }
        
        public static RecognitionParameters Default => new RecognitionParameters();
        
        public bool Equals(RecognitionParameters other) => 
            ConfidenceThreshold == other.ConfidenceThreshold &&
            TimeoutMs == other.TimeoutMs &&
            UseGrayscale == other.UseGrayscale &&
            EnableImageEnhancement == other.EnableImageEnhancement &&
            MaxAttempts == other.MaxAttempts;
        
        public override bool Equals(object obj) => obj is RecognitionParameters other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ConfidenceThreshold, TimeoutMs, UseGrayscale, EnableImageEnhancement, MaxAttempts);
        
        public static bool operator ==(RecognitionParameters left, RecognitionParameters right) => left.Equals(right);
        public static bool operator !=(RecognitionParameters left, RecognitionParameters right) => !left.Equals(right);
    }

    /// <summary>
    /// 屏幕位置值对象
    /// 原本实现：复杂的屏幕位置处理
    /// 简化实现：基础的屏幕位置
    /// </summary>
    public readonly struct ScreenLocation : IEquatable<ScreenLocation>
    {
        public int X { get; }
        public int Y { get; }
        
        public ScreenLocation(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public static ScreenLocation From(int x, int y) => new ScreenLocation(x, y);
        public static ScreenLocation Empty => new ScreenLocation(0, 0);
        
        public bool Equals(ScreenLocation other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is ScreenLocation other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        
        public static bool operator ==(ScreenLocation left, ScreenLocation right) => left.Equals(right);
        public static bool operator !=(ScreenLocation left, ScreenLocation right) => !left.Equals(right);
        
        public override string ToString() => $"({X}, {Y})";
    }

    // 时间戳和持续时间已经在BasicTypes.cs中定义，避免重复定义

    /// <summary>
    /// 置信度分数值对象
    /// 原本实现：复杂的置信度计算
    /// 简化实现：基础的置信度分数
    /// </summary>
    public readonly struct ConfidenceScore : IEquatable<ConfidenceScore>
    {
        public double Value { get; }
        
        public ConfidenceScore(double value)
        {
            if (value < 0 || value > 1)
                throw new ArgumentException("置信度必须在0到1之间", nameof(value));
            
            Value = value;
        }
        
        public static ConfidenceScore From(double value) => new ConfidenceScore(value);
        public static ConfidenceScore Zero => new ConfidenceScore(0);
        public static ConfidenceScore One => new ConfidenceScore(1);
        
        public bool Equals(ConfidenceScore other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ConfidenceScore other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        
        public static bool operator ==(ConfidenceScore left, ConfidenceScore right) => left.Equals(right);
        public static bool operator !=(ConfidenceScore left, ConfidenceScore right) => !left.Equals(right);
        
        public override string ToString() => $"{Value:P2}";
    }
}