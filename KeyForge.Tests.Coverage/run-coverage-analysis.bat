@echo off
setlocal enabledelayedexpansion

REM KeyForge 测试覆盖率分析运行脚本 (Windows版本)
REM 此脚本用于自动化运行测试覆盖率分析

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
if not exist "KeyForge.Tests.Coverage.csproj" (
    call :log_error "未找到项目文件 KeyForge.Tests.Coverage.csproj"
    exit /b 1
)

REM 检查配置文件
if not exist "coverage-settings.json" (
    call :log_warning "未找到配置文件 coverage-settings.json，将使用默认配置"
)

call :log_success "前置条件检查完成"
goto :eof

REM 创建输出目录
:create_output_directories
call :log_info "创建输出目录..."

if not exist "coverage" mkdir "coverage"
if not exist "coverage\report" mkdir "coverage\report"
if not exist "coverage\history" mkdir "coverage\history"
if not exist "coverage\details" mkdir "coverage\details"
if not exist "logs" mkdir "logs"

call :log_success "输出目录创建完成"
goto :eof

REM 清理之前的测试结果
:cleanup_previous_results
call :log_info "清理之前的测试结果..."

REM 清理编译输出
dotnet clean -v quiet >nul 2>&1

REM 清理覆盖率数据
if exist "coverage\*.xml" del /Q "coverage\*.xml"
if exist "coverage\*.json" del /Q "coverage\*.json"
if exist "coverage\report\*" del /Q "coverage\report\*"

REM 清理日志文件
if exist "logs\*.log" del /Q "logs\*.log"

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

REM 运行覆盖率分析
:run_coverage_analysis
call :log_info "运行覆盖率分析..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- run > logs\coverage-analysis.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "覆盖率分析完成，耗时: !duration!s"
) else (
    call :log_error "覆盖率分析失败"
    call :log_error "查看日志: logs\coverage-analysis.log"
    exit /b 1
)
goto :eof

REM 生成覆盖率报告
:generate_coverage_report
call :log_info "生成覆盖率报告..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- report > logs\report-generation.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "覆盖率报告生成完成，耗时: !duration!s"
    call :log_info "报告位置: coverage\report\"
) else (
    call :log_error "覆盖率报告生成失败"
    call :log_error "查看日志: logs\report-generation.log"
    exit /b 1
)
goto :eof

REM 验证覆盖率阈值
:validate_coverage_thresholds
call :log_info "验证覆盖率阈值..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- validate > logs\coverage-validation.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "覆盖率阈值验证通过，耗时: !duration!s"
) else (
    call :log_error "覆盖率阈值验证失败"
    call :log_error "查看日志: logs\coverage-validation.log"
    exit /b 1
)
goto :eof

REM 显示覆盖率建议
:show_coverage_recommendations
call :log_info "显示覆盖率建议..."

dotnet run --configuration Release -- recommendations > logs\coverage-recommendations.log 2>&1

if %errorlevel% equ 0 (
    call :log_success "覆盖率建议生成完成"
) else (
    call :log_error "覆盖率建议生成失败"
    call :log_error "查看日志: logs\coverage-recommendations.log"
    exit /b 1
)
goto :eof

REM 显示覆盖率历史
:show_coverage_history
call :log_info "显示覆盖率历史..."

dotnet run --configuration Release -- history > logs\coverage-history.log 2>&1

if %errorlevel% equ 0 (
    call :log_success "覆盖率历史显示完成"
) else (
    call :log_error "覆盖率历史显示失败"
    call :log_error "查看日志: logs\coverage-history.log"
    exit /b 1
)
goto :eof

REM 比较覆盖率数据
:compare_coverage_data
call :log_info "比较覆盖率数据..."

dotnet run --configuration Release -- compare > logs\coverage-comparison.log 2>&1

if %errorlevel% equ 0 (
    call :log_success "覆盖率数据比较完成"
) else (
    call :log_error "覆盖率数据比较失败"
    call :log_error "查看日志: logs\coverage-comparison.log"
    exit /b 1
)
goto :eof

REM 显示配置信息
:show_configuration
call :log_info "显示配置信息..."

dotnet run --configuration Release -- config
goto :eof

REM 显示阈值设置
:show_thresholds
call :log_info "显示阈值设置..."

dotnet run --configuration Release -- thresholds
goto :eof

REM 保存覆盖率历史
:save_coverage_history
call :log_info "保存覆盖率历史..."

set "history_dir=coverage\history"
set "timestamp=%date:~0,4%%date:~5,2%%date:~8,2%_%time:~0,2%%time:~3,2%"
set "timestamp=!timestamp: =0!"
set "history_file=!history_dir!\coverage_!timestamp!.json"

REM 查找最新的覆盖率数据
for /f "delims=" %%f in ('dir /b /o-d "coverage\*.json" 2^>nul ^| findstr /v "history" ^| head -1') do (
    set "latest_coverage=coverage\%%f"
    goto :found_coverage
)

call :log_warning "未找到最新的覆盖率数据"
goto :eof

:found_coverage
if exist "!latest_coverage!" (
    copy "!latest_coverage!" "!history_file!" >nul
    call :log_success "覆盖率历史已保存: !history_file!"
    
    REM 清理旧的历史文件（保留最近30天）
    forfiles /p "!history_dir!" /m *.json /d -30 /c "cmd /c del @path" >nul 2>&1
) else (
    call :log_warning "未找到最新的覆盖率数据"
)
goto :eof

REM 生成覆盖率徽章
:generate_coverage_badge
call :log_info "生成覆盖率徽章..."

set "badge_file=coverage\coverage.svg"

if exist "coverage\report\coverage.svg" (
    copy "coverage\report\coverage.svg" "!badge_file!" >nul
    call :log_success "覆盖率徽章已生成: !badge_file!"
) else (
    call :log_warning "未找到覆盖率徽章文件"
)
goto :eof

REM 检查覆盖率结果
:check_coverage_results
call :log_info "检查覆盖率结果..."

set "report_dir=coverage\report"
if exist "!report_dir!" (
    for /f "delims=" %%f in ('dir /b /o-d "!report_dir!\*.json" 2^>nul ^| head -1') do (
        set "latest_report=!report_dir!\%%f"
        goto :found_report
    )
    
    call :log_warning "未找到覆盖率报告"
    goto :eof
    
    :found_report
    call :log_info "最新报告: !latest_report!"
    
    REM 检查是否有jq工具来解析JSON
    jq --version >nul 2>&1
    if %errorlevel% equ 0 (
        for /f "delims=" %%s in ('jq -r ".OverallCoverage // 0" "!latest_report!"') do set "overall_coverage=%%s"
        for /f "delims=" %%s in ('jq -r ".LineCoverage // 0" "!latest_report!"') do set "line_coverage=%%s"
        for /f "delims=" %%s in ('jq -r ".BranchCoverage // 0" "!latest_report!"') do set "branch_coverage=%%s"
        for /f "delims=" %%s in ('jq -r ".MethodCoverage // 0" "!latest_report!"') do set "method_coverage=%%s"
        for /f "delims=" %%s in ('jq -r ".ClassCoverage // 0" "!latest_report!"') do set "class_coverage=%%s"
        
        call :log_info "覆盖率结果:"
        call :log_info "  总覆盖率: !overall_coverage!%"
        call :log_info "  行覆盖率: !line_coverage!%"
        call :log_info "  分支覆盖率: !branch_coverage!%"
        call :log_info "  方法覆盖率: !method_coverage!%"
        call :log_info "  类覆盖率: !class_coverage!%"
        
        REM 简单的数值比较（Windows批处理不支持浮点数比较，这里简化处理）
        if !overall_coverage! lss 80 (
            call :log_warning "总覆盖率较低，请关注测试覆盖率"
        ) else if !overall_coverage! lss 90 (
            call :log_warning "总覆盖率中等，建议改进"
        ) else (
            call :log_success "总覆盖率良好"
        )
    ) else (
        call :log_warning "未安装jq工具，无法解析覆盖率数据"
    )
) else (
    call :log_warning "未找到覆盖率报告目录"
)
goto :eof

REM 显示帮助信息
:show_help
echo KeyForge 测试覆盖率分析运行脚本 (Windows版本)
echo ==================================================
echo.
echo 用法: %~nx0 [选项]
echo.
echo 选项:
echo    run            - 运行覆盖率分析
echo    report         - 生成覆盖率报告
echo    validate       - 验证覆盖率阈值
echo    recommendations - 显示覆盖率建议
echo    history        - 显示覆盖率历史
echo    compare        - 比较覆盖率数据
echo    config         - 显示当前配置
echo    thresholds     - 显示阈值设置
echo    clean          - 清理之前的测试结果
echo    full           - 运行完整覆盖率分析
echo    help           - 显示此帮助信息
echo.
echo 示例:
echo    %~nx0 run            # 运行覆盖率分析
echo    %~nx0 full           # 运行完整覆盖率分析
echo    %~nx0 report         # 生成覆盖率报告
echo.
goto :eof

REM 主函数
:main
set "option=%~1"
if "%option%"=="" set "option=help"

if "%option%"=="run" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :run_coverage_analysis
    call :check_coverage_results
) else if "%option%"=="report" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :generate_coverage_report
) else if "%option%"=="validate" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :validate_coverage_thresholds
) else if "%option%"=="recommendations" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :show_coverage_recommendations
) else if "%option%"=="history" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :show_coverage_history
) else if "%option%"=="compare" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :compare_coverage_data
) else if "%option%"=="config" (
    call :show_configuration
) else if "%option%"=="thresholds" (
    call :show_thresholds
) else if "%option%"=="clean" (
    call :cleanup_previous_results
    call :log_success "清理完成"
) else if "%option%"=="full" (
    call :check_prerequisites
    call :create_output_directories
    call :cleanup_previous_results
    call :build_project
    call :run_coverage_analysis
    call :generate_coverage_report
    call :validate_coverage_thresholds
    call :show_coverage_recommendations
    call :generate_coverage_badge
    call :save_coverage_history
    call :check_coverage_results
    call :log_success "完整覆盖率分析执行完成"
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