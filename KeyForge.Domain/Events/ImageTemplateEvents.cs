using System;

namespace KeyForge.Domain.Events
{
    /// <summary>
    /// 图像模板创建事件
    /// </summary>
    public class ImageTemplateCreatedEvent : DomainEvent
    {
        public Guid TemplateId { get; }
        public string TemplateName { get; }

        public ImageTemplateCreatedEvent(Guid templateId, string templateName)
        {
            TemplateId = templateId;
            TemplateName = templateName;
        }
    }

    /// <summary>
    /// 图像模板更新事件
    /// </summary>
    public class ImageTemplateUpdatedEvent : DomainEvent
    {
        public Guid TemplateId { get; }
        public string TemplateName { get; }

        public ImageTemplateUpdatedEvent(Guid templateId, string templateName)
        {
            TemplateId = templateId;
            TemplateName = templateName;
        }
    }

    /// <summary>
    /// 图像模板数据更新事件
    /// </summary>
    public class ImageTemplateDataUpdatedEvent : DomainEvent
    {
        public Guid TemplateId { get; }
        public string TemplateName { get; }

        public ImageTemplateDataUpdatedEvent(Guid templateId, string templateName)
        {
            TemplateId = templateId;
            TemplateName = templateName;
        }
    }

    /// <summary>
    /// 图像模板激活事件
    /// </summary>
    public class ImageTemplateActivatedEvent : DomainEvent
    {
        public Guid TemplateId { get; }
        public string TemplateName { get; }

        public ImageTemplateActivatedEvent(Guid templateId, string templateName)
        {
            TemplateId = templateId;
            TemplateName = templateName;
        }
    }

    /// <summary>
    /// 图像模板停用事件
    /// </summary>
    public class ImageTemplateDeactivatedEvent : DomainEvent
    {
        public Guid TemplateId { get; }
        public string TemplateName { get; }

        public ImageTemplateDeactivatedEvent(Guid templateId, string templateName)
        {
            TemplateId = templateId;
            TemplateName = templateName;
        }
    }

    /// <summary>
    /// 图像模板删除事件
    /// </summary>
    public class ImageTemplateDeletedEvent : DomainEvent
    {
        public Guid TemplateId { get; }
        public string TemplateName { get; }

        public ImageTemplateDeletedEvent(Guid templateId, string templateName)
        {
            TemplateId = templateId;
            TemplateName = templateName;
        }
    }

    /// <summary>
    /// 图像识别成功事件
    /// </summary>
    public class ImageRecognitionSuccessEvent : DomainEvent
    {
        public Guid TemplateId { get; }
        public string TemplateName { get; }
        public double Confidence { get; }
        public System.Drawing.Rectangle MatchArea { get; }

        public ImageRecognitionSuccessEvent(Guid templateId, string templateName, double confidence, System.Drawing.Rectangle matchArea)
        {
            TemplateId = templateId;
            TemplateName = templateName;
            Confidence = confidence;
            MatchArea = matchArea;
        }
    }

    /// <summary>
    /// 图像识别失败事件
    /// </summary>
    public class ImageRecognitionFailedEvent : DomainEvent
    {
        public Guid TemplateId { get; }
        public string TemplateName { get; }
        public string Reason { get; }

        public ImageRecognitionFailedEvent(Guid templateId, string templateName, string reason)
        {
            TemplateId = templateId;
            TemplateName = templateName;
            Reason = reason;
        }
    }
}