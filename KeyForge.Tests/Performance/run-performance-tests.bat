@echo off
setlocal enabledelayedexpansion

REM KeyForge 性能测试运行脚本 (Windows版本)
REM 此脚本用于自动化运行所有性能测试

REM 颜色定义 (Windows 10+)
set "RED=[91m"
set "GREEN=[92m"
set "YELLOW=[93m"
set "BLUE=[94m"
set "NC=[0m"

REM 日志函数
:log_info
echo %BLUE%[INFO]%NC% %~1
goto :eof

:log_success
echo %GREEN%[SUCCESS]%NC% %~1
goto :eof

:log_warning
echo %YELLOW%[WARNING]%NC% %~1
goto :eof

:log_error
echo %RED%[ERROR]%NC% %~1
goto :eof

REM 检查前置条件
:check_prerequisites
call :log_info "检查前置条件..."

REM 检查.NET SDK
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    call :log_error "未找到 .NET SDK，请先安装 .NET 8.0 SDK"
    exit /b 1
)

REM 检查.NET版本
for /f "tokens=*" %%i in ('dotnet --version') do set dotnet_version=%%i
call :log_info "检测到 .NET 版本: !dotnet_version!"

echo !dotnet_version! | findstr /r "^8\." >nul
if %errorlevel% neq 0 (
    call :log_warning "建议使用 .NET 8.0 SDK，当前版本: !dotnet_version!"
)

REM 检查项目文件
if not exist "KeyForge.Tests.Performance.csproj" (
    call :log_error "未找到项目文件 KeyForge.Tests.Performance.csproj"
    exit /b 1
)

REM 检查配置文件
if not exist "PerformanceSettings.json" (
    call :log_warning "未找到配置文件 PerformanceSettings.json，将使用默认配置"
)

call :log_success "前置条件检查完成"
goto :eof

REM 创建输出目录
:create_output_directories
call :log_info "创建输出目录..."

if not exist "PerformanceReports" mkdir "PerformanceReports"
if not exist "BenchmarkDotNet.Artifacts" mkdir "BenchmarkDotNet.Artifacts"
if not exist "logs" mkdir "logs"

call :log_success "输出目录创建完成"
goto :eof

REM 清理之前的测试结果
:cleanup_previous_results
call :log_info "清理之前的测试结果..."

REM 清理编译输出
dotnet clean -v quiet >nul 2>&1

REM 清理BenchmarkDotNet输出
if exist "BenchmarkDotNet.Artifacts\*" del /Q "BenchmarkDotNet.Artifacts\*"

REM 清理日志文件
if exist "logs\*.log" del /Q "logs\*.log"

REM 清理性能报告
if exist "PerformanceReports\*" del /Q "PerformanceReports\*"

call :log_success "清理完成"
goto :eof

REM 构建项目
:build_project
call :log_info "构建项目..."

REM 恢复依赖
dotnet restore >nul 2>&1

REM 构建项目
dotnet build --configuration Release >nul 2>&1

if %errorlevel% equ 0 (
    call :log_success "项目构建成功"
) else (
    call :log_error "项目构建失败"
    exit /b 1
)
goto :eof

REM 运行快速性能检查
:run_quick_test
call :log_info "运行快速性能检查..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- quick > logs\quick-test.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "快速性能检查完成，耗时: !duration!s"
) else (
    call :log_error "快速性能检查失败"
    call :log_error "查看日志: logs\quick-test.log"
    exit /b 1
)
goto :eof

REM 运行基准测试
:run_benchmark_tests
call :log_info "运行基准测试..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- benchmark > logs\benchmark-test.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "基准测试完成，耗时: !duration!s"
    call :log_info "基准测试结果保存在 BenchmarkDotNet.Artifacts\ 目录"
) else (
    call :log_error "基准测试失败"
    call :log_error "查看日志: logs\benchmark-test.log"
    exit /b 1
)
goto :eof

