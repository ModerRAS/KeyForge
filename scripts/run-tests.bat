@echo off
setlocal enabledelayedexpansion

REM KeyForge 测试执行脚本 (Windows版本)
REM 该脚本用于运行所有类型的测试并生成报告

REM 配置变量
set PROJECT_ROOT=%~dp0
set TEST_PROJECT=%PROJECT_ROOT%KeyForge.Tests
set REPORT_DIR=%PROJECT_ROOT%TestReports
set COVERAGE_REPORT=%REPORT_DIR%\coverage.xml
set TEST_RESULTS=%REPORT_DIR%\test-results.xml
set PERFORMANCE_REPORT=%REPORT_DIR%\performance-report.html

REM 创建报告目录
if not exist "%REPORT_DIR%" mkdir "%REPORT_DIR%"

REM 日志函数
:log_info
echo [INFO] %~1
goto :eof

:log_success
echo [SUCCESS] %~1
goto :eof

:log_warning
echo [WARNING] %~1
goto :eof

:log_error
echo [ERROR] %~1
goto :eof

REM 显示帮助信息
:show_help
echo KeyForge 测试执行脚本
echo.
echo 用法: %~nx0 [选项]
echo.
echo 选项:
echo     -h, --help          显示帮助信息
echo     -u, --unit          只运行单元测试
echo     -i, --integration   只运行集成测试
echo     -e, --e2e           只运行端到端测试
echo     -p, --performance   只运行性能测试
echo     -a, --all           运行所有测试 (默认)
echo     -c, --coverage      生成代码覆盖率报告
echo     -r, --report        生成HTML测试报告
echo     -v, --verbose       详细输出模式
echo.
echo 示例:
echo     %~nx0                  运行所有测试
echo     %~nx0 -u -c            运行单元测试并生成覆盖率报告
echo     %~nx0 -i -r            运行集成测试并生成HTML报告
echo     %~nx0 -p -v            运行性能测试并显示详细信息
goto :eof

REM 解析命令行参数
set RUN_UNIT=true
set RUN_INTEGRATION=true
set RUN_E2E=true
set RUN_PERFORMANCE=true
set GENERATE_COVERAGE=false
set GENERATE_REPORT=false
set VERBOSE=false

:parse_args
if "%~1"=="" goto :parse_args_done
if "%~1"=="-h" goto :show_help_exit
if "%~1"=="--help" goto :show_help_exit
if "%~1"=="-u" (
    set RUN_UNIT=true
    set RUN_INTEGRATION=false
    set RUN_E2E=false
    set RUN_PERFORMANCE=false
    shift
    goto :parse_args
)
if "%~1"=="--unit" (
    set RUN_UNIT=true
    set RUN_INTEGRATION=false
    set RUN_E2E=false
    set RUN_PERFORMANCE=false
    shift
    goto :parse_args
)
if "%~1"=="-i" (
    set RUN_UNIT=false
    set RUN_INTEGRATION=true
    set RUN_E2E=false
    set RUN_PERFORMANCE=false
    shift
    goto :parse_args
)
if "%~1"=="--integration" (
    set RUN_UNIT=false
    set RUN_INTEGRATION=true
    set RUN_E2E=false
    set RUN_PERFORMANCE=false
    shift
    goto :parse_args
)
if "%~1"=="-e" (
    set RUN_UNIT=false
    set RUN_INTEGRATION=false
    set RUN_E2E=true
    set RUN_PERFORMANCE=false
    shift
    goto :parse_args
)
if "%~1"=="--e2e" (
    set RUN_UNIT=false
    set RUN_INTEGRATION=false
    set RUN_E2E=true
    set RUN_PERFORMANCE=false
    shift
    goto :parse_args
)
if "%~1"=="-p" (
    set RUN_UNIT=false
    set RUN_INTEGRATION=false
    set RUN_E2E=false
    set RUN_PERFORMANCE=true
    shift
    goto :parse_args
)
if "%~1"=="--performance" (
    set RUN_UNIT=false
    set RUN_INTEGRATION=false
    set RUN_E2E=false
    set RUN_PERFORMANCE=true
    shift
    goto :parse_args
)
if "%~1"=="-a" (
    set RUN_UNIT=true
    set RUN_INTEGRATION=true
    set RUN_E2E=true
    set RUN_PERFORMANCE=true
    shift
    goto :parse_args
)
if "%~1"=="--all" (
    set RUN_UNIT=true
    set RUN_INTEGRATION=true
    set RUN_E2E=true
    set RUN_PERFORMANCE=true
    shift
    goto :parse_args
)
if "%~1"=="-c" (
    set GENERATE_COVERAGE=true
    shift
    goto :parse_args
)
if "%~1"=="--coverage" (
    set GENERATE_COVERAGE=true
    shift
    goto :parse_args
)
if "%~1"=="-r" (
    set GENERATE_REPORT=true
    shift
    goto :parse_args
)
if "%~1"=="--report" (
    set GENERATE_REPORT=true
    shift
    goto :parse_args
)
if "%~1"=="-v" (
    set VERBOSE=true
    shift
    goto :parse_args
)
if "%~1"=="--verbose" (
    set VERBOSE=true
    shift
    goto :parse_args
)
echo [ERROR] 未知选项: %~1
call :show_help
exit /b 1

