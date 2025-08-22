using System;
using System.Collections.Generic;

namespace KeyForge.Application.DTOs
{
    /// <summary>
    /// 执行历史DTO
    /// </summary>
    public class ExecutionHistoryDto
    {
        public Guid Id { get; set; }
        public Guid ScriptId { get; set; }
        public string ScriptName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public ExecutionStatusDto Status { get; set; }
        public string ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }
        public int ActionsExecuted { get; set; }
    }

    /// <summary>
    /// 系统统计DTO
    /// </summary>
    public class SystemStatsDto
    {
        public int TotalScripts { get; set; }
        public int ActiveScripts { get; set; }
        public int TotalImageTemplates { get; set; }
        public int ActiveImageTemplates { get; set; }
        public int TotalStateMachines { get; set; }
        public int ActiveStateMachines { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public double AverageExecutionTime { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// 分页结果DTO
    /// </summary>
    public class PagedResultDto<T>
    {
        public T[] Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// API响应DTO
    /// </summary>
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public DateTime Timestamp { get; set; }

        public static ApiResponseDto<T> SuccessResult(T data, string message = null)
        {
            return new ApiResponseDto<T>
            {
                Success = true,
                Data = data,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponseDto<T> ErrorResult(string message, params string[] errors)
        {
            return new ApiResponseDto<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// 执行请求DTO
    /// </summary>
    public class ExecuteScriptRequest
    {
        public Guid ScriptId { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public bool Repeat { get; set; }
        public int RepeatCount { get; set; } = 1;
        public int RepeatInterval { get; set; } = 1000;
    }

    /// <summary>
    /// 执行响应DTO
    /// </summary>
    public class ExecuteScriptResponse
    {
        public Guid ExecutionId { get; set; }
        public Guid ScriptId { get; set; }
        public string ScriptName { get; set; }
        public ExecutionStatusDto Status { get; set; }
        public DateTime StartTime { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// 图像识别请求DTO
    /// </summary>
    public class RecognizeImageRequest
    {
        public byte[] ImageData { get; set; }
        public string TemplateName { get; set; }
        public RectangleDto SearchArea { get; set; }
        public double MinimumConfidence { get; set; } = 0.8;
    }

    /// <summary>
    /// 图像识别响应DTO
    /// </summary>
    public class RecognizeImageResponse
    {
        public bool IsMatch { get; set; }
        public RectangleDto MatchArea { get; set; }
        public double Confidence { get; set; }
        public string TemplateName { get; set; }
        public DateTime RecognizedAt { get; set; }
    }
}