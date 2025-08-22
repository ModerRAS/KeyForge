using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using KeyForge.Core.Interfaces;

namespace KeyForge.Core.Services
{
    /// <summary>
    /// 图像识别服务 - 简化实现
    /// </summary>
    public class ImageRecognitionService
    {
        private readonly ILoggerService _logger;
        private readonly double _matchThreshold;

        public ImageRecognitionService(ILoggerService logger, double matchThreshold = 0.8)
        {
            _logger = logger;
            _matchThreshold = matchThreshold;
        }

        /// <summary>
        /// 在屏幕上查找图像
        /// </summary>
        public ImageRecognitionResult FindImageOnScreen(string templatePath, Rectangle searchArea = default)
        {
            try
            {
                // 截取屏幕
                using (var screenCapture = CaptureScreen(searchArea))
                using (var screenMat = screenCapture.ToMat())
                {
                    return FindImageInMat(screenMat, templatePath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"图像识别失败: {ex.Message}");
                return new ImageRecognitionResult { Success = false };
            }
        }

        /// <summary>
        /// 在指定Mat中查找图像
        /// </summary>
        public ImageRecognitionResult FindImageInMat(Mat sourceMat, string templatePath)
        {
            try
            {
                if (!File.Exists(templatePath))
                {
                    _logger.Error($"模板文件不存在: {templatePath}");
                    return new ImageRecognitionResult { Success = false };
                }

                // 读取模板图像
                using (var templateMat = Cv2.ImRead(templatePath, ImreadModes.Color))
                {
                    if (templateMat.Empty())
                    {
                        _logger.Error($"无法读取模板文件: {templatePath}");
                        return new ImageRecognitionResult { Success = false };
                    }

                    return FindTemplate(sourceMat, templateMat);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"图像识别失败: {ex.Message}");
                return new ImageRecognitionResult { Success = false };
            }
        }

        /// <summary>
        /// 模板匹配核心算法
        /// </summary>
        private ImageRecognitionResult FindTemplate(Mat source, Mat template)
        {
            // 转换为灰度图像
            using (var sourceGray = new Mat())
            using (var templateGray = new Mat())
            using (var result = new Mat())
            {
                Cv2.CvtColor(source, sourceGray, ColorConversionCodes.BGR2GRAY);
                Cv2.CvtColor(template, templateGray, ColorConversionCodes.BGR2GRAY);

                // 执行模板匹配
                Cv2.MatchTemplate(sourceGray, templateGray, result, TemplateMatchModes.CCoeffNormed);

                // 查找最佳匹配位置
                Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

                // 判断是否匹配成功
                if (maxVal >= _matchThreshold)
                {
                    return new ImageRecognitionResult
                    {
                        Success = true,
                        Location = new Rectangle(maxLoc.X, maxLoc.Y, template.Width, template.Height),
                        Confidence = maxVal,
                        Center = new System.Drawing.Point(maxLoc.X + template.Width / 2, maxLoc.Y + template.Height / 2)
                    };
                }
                else
                {
                    return new ImageRecognitionResult
                    {
                        Success = false,
                        Confidence = maxVal
                    };
                }
            }
        }

        /// <summary>
        /// 查找所有匹配的图像
        /// </summary>
        public ImageRecognitionResult[] FindAllImagesOnScreen(string templatePath, Rectangle searchArea = default)
        {
            try
            {
                using (var screenCapture = CaptureScreen(searchArea))
                using (var screenMat = screenCapture.ToMat())
                {
                    return FindAllImagesInMat(screenMat, templatePath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"批量图像识别失败: {ex.Message}");
                return new ImageRecognitionResult[0];
            }
        }

        /// <summary>
        /// 在指定Mat中查找所有匹配的图像
        /// </summary>
        public ImageRecognitionResult[] FindAllImagesInMat(Mat sourceMat, string templatePath)
        {
            try
            {
                if (!File.Exists(templatePath))
                {
                    _logger.Error($"模板文件不存在: {templatePath}");
                    return new ImageRecognitionResult[0];
                }

                using (var templateMat = Cv2.ImRead(templatePath, ImreadModes.Color))
                {
                    if (templateMat.Empty())
                    {
                        _logger.Error($"无法读取模板文件: {templatePath}");
                        return new ImageRecognitionResult[0];
                    }

                    return FindAllTemplates(sourceMat, templateMat);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"批量图像识别失败: {ex.Message}");
                return new ImageRecognitionResult[0];
            }
        }

        /// <summary>
        /// 查找所有模板匹配
        /// </summary>
        private ImageRecognitionResult[] FindAllTemplates(Mat source, Mat template)
        {
            using (var sourceGray = new Mat())
            using (var templateGray = new Mat())
            using (var result = new Mat())
            {
                Cv2.CvtColor(source, sourceGray, ColorConversionCodes.BGR2GRAY);
                Cv2.CvtColor(template, templateGray, ColorConversionCodes.BGR2GRAY);

                Cv2.MatchTemplate(sourceGray, templateGray, result, TemplateMatchModes.CCoeffNormed);

                // 设置阈值进行过滤
                var mask = result.InRange(_matchThreshold, 1.0);
                var locations = new Mat();
                Cv2.FindNonZero(mask, locations);

                if (locations.Empty())
                {
                    return new ImageRecognitionResult[0];
                }

                // 按置信度排序
                var results = new System.Collections.Generic.List<ImageRecognitionResult>();
                for (int i = 0; i < locations.Rows; i++)
                {
                    var point = locations.Get<OpenCvSharp.Point>(i);
                    var confidence = result.At<float>(point.Y, point.X);

                    results.Add(new ImageRecognitionResult
                    {
                        Success = true,
                        Location = new Rectangle(point.X, point.Y, template.Width, template.Height),
                        Confidence = confidence,
                        Center = new System.Drawing.Point(point.X + template.Width / 2, point.Y + template.Height / 2)
                    });
                }

                // 按置信度降序排序
                return results.OrderByDescending(r => r.Confidence).ToArray();
            }
        }

        /// <summary>
        /// 截取屏幕
        /// </summary>
        private Bitmap CaptureScreen(Rectangle area)
        {
            var bounds = area == default ? ScreenBounds : area;
            var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            }

            return bitmap;
        }

        /// <summary>
        /// 获取屏幕边界
        /// </summary>
        private Rectangle ScreenBounds
        {
            get
            {
                var screen = System.Windows.Forms.Screen.PrimaryScreen;
                return screen.Bounds;
            }
        }

        /// <summary>
        /// 检查指定位置的颜色
        /// </summary>
        public Color GetPixelColor(int x, int y)
        {
            try
            {
                using (var screenCapture = CaptureScreen(ScreenBounds))
                {
                    return screenCapture.GetPixel(x, y);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"获取像素颜色失败: {ex.Message}");
                return Color.Empty;
            }
        }

        /// <summary>
        /// 在指定区域查找指定颜色
        /// </summary>
        public System.Drawing.Point? FindColor(Color targetColor, Rectangle searchArea, int tolerance = 10)
        {
            try
            {
                using (var screenCapture = CaptureScreen(searchArea))
                {
                    for (int y = 0; y < screenCapture.Height; y++)
                    {
                        for (int x = 0; x < screenCapture.Width; x++)
                        {
                            var pixelColor = screenCapture.GetPixel(x, y);
                            if (ColorsMatch(pixelColor, targetColor, tolerance))
                            {
                                return new System.Drawing.Point(searchArea.X + x, searchArea.Y + y);
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error($"查找颜色失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 检查颜色是否匹配
        /// </summary>
        private bool ColorsMatch(Color color1, Color color2, int tolerance)
        {
            return Math.Abs(color1.R - color2.R) <= tolerance &&
                   Math.Abs(color1.G - color2.G) <= tolerance &&
                   Math.Abs(color1.B - color2.B) <= tolerance;
        }

        /// <summary>
        /// 保存屏幕截图
        /// </summary>
        public void SaveScreenshot(string filePath, Rectangle area = default)
        {
            try
            {
                using (var screenCapture = CaptureScreen(area))
                {
                    screenCapture.Save(filePath, ImageFormat.Png);
                    _logger.Info($"屏幕截图已保存: {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"保存屏幕截图失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 图像识别结果
    /// </summary>
    public class ImageRecognitionResult
    {
        public bool Success { get; set; }
        public Rectangle Location { get; set; }
        public System.Drawing.Point Center { get; set; }
        public double Confidence { get; set; }
        public string TemplatePath { get; set; }

        public override string ToString()
        {
            return Success ? 
                $"找到图像: 位置({Location.X}, {Location.Y}), 置信度: {Confidence:P2}" : 
                $"未找到图像，最高置信度: {Confidence:P2}";
        }
    }
}