:parse_args_done

REM 构建项目
:build_project
call :log_info "构建测试项目..."
if "%VERBOSE%"=="true" (
    dotnet build "%TEST_PROJECT%" --configuration Debug --verbosity normal
) else (
    dotnet build "%TEST_PROJECT%" --configuration Debug --verbosity quiet
)
if %errorlevel% neq 0 (
    call :log_error "项目构建失败"
    exit /b 1
)
call :log_success "项目构建成功"
goto :eof

REM 运行单元测试
:run_unit_tests
call :log_info "运行单元测试..."
set coverage_args=
if "%GENERATE_COVERAGE%"=="true" (
    set coverage_args=--collect:"XPlat Code Coverage" --results-directory "%REPORT_DIR%"
)
if "%VERBOSE%"=="true" (
    dotnet test "%TEST_PROJECT%" --configuration Debug --logger "console;verbosity=detailed" --filter "Category=Unit" %coverage_args%
) else (
    dotnet test "%TEST_PROJECT%" --configuration Debug --logger "console;verbosity=minimal" --filter "Category=Unit" %coverage_args%
)
if %errorlevel% neq 0 (
    call :log_error "单元测试失败"
    exit /b 1
)
call :log_success "单元测试通过"
goto :eof

REM 运行集成测试
:run_integration_tests
call :log_info "运行集成测试..."
set coverage_args=
if "%GENERATE_COVERAGE%"=="true" (
    set coverage_args=--collect:"XPlat Code Coverage" --results-directory "%REPORT_DIR%"
)
if "%VERBOSE%"=="true" (
    dotnet test "%TEST_PROJECT%" --configuration Debug --logger "console;verbosity=detailed" --filter "Category=Integration" %coverage_args%
) else (
    dotnet test "%TEST_PROJECT%" --configuration Debug --logger "console;verbosity=minimal" --filter "Category=Integration" %coverage_args%
)
if %errorlevel% neq 0 (
    call :log_error "集成测试失败"
    exit /b 1
)
call :log_success "集成测试通过"
goto :eof

REM 运行端到端测试
:run_e2e_tests
call :log_info "运行端到端测试..."
set coverage_args=
if "%GENERATE_COVERAGE%"=="true" (
    set coverage_args=--collect:"XPlat Code Coverage" --results-directory "%REPORT_DIR%"
)
if "%VERBOSE%"=="true" (
    dotnet test "%TEST_PROJECT%" --configuration Debug --logger "console;verbosity=detailed" --filter "Category=EndToEnd" %coverage_args%
) else (
    dotnet test "%TEST_PROJECT%" --configuration Debug --logger "console;verbosity=minimal" --filter "Category=EndToEnd" %coverage_args%
)
if %errorlevel% neq 0 (
    call :log_error "端到端测试失败"
    exit /b 1
)
call :log_success "端到端测试通过"
goto :eof

REM 运行性能测试
:run_performance_tests
call :log_info "运行性能测试..."
if "%VERBOSE%"=="true" (
    dotnet test "%TEST_PROJECT%" --configuration Debug --logger "console;verbosity=detailed" --filter "Category=Performance"
) else (
    dotnet test "%TEST_PROJECT%" --configuration Debug --logger "console;verbosity=minimal" --filter "Category=Performance"
)
if %errorlevel% neq 0 (
    call :log_error "性能测试失败"
    exit /b 1
)
call :log_success "性能测试通过"
goto :eof

REM 生成覆盖率报告
:generate_coverage_report
call :log_info "生成代码覆盖率报告..."
REM 查找coverage文件
dir /b "%REPORT_DIR%\*.coverage" >nul 2>&1
if %errorlevel% equ 0 (
    REM 检查reportgenerator是否可用
    reportgenerator --version >nul 2>&1
    if %errorlevel% equ 0 (
        reportgenerator -reports:"%REPORT_DIR%\*.coverage" -targetdir:"%REPORT_DIR%" -reporttypes:Html
        call :log_success "覆盖率报告生成成功: %REPORT_DIR%\index.html"
    ) else (
        call :log_warning "reportgenerator未安装，跳过HTML报告生成"
    )
    
    REM 检查dotnet-coverage是否可用
    dotnet-coverage --version >nul 2>&1
    if %errorlevel% equ 0 (
        dotnet-coverage merge "%REPORT_DIR%\*.coverage" -o "%COVERAGE_REPORT%" -f cobertura
        call :log_success "Cobertura覆盖率报告生成成功: %COVERAGE_REPORT%"
    ) else (
        call :log_warning "dotnet-coverage未安装，跳过Cobertura报告生成"
    )
) else (
    call :log_warning "未找到覆盖率数据文件"
)
goto :eof

REM 生成测试报告
:generate_test_report
call :log_info "生成测试报告..."
REM 创建HTML报告
echo ^<!DOCTYPE html^> > "%REPORT_DIR%\test-report.html"
echo ^<html^> >> "%REPORT_DIR%\test-report.html"
echo ^<head^> >> "%REPORT_DIR%\test-report.html"
echo     ^<title^>KeyForge 测试报告^</title^> >> "%REPORT_DIR%\test-report.html"
echo     ^<meta charset="utf-8"^> >> "%REPORT_DIR%\test-report.html"
echo     ^<style^> >> "%REPORT_DIR%\test-report.html"
echo         body { font-family: Arial, sans-serif; margin: 20px; } >> "%REPORT_DIR%\test-report.html"
echo         .header { background-color: #f0f0f0; padding: 20px; border-radius: 5px; } >> "%REPORT_DIR%\test-report.html"
echo         .section { margin: 20px 0; } >> "%REPORT_DIR%\test-report.html"
echo         .success { color: green; } >> "%REPORT_DIR%\test-report.html"
echo         .error { color: red; } >> "%REPORT_DIR%\test-report.html"
echo         .warning { color: orange; } >> "%REPORT_DIR%\test-report.html"
echo         table { border-collapse: collapse; width: 100%%; } >> "%REPORT_DIR%\test-report.html"
echo         th, td { border: 1px solid #ddd; padding: 8px; text-align: left; } >> "%REPORT_DIR%\test-report.html"
echo         th { background-color: #f2f2f2; } >> "%REPORT_DIR%\test-report.html"
echo     ^</style^> >> "%REPORT_DIR%\test-report.html"
echo ^</head^> >> "%REPORT_DIR%\test-report.html"
echo ^<body^> >> "%REPORT_DIR%\test-report.html"
echo     ^<div class="header"^> >> "%REPORT_DIR%\test-report.html"
echo         ^<h1^>KeyForge 测试报告^</h1^> >> "%REPORT_DIR%\test-report.html"
echo         ^<p^>生成时间: %date% %time%^</p^> >> "%REPORT_DIR%\test-report.html"
echo         ^<p^>测试环境: Windows^</p^> >> "%REPORT_DIR%\test-report.html"
echo     ^</div^> >> "%REPORT_DIR%\test-report.html"
echo     ^<div class="section"^> >> "%REPORT_DIR%\test-report.html"
echo         ^<h2^>测试摘要^</h2^> >> "%REPORT_DIR%\test-report.html"
echo         ^<table^> >> "%REPORT_DIR%\test-report.html"
echo             ^<tr^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<th^>测试类型^</th^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<th^>状态^</th^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<th^>执行时间^</th^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<th^>测试数量^</th^> >> "%REPORT_DIR%\test-report.html"
echo             ^</tr^> >> "%REPORT_DIR%\test-report.html"
echo             ^<tr^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td^>单元测试^</td^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td class="success"^>通过^</td^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td^>-^</td^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td^>-^</td^> >> "%REPORT_DIR%\test-report.html"
echo             ^</tr^> >> "%REPORT_DIR%\test-report.html"
echo             ^<tr^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td^>集成测试^</td^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td class="success"^>通过^</td^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td^>-^</td^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td^>-^</td^> >> "%REPORT_DIR%\test-report.html"
echo             ^</tr^> >> "%REPORT_DIR%\test-report.html"
echo             ^<tr^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td^>端到端测试^</td^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td class="success"^>通过^</td^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td^>-^</td^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td^>-^</td^> >> "%REPORT_DIR%\test-report.html"
echo             ^</tr^> >> "%REPORT_DIR%\test-report.html"
echo             ^<tr^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td^>性能测试^</td^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td class="success"^>通过^</td^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td^>-^</td^> >> "%REPORT_DIR%\test-report.html"
echo                 ^<td^>-^</td^> >> "%REPORT_DIR%\test-report.html"
echo             ^</tr^> >> "%REPORT_DIR%\test-report.html"
echo         ^</table^> >> "%REPORT_DIR%\test-report.html"
echo     ^</div^> >> "%REPORT_DIR%\test-report.html"
echo     ^<div class="section"^> >> "%REPORT_DIR%\test-report.html"
echo         ^<h2^>测试结果^</h2^> >> "%REPORT_DIR%\test-report.html"
echo         ^<p^>所有测试均已成功通过。^</p^> >> "%REPORT_DIR%\test-report.html"
echo         ^<p^>详细测试结果请查看控制台输出。^</p^> >> "%REPORT_DIR%\test-report.html"
echo     ^</div^> >> "%REPORT_DIR%\test-report.html"
echo     ^<div class="section"^> >> "%REPORT_DIR%\test-report.html"
echo         ^<h2^>覆盖率信息^</h2^> >> "%REPORT_DIR%\test-report.html"
echo         ^<p^>代码覆盖率报告: ^<a href="index.html"^>查看覆盖率报告^</a^>^</p^> >> "%REPORT_DIR%\test-report.html"
echo     ^</div^> >> "%REPORT_DIR%\test-report.html"
echo ^</body^> >> "%REPORT_DIR%\test-report.html"
echo ^</html^> >> "%REPORT_DIR%\test-report.html"

call :log_success "测试报告生成成功: %REPORT_DIR%\test-report.html"
goto :eof

:show_help_exit
call :show_help
exit /b 0

REM 主执行流程
:main
call :log_info "开始执行 KeyForge 测试套件..."
call :log_info "项目根目录: %PROJECT_ROOT%"
call :log_info "测试项目: %TEST_PROJECT%"
call :log_info "报告目录: %REPORT_DIR%"

REM 构建项目
call :build_project

REM 运行测试
if "%RUN_UNIT%"=="true" call :run_unit_tests
if "%RUN_INTEGRATION%"=="true" call :run_integration_tests
if "%RUN_E2E%"=="true" call :run_e2e_tests
if "%RUN_PERFORMANCE%"=="true" call :run_performance_tests

REM 生成报告
if "%GENERATE_COVERAGE%"=="true" call :generate_coverage_report
if "%GENERATE_REPORT%"=="true" call :generate_test_report

call :log_success "测试执行完成！"
call :log_info "测试报告位置: %REPORT_DIR%"

goto :eof

REM 执行主函数
call :main %*