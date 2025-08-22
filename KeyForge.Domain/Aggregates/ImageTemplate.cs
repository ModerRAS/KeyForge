using System;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Events;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Common;
using TemplateType = KeyForge.Domain.Common.TemplateType;
using Rectangle = KeyForge.Domain.Common.Rectangle;

namespace KeyForge.Domain.Aggregates
{
    /// <summary>
    /// 图像模板聚合根
    /// </summary>
    public class ImageTemplate : AggregateRoot
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public byte[] TemplateData { get; private set; }
        public Rectangle TemplateArea { get; private set; }
        public double MatchThreshold { get; private set; }
        public TemplateType TemplateType { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public int Version { get; private set; }

        protected ImageTemplate() { }

        public ImageTemplate(Guid id, string name, string description, byte[] templateData, 
            Rectangle templateArea, double matchThreshold = 0.8, TemplateType templateType = TemplateType.Image)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Template name cannot be empty.");

            if (templateData == null || templateData.Length == 0)
                throw new ValidationException("Template data cannot be empty.");

            if (matchThreshold < 0 || matchThreshold > 1)
                throw new ValidationException("Match threshold must be between 0 and 1.");

            Id = id;
            Name = name;
            Description = description;
            TemplateData = templateData;
            TemplateArea = templateArea;
            MatchThreshold = matchThreshold;
            TemplateType = templateType;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Version = 1;

            AddDomainEvent(new ImageTemplateCreatedEvent(Id, Name));
        }

        public void Update(string name, string description, Rectangle templateArea, double matchThreshold)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Template name cannot be empty.");

            if (matchThreshold < 0 || matchThreshold > 1)
                throw new ValidationException("Match threshold must be between 0 and 1.");

            Name = name;
            Description = description;
            TemplateArea = templateArea;
            MatchThreshold = matchThreshold;
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new ImageTemplateUpdatedEvent(Id, Name));
        }

        public void UpdateTemplateData(byte[] newTemplateData)
        {
            if (newTemplateData == null || newTemplateData.Length == 0)
                throw new ValidationException("Template data cannot be empty.");

            TemplateData = newTemplateData;
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new ImageTemplateDataUpdatedEvent(Id, Name));
        }

        public void Activate()
        {
            if (IsActive)
                return;

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new ImageTemplateActivatedEvent(Id, Name));
        }

        public void Deactivate()
        {
            if (!IsActive)
                return;

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new ImageTemplateDeactivatedEvent(Id, Name));
        }

        public void Delete()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            Version++;

            AddDomainEvent(new ImageTemplateDeletedEvent(Id, Name));
        }

        public bool IsMatch(RecognitionResult result)
        {
            if (result == null || !result.IsMatch)
                return false;

            return result.Confidence >= MatchThreshold;
        }

        public bool IsAreaMatch(Rectangle searchArea)
        {
            return TemplateArea.IntersectsWith(searchArea);
        }
    }

  }