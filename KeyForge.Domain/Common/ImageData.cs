using System;

namespace KeyForge.Domain.Common
{
    /// <summary>
    /// 跨平台的图像表示
    /// 原本实现：使用System.Drawing.Bitmap
    /// 简化实现：使用byte数组表示图像数据
    /// </summary>
    public class ImageData
    {
        public byte[] Data { get; }
        public int Width { get; }
        public int Height { get; }
        public string Format { get; }

        public ImageData(byte[] data, int width, int height, string format = "png")
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Width = width > 0 ? width : throw new ArgumentOutOfRangeException(nameof(width));
            Height = height > 0 ? height : throw new ArgumentOutOfRangeException(nameof(height));
            Format = format ?? "png";
        }

        public static ImageData Empty => new ImageData(Array.Empty<byte>(), 0, 0);
    }
}