#!/bin/bash

# KeyForge 简化测试运行脚本
# 只运行基本的单元测试

set -e

# 配置变量
SCRIPT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_PATH")"
TEST_PROJECT_PATH="$SCRIPT_PATH"
RESULTS_PATH="$SCRIPT_PATH/TestResults"
CONFIGURATION="Debug"

# 创建结果目录
mkdir -p "$RESULTS_PATH"

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 日志函数
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# 清理之前的测试结果
log_info "清理之前的测试结果..."
rm -rf "$RESULTS_PATH"/*
mkdir -p "$RESULTS_PATH"

log_info "开始运行简化测试..."

# 运行测试
START_TIME=$(date +%s)

if dotnet test "$TEST_PROJECT_PATH/KeyForge.Tests.Simple.csproj" \
    --configuration "$CONFIGURATION" \
    --logger "console;verbosity=normal" \
    --logger "trx;LogFileName=$RESULTS_PATH/test-results.trx" \
    --results-directory "$RESULTS_PATH" \
    --no-build; then
    
    END_TIME=$(date +%s)
    DURATION=$((END_TIME - START_TIME))
    
    log_success "简化测试运行完成！"
    log_info "耗时: $DURATION 秒"
    
    # 生成报告
    log_info "生成测试报告..."
    if [ -f "$SCRIPT_PATH/generate-report.sh" ]; then
        bash "$SCRIPT_PATH/generate-report.sh" \
            --results-path "$RESULTS_PATH" \
            --configuration "$CONFIGURATION" \
            --test-category "Simple"
    fi
    
    # 显示结果
    log_info "测试结果目录: $RESULTS_PATH"
    
    exit 0
else
    EXIT_CODE=$?
    END_TIME=$(date +%s)
    DURATION=$((END_TIME - START_TIME))
    
    log_error "简化测试运行失败，退出代码: $EXIT_CODE"
    log_info "耗时: $DURATION 秒"
    
    exit $EXIT_CODE
fi