REM 运行负载测试
:run_load_tests
call :log_info "运行负载测试..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- load > logs\load-test.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "负载测试完成，耗时: !duration!s"
) else (
    call :log_error "负载测试失败"
    call :log_error "查看日志: logs\load-test.log"
    exit /b 1
)
goto :eof

REM 运行内存分析测试
:run_memory_tests
call :log_info "运行内存分析测试..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- memory > logs\memory-test.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "内存分析测试完成，耗时: !duration!s"
) else (
    call :log_error "内存分析测试失败"
    call :log_error "查看日志: logs\memory-test.log"
    exit /b 1
)
goto :eof

REM 运行压力测试
:run_stress_tests
call :log_info "运行压力测试..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- stress > logs\stress-test.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "压力测试完成，耗时: !duration!s"
) else (
    call :log_error "压力测试失败"
    call :log_error "查看日志: logs\stress-test.log"
    exit /b 1
)
goto :eof

REM 运行所有测试
:run_all_tests
call :log_info "运行所有性能测试..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- all > logs\all-tests.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "所有性能测试完成，耗时: !duration!s"
) else (
    call :log_error "性能测试失败"
    call :log_error "查看日志: logs\all-tests.log"
    exit /b 1
)
goto :eof

REM 生成报告
:generate_report
call :log_info "生成性能测试报告..."

dotnet run --configuration Release -- report > logs\report-generation.log 2>&1

if %errorlevel% equ 0 (
    call :log_success "性能测试报告生成完成"
    call :log_info "报告保存在 PerformanceReports\ 目录"
) else (
    call :log_error "报告生成失败"
    call :log_error "查看日志: logs\report-generation.log"
    exit /b 1
)
goto :eof

REM 显示系统信息
:show_system_info
call :log_info "系统信息:"
call :log_info "  操作系统: %OS%"
call :log_info "  计算机名: %COMPUTERNAME%"
call :log_info "  处理器: %PROCESSOR_IDENTIFIER%"
call :log_info "  内存: "
systeminfo | find "Total Physical Memory"
call :log_info "  .NET版本:"
dotnet --version
goto :eof

REM 显示帮助信息
:show_help
echo KeyForge 性能测试运行脚本 (Windows版本)
echo ==========================================
echo.
echo 用法: %~nx0 [选项]
echo.
echo 选项:
echo   quick       运行快速性能检查
echo   benchmark   运行基准测试
echo   load        运行负载测试
echo   memory      运行内存分析测试
echo   stress      运行压力测试
echo   all         运行所有性能测试
echo   report      生成性能测试报告
echo   full        运行完整测试套件
echo   clean       清理之前的测试结果
echo   help        显示此帮助信息
echo.
echo 示例:
echo   %~nx0 quick          运行快速性能检查
echo   %~nx0 all            运行所有性能测试
echo   %~nx0 full           运行完整测试套件
echo.
goto :eof

REM 主函数
:main
set "option=%~1"
if "%option%"=="" set "option=help"

if "%option%"=="quick" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :run_quick_test
) else if "%option%"=="benchmark" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :run_benchmark_tests
) else if "%option%"=="load" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :run_load_tests
) else if "%option%"=="memory" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :run_memory_tests
) else if "%option%"=="stress" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :run_stress_tests
) else if "%option%"=="all" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :run_all_tests
) else if "%option%"=="report" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :generate_report
) else if "%option%"=="full" (
    call :show_system_info
    call :check_prerequisites
    call :create_output_directories
    call :cleanup_previous_results
    call :build_project
    call :run_quick_test
    call :run_benchmark_tests
    call :run_load_tests
    call :run_memory_tests
    call :run_stress_tests
    call :run_all_tests
    call :generate_report
    call :log_success "完整测试套件执行完成"
) else if "%option%"=="clean" (
    call :cleanup_previous_results
    call :log_success "清理完成"
) else if "%option%"=="help" (
    call :show_help
) else if "%option%"=="-h" (
    call :show_help
) else if "%option%"=="--help" (
    call :show_help
) else (
    call :log_error "未知选项: %option%"
    call :show_help
    exit /b 1
)

goto :eof

REM 脚本入口点
call :main %*