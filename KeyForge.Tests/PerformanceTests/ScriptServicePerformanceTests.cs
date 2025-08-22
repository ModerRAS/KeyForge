using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using KeyForge.Application.Services;
using KeyForge.Application.Commands;
using KeyForge.Application.Queries;
using KeyForge.Core.Interfaces;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.PerformanceTests
{
    /// <summary>
    /// 脚本服务性能测试
    /// </summary>
    public class ScriptServicePerformanceTests : TestBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IScriptService _scriptService;

        public ScriptServicePerformanceTests()
        {
            var services = new ServiceCollection();
            services.AddInfrastructure();
            services.AddScoped<IScriptService, ScriptService>();
            
            _serviceProvider = services.BuildServiceProvider();
            _scriptService = _serviceProvider.GetRequiredService<IScriptService>();
        }

        [Fact]
        public async Task CreateScript_Performance_ShouldBeUnderThreshold()
        {
            // Arrange
            const int iterationCount = 100;
            const int maxExecutionTimeMs = 5000; // 5秒阈值

            var stopwatch = new Stopwatch();
            var results = new List<Application.DTOs.ScriptDto>();

            // Act
            stopwatch.Start();

            for (int i = 0; i < iterationCount; i++)
            {
                var command = TestDataFactory.CreateCreateScriptCommand();
                var result = await _scriptService.Handle(command, default);
                results.Add(result);
            }

            stopwatch.Stop();

            // Assert
            var totalTime = stopwatch.ElapsedMilliseconds;
            var averageTime = totalTime / iterationCount;

            totalTime.Should().BeLessThan(maxExecutionTimeMs);
            averageTime.Should().BeLessThan(100); // 平均每个脚本创建时间应小于100ms
            results.Should().HaveCount(iterationCount);
            results.All(r => r != null).Should().BeTrue();
        }

        [Fact]
        public async Task UpdateScript_Performance_ShouldBeUnderThreshold()
        {
            // Arrange
            const int iterationCount = 100;
            const int maxExecutionTimeMs = 3000; // 3秒阈值

            var scripts = new List<Application.DTOs.ScriptDto>();
            for (int i = 0; i < iterationCount; i++)
            {
                var command = TestDataFactory.CreateCreateScriptCommand();
                var script = await _scriptService.Handle(command, default);
                scripts.Add(script);
            }

            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();

            foreach (var script in scripts)
            {
                var updateCommand = new UpdateScriptCommand
                {
                    Id = script.Id,
                    Name = $"Updated {script.Name}",
                    Description = $"Updated {script.Description}"
                };
                await _scriptService.Handle(updateCommand, default);
            }

            stopwatch.Stop();

            // Assert
            var totalTime = stopwatch.ElapsedMilliseconds;
            var averageTime = totalTime / iterationCount;

            totalTime.Should().BeLessThan(maxExecutionTimeMs);
            averageTime.Should().BeLessThan(50); // 平均每个脚本更新时间应小于50ms
        }

        [Fact]
        public async Task QueryScripts_Performance_ShouldBeUnderThreshold()
        {
            // Arrange
            const int scriptCount = 1000;
            const int maxExecutionTimeMs = 2000; // 2秒阈值

            // 创建测试数据
            for (int i = 0; i < scriptCount; i++)
            {
                var command = TestDataFactory.CreateCreateScriptCommand();
                await _scriptService.Handle(command, default);
            }

            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();

            var result = await _scriptService.Handle(new GetAllScriptsQuery(), default);

            stopwatch.Stop();

            // Assert
            var totalTime = stopwatch.ElapsedMilliseconds;

            totalTime.Should().BeLessThan(maxExecutionTimeMs);
            result.Should().HaveCountGreaterThanOrEqualTo(scriptCount);
        }

        [Fact]
        public async Task MemoryUsage_ShouldBeReasonable()
        {
            // Arrange
            const int scriptCount = 1000;
            const int maxMemoryMB = 100; // 最大内存使用100MB

            var initialMemory = GC.GetTotalMemory(false);

            // 创建大量脚本
            for (int i = 0; i < scriptCount; i++)
            {
                var command = TestDataFactory.CreateCreateScriptCommand();
                await _scriptService.Handle(command, default);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            var finalMemory = GC.GetTotalMemory(false);

            // Assert
            var memoryIncreaseMB = (finalMemory - initialMemory) / (1024.0 * 1024.0);
            memoryIncreaseMB.Should().BeLessThan(maxMemoryMB);
        }

        [Fact]
        public async Task ConcurrentOperations_Performance_ShouldScale()
        {
            // Arrange
            const int concurrentUsers = 50;
            const int operationsPerUser = 20;
            const int maxExecutionTimeMs = 10000; // 10秒阈值

            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();

            var tasks = new List<Task<Application.DTOs.ScriptDto>>();
            for (int i = 0; i < concurrentUsers; i++)
            {
                var userTasks = new List<Task<Application.DTOs.ScriptDto>>();
                for (int j = 0; j < operationsPerUser; j++)
                {
                    var command = TestDataFactory.CreateCreateScriptCommand();
                    userTasks.Add(_scriptService.Handle(command, default));
                }
                tasks.AddRange(userTasks);
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            var totalTime = stopwatch.ElapsedMilliseconds;
            var totalOperations = concurrentUsers * operationsPerUser;
            var operationsPerSecond = totalOperations / (totalTime / 1000.0);

            totalTime.Should().BeLessThan(maxExecutionTimeMs);
            operationsPerSecond.Should().BeGreaterThan(50); // 每秒至少50个操作
            tasks.All(t => t.Result != null).Should().BeTrue();
        }

        [Fact]
        public async Task LargeScript_Performance_ShouldHandleEfficiently()
        {
            // Arrange
            const int actionCount = 10000;
            const int maxExecutionTimeMs = 5000; // 5秒阈值

            var command = new CreateScriptCommand
            {
                Name = "Large Script",
                Description = "Script with many actions",
                Actions = new List<Application.DTOs.GameActionDto>()
            };

            // 创建大量动作
            for (int i = 0; i < actionCount; i++)
            {
                command.Actions.Add(new Application.DTOs.GameActionDto
                {
                    Type = i % 2 == 0 ? ActionType.KeyDown : ActionType.MouseDown,
                    Key = i % 2 == 0 ? KeyCode.A : null,
                    Button = i % 2 == 1 ? MouseButton.Left : null,
                    X = i % 2 == 1 ? 100 + i : 0,
                    Y = i % 2 == 1 ? 200 + i : 0,
                    Delay = 10
                });
            }

            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();

            var result = await _scriptService.Handle(command, default);

            stopwatch.Stop();

            // Assert
            var totalTime = stopwatch.ElapsedMilliseconds;

            totalTime.Should().BeLessThan(maxExecutionTimeMs);
            result.Should().NotBeNull();
            result.Actions.Should().HaveCount(actionCount);
        }

        [Fact]
        public async Task DatabaseConnection_ShouldBeEfficient()
        {
            // Arrange
            const int iterationCount = 1000;
            const int maxExecutionTimeMs = 15000; // 15秒阈值

            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();

            for (int i = 0; i < iterationCount; i++)
            {
                var command = TestDataFactory.CreateCreateScriptCommand();
                var script = await _scriptService.Handle(command, default);

                var updateCommand = new UpdateScriptCommand
                {
                    Id = script.Id,
                    Name = $"Updated {script.Name}",
                    Description = $"Updated {script.Description}"
                };
                await _scriptService.Handle(updateCommand, default);

                var query = new GetScriptQuery { Id = script.Id };
                await _scriptService.Handle(query, default);
            }

            stopwatch.Stop();

            // Assert
            var totalTime = stopwatch.ElapsedMilliseconds;
            var averageTime = totalTime / iterationCount;

            totalTime.Should().BeLessThan(maxExecutionTimeMs);
            averageTime.Should().BeLessThan(20); // 平均每个完整周期应小于20ms
        }

        [Fact]
        public async Task StressTest_ShouldHandleHighLoad()
        {
            // Arrange
            const int durationSeconds = 30;
            const int maxExecutionTimeMs = durationSeconds * 1000 + 5000; // 30秒+5秒缓冲

            var stopwatch = new Stopwatch();
            var cts = new System.Threading.CancellationTokenSource();
            var operationCount = 0;
            var errorCount = 0;

            // Act
            stopwatch.Start();

            var loadTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var command = TestDataFactory.CreateCreateScriptCommand();
                        await _scriptService.Handle(command, default);
                        operationCount++;
                    }
                    catch
                    {
                        errorCount++;
                    }
                }
            }, cts.Token);

            // 运行指定时间
            await Task.Delay(durationSeconds * 1000);
            cts.Cancel();
            await loadTask;

            stopwatch.Stop();

            // Assert
            var totalTime = stopwatch.ElapsedMilliseconds;
            var operationsPerSecond = operationCount / (totalTime / 1000.0);

            totalTime.Should().BeLessThan(maxExecutionTimeMs);
            operationCount.Should().BeGreaterThan(100); // 至少完成100个操作
            errorCount.Should().BeLessThan(operationCount * 0.05); // 错误率应小于5%
            operationsPerSecond.Should().BeGreaterThan(3); // 每秒至少3个操作
        }
    }
}