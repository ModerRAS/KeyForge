using System;
using KeyForge.Domain.Common;
using Rectangle = KeyForge.Domain.Common.Rectangle;

namespace KeyForge.Domain.ValueObjects
{
    /// <summary>
    /// 图像识别结果值对象
    /// </summary>
    public class RecognitionResult : ValueObject
    {
        public bool IsMatch { get; }
        public Rectangle MatchArea { get; }
        public double Confidence { get; }
        public string TemplateName { get; }
        public DateTime RecognizedAt { get; }

        public RecognitionResult(bool isMatch, Rectangle matchArea, double confidence, string templateName)
        {
            IsMatch = isMatch;
            MatchArea = matchArea;
            Confidence = confidence;
            TemplateName = templateName;
            RecognizedAt = DateTime.UtcNow;
        }

        public static RecognitionResult NoMatch(string templateName)
        {
            return new RecognitionResult(false, Rectangle.Empty, 0, templateName);
        }

        public static RecognitionResult Match(Rectangle matchArea, double confidence, string templateName)
        {
            if (confidence < 0 || confidence > 1)
                throw new ArgumentException("Confidence must be between 0 and 1.", nameof(confidence));

            return new RecognitionResult(true, matchArea, confidence, templateName);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return IsMatch;
            yield return MatchArea;
            yield return Confidence;
            yield return TemplateName;
            yield return RecognizedAt;
        }

        public override string ToString()
        {
            return $"RecognitionResult: Match={IsMatch}, Confidence={Confidence:F2}, Template={TemplateName}";
        }
    }
}