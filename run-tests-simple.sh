#!/bin/bash

# KeyForge 简化测试运行脚本
# 原本实现：复杂的测试分类和报告生成
# 简化实现：快速、实用的测试执行

echo "开始运行KeyForge测试套件..."

# 设置变量
TEST_PROJECT="KeyForge.Tests"
OUTPUT_DIR="TestResults"
COVERAGE_THRESHOLD=60

# 创建输出目录
mkdir -p $OUTPUT_DIR

# 运行测试并生成覆盖率报告
echo "运行测试..."
dotnet test $TEST_PROJECT \
    --configuration Release \
    --collect:"XPlat Code Coverage" \
    --results-directory $OUTPUT_DIR \
    --logger "console;verbosity=minimal" \
    --logger "trx;LogFileName=$OUTPUT_DIR/test-results.trx"

# 检查测试结果
if [ $? -ne 0 ]; then
    echo "错误: 测试执行失败"
    exit 1
fi

# 生成HTML覆盖率报告（如果安装了reportgenerator）
echo "生成覆盖率报告..."
if command -v reportgenerator &> /dev/null; then
    reportgenerator \
        -reports:"$OUTPUT_DIR/*.coverage" \
        -targetdir:"$OUTPUT_DIR" \
        -reporttypes:Html
    echo "HTML覆盖率报告已生成: $OUTPUT_DIR/index.html"
else
    echo "提示: 安装reportgenerator以生成HTML报告: dotnet tool install -g dotnet-reportgenerator-globaltool"
fi

# 简化的覆盖率检查
echo "检查覆盖率..."
if [ -f "$OUTPUT_DIR/coverage.json" ]; then
    coverage_line=$(grep '"line"' $OUTPUT_DIR/coverage.json | head -1 | cut -d':' -f2 | cut -d',' -f1 | tr -d ' ')
    if [ ! -z "$coverage_line" ]; then
        coverage_percent=$(echo "$coverage_line" | cut -d'.' -f1)
        if [ "$coverage_percent" -lt "$COVERAGE_THRESHOLD" ]; then
            echo "警告: 代码覆盖率 $coverage_percent% 低于阈值 $COVERAGE_THRESHOLD%"
            echo "建议增加测试覆盖率或调整阈值"
        else
            echo "代码覆盖率: $coverage_percent% (达标)"
        fi
    fi
fi

echo "测试完成！"
echo "结果保存在: $OUTPUT_DIR/"
echo "主要文件:"
echo "  - test-results.trx: 测试结果"
echo "  - coverage.json: 覆盖率数据"
echo "  - index.html: HTML报告（如果生成）"

# 显示测试统计
if [ -f "$OUTPUT_DIR/test-results.trx" ]; then
    echo ""
    echo "测试统计:"
    total_tests=$(grep -o 'total="[^"]*"' $OUTPUT_DIR/test-results.trx | cut -d'"' -f2)
    passed_tests=$(grep -o 'passed="[^"]*"' $OUTPUT_DIR/test-results.trx | cut -d'"' -f2)
    failed_tests=$(grep -o 'failed="[^"]*"' $OUTPUT_DIR/test-results.trx | cut -d'"' -f2)
    
    echo "  总测试数: $total_tests"
    echo "  通过: $passed_tests"
    echo "  失败: $failed_tests"
fi