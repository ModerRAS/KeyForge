#!/bin/bash

# KeyForge 简化测试套件执行脚本
# 专门为跨平台环境设计

set -e

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 项目路径
PROJECT_PATH="KeyForge.Tests/KeyForge.Tests.csproj"
TEST_RESULTS_DIR="TestResults"
COVERAGE_DIR="TestResults/coverage"

# 打印带颜色的消息
print_message() {
    local color=$1
    local message=$2
    echo -e "${color}[$(date '+%Y-%m-%d %H:%M:%S')] ${message}${NC}"
}

# 显示帮助信息
show_help() {
    echo "KeyForge 简化测试套件执行脚本"
    echo ""
    echo "用法: $0 [选项]"
    echo ""
    echo "选项:"
    echo "  -h, --help           显示帮助信息"
    echo "  -v, --verbose        详细输出模式"
    echo "  -c, --coverage       生成代码覆盖率报告"
    echo "  -r, --report         生成HTML测试报告"
    echo "  -a, --all            运行所有测试（包括覆盖率）"
    echo "  -s, --simplified     只运行简化测试"
    echo "  -d, --demo           运行演示模式"
    echo ""
    echo "示例:"
    echo "  $0                   运行基础测试"
    echo "  $0 -c                运行测试并生成覆盖率报告"
    echo "  $0 -a                运行完整测试套件"
    echo "  $0 -s                只运行简化测试"
    echo ""
}

# 检查依赖
check_dependencies() {
    print_message $BLUE "检查依赖..."
    
    if ! command -v dotnet &> /dev/null; then
        print_message $RED "错误: .NET SDK 未安装"
        exit 1
    fi
    
    if ! dotnet --list-sdks | grep -q "9\.0"; then
        print_message $YELLOW "警告: 建议使用 .NET 9.0 SDK"
    fi
    
    print_message $GREEN "依赖检查完成"
}

# 创建测试结果目录
create_directories() {
    print_message $BLUE "创建测试结果目录..."
    mkdir -p "$TEST_RESULTS_DIR"
    mkdir -p "$COVERAGE_DIR"
    print_message $GREEN "目录创建完成"
}

# 恢复依赖
restore_dependencies() {
    print_message $BLUE "恢复依赖..."
    dotnet restore "$PROJECT_PATH"
    print_message $GREEN "依赖恢复完成"
}

# 构建项目
build_project() {
    print_message $BLUE "构建项目..."
    dotnet build "$PROJECT_PATH" --configuration Release
    print_message $GREEN "项目构建完成"
}

# 运行基础测试
run_basic_tests() {
    print_message $BLUE "运行基础测试..."
    dotnet test "$PROJECT_PATH" --configuration Release --logger "console;verbosity=normal" --results-directory "$TEST_RESULTS_DIR"
    print_message $GREEN "基础测试完成"
}

# 运行简化测试
run_simplified_tests() {
    print_message $BLUE "运行简化测试..."
    dotnet test "$PROJECT_PATH" --configuration Release --logger "console;verbosity=normal" --filter "FullyQualifiedName~Simplified" --results-directory "$TEST_RESULTS_DIR"
    print_message $GREEN "简化测试完成"
}

# 运行覆盖率测试
run_coverage_tests() {
    print_message $BLUE "运行覆盖率测试..."
    dotnet test "$PROJECT_PATH" --configuration Release --collect:"XPlat Code Coverage" --results-directory "$TEST_RESULTS_DIR"
    print_message $GREEN "覆盖率测试完成"
}

# 生成覆盖率报告
generate_coverage_report() {
    print_message $BLUE "生成覆盖率报告..."
    
    # 查找覆盖率文件
    local coverage_file=$(find "$TEST_RESULTS_DIR" -name "*.coverage" | head -1)
    
    if [ -z "$coverage_file" ]; then
        print_message $YELLOW "未找到覆盖率文件，跳过报告生成"
        return 1
    fi
    
    # 检查是否有reportgenerator工具
    if ! command -v reportgenerator &> /dev/null; then
        print_message $YELLOW "reportgenerator 未安装，跳过HTML报告生成"
        return 1
    fi
    
    # 生成HTML报告
    reportgenerator -reports:"$coverage_file" -targetdir:"$COVERAGE_DIR" -reporttypes:Html
    
    print_message $GREEN "覆盖率报告生成完成: $COVERAGE_DIR/index.html"
}

