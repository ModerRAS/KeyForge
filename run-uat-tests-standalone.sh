#!/bin/bash

# KeyForge UAT测试运行脚本
# 用于运行用户验收测试并生成报告

echo "=== KeyForge UAT测试运行脚本 ==="
echo "开始时间: $(date)"

# 创建测试报告目录
mkdir -p UAT-Reports

# 设置环境变量
export DOTNET_ENVIRONMENT=Testing
export UAT_TEST_MODE=true

echo "1. 验证测试环境..."
cd /root/WorkSpace/CSharp/KeyForge/KeyForge.UAT.Tests

# 验证编译
dotnet build --verbosity quiet

if [ $? -ne 0 ]; then
    echo "❌ 编译失败，请检查代码"
    exit 1
fi

echo "✅ 编译成功"

echo "2. 运行UAT测试..."

# 运行UAT测试
dotnet test --logger "console;verbosity=minimal"

if [ $? -eq 0 ]; then
    echo "✅ UAT测试完成"
else
    echo "❌ UAT测试失败"
    exit 1
fi

echo "3. 测试结果统计..."

# 统计测试结果
echo "=== 测试结果摘要 ==="
echo "测试时间: $(date)"
echo "测试报告位置: KeyForge.UAT.Tests/bin/Debug/net9.0/UAT-Reports/"

# 列出生成的报告文件
echo "生成的报告文件:"
ls -la KeyForge.UAT.Tests/bin/Debug/net9.0/UAT-Reports/ | grep -E "\.(md)$"

echo "=== UAT测试运行完成 ==="
echo "完成时间: $(date)"

echo ""
echo "查看测试报告:"
echo "- 测试报告: KeyForge.UAT.Tests/bin/Debug/net9.0/UAT-Reports/UAT-Report-*.md"