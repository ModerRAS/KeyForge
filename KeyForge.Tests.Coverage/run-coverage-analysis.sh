#!/bin/bash

# KeyForge 测试覆盖率分析运行脚本
# 此脚本用于自动化运行测试覆盖率分析

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
    if [[ ! -f "KeyForge.Tests.Coverage.csproj" ]]; then
        log_error "未找到项目文件 KeyForge.Tests.Coverage.csproj"
        exit 1
    fi
    
    # 检查配置文件
    if [[ ! -f "coverage-settings.json" ]]; then
        log_warning "未找到配置文件 coverage-settings.json，将使用默认配置"
    fi
    
    log_success "前置条件检查完成"
}

# 创建输出目录
create_output_directories() {
    log_info "创建输出目录..."
    
    mkdir -p coverage
    mkdir -p coverage/report
    mkdir -p coverage/history
    mkdir -p coverage/details
    mkdir -p logs
    
    log_success "输出目录创建完成"
}

# 清理之前的测试结果
cleanup_previous_results() {
    log_info "清理之前的测试结果..."
    
    # 清理编译输出
    dotnet clean -v quiet
    
    # 清理覆盖率数据
    rm -rf coverage/*.xml
    rm -rf coverage/*.json
    rm -rf coverage/report/*
    
    # 清理日志文件
    rm -f logs/*.log
    
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

# 运行覆盖率分析
run_coverage_analysis() {
    log_info "运行覆盖率分析..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- run > logs/coverage-analysis.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "覆盖率分析完成，耗时: ${duration}s"
    else
        log_error "覆盖率分析失败"
        log_error "查看日志: logs/coverage-analysis.log"
        return 1
    fi
}

# 生成覆盖率报告
generate_coverage_report() {
    log_info "生成覆盖率报告..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- report > logs/report-generation.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "覆盖率报告生成完成，耗时: ${duration}s"
        log_info "报告位置: coverage/report/"
    else
        log_error "覆盖率报告生成失败"
        log_error "查看日志: logs/report-generation.log"
        return 1
    fi
}

# 验证覆盖率阈值
validate_coverage_thresholds() {
    log_info "验证覆盖率阈值..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- validate > logs/coverage-validation.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "覆盖率阈值验证通过，耗时: ${duration}s"
    else
        log_error "覆盖率阈值验证失败"
        log_error "查看日志: logs/coverage-validation.log"
        return 1
    fi
}

# 显示覆盖率建议
show_coverage_recommendations() {
    log_info "显示覆盖率建议..."
    
    dotnet run --configuration Release -- recommendations > logs/coverage-recommendations.log 2>&1
    
    if [[ $? -eq 0 ]]; then
        log_success "覆盖率建议生成完成"
    else
        log_error "覆盖率建议生成失败"
        log_error "查看日志: logs/coverage-recommendations.log"
        return 1
    fi
}

# 显示覆盖率历史
show_coverage_history() {
    log_info "显示覆盖率历史..."
    
    dotnet run --configuration Release -- history > logs/coverage-history.log 2>&1
    
    if [[ $? -eq 0 ]]; then
        log_success "覆盖率历史显示完成"
    else
        log_error "覆盖率历史显示失败"
        log_error "查看日志: logs/coverage-history.log"
        return 1
    fi
}

# 比较覆盖率数据
compare_coverage_data() {
    log_info "比较覆盖率数据..."
    
    dotnet run --configuration Release -- compare > logs/coverage-comparison.log 2>&1
    
    if [[ $? -eq 0 ]]; then
        log_success "覆盖率数据比较完成"
    else
        log_error "覆盖率数据比较失败"
        log_error "查看日志: logs/coverage-comparison.log"
        return 1
    fi
}

# 显示配置信息
show_configuration() {
    log_info "显示配置信息..."
    
    dotnet run --configuration Release -- config
}

# 显示阈值设置
show_thresholds() {
    log_info "显示阈值设置..."
    
    dotnet run --configuration Release -- thresholds
}

# 保存覆盖率历史
save_coverage_history() {
    log_info "保存覆盖率历史..."
    
    local history_dir="coverage/history"
    local timestamp=$(date +%Y%m%d_%H%M%S)
    local history_file="$history_dir/coverage_$timestamp.json"
    
    # 查找最新的覆盖率数据
    local latest_coverage=$(find coverage -name "*.json" -type f -newer coverage/history 2>/dev/null | head -1)
    
    if [[ -n "$latest_coverage" ]]; then
        cp "$latest_coverage" "$history_file"
        log_success "覆盖率历史已保存: $history_file"
        
        # 清理旧的历史文件（保留最近30天）
        find "$history_dir" -name "*.json" -type f -mtime +30 -delete 2>/dev/null || true
    else
        log_warning "未找到最新的覆盖率数据"
    fi
}

# 生成覆盖率徽章
generate_coverage_badge() {
    log_info "生成覆盖率徽章..."
    
    local badge_file="coverage/coverage.svg"
    
    if [[ -f "coverage/report/coverage.svg" ]]; then
        cp "coverage/report/coverage.svg" "$badge_file"
        log_success "覆盖率徽章已生成: $badge_file"
    else
        log_warning "未找到覆盖率徽章文件"
    fi
}

# 检查覆盖率结果
check_coverage_results() {
    log_info "检查覆盖率结果..."
    
    local report_dir="coverage/report"
    if [[ -d "$report_dir" ]]; then
        local latest_report=$(ls -t "$report_dir"/*.json 2>/dev/null | head -1)
        
        if [[ -n "$latest_report" ]]; then
            log_info "最新报告: $latest_report"
            
            # 提取覆盖率数据
            if command -v jq &> /dev/null; then
                local overall_coverage=$(jq -r '.OverallCoverage // 0' "$latest_report")
                local line_coverage=$(jq -r '.LineCoverage // 0' "$latest_report")
                local branch_coverage=$(jq -r '.BranchCoverage // 0' "$latest_report")
                local method_coverage=$(jq -r '.MethodCoverage // 0' "$latest_report")
                local class_coverage=$(jq -r '.ClassCoverage // 0' "$latest_report")
                
                log_info "覆盖率结果:"
                log_info "  总覆盖率: $overall_coverage%"
                log_info "  行覆盖率: $line_coverage%"
                log_info "  分支覆盖率: $branch_coverage%"
                log_info "  方法覆盖率: $method_coverage%"
                log_info "  类覆盖率: $class_coverage%"
                
                # 简单的数值比较
                if (( $(echo "$overall_coverage < 80" | bc -l) )); then
                    log_warning "总覆盖率较低，请关注测试覆盖率"
                elif (( $(echo "$overall_coverage < 90" | bc -l) )); then
                    log_warning "总覆盖率中等，建议改进"
                else
                    log_success "总覆盖率良好"
                fi
            else
                log_warning "未安装jq工具，无法解析覆盖率数据"
            fi
        else
            log_warning "未找到覆盖率报告"
        fi
    else
        log_warning "未找到覆盖率报告目录"
    fi
}

# 显示帮助信息
show_help() {
    echo "KeyForge 测试覆盖率分析运行脚本"
    echo "=================================="
    echo ""
    echo "用法: $0 [选项]"
    echo ""
    echo "选项:"
    echo "  run            - 运行覆盖率分析"
    echo "  report         - 生成覆盖率报告"
    echo "  validate       - 验证覆盖率阈值"
    echo "  recommendations - 显示覆盖率建议"
    echo "  history        - 显示覆盖率历史"
    echo "  compare        - 比较覆盖率数据"
    echo "  config         - 显示当前配置"
    echo "  thresholds     - 显示阈值设置"
    echo "  clean          - 清理之前的测试结果"
    echo "  full           - 运行完整覆盖率分析"
    echo "  help           - 显示此帮助信息"
    echo ""
    echo "示例:"
    echo "  $0 run            # 运行覆盖率分析"
    echo "  $0 full           # 运行完整覆盖率分析"
    echo "  $0 report         # 生成覆盖率报告"
    echo ""
}

# 主函数
main() {
    local option=${1:-help}
    
    case $option in
        "run")
            check_prerequisites
            create_output_directories
            build_project
            run_coverage_analysis
            check_coverage_results
            ;;
        "report")
            check_prerequisites
            create_output_directories
            build_project
            generate_coverage_report
            ;;
        "validate")
            check_prerequisites
            create_output_directories
            build_project
            validate_coverage_thresholds
            ;;
        "recommendations")
            check_prerequisites
            create_output_directories
            build_project
            show_coverage_recommendations
            ;;
        "history")
            check_prerequisites
            create_output_directories
            build_project
            show_coverage_history
            ;;
        "compare")
            check_prerequisites
            create_output_directories
            build_project
            compare_coverage_data
            ;;
        "config")
            show_configuration
            ;;
        "thresholds")
            show_thresholds
            ;;
        "clean")
            cleanup_previous_results
            log_success "清理完成"
            ;;
        "full")
            check_prerequisites
            create_output_directories
            cleanup_previous_results
            build_project
            run_coverage_analysis
            generate_coverage_report
            validate_coverage_thresholds
            show_coverage_recommendations
            generate_coverage_badge
            save_coverage_history
            check_coverage_results
            log_success "完整覆盖率分析执行完成"
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