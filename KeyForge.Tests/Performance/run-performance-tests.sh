#!/bin/bash

# KeyForge 性能测试运行脚本
# 此脚本用于自动化运行所有性能测试

set -e  # 遇到错误时退出

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

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 检查前置条件
check_prerequisites() {
    log_info "检查前置条件..."
    
    # 检查.NET SDK
    if ! command -v dotnet &> /dev/null; then
        log_error "未找到 .NET SDK，请先安装 .NET 8.0 SDK"
        exit 1
    fi
    
    # 检查.NET版本
    local dotnet_version=$(dotnet --version)
    log_info "检测到 .NET 版本: $dotnet_version"
    
    if [[ ! $dotnet_version =~ ^8\. ]]; then
        log_warning "建议使用 .NET 8.0 SDK，当前版本: $dotnet_version"
    fi
    
    # 检查项目文件
    if [[ ! -f "KeyForge.Tests.Performance.csproj" ]]; then
        log_error "未找到项目文件 KeyForge.Tests.Performance.csproj"
        exit 1
    fi
    
    # 检查配置文件
    if [[ ! -f "PerformanceSettings.json" ]]; then
        log_warning "未找到配置文件 PerformanceSettings.json，将使用默认配置"
    fi
    
    log_success "前置条件检查完成"
}

# 创建输出目录
create_output_directories() {
    log_info "创建输出目录..."
    
    mkdir -p PerformanceReports
    mkdir -p BenchmarkDotNet.Artifacts
    mkdir -p logs
    
    log_success "输出目录创建完成"
}

# 清理之前的测试结果
cleanup_previous_results() {
    log_info "清理之前的测试结果..."
    
    # 清理编译输出
    dotnet clean -v quiet
    
    # 清理BenchmarkDotNet输出
    rm -rf BenchmarkDotNet.Artifacts/*
    
    # 清理日志文件
    rm -f logs/*.log
    
    # 清理性能报告
    rm -f PerformanceReports/*
    
    log_success "清理完成"
}

# 构建项目
build_project() {
    log_info "构建项目..."
    
    # 恢复依赖
    dotnet restore
    
    # 构建项目
    dotnet build --configuration Release
    
    if [[ $? -eq 0 ]]; then
        log_success "项目构建成功"
    else
        log_error "项目构建失败"
        exit 1
    fi
}

# 运行快速性能检查
run_quick_test() {
    log_info "运行快速性能检查..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- quick > logs/quick-test.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "快速性能检查完成，耗时: ${duration}s"
    else
        log_error "快速性能检查失败"
        log_error "查看日志: logs/quick-test.log"
        return 1
    fi
}

# 运行基准测试
run_benchmark_tests() {
    log_info "运行基准测试..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- benchmark > logs/benchmark-test.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "基准测试完成，耗时: ${duration}s"
        log_info "基准测试结果保存在 BenchmarkDotNet.Artifacts/ 目录"
    else
        log_error "基准测试失败"
        log_error "查看日志: logs/benchmark-test.log"
        return 1
    fi
}

# 运行负载测试
run_load_tests() {
    log_info "运行负载测试..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- load > logs/load-test.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "负载测试完成，耗时: ${duration}s"
    else
        log_error "负载测试失败"
        log_error "查看日志: logs/load-test.log"
        return 1
    fi
}

# 运行内存分析测试
run_memory_tests() {
    log_info "运行内存分析测试..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- memory > logs/memory-test.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "内存分析测试完成，耗时: ${duration}s"
    else
        log_error "内存分析测试失败"
        log_error "查看日志: logs/memory-test.log"
        return 1
    fi
}

# 运行压力测试
run_stress_tests() {
    log_info "运行压力测试..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- stress > logs/stress-test.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "压力测试完成，耗时: ${duration}s"
    else
        log_error "压力测试失败"
        log_error "查看日志: logs/stress-test.log"
        return 1
    fi
}

# 运行所有测试
run_all_tests() {
    log_info "运行所有性能测试..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- all > logs/all-tests.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "所有性能测试完成，耗时: ${duration}s"
    else
        log_error "性能测试失败"
        log_error "查看日志: logs/all-tests.log"
        return 1
    fi
}

# 生成报告
generate_report() {
    log_info "生成性能测试报告..."
    
    dotnet run --configuration Release -- report > logs/report-generation.log 2>&1
    
    if [[ $? -eq 0 ]]; then
        log_success "性能测试报告生成完成"
        log_info "报告保存在 PerformanceReports/ 目录"
    else
        log_error "报告生成失败"
        log_error "查看日志: logs/report-generation.log"
        return 1
    fi
}

# 显示系统信息
show_system_info() {
    log_info "系统信息:"
    log_info "  操作系统: $(uname -s)"
    log_info "  内核版本: $(uname -r)"
    log_info "  架构: $(uname -m)"
    log_info "  内存: $(free -h | grep Mem | awk '{print $2}')"
    log_info "  CPU核心数: $(nproc)"
    log_info "  .NET版本: $(dotnet --version)"
}

# 显示帮助信息
show_help() {
    echo "KeyForge 性能测试运行脚本"
    echo "=============================="
    echo ""
    echo "用法: $0 [选项]"
    echo ""
    echo "选项:"
    echo "  quick       运行快速性能检查"
    echo "  benchmark   运行基准测试"
    echo "  load        运行负载测试"
    echo "  memory      运行内存分析测试"
    echo "  stress      运行压力测试"
    echo "  all         运行所有性能测试"
    echo "  report      生成性能测试报告"
    echo "  full        运行完整测试套件"
    echo "  clean       清理之前的测试结果"
    echo "  help        显示此帮助信息"
    echo ""
    echo "示例:"
    echo "  $0 quick          # 运行快速性能检查"
    echo "  $0 all            # 运行所有性能测试"
    echo "  $0 full           # 运行完整测试套件"
    echo ""
}

# 主函数
main() {
    local option=${1:-help}
    
    case $option in
        "quick")
            check_prerequisites
            create_output_directories
            build_project
            run_quick_test
            ;;
        "benchmark")
            check_prerequisites
            create_output_directories
            build_project
            run_benchmark_tests
            ;;
        "load")
            check_prerequisites
            create_output_directories
            build_project
            run_load_tests
            ;;
        "memory")
            check_prerequisites
            create_output_directories
            build_project
            run_memory_tests
            ;;
        "stress")
            check_prerequisites
            create_output_directories
            build_project
            run_stress_tests
            ;;
        "all")
            check_prerequisites
            create_output_directories
            build_project
            run_all_tests
            ;;
        "report")
            check_prerequisites
            create_output_directories
            build_project
            generate_report
            ;;
        "full")
            show_system_info
            check_prerequisites
            create_output_directories
            cleanup_previous_results
            build_project
            run_quick_test
            run_benchmark_tests
            run_load_tests
            run_memory_tests
            run_stress_tests
            run_all_tests
            generate_report
            log_success "完整测试套件执行完成"
            ;;
        "clean")
            cleanup_previous_results
            log_success "清理完成"
            ;;
        "help"|"-h"|"--help")
            show_help
            ;;
        *)
            log_error "未知选项: $option"
            show_help
            exit 1
            ;;
    esac
}

# 脚本入口点
main "$@"