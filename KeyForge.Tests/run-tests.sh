#!/bin/bash

# KeyForge 测试运行脚本
# 原本实现：复杂的测试运行脚本
# 简化实现：基础的测试运行功能

set -e  # 遇到错误时退出

# 默认参数
CONFIGURATION="Debug"
TEST_CATEGORY="All"
FILTER=""
NO_COVERAGE=false
NO_REPORT=false
VERBOSE=false
CLEAN=false
HELP=false

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 显示帮助信息
show_help() {
    echo -e "${BLUE}KeyForge 测试运行脚本${NC}"
    echo ""
    echo "用法: $0 [选项]"
    echo ""
    echo "选项:"
    echo "  -c, --configuration <string>    构建配置 (Debug/Release) [默认: Debug]"
    echo "  -t, --category <string>         测试类别 (All/Unit/Integration/Performance/E2E) [默认: All]"
    echo "  -f, --filter <string>           测试过滤器"
    echo "  -n, --no-coverage              不收集代码覆盖率"
    echo "  -r, --no-report                不生成测试报告"
    echo "  -v, --verbose                   详细输出"
    echo "  -C, --clean                    清理之前的测试结果"
    echo "  -h, --help                     显示此帮助信息"
    echo ""
    echo "示例:"
    echo "  $0 -t Unit -v"
    echo "  $0 -c Release -n"
    echo "  $0 -f 'Performance' -C"
    exit 0
}

# 解析命令行参数
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -t|--category)
            TEST_CATEGORY="$2"
            shift 2
            ;;
        -f|--filter)
            FILTER="$2"
            shift 2
            ;;
        -n|--no-coverage)
            NO_COVERAGE=true
            shift
            ;;
        -r|--no-report)
            NO_REPORT=true
            shift
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -C|--clean)
            CLEAN=true
            shift
            ;;
        -h|--help)
            HELP=true
            shift
            ;;
        *)
            echo -e "${RED}未知选项: $1${NC}"
            show_help
            exit 1
            ;;
    esac
done

# 显示帮助信息
if [ "$HELP" = true ]; then
    show_help
fi

# 设置脚本路径
SCRIPT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_PATH")"
TEST_PROJECT_PATH="$PROJECT_ROOT/KeyForge.Tests"
RESULTS_PATH="$SCRIPT_PATH/TestResults"

# 创建结果目录
mkdir -p "$RESULTS_PATH"

# 日志函数
log() {
    local level=$1
    shift
    local message=$*
    local timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    local log_message="[$timestamp] [$level] $message"
    
    echo -e "$log_message"
    
    # 写入日志文件
    echo "$log_message" >> "$RESULTS_PATH/test-run.log"
}

log_info() {
    log "INFO" "$@"
}

log_verbose() {
    if [ "$VERBOSE" = true ]; then
        log "VERBOSE" "$@"
    fi
}

log_error() {
    log "ERROR" "$@"
}

log_success() {
    log "SUCCESS" "$@"
}

log_warning() {
    log "WARNING" "$@"
}

# 清理之前的测试结果
if [ "$CLEAN" = true ]; then
    log_info "清理之前的测试结果..."
    rm -rf "$RESULTS_PATH"
    mkdir -p "$RESULTS_PATH"
    log_info "清理完成"
fi

# 测试类别配置
declare -A TEST_CATEGORY_CONFIG
TEST_CATEGORY_CONFIG["All"]="**/UnitTests/**/*.cs **/IntegrationTests/**/*.cs **/PerformanceTests/**/*..cs **/EndToEndTests/**/*.cs|所有测试"
TEST_CATEGORY_CONFIG["Unit"]="**/UnitTests/**/*.cs|单元测试"
TEST_CATEGORY_CONFIG["Integration"]="**/IntegrationTests/**/*.cs|集成测试"
TEST_CATEGORY_CONFIG["Performance"]="**/PerformanceTests/**/*.cs|性能测试"
TEST_CATEGORY_CONFIG["E2E"]="**/EndToEndTests/**/*.cs|端到端测试"

# 验证测试类别
if [[ -z "${TEST_CATEGORY_CONFIG[$TEST_CATEGORY]}" ]]; then
    log_error "无效的测试类别: $TEST_CATEGORY"
    log_error "有效的测试类别: ${!TEST_CATEGORY_CONFIG[@]}"
    exit 1
fi

# 获取测试配置
TEST_CONFIG_STRING="${TEST_CATEGORY_CONFIG[$TEST_CATEGORY]}"
INCLUDE_PATTERNS=$(echo "$TEST_CONFIG_STRING" | cut -d'|' -f1)
DESCRIPTION=$(echo "$TEST_CONFIG_STRING" | cut -d'|' -f2)

log_info "运行测试类别: $DESCRIPTION"

# 设置dotnet测试参数
TEST_ARGS=(
    "test"
    "$TEST_PROJECT_PATH/KeyForge.Tests.csproj"
    "--configuration"
    "$CONFIGURATION"
    "--logger"
    "console;verbosity=normal"
    "--logger"
    "trx;LogFileName=$RESULTS_PATH/test-results.trx"
    "--results-directory"
    "$RESULTS_PATH"
)

# 添加测试过滤器
if [ -n "$FILTER" ]; then
    TEST_ARGS+=("--filter" "$FILTER")
    log_info "测试过滤器: $FILTER"
fi

# 添加包含模式（使用 --filter 替代 --test）
for pattern in $INCLUDE_PATTERNS; do
    # 将文件模式转换为命名空间过滤器
    if [[ "$pattern" == **/UnitTests/**/*.cs ]]; then
        TEST_ARGS+=("--filter" "FullyQualifiedName~UnitTests")
    elif [[ "$pattern" == **/IntegrationTests/**/*.cs ]]; then
        TEST_ARGS+=("--filter" "FullyQualifiedName~IntegrationTests")
    elif [[ "$pattern" == **/PerformanceTests/**/*.cs ]]; then
        TEST_ARGS+=("--filter" "FullyQualifiedName~PerformanceTests")
    elif [[ "$pattern" == **/EndToEndTests/**/*.cs ]]; then
        TEST_ARGS+=("--filter" "FullyQualifiedName~EndToEndTests")
    fi
done

# 添加代码覆盖率
if [ "$NO_COVERAGE" = false ]; then
    TEST_ARGS+=("--collect" "XPlat Code Coverage")
    log_info "启用代码覆盖率收集"
fi

# 添加详细输出
if [ "$VERBOSE" = true ]; then
    TEST_ARGS+=("--verbosity" "detailed")
    log_info "启用详细输出"
fi

# 运行测试
log_info "开始运行测试..."
log_verbose "测试命令: dotnet ${TEST_ARGS[*]}"

START_TIME=$(date +%s)

# 捕获退出代码
set +e
dotnet "${TEST_ARGS[@]}"
EXIT_CODE=$?
set -e

END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))

log_info "测试完成，耗时: $DURATION 秒"

if [ $EXIT_CODE -eq 0 ]; then
    log_success "所有测试通过!"
else
    log_error "有测试失败，退出代码: $EXIT_CODE"
fi

# 生成测试报告
if [ "$NO_REPORT" = false ]; then
    log_info "生成测试报告..."
    if [ -f "$SCRIPT_PATH/generate-report.sh" ]; then
        bash "$SCRIPT_PATH/generate-report.sh" \
            --results-path "$RESULTS_PATH" \
            --configuration "$CONFIGURATION" \
            --test-category "$TEST_CATEGORY"
    else
        log_warning "报告生成脚本不存在: $SCRIPT_PATH/generate-report.sh"
    fi
fi

# 显示测试结果摘要
log_info "测试结果摘要:"
log_info "  - 测试类别: $DESCRIPTION"
log_info "  - 配置: $CONFIGURATION"
log_info "  - 耗时: $DURATION 秒"
log_info "  - 退出代码: $EXIT_CODE"
log_info "  - 结果目录: $RESULTS_PATH"

if [ "$NO_COVERAGE" = false ]; then
    COVERAGE_FILE="$RESULTS_PATH/coverage.cobertura.xml"
    if [ -f "$COVERAGE_FILE" ]; then
        log_info "  - 覆盖率报告: $COVERAGE_FILE"
    fi
fi

# 检查是否有测试失败
if [ $EXIT_CODE -ne 0 ]; then
    log_error "测试运行失败，请检查日志文件: $RESULTS_PATH/test-run.log"
    exit $EXIT_CODE
fi

log_success "测试运行完成!"