# 生成测试报告
generate_test_report() {
    print_message $BLUE "生成测试报告..."
    
    # 查找测试结果文件
    local test_results_file=$(find "$TEST_RESULTS_DIR" -name "*.trx" | head -1)
    
    if [ -z "$test_results_file" ]; then
        print_message $YELLOW "未找到测试结果文件，跳过报告生成"
        return 1
    fi
    
    # 创建简单的HTML报告
    cat > "$TEST_RESULTS_DIR/test-report.html" << EOF
<!DOCTYPE html>
<html>
<head>
    <title>KeyForge 测试报告</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background-color: #f0f0f0; padding: 20px; border-radius: 5px; }
        .section { margin: 20px 0; }
        .success { color: green; }
        .error { color: red; }
        .warning { color: orange; }
    </style>
</head>
<body>
    <div class="header">
        <h1>KeyForge 测试报告</h1>
        <p>生成时间: $(date)</p>
    </div>
    
    <div class="section">
        <h2>测试结果</h2>
        <p>测试结果文件: $test_results_file</p>
        <p>测试项目: $PROJECT_PATH</p>
    </div>
    
    <div class="section">
        <h2>覆盖率报告</h2>
        <p>如果生成了覆盖率报告，请查看: $COVERAGE_DIR/index.html</p>
    </div>
</body>
</html>
EOF
    
    print_message $GREEN "测试报告生成完成: $TEST_RESULTS_DIR/test-report.html"
}

# 运行演示模式
run_demo() {
    print_message $BLUE "运行演示模式..."
    
    echo "=== KeyForge 测试套件演示 ==="
    echo ""
    
    echo "1. 检查依赖..."
    check_dependencies
    
    echo ""
    echo "2. 创建目录..."
    create_directories
    
    echo ""
    echo "3. 恢复依赖..."
    restore_dependencies
    
    echo ""
    echo "4. 构建项目..."
    build_project
    
    echo ""
    echo "5. 运行简化测试..."
    run_simplified_tests
    
    echo ""
    echo "6. 生成报告..."
    generate_test_report
    
    echo ""
    echo "=== 演示完成 ==="
    echo "测试结果目录: $TEST_RESULTS_DIR"
    echo "测试报告: $TEST_RESULTS_DIR/test-report.html"
}

# 清理函数
cleanup() {
    print_message $YELLOW "清理临时文件..."
    # 清理临时文件
    find . -name "*.tmp" -delete 2>/dev/null || true
    find . -name "*.temp" -delete 2>/dev/null || true
    print_message $GREEN "清理完成"
}

# 主函数
main() {
    local verbose=false
    local coverage=false
    local report=false
    local all=false
    local simplified=false
    local demo=false
    
    # 解析命令行参数
    while [[ $# -gt 0 ]]; do
        case $1 in
            -h|--help)
                show_help
                exit 0
                ;;
            -v|--verbose)
                verbose=true
                shift
                ;;
            -c|--coverage)
                coverage=true
                shift
                ;;
            -r|--report)
                report=true
                shift
                ;;
            -a|--all)
                all=true
                shift
                ;;
            -s|--simplified)
                simplified=true
                shift
                ;;
            -d|--demo)
                demo=true
                shift
                ;;
            *)
                print_message $RED "未知选项: $1"
                show_help
                exit 1
                ;;
        esac
    done
    
    # 设置详细模式
    if [ "$verbose" = true ]; then
        set -x
    fi
    
    # 设置退出时清理
    trap cleanup EXIT
    
    # 运行演示模式
    if [ "$demo" = true ]; then
        run_demo
        exit 0
    fi
    
    # 检查依赖
    check_dependencies
    
    # 创建目录
    create_directories
    
    # 恢复依赖
    restore_dependencies
    
    # 构建项目
    build_project
    
    # 运行测试
    if [ "$all" = true ]; then
        print_message $BLUE "运行完整测试套件..."
        run_basic_tests
        run_coverage_tests
        generate_coverage_report
        generate_test_report
    elif [ "$simplified" = true ]; then
        run_simplified_tests
    elif [ "$coverage" = true ]; then
        run_coverage_tests
        generate_coverage_report
    else
        run_basic_tests
    fi
    
    # 生成报告
    if [ "$report" = true ] || [ "$all" = true ]; then
        generate_test_report
    fi
    
    print_message $GREEN "测试执行完成！"
    
    # 显示结果
    if [ -f "$TEST_RESULTS_DIR/test-report.html" ]; then
        print_message $BLUE "测试报告: $TEST_RESULTS_DIR/test-report.html"
    fi
    
    if [ -f "$COVERAGE_DIR/index.html" ]; then
        print_message $BLUE "覆盖率报告: $COVERAGE_DIR/index.html"
    fi
}

# 运行主函数
main "$@"