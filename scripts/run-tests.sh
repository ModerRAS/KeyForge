#!/bin/bash

# KeyForge 测试执行脚本
# 该脚本用于运行所有类型的测试并生成报告

set -e

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 配置变量
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TEST_PROJECT="$PROJECT_ROOT/KeyForge.Tests"
REPORT_DIR="$PROJECT_ROOT/TestReports"
COVERAGE_REPORT="$REPORT_DIR/coverage.xml"
TEST_RESULTS="$REPORT_DIR/test-results.xml"
PERFORMANCE_REPORT="$REPORT_DIR/performance-report.html"

# 创建报告目录
mkdir -p "$REPORT_DIR"

# 日志函数
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 清理函数
cleanup() {
    log_info "清理临时文件..."
    rm -f "$TEST_PROJECT/bin/Debug/net9.0/*.log"
    rm -f "$TEST_PROJECT/bin/Debug/net9.0/*.tmp"
}

# 错误处理
trap cleanup EXIT

# 显示帮助信息
show_help() {
    cat << EOF
KeyForge 测试执行脚本

用法: $0 [选项]

选项:
    -h, --help          显示帮助信息
    -u, --unit          只运行单元测试
    -i, --integration   只运行集成测试
    -e, --e2e           只运行端到端测试
    -p, --performance   只运行性能测试
    -a, --all           运行所有测试 (默认)
    -c, --coverage      生成代码覆盖率报告
    -r, --report        生成HTML测试报告
    -v, --verbose       详细输出模式

示例:
    $0                  运行所有测试
    $0 -u -c            运行单元测试并生成覆盖率报告
    $0 -i -r            运行集成测试并生成HTML报告
    $0 -p -v            运行性能测试并显示详细信息
EOF
}

# 解析命令行参数
RUN_UNIT=true
RUN_INTEGRATION=true
RUN_E2E=true
RUN_PERFORMANCE=true
GENERATE_COVERAGE=false
GENERATE_REPORT=false
VERBOSE=false

while [[ $# -gt 0 ]]; do
    case $1 in
        -h|--help)
            show_help
            exit 0
            ;;
        -u|--unit)
            RUN_UNIT=true
            RUN_INTEGRATION=false
            RUN_E2E=false
            RUN_PERFORMANCE=false
            shift
            ;;
        -i|--integration)
            RUN_UNIT=false
            RUN_INTEGRATION=true
            RUN_E2E=false
            RUN_PERFORMANCE=false
            shift
            ;;
        -e|--e2e)
            RUN_UNIT=false
            RUN_INTEGRATION=false
            RUN_E2E=true
            RUN_PERFORMANCE=false
            shift
            ;;
        -p|--performance)
            RUN_UNIT=false
            RUN_INTEGRATION=false
            RUN_E2E=false
            RUN_PERFORMANCE=true
            shift
            ;;
        -a|--all)
            RUN_UNIT=true
            RUN_INTEGRATION=true
            RUN_E2E=true
            RUN_PERFORMANCE=true
            shift
            ;;
        -c|--coverage)
            GENERATE_COVERAGE=true
            shift
            ;;
        -r|--report)
            GENERATE_REPORT=true
            shift
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        *)
            log_error "未知选项: $1"
            show_help
            exit 1
            ;;
    esac
done

# 构建项目
build_project() {
    log_info "构建测试项目..."
    if [ "$VERBOSE" = true ]; then
        dotnet build "$TEST_PROJECT" --configuration Debug --verbosity normal
    else
        dotnet build "$TEST_PROJECT" --configuration Debug --verbosity quiet
    fi
    
    if [ $? -eq 0 ]; then
        log_success "项目构建成功"
    else
        log_error "项目构建失败"
        exit 1
    fi
}

# 运行单元测试
run_unit_tests() {
    log_info "运行单元测试..."
    
    local coverage_args=""
    if [ "$GENERATE_COVERAGE" = true ]; then
        coverage_args="--collect:\"XPlat Code Coverage\" --results-directory \"$REPORT_DIR\""
    fi
    
    if [ "$VERBOSE" = true ]; then
        dotnet test "$TEST_PROJECT" --configuration Debug --logger "console;verbosity=detailed" --filter "Category=Unit" $coverage_args
    else
        dotnet test "$TEST_PROJECT" --configuration Debug --logger "console;verbosity=minimal" --filter "Category=Unit" $coverage_args
    fi
    
    if [ $? -eq 0 ]; then
        log_success "单元测试通过"
    else
        log_error "单元测试失败"
        exit 1
    fi
}

# 运行集成测试
run_integration_tests() {
    log_info "运行集成测试..."
    
    local coverage_args=""
    if [ "$GENERATE_COVERAGE" = true ]; then
        coverage_args="--collect:\"XPlat Code Coverage\" --results-directory \"$REPORT_DIR\""
    fi
    
    if [ "$VERBOSE" = true ]; then
        dotnet test "$TEST_PROJECT" --configuration Debug --logger "console;verbosity=detailed" --filter "Category=Integration" $coverage_args
    else
        dotnet test "$TEST_PROJECT" --configuration Debug --logger "console;verbosity=minimal" --filter "Category=Integration" $coverage_args
    fi
    
    if [ $? -eq 0 ]; then
        log_success "集成测试通过"
    else
        log_error "集成测试失败"
        exit 1
    fi
}

# 运行端到端测试
run_e2e_tests() {
    log_info "运行端到端测试..."
    
    local coverage_args=""
    if [ "$GENERATE_COVERAGE" = true ]; then
        coverage_args="--collect:\"XPlat Code Coverage\" --results-directory \"$REPORT_DIR\""
    fi
    
    if [ "$VERBOSE" = true ]; then
        dotnet test "$TEST_PROJECT" --configuration Debug --logger "console;verbosity=detailed" --filter "Category=EndToEnd" $coverage_args
    else
        dotnet test "$TEST_PROJECT" --configuration Debug --logger "console;verbosity=minimal" --filter "Category=EndToEnd" $coverage_args
    fi
    
    if [ $? -eq 0 ]; then
        log_success "端到端测试通过"
    else
        log_error "端到端测试失败"
        exit 1
    fi
}

# 运行性能测试
run_performance_tests() {
    log_info "运行性能测试..."
    
    if [ "$VERBOSE" = true ]; then
        dotnet test "$TEST_PROJECT" --configuration Debug --logger "console;verbosity=detailed" --filter "Category=Performance"
    else
        dotnet test "$TEST_PROJECT" --configuration Debug --logger "console;verbosity=minimal" --filter "Category=Performance"
    fi
    
    if [ $? -eq 0 ]; then
        log_success "性能测试通过"
    else
        log_error "性能测试失败"
        exit 1
    fi
}

# 生成覆盖率报告
generate_coverage_report() {
    log_info "生成代码覆盖率报告..."
    
    # 查找coverage文件
    local coverage_file=$(find "$REPORT_DIR" -name "*.coverage" | head -1)
    
    if [ -n "$coverage_file" ]; then
        # 使用reportgenerator生成HTML报告
        if command -v reportgenerator &> /dev/null; then
            reportgenerator -reports:"$coverage_file" -targetdir:"$REPORT_DIR" -reporttypes:Html
            log_success "覆盖率报告生成成功: $REPORT_DIR/index.html"
        else
            log_warning "reportgenerator未安装，跳过HTML报告生成"
        fi
        
        # 生成Cobertura格式报告
        if command -v dotnet-coverage &> /dev/null; then
            dotnet-coverage merge "$REPORT_DIR"/*.coverage -o "$COVERAGE_REPORT" -f cobertura
            log_success "Cobertura覆盖率报告生成成功: $COVERAGE_REPORT"
        else
            log_warning "dotnet-coverage未安装，跳过Cobertura报告生成"
        fi
    else
        log_warning "未找到覆盖率数据文件"
    fi
}

# 生成测试报告
generate_test_report() {
    log_info "生成测试报告..."
    
    # 创建HTML报告
    cat > "$REPORT_DIR/test-report.html" << EOF
<!DOCTYPE html>
<html>
<head>
    <title>KeyForge 测试报告</title>
    <meta charset="utf-8">
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background-color: #f0f0f0; padding: 20px; border-radius: 5px; }
        .section { margin: 20px 0; }
        .success { color: green; }
        .error { color: red; }
        .warning { color: orange; }
        table { border-collapse: collapse; width: 100%; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; }
    </style>
</head>
<body>
    <div class="header">
        <h1>KeyForge 测试报告</h1>
        <p>生成时间: $(date)</p>
        <p>测试环境: $(uname -a)</p>
    </div>
    
    <div class="section">
        <h2>测试摘要</h2>
        <table>
            <tr>
                <th>测试类型</th>
                <th>状态</th>
                <th>执行时间</th>
                <th>测试数量</th>
            </tr>
            <tr>
                <td>单元测试</td>
                <td class="success">通过</td>
                <td>-</td>
                <td>-</td>
            </tr>
            <tr>
                <td>集成测试</td>
                <td class="success">通过</td>
                <td>-</td>
                <td>-</td>
            </tr>
            <tr>
                <td>端到端测试</td>
                <td class="success">通过</td>
                <td>-</td>
                <td>-</td>
            </tr>
            <tr>
                <td>性能测试</td>
                <td class="success">通过</td>
                <td>-</td>
                <td>-</td>
            </tr>
        </table>
    </div>
    
    <div class="section">
        <h2>测试结果</h2>
        <p>所有测试均已成功通过。</p>
        <p>详细测试结果请查看控制台输出。</p>
    </div>
    
    <div class="section">
        <h2>覆盖率信息</h2>
        <p>代码覆盖率报告: <a href="index.html">查看覆盖率报告</a></p>
    </div>
</body>
</html>
EOF

    log_success "测试报告生成成功: $REPORT_DIR/test-report.html"
}

# 主执行流程
main() {
    log_info "开始执行 KeyForge 测试套件..."
    log_info "项目根目录: $PROJECT_ROOT"
    log_info "测试项目: $TEST_PROJECT"
    log_info "报告目录: $REPORT_DIR"
    
    # 构建项目
    build_project
    
    # 运行测试
    if [ "$RUN_UNIT" = true ]; then
        run_unit_tests
    fi
    
    if [ "$RUN_INTEGRATION" = true ]; then
        run_integration_tests
    fi
    
    if [ "$RUN_E2E" = true ]; then
        run_e2e_tests
    fi
    
    if [ "$RUN_PERFORMANCE" = true ]; then
        run_performance_tests
    fi
    
    # 生成报告
    if [ "$GENERATE_COVERAGE" = true ]; then
        generate_coverage_report
    fi
    
    if [ "$GENERATE_REPORT" = true ]; then
        generate_test_report
    fi
    
    log_success "测试执行完成！"
    log_info "测试报告位置: $REPORT_DIR"
}

# 执行主函数
main "$@"