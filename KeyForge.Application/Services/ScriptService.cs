using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KeyForge.Application.DTOs;
using KeyForge.Application.Interfaces;
using KeyForge.Application.Queries;
using KeyForge.Application.Commands;
using KeyForge.Domain.Aggregates;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Exceptions;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Common;
using MediatR;

namespace KeyForge.Application.Services
{
    /// <summary>
    /// 脚本服务实现
    /// 原本实现：复杂的依赖注入和配置
    /// 简化实现：直接使用MediatR模式
    /// </summary>
    public class ScriptService :
        IRequestHandler<CreateScriptCommand, ScriptDto>,
        IRequestHandler<UpdateScriptCommand, ScriptDto>,
        IRequestHandler<DeleteScriptCommand, bool>,
        IRequestHandler<AddActionToScriptCommand, ScriptDto>,
        IRequestHandler<RemoveActionFromScriptCommand, ScriptDto>,
        IRequestHandler<ActivateScriptCommand, ScriptDto>,
        IRequestHandler<DeactivateScriptCommand, ScriptDto>,
        IRequestHandler<GetAllScriptsQuery, ScriptDto[]>,
        IRequestHandler<GetScriptDetailsQuery, ScriptDto>
    {
        private readonly IScriptRepository _scriptRepository;

        public ScriptService(IScriptRepository scriptRepository)
        {
            _scriptRepository = scriptRepository;
        }

        public async Task<ScriptDto> Handle(CreateScriptCommand request, CancellationToken cancellationToken)
        {
            var script = new Script(Guid.NewGuid(), request.Name, request.Description);
            
            await _scriptRepository.AddAsync(script);
            await _scriptRepository.SaveChangesAsync();

            return MapToDto(script);
        }

        public async Task<ScriptDto> Handle(UpdateScriptCommand request, CancellationToken cancellationToken)
        {
            var script = await _scriptRepository.GetByIdAsync(request.Id);
            if (script == null)
                throw new EntityNotFoundException(nameof(Script), request.Id);

            script.Update(request.Name, request.Description);
            await _scriptRepository.SaveChangesAsync();

            return MapToDto(script);
        }

        public async Task<bool> Handle(DeleteScriptCommand request, CancellationToken cancellationToken)
        {
            var script = await _scriptRepository.GetByIdAsync(request.Id);
            if (script == null)
                return false;

            script.Delete();
            await _scriptRepository.SaveChangesAsync();

            return true;
        }

        public async Task<ScriptDto> Handle(AddActionToScriptCommand request, CancellationToken cancellationToken)
        {
            var script = await _scriptRepository.GetByIdAsync(request.ScriptId);
            if (script == null)
                throw new EntityNotFoundException(nameof(Script), request.ScriptId);

            var action = CreateGameActionFromDto(request.Action);
            script.AddAction(action);
            await _scriptRepository.SaveChangesAsync();

            return MapToDto(script);
        }

        public async Task<ScriptDto> Handle(RemoveActionFromScriptCommand request, CancellationToken cancellationToken)
        {
            var script = await _scriptRepository.GetByIdAsync(request.ScriptId);
            if (script == null)
                throw new EntityNotFoundException(nameof(Script), request.ScriptId);

            script.RemoveAction(request.ActionId);
            await _scriptRepository.SaveChangesAsync();

            return MapToDto(script);
        }

        public async Task<ScriptDto> Handle(ActivateScriptCommand request, CancellationToken cancellationToken)
        {
            var script = await _scriptRepository.GetByIdAsync(request.Id);
            if (script == null)
                throw new EntityNotFoundException(nameof(Script), request.Id);

            script.Activate();
            await _scriptRepository.SaveChangesAsync();

            return MapToDto(script);
        }

        public async Task<ScriptDto> Handle(DeactivateScriptCommand request, CancellationToken cancellationToken)
        {
            var script = await _scriptRepository.GetByIdAsync(request.Id);
            if (script == null)
                throw new EntityNotFoundException(nameof(Script), request.Id);

            script.Deactivate();
            await _scriptRepository.SaveChangesAsync();

            return MapToDto(script);
        }

        public async Task<ScriptDto[]> Handle(GetAllScriptsQuery request, CancellationToken cancellationToken)
        {
            var scripts = request.Status.HasValue
                ? await _scriptRepository.GetByStatusAsync(MapScriptStatusToDomain(request.Status.Value))
                : await _scriptRepository.GetAllAsync();

            return scripts.Select(MapToDto).ToArray();
        }

        public async Task<ScriptDto> Handle(GetScriptDetailsQuery request, CancellationToken cancellationToken)
        {
            var script = await _scriptRepository.GetByIdAsync(request.Id);
            if (script == null)
                throw new EntityNotFoundException(nameof(Script), request.Id);

            return MapToDto(script);
        }

        private static ScriptDto MapToDto(Script script)
        {
            return new ScriptDto
            {
                Id = script.Id,
                Name = script.Name,
                Description = script.Description,
                Status = MapScriptStatusToDto(script.Status),
                Actions = script.Actions.Select(MapToDto).ToList(),
                CreatedAt = script.CreatedAt,
                UpdatedAt = script.UpdatedAt,
                Version = script.Version,
                EstimatedDuration = script.GetEstimatedDuration()
            };
        }

        private static GameActionDto MapToDto(GameAction action)
        {
            return new GameActionDto
            {
                Id = action.Id,
                Type = MapActionTypeToDto(action.Type),
                Key = MapKeyCodeToDto(action.Key),
                Button = MapMouseButtonToDto(action.Button),
                X = action.X,
                Y = action.Y,
                Delay = action.Delay,
                Timestamp = action.Timestamp,
                Description = action.Description
            };
        }

        private static GameAction CreateGameActionFromDto(GameActionDto dto)
        {
            var domainActionType = MapActionTypeToDomain(dto.Type);
            var domainKeyCode = MapKeyCodeToDomain(dto.Key);
            var domainMouseButton = MapMouseButtonToDomain(dto.Button);

            return dto.Type switch
            {
                ActionTypeDto.KeyDown or ActionTypeDto.KeyUp => new GameAction(dto.Id, domainActionType, domainKeyCode, dto.Delay, dto.Description),
                ActionTypeDto.MouseDown or ActionTypeDto.MouseUp or ActionTypeDto.MouseMove => new GameAction(dto.Id, domainActionType, domainMouseButton, dto.X, dto.Y, dto.Delay, dto.Description),
                ActionTypeDto.Delay => new GameAction(dto.Id, domainActionType, dto.Delay, dto.Description),
                _ => throw new ArgumentException($"Unsupported action type: {dto.Type}")
            };
        }

        #region 枚举映射方法

        private static ScriptStatusDto MapScriptStatusToDto(ScriptStatus status)
        {
            return status switch
            {
                ScriptStatus.Draft => ScriptStatusDto.Draft,
                ScriptStatus.Active => ScriptStatusDto.Active,
                ScriptStatus.Inactive => ScriptStatusDto.Inactive,
                ScriptStatus.Deleted => ScriptStatusDto.Deleted,
                _ => throw new ArgumentException($"Unknown script status: {status}")
            };
        }

        private static ScriptStatus MapScriptStatusToDomain(ScriptStatusDto status)
        {
            return status switch
            {
                ScriptStatusDto.Draft => ScriptStatus.Draft,
                ScriptStatusDto.Active => ScriptStatus.Active,
                ScriptStatusDto.Inactive => ScriptStatus.Inactive,
                ScriptStatusDto.Deleted => ScriptStatus.Deleted,
                _ => throw new ArgumentException($"Unknown script status: {status}")
            };
        }

        private static ActionTypeDto MapActionTypeToDto(ActionType type)
        {
            return type switch
            {
                ActionType.KeyDown => ActionTypeDto.KeyDown,
                ActionType.KeyUp => ActionTypeDto.KeyUp,
                ActionType.MouseMove => ActionTypeDto.MouseMove,
                ActionType.MouseDown => ActionTypeDto.MouseDown,
                ActionType.MouseUp => ActionTypeDto.MouseUp,
                ActionType.Delay => ActionTypeDto.Delay,
                _ => throw new ArgumentException($"Unknown action type: {type}")
            };
        }

        private static ActionType MapActionTypeToDomain(ActionTypeDto type)
        {
            return type switch
            {
                ActionTypeDto.KeyDown => ActionType.KeyDown,
                ActionTypeDto.KeyUp => ActionType.KeyUp,
                ActionTypeDto.MouseMove => ActionType.MouseMove,
                ActionTypeDto.MouseDown => ActionType.MouseDown,
                ActionTypeDto.MouseUp => ActionType.MouseUp,
                ActionTypeDto.Delay => ActionType.Delay,
                _ => throw new ArgumentException($"Unknown action type: {type}")
            };
        }

        private static KeyCodeDto MapKeyCodeToDto(KeyCode key)
        {
            return (KeyCodeDto)key;
        }

        private static MouseButtonDto MapMouseButtonToDto(MouseButton button)
        {
            return button switch
            {
                MouseButton.None => MouseButtonDto.None,
                MouseButton.Left => MouseButtonDto.Left,
                MouseButton.Right => MouseButtonDto.Right,
                MouseButton.Middle => MouseButtonDto.Middle,
                _ => throw new ArgumentException($"Unknown mouse button: {button}")
            };
        }

        private static KeyCode MapKeyCodeToDomain(KeyCodeDto key)
        {
            return (KeyCode)key;
        }

        private static MouseButton MapMouseButtonToDomain(MouseButtonDto button)
        {
            return button switch
            {
                MouseButtonDto.None => MouseButton.None,
                MouseButtonDto.Left => MouseButton.Left,
                MouseButtonDto.Right => MouseButton.Right,
                MouseButtonDto.Middle => MouseButton.Middle,
                _ => throw new ArgumentException($"Unknown mouse button: {button}")
            };
        }

        #endregion
    }
}