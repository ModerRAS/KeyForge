using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KeyForge.Domain.Common;
using KeyForge.Domain.Entities;
using KeyForge.Domain.Interfaces;
using KeyForge.Domain.ValueObjects;
using KeyForge.Domain.Aggregates;

namespace KeyForge.Infrastructure.Services
{
    /// <summary>
    /// 输入验证服务实现
    /// 
    /// 原本实现：复杂的验证规则和自定义验证器
    /// 简化实现：使用数据注解和基本验证逻辑
    /// 
    /// 优化建议：
    /// 1. 实现自定义验证属性
    /// 2. 添加验证规则配置
    /// 3. 支持异步验证
    /// 4. 实现验证缓存
    /// </summary>
    public class InputValidationService : IInputValidationService
    {
        public async Task<Result> ValidateAsync(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return Result.Failure("验证对象不能为null");
                }

                var validationContext = new ValidationContext(obj);
                var validationResults = new List<ValidationResult>();
                
                var isValid = Validator.TryValidateObject(obj, validationContext, validationResults, true);
                
                if (!isValid)
                {
                    var errorMessages = validationResults
                        .Select(vr => string.Join(", ", vr.MemberNames) + ": " + vr.ErrorMessage)
                        .ToArray();
                    
                    return Result.Failure($"验证失败: {string.Join("; ", errorMessages)}");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"验证过程中发生错误: {ex.Message}");
            }
        }

        public async Task<Result> ValidateScriptAsync(Script script)
        {
            try
            {
                if (script == null)
                {
                    return Result.Failure("脚本不能为null");
                }

                var errors = new List<string>();

                // 验证脚本名称
                if (string.IsNullOrWhiteSpace(script.Name))
                {
                    errors.Add("脚本名称不能为空");
                }
                else if (script.Name.Length < 3 || script.Name.Length > 100)
                {
                    errors.Add("脚本名称长度必须在3-100个字符之间");
                }

                // 验证脚本描述
                if (string.IsNullOrWhiteSpace(script.Description))
                {
                    errors.Add("脚本描述不能为空");
                }

                // 验证脚本状态
                if (!Enum.IsDefined(typeof(ScriptStatus), script.Status))
                {
                    errors.Add("无效的脚本状态");
                }

                if (errors.Count > 0)
                {
                    return Result.Failure($"脚本验证失败: {string.Join("; ", errors)}");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"脚本验证过程中发生错误: {ex.Message}");
            }
        }

        public async Task<Result> ValidateStateMachineAsync(StateMachine stateMachine)
        {
            try
            {
                if (stateMachine == null)
                {
                    return Result.Failure("状态机不能为null");
                }

                var errors = new List<string>();

                // 验证状态机名称
                if (string.IsNullOrWhiteSpace(stateMachine.Name))
                {
                    errors.Add("状态机名称不能为空");
                }
                else if (stateMachine.Name.Length < 3 || stateMachine.Name.Length > 100)
                {
                    errors.Add("状态机名称长度必须在3-100个字符之间");
                }

                // 验证状态机状态
                if (!Enum.IsDefined(typeof(StateMachineStatus), stateMachine.Status))
                {
                    errors.Add("无效的状态机状态");
                }

                // 验证状态集合
                if (stateMachine.States == null || stateMachine.States.Count == 0)
                {
                    errors.Add("状态机必须包含至少一个状态");
                }

                if (errors.Count > 0)
                {
                    return Result.Failure($"状态机验证失败: {string.Join("; ", errors)}");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"状态机验证过程中发生错误: {ex.Message}");
            }
        }

        public async Task<Result> ValidateImageTemplateAsync(ImageTemplate template)
        {
            try
            {
                if (template == null)
                {
                    return Result.Failure("图像模板不能为null");
                }

                var errors = new List<string>();

                // 验证模板名称
                if (string.IsNullOrWhiteSpace(template.Name))
                {
                    errors.Add("模板名称不能为空");
                }
                else if (template.Name.Length < 3 || template.Name.Length > 100)
                {
                    errors.Add("模板名称长度必须在3-100个字符之间");
                }

                // 验证模板图像
                if (template.TemplateData == null || template.TemplateData.Length == 0)
                {
                    errors.Add("模板图像不能为空");
                }

                // 验证模板类型
                if (!Enum.IsDefined(typeof(TemplateType), template.TemplateType))
                {
                    errors.Add("无效的模板类型");
                }

                if (errors.Count > 0)
                {
                    return Result.Failure($"图像模板验证失败: {string.Join("; ", errors)}");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"图像模板验证过程中发生错误: {ex.Message}");
            }
        }

        public async Task<Result> ValidateConfigurationAsync(Dictionary<string, object> configuration)
        {
            try
            {
                if (configuration == null)
                {
                    return Result.Failure("配置不能为null");
                }

                var errors = new List<string>();

                foreach (var kvp in configuration)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key))
                    {
                        errors.Add("配置键不能为空");
                    }
                    else if (kvp.Value == null)
                    {
                        errors.Add($"配置键 '{kvp.Key}' 的值不能为null");
                    }
                }

                if (errors.Count > 0)
                {
                    return Result.Failure($"配置验证失败: {string.Join("; ", errors)}");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"配置验证过程中发生错误: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<string>>> GetValidationErrorsAsync(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return Result<IEnumerable<string>>.Success(new[] { "验证对象不能为null" });
                }

                var validationContext = new ValidationContext(obj);
                var validationResults = new List<ValidationResult>();
                
                Validator.TryValidateObject(obj, validationContext, validationResults, true);
                
                var errors = validationResults
                    .Select(vr => string.Join(", ", vr.MemberNames) + ": " + vr.ErrorMessage)
                    .ToList();

                return Result<IEnumerable<string>>.Success(errors);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<string>>.Failure($"获取验证错误过程中发生错误: {ex.Message}");
            }
        }
    }
}