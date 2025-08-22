@echo off
setlocal enabledelayedexpansion

REM KeyForge 安全测试运行脚本 (Windows版本)
REM 此脚本用于自动化运行所有安全测试

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
if not exist "KeyForge.Tests.Security.csproj" (
    call :log_error "未找到项目文件 KeyForge.Tests.Security.csproj"
    exit /b 1
)

REM 检查配置文件
if not exist "security-test-config.json" (
    call :log_warning "未找到配置文件 security-test-config.json，将使用默认配置"
)

call :log_success "前置条件检查完成"
goto :eof

REM 创建输出目录
:create_output_directories
call :log_info "创建输出目录..."

if not exist "SecurityReports" mkdir "SecurityReports"
if not exist "logs" mkdir "logs"
if not exist "temp" mkdir "temp"

call :log_success "输出目录创建完成"
goto :eof

REM 清理之前的测试结果
:cleanup_previous_results
call :log_info "清理之前的测试结果..."

REM 清理编译输出
dotnet clean -v quiet >nul 2>&1

REM 清理日志文件
if exist "logs\*.log" del /Q "logs\*.log"

REM 清理安全报告
if exist "SecurityReports\*" del /Q "SecurityReports\*"

REM 清理临时文件
if exist "temp\*" del /Q "temp\*"

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

REM 运行快速安全检查
:run_quick_test
call :log_info "运行快速安全检查..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- quick > logs\quick-security-test.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "快速安全检查完成，耗时: !duration!s"
) else (
    call :log_error "快速安全检查失败"
    call :log_error "查看日志: logs\quick-security-test.log"
    exit /b 1
)
goto :eof

REM 运行认证测试
:run_auth_tests
call :log_info "运行认证和授权测试..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- auth > logs\auth-test.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "认证和授权测试完成，耗时: !duration!s"
) else (
    call :log_error "认证和授权测试失败"
    call :log_error "查看日志: logs\auth-test.log"
    exit /b 1
)
goto :eof

REM 运行输入验证测试
:run_input_tests
call :log_info "运行输入验证测试..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- input > logs\input-test.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "输入验证测试完成，耗时: !duration!s"
) else (
    call :log_error "输入验证测试失败"
    call :log_error "查看日志: logs\input-test.log"
    exit /b 1
)
goto :eof

REM 运行HTTP安全测试
:run_http_tests
call :log_info "运行HTTP安全测试..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- http > logs\http-test.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "HTTP安全测试完成，耗时: !duration!s"
) else (
    call :log_error "HTTP安全测试失败"
    call :log_error "查看日志: logs\http-test.log"
    exit /b 1
)
goto :eof

REM 运行加密测试
:run_encryption_tests
call :log_info "运行加密安全测试..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- encryption > logs\encryption-test.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "加密安全测试完成，耗时: !duration!s"
) else (
    call :log_error "加密安全测试失败"
    call :log_error "查看日志: logs\encryption-test.log"
    exit /b 1
)
goto :eof

REM 运行漏洞扫描
:run_vulnerability_scan
call :log_info "运行漏洞扫描..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- scan > logs\vulnerability-scan.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "漏洞扫描完成，耗时: !duration!s"
) else (
    call :log_error "漏洞扫描失败"
    call :log_error "查看日志: logs\vulnerability-scan.log"
    exit /b 1
)
goto :eof

REM 运行渗透测试
:run_penetration_tests
call :log_info "运行渗透测试..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- pentest > logs\penetration-test.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "渗透测试完成，耗时: !duration!s"
) else (
    call :log_error "渗透测试失败"
    call :log_error "查看日志: logs\penetration-test.log"
    exit /b 1
)
goto :eof

REM 运行完整安全测试套件
:run_full_security_tests
call :log_info "运行完整安全测试套件..."

powershell -Command "$start = Get-Date; dotnet run --configuration Release -- full > logs\full-security-test.log 2>&1; $end = Get-Date; $duration = ($end - $start).TotalSeconds; Write-Output $duration" > temp_duration.txt
set /p duration=<temp_duration.txt
del temp_duration.txt

if %errorlevel% equ 0 (
    call :log_success "完整安全测试套件完成，耗时: !duration!s"
) else (
    call :log_error "完整安全测试套件失败"
    call :log_error "查看日志: logs\full-security-test.log"
    exit /b 1
)
goto :eof

REM 生成安全测试报告
:generate_report
call :log_info "生成安全测试报告..."

dotnet run --configuration Release -- report > logs\report-generation.log 2>&1

if %errorlevel% equ 0 (
    call :log_success "安全测试报告生成完成"
    call :log_info "报告保存在 SecurityReports\ 目录"
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

REM 检查安全测试结果
:check_security_results
call :log_info "检查安全测试结果..."

if exist "SecurityReports" (
    set "report_dir=SecurityReports"
    for /f "delims=" %%f in ('dir /b /o-d "!report_dir!\*.json" 2^>nul') do (
        set "latest_report=!report_dir!\%%f"
        goto :found_report
    )
    
    call :log_warning "未找到安全测试报告"
    goto :eof
    
    :found_report
    call :log_info "最新报告: !latest_report!"
    
    REM 检查是否有jq工具来解析JSON
    jq --version >nul 2>&1
    if %errorlevel% equ 0 (
        for /f "delims=" %%s in ('jq -r ".OverallSecurityScore // 0" "!latest_report!"') do set "security_score=%%s"
        call :log_info "安全评分: !security_score!/100"
        
        REM 简单的数值比较（Windows批处理不支持浮点数比较，这里简化处理）
        if !security_score! lss 80 (
            call :log_warning "安全评分较低，请关注安全问题"
        ) else if !security_score! lss 90 (
            call :log_warning "安全评分中等，建议改进"
        ) else (
            call :log_success "安全评分良好"
        )
    ) else (
        call :log_warning "未安装jq工具，无法解析安全评分"
    )
) else (
    call :log_warning "未找到安全测试报告目录"
)
goto :eof

REM 显示帮助信息
:show_help
echo KeyForge 安全测试运行脚本 (Windows版本)
echo =========================================
echo.
echo 用法: %~nx0 [选项]
echo.
echo 选项:
echo   quick       运行快速安全检查
echo   auth        运行认证和授权测试
echo   input       运行输入验证测试
echo   http        运行HTTP安全测试
echo   encryption  运行加密安全测试
echo   scan        运行漏洞扫描
echo   pentest     运行渗透测试
echo   full        运行完整安全测试套件
echo   report      生成安全测试报告
echo   config      显示当前配置
echo   clean       清理之前的测试结果
echo   help        显示此帮助信息
echo.
echo 示例:
echo   %~nx0 quick          运行快速安全检查
echo   %~nx0 full           运行完整安全测试套件
echo   %~nx0 scan           运行漏洞扫描
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
    call :check_security_results
) else if "%option%"=="auth" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :run_auth_tests
    call :check_security_results
) else if "%option%"=="input" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :run_input_tests
    call :check_security_results
) else if "%option%"=="http" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :run_http_tests
    call :check_security_results
) else if "%option%"=="encryption" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :run_encryption_tests
    call :check_security_results
) else if "%option%"=="scan" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :run_vulnerability_scan
    call :check_security_results
) else if "%option%"=="pentest" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :run_penetration_tests
    call :check_security_results
) else if "%option%"=="full" (
    call :show_system_info
    call :check_prerequisites
    call :create_output_directories
    call :cleanup_previous_results
    call :build_project
    call :run_full_security_tests
    call :check_security_results
    call :generate_report
    call :log_success "完整安全测试套件执行完成"
) else if "%option%"=="report" (
    call :check_prerequisites
    call :create_output_directories
    call :build_project
    call :generate_report
) else if "%option%"=="config" (
    dotnet run --configuration Release -- config
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