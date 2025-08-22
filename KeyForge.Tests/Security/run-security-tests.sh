#!/bin/bash

# KeyForge 安全测试运行脚本
# 此脚本用于自动化运行所有安全测试

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
    if [[ ! -f "KeyForge.Tests.Security.csproj" ]]; then
        log_error "未找到项目文件 KeyForge.Tests.Security.csproj"
        exit 1
    fi
    
    # 检查配置文件
    if [[ ! -f "security-test-config.json" ]]; then
        log_warning "未找到配置文件 security-test-config.json，将使用默认配置"
    fi
    
    log_success "前置条件检查完成"
}

# 创建输出目录
create_output_directories() {
    log_info "创建输出目录..."
    
    mkdir -p SecurityReports
    mkdir -p logs
    mkdir -p temp
    
    log_success "输出目录创建完成"
}

# 清理之前的测试结果
cleanup_previous_results() {
    log_info "清理之前的测试结果..."
    
    # 清理编译输出
    dotnet clean -v quiet
    
    # 清理日志文件
    rm -f logs/*.log
    
    # 清理安全报告
    rm -f SecurityReports/*
    
    # 清理临时文件
    rm -rf temp/*
    
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

# 运行快速安全检查
run_quick_test() {
    log_info "运行快速安全检查..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- quick > logs/quick-security-test.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "快速安全检查完成，耗时: ${duration}s"
    else
        log_error "快速安全检查失败"
        log_error "查看日志: logs/quick-security-test.log"
        return 1
    fi
}

# 运行认证测试
run_auth_tests() {
    log_info "运行认证和授权测试..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- auth > logs/auth-test.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "认证和授权测试完成，耗时: ${duration}s"
    else
        log_error "认证和授权测试失败"
        log_error "查看日志: logs/auth-test.log"
        return 1
    fi
}

# 运行输入验证测试
run_input_tests() {
    log_info "运行输入验证测试..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- input > logs/input-test.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "输入验证测试完成，耗时: ${duration}s"
    else
        log_error "输入验证测试失败"
        log_error "查看日志: logs/input-test.log"
        return 1
    fi
}

# 运行HTTP安全测试
run_http_tests() {
    log_info "运行HTTP安全测试..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- http > logs/http-test.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "HTTP安全测试完成，耗时: ${duration}s"
    else
        log_error "HTTP安全测试失败"
        log_error "查看日志: logs/http-test.log"
        return 1
    fi
}

# 运行加密测试
run_encryption_tests() {
    log_info "运行加密安全测试..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- encryption > logs/encryption-test.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "加密安全测试完成，耗时: ${duration}s"
    else
        log_error "加密安全测试失败"
        log_error "查看日志: logs/encryption-test.log"
        return 1
    fi
}

# 运行漏洞扫描
run_vulnerability_scan() {
    log_info "运行漏洞扫描..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- scan > logs/vulnerability-scan.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "漏洞扫描完成，耗时: ${duration}s"
    else
        log_error "漏洞扫描失败"
        log_error "查看日志: logs/vulnerability-scan.log"
        return 1
    fi
}

# 运行渗透测试
run_penetration_tests() {
    log_info "运行渗透测试..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- pentest > logs/penetration-test.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "渗透测试完成，耗时: ${duration}s"
    else
        log_error "渗透测试失败"
        log_error "查看日志: logs/penetration-test.log"
        return 1
    fi
}

# 运行完整安全测试套件
run_full_security_tests() {
    log_info "运行完整安全测试套件..."
    
    local start_time=$(date +%s)
    
    dotnet run --configuration Release -- full > logs/full-security-test.log 2>&1
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [[ $? -eq 0 ]]; then
        log_success "完整安全测试套件完成，耗时: ${duration}s"
    else
        log_error "完整安全测试套件失败"
        log_error "查看日志: logs/full-security-test.log"
        return 1
    fi
}

# 生成安全测试报告
generate_report() {
    log_info "生成安全测试报告..."
    
    dotnet run --configuration Release -- report > logs/report-generation.log 2>&1
    
    if [[ $? -eq 0 ]]; then
        log_success "安全测试报告生成完成"
        log_info "报告保存在 SecurityReports/ 目录"
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
    echo "KeyForge 安全测试运行脚本"
    echo "=============================="
    echo ""
    echo "用法: $0 [选项]"
    echo ""
    echo "选项:"
    echo "  quick       运行快速安全检查"
    echo "  auth        运行认证和授权测试"
    echo "  input       运行输入验证测试"
    echo "  http        运行HTTP安全测试"
    echo "  encryption  运行加密安全测试"
    echo "  scan        运行漏洞扫描"
    echo "  pentest     运行渗透测试"
    echo "  full        运行完整安全测试套件"
    echo "  report      生成安全测试报告"
    echo "  config      显示当前配置"
    echo "  clean       清理之前的测试结果"
    echo "  help        显示此帮助信息"
    echo ""
    echo "示例:"
    echo "  $0 quick          # 运行快速安全检查"
    echo "  $0 full           # 运行完整安全测试套件"
    echo "  $0 scan           # 运行漏洞扫描"
    echo ""
}

# 检查安全测试结果
check_security_results() {
    log_info "检查安全测试结果..."
    
    local report_dir="SecurityReports"
    if [[ -d "$report_dir" ]]; then
        local latest_report=$(ls -t "$report_dir"/*.json 2>/dev/null | head -1)
        
        if [[ -n "$latest_report" ]]; then
            log_info "最新报告: $latest_report"
            
            # 提取安全评分
            if command -v jq &> /dev/null; then
                local security_score=$(jq -r '.OverallSecurityScore // 0' "$latest_report")
                log_info "安全评分: $security_score/100"
                
                if (( $(echo "$security_score < 80" | bc -l) )); then
                    log_warning "安全评分较低，请关注安全问题"
                elif (( $(echo "$security_score < 90" | bc -l) )); then
                    log_warning "安全评分中等，建议改进"
                else
                    log_success "安全评分良好"
                fi
            fi
        else
            log_warning "未找到安全测试报告"
        fi
    else
        log_warning "未找到安全测试报告目录"
    fi
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
            check_security_results
            ;;
        "auth")
            check_prerequisites
            create_output_directories
            build_project
            run_auth_tests
            check_security_results
            ;;
        "input")
            check_prerequisites
            create_output_directories
            build_project
            run_input_tests
            check_security_results
            ;;
        "http")
            check_prerequisites
            create_output_directories
            build_project
            run_http_tests
            check_security_results
            ;;
        "encryption")
            check_prerequisites
            create_output_directories
            build_project
            run_encryption_tests
            check_security_results
            ;;
        "scan")
            check_prerequisites
            create_output_directories
            build_project
            run_vulnerability_scan
            check_security_results
            ;;
        "pentest")
            check_prerequisites
            create_output_directories
            build_project
            run_penetration_tests
            check_security_results
            ;;
        "full")
            show_system_info
            check_prerequisites
            create_output_directories
            cleanup_previous_results
            build_project
            run_full_security_tests
            check_security_results
            generate_report
            log_success "完整安全测试套件执行完成"
            ;;
        "report")
            check_prerequisites
            create_output_directories
            build_project
            generate_report
            ;;
        "config")
            dotnet run --configuration Release -- config
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