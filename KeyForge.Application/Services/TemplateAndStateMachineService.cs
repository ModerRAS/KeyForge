using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rectangle = KeyForge.Domain.Common.Rectangle;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.Interfaces;
using KeyForge.Domain.Common;
using KeyForge.Application.DTOs;
using KeyForge.Application.Commands;
using KeyForge.Application.Queries;
using MediatR;

namespace KeyForge.Application.Services
{
    /// <summary>
    /// 图像模板应用服务（简化实现）
    /// </summary>
    public class ImageTemplateService : 
        IRequestHandler<CreateImageTemplateCommand, ImageTemplateDto>,
        IRequestHandler<UpdateImageTemplateCommand, ImageTemplateDto>,
        IRequestHandler<DeleteImageTemplateCommand, bool>,
        IRequestHandler<GetImageTemplateQuery, ImageTemplateDto>,
        IRequestHandler<GetAllImageTemplatesQuery, ImageTemplateDto[]>
    {
        private readonly IImageTemplateRepository _templateRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ImageTemplateService(IImageTemplateRepository templateRepository, IUnitOfWork unitOfWork)
        {
            _templateRepository = templateRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ImageTemplateDto> Handle(CreateImageTemplateCommand request, CancellationToken cancellationToken)
        {
            // 简化实现：直接创建模板
            var template = new ImageTemplate(
                Guid.NewGuid(),
                request.Name,
                request.Description,
                request.TemplateData,
                MapRectangleToDomain(request.TemplateArea),
                request.MatchThreshold,
                MapTemplateTypeToDomain(request.TemplateType)
            );

            await _templateRepository.AddAsync(template);
            await _unitOfWork.CommitAsync();

            return MapToDto(template);
        }

        public async Task<ImageTemplateDto> Handle(UpdateImageTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdAsync(request.Id);
            if (template == null)
                throw new EntityNotFoundException(nameof(ImageTemplate), request.Id);

            template.Update(request.Name, request.Description, MapRectangleToDomain(request.TemplateArea), request.MatchThreshold);
            await _templateRepository.UpdateAsync(template);
            await _unitOfWork.CommitAsync();

            return MapToDto(template);
        }

        public async Task<bool> Handle(DeleteImageTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdAsync(request.Id);
            if (template == null)
                return false;

            template.Delete();
            await _templateRepository.UpdateAsync(template);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<ImageTemplateDto> Handle(GetImageTemplateQuery request, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdAsync(request.Id);
            if (template == null)
                throw new EntityNotFoundException(nameof(ImageTemplate), request.Id);

            return MapToDto(template);
        }

        public async Task<ImageTemplateDto[]> Handle(GetAllImageTemplatesQuery request, CancellationToken cancellationToken)
        {
            var templates = request.ActiveOnly.HasValue
                ? await _templateRepository.GetActiveTemplatesAsync()
                : await _templateRepository.GetAllAsync();

            return templates.Select(MapToDto).ToArray();
        }

        private static ImageTemplateDto MapToDto(ImageTemplate template)
        {
            return new ImageTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                TemplateData = template.TemplateData,
                TemplateArea = MapRectangleToDto(template.TemplateArea),
                MatchThreshold = template.MatchThreshold,
                TemplateType = MapTemplateTypeToDto(template.TemplateType),
                IsActive = template.IsActive,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt,
                Version = template.Version
            };
        }

        private static RectangleDto MapRectangleToDto(Rectangle rectangle)
        {
            return new RectangleDto(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        private static TemplateTypeDto MapTemplateTypeToDto(TemplateType templateType)
        {
            return templateType switch
            {
                TemplateType.Image => TemplateTypeDto.Image,
                TemplateType.Color => TemplateTypeDto.Color,
                TemplateType.Text => TemplateTypeDto.Text,
                _ => throw new ArgumentException($"Unknown template type: {templateType}")
            };
        }

        private static TemplateType MapTemplateTypeToDomain(TemplateTypeDto templateType)
        {
            return templateType switch
            {
                TemplateTypeDto.Image => TemplateType.Image,
                TemplateTypeDto.Color => TemplateType.Color,
                TemplateTypeDto.Text => TemplateType.Text,
                _ => throw new ArgumentException($"Unknown template type: {templateType}")
            };
        }

        private static Rectangle MapRectangleToDomain(RectangleDto rectangle)
        {
            return new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        private static StateMachineStatusDto MapStateMachineStatusToDto(StateMachineStatus status)
        {
            return status switch
            {
                StateMachineStatus.Draft => StateMachineStatusDto.Draft,
                StateMachineStatus.Active => StateMachineStatusDto.Active,
                StateMachineStatus.Inactive => StateMachineStatusDto.Inactive,
                StateMachineStatus.Deleted => StateMachineStatusDto.Deleted,
                _ => throw new ArgumentException($"Unknown state machine status: {status}")
            };
        }

        private static StateMachineStatus MapStateMachineStatusToDomain(StateMachineStatusDto status)
        {
            return status switch
            {
                StateMachineStatusDto.Draft => StateMachineStatus.Draft,
                StateMachineStatusDto.Active => StateMachineStatus.Active,
                StateMachineStatusDto.Inactive => StateMachineStatus.Inactive,
                StateMachineStatusDto.Deleted => StateMachineStatus.Deleted,
                _ => throw new ArgumentException($"Unknown state machine status: {status}")
            };
        }
    }

    /// <summary>
    /// 状态机应用服务（简化实现）
    /// </summary>
    public class StateMachineService : 
        IRequestHandler<CreateStateMachineCommand, StateMachineDto>,
        IRequestHandler<UpdateStateMachineCommand, StateMachineDto>,
        IRequestHandler<DeleteStateMachineCommand, bool>,
        IRequestHandler<ActivateStateMachineCommand, bool>,
        IRequestHandler<DeactivateStateMachineCommand, bool>,
        IRequestHandler<GetStateMachineQuery, StateMachineDto>,
        IRequestHandler<GetAllStateMachinesQuery, StateMachineDto[]>
    {
        private readonly IStateMachineRepository _stateMachineRepository;
        private readonly IUnitOfWork _unitOfWork;

        public StateMachineService(IStateMachineRepository stateMachineRepository, IUnitOfWork unitOfWork)
        {
            _stateMachineRepository = stateMachineRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<StateMachineDto> Handle(CreateStateMachineCommand request, CancellationToken cancellationToken)
        {
            var stateMachine = new StateMachine(Guid.NewGuid(), request.Name, request.Description);
            
            await _stateMachineRepository.AddAsync(stateMachine);
            await _unitOfWork.CommitAsync();

            return MapToDto(stateMachine);
        }

        public async Task<StateMachineDto> Handle(UpdateStateMachineCommand request, CancellationToken cancellationToken)
        {
            var stateMachine = await _stateMachineRepository.GetByIdAsync(request.Id);
            if (stateMachine == null)
                throw new EntityNotFoundException(nameof(StateMachine), request.Id);

            // 简化实现：只更新基本信息
            // stateMachine.Update(request.Name, request.Description);
            await _stateMachineRepository.UpdateAsync(stateMachine);
            await _unitOfWork.CommitAsync();

            return MapToDto(stateMachine);
        }

        public async Task<bool> Handle(DeleteStateMachineCommand request, CancellationToken cancellationToken)
        {
            var stateMachine = await _stateMachineRepository.GetByIdAsync(request.Id);
            if (stateMachine == null)
                return false;

            await _stateMachineRepository.DeleteAsync(request.Id);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> Handle(ActivateStateMachineCommand request, CancellationToken cancellationToken)
        {
            var stateMachine = await _stateMachineRepository.GetByIdAsync(request.Id);
            if (stateMachine == null)
                throw new EntityNotFoundException(nameof(StateMachine), request.Id);

            stateMachine.Activate();
            await _stateMachineRepository.UpdateAsync(stateMachine);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> Handle(DeactivateStateMachineCommand request, CancellationToken cancellationToken)
        {
            var stateMachine = await _stateMachineRepository.GetByIdAsync(request.Id);
            if (stateMachine == null)
                throw new EntityNotFoundException(nameof(StateMachine), request.Id);

            stateMachine.Deactivate();
            await _stateMachineRepository.UpdateAsync(stateMachine);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<StateMachineDto> Handle(GetStateMachineQuery request, CancellationToken cancellationToken)
        {
            var stateMachine = await _stateMachineRepository.GetByIdAsync(request.Id);
            if (stateMachine == null)
                throw new EntityNotFoundException(nameof(StateMachine), request.Id);

            return MapToDto(stateMachine);
        }

        public async Task<StateMachineDto[]> Handle(GetAllStateMachinesQuery request, CancellationToken cancellationToken)
        {
            var stateMachines = request.Status.HasValue
                ? await _stateMachineRepository.GetByStatusAsync(MapStateMachineStatusToDomain(request.Status.Value))
                : await _stateMachineRepository.GetAllAsync();

            return stateMachines.Select(MapToDto).ToArray();
        }

        private static StateMachineDto MapToDto(StateMachine stateMachine)
        {
            return new StateMachineDto
            {
                Id = stateMachine.Id,
                Name = stateMachine.Name,
                Description = stateMachine.Description,
                Status = MapStateMachineStatusToDto(stateMachine.Status),
                CreatedAt = stateMachine.CreatedAt,
                UpdatedAt = stateMachine.UpdatedAt,
                Version = stateMachine.Version,
                States = stateMachine.States.Select(s => new StateDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Variables = s.Variables,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                }).ToList(),
                CurrentState = stateMachine.CurrentState != null ? new StateDto
                {
                    Id = stateMachine.CurrentState.Id,
                    Name = stateMachine.CurrentState.Name,
                    Description = stateMachine.CurrentState.Description,
                    Variables = stateMachine.CurrentState.Variables,
                    CreatedAt = stateMachine.CurrentState.CreatedAt,
                    UpdatedAt = stateMachine.CurrentState.UpdatedAt
                } : null
            };
        }

        // 添加缺失的映射方法
        private static StateMachineStatus MapStateMachineStatusToDomain(StateMachineStatusDto status)
        {
            return status switch
            {
                StateMachineStatusDto.Draft => StateMachineStatus.Draft,
                StateMachineStatusDto.Active => StateMachineStatus.Active,
                StateMachineStatusDto.Inactive => StateMachineStatus.Inactive,
                StateMachineStatusDto.Deleted => StateMachineStatus.Deleted,
                _ => StateMachineStatus.Draft
            };
        }

        private static StateMachineStatusDto MapStateMachineStatusToDto(StateMachineStatus status)
        {
            return status switch
            {
                StateMachineStatus.Draft => StateMachineStatusDto.Draft,
                StateMachineStatus.Active => StateMachineStatusDto.Active,
                StateMachineStatus.Inactive => StateMachineStatusDto.Inactive,
                StateMachineStatus.Deleted => StateMachineStatusDto.Deleted,
                _ => StateMachineStatusDto.Draft
            };
        }
    }
}