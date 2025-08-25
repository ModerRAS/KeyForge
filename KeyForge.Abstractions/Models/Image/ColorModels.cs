namespace KeyForge.Abstractions.Models.Image
{
    /// <summary>
    /// 颜色结构
    /// 【优化实现】定义统一的颜色结构，支持跨平台颜色处理
    /// 原实现：颜色格式不统一，跨平台兼容性差
    /// 优化：通过统一的颜色结构，提高跨平台兼容性
    /// </summary>
    public struct Color
    {
        /// <summary>
        /// 红色分量
        /// </summary>
        public byte R { get; set; }
        
        /// <summary>
        /// 绿色分量
        /// </summary>
        public byte G { get; set; }
        
        /// <summary>
        /// 蓝色分量
        /// </summary>
        public byte B { get; set; }
        
        /// <summary>
        /// 透明度分量
        /// </summary>
        public byte A { get; set; }
        
        /// <summary>
        /// 创建颜色
        /// </summary>
        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        
        /// <summary>
        /// 从ARGB值创建颜色
        /// </summary>
        public static Color FromArgb(int argb)
        {
            return new Color(
                (byte)((argb >> 16) & 0xFF),
                (byte)((argb >> 8) & 0xFF),
                (byte)(argb & 0xFF),
                (byte)((argb >> 24) & 0xFF)
            );
        }
        
        /// <summary>
        /// 从RGB值创建颜色
        /// </summary>
        public static Color FromRgb(byte r, byte g, byte b)
        {
            return new Color(r, g, b);
        }
        
        /// <summary>
        /// 转换为ARGB整数值
        /// </summary>
        public int ToArgb()
        {
            return (A << 24) | (R << 16) | (G << 8) | B;
        }
        
        /// <summary>
        /// 黑色
        /// </summary>
        public static Color Black => new Color(0, 0, 0);
        
        /// <summary>
        /// 白色
        /// </summary>
        public static Color White => new Color(255, 255, 255);
        
        /// <summary>
        /// 红色
        /// </summary>
        public static Color Red => new Color(255, 0, 0);
        
        /// <summary>
        /// 绿色
        /// </summary>
        public static Color Green => new Color(0, 255, 0);
        
        /// <summary>
        /// 蓝色
        /// </summary>
        public static Color Blue => new Color(0, 0, 255);
        
        /// <summary>
        /// 透明色
        /// </summary>
        public static Color Transparent => new Color(0, 0, 0, 0);
    }
    
    /// <summary>
    /// 颜色直方图
    /// </summary>
    public class ColorHistogram
    {
        /// <summary>
        /// 红色通道直方图
        /// </summary>
        public int[] RedChannel { get; set; } = new int[256];
        
        /// <summary>
        /// 绿色通道直方图
        /// </summary>
        public int[] GreenChannel { get; set; } = new int[256];
        
        /// <summary>
        /// 蓝色通道直方图
        /// </summary>
        public int[] BlueChannel { get; set; } = new int[256];
        
        /// <summary>
        /// 总像素数
        /// </summary>
        public int TotalPixels { get; set; }
        
        /// <summary>
        /// 平均颜色
        /// </summary>
        public Color AverageColor { get; set; }
        
        /// <summary>
        /// 主导颜色
        /// </summary>
        public Color DominantColor { get; set; }
        
        /// <summary>
        /// 创建直方图
        /// </summary>
        public static ColorHistogram Create()
        {
            return new ColorHistogram();
        }
        
        /// <summary>
        /// 添加颜色样本
        /// </summary>
        public void AddColor(Color color)
        {
            RedChannel[color.R]++;
            GreenChannel[color.G]++;
            BlueChannel[color.B]++;
            TotalPixels++;
        }
        
        /// <summary>
        /// 计算平均颜色
        /// </summary>
        public void CalculateAverageColor()
        {
            if (TotalPixels == 0) return;
            
            long r = 0, g = 0, b = 0;
            
            for (int i = 0; i < 256; i++)
            {
                r += RedChannel[i] * i;
                g += GreenChannel[i] * i;
                b += BlueChannel[i] * i;
            }
            
            AverageColor = new Color(
                (byte)(r / TotalPixels),
                (byte)(g / TotalPixels),
                (byte)(b / TotalPixels)
            );
        }
    }
}