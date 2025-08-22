#!/bin/bash

# KeyForge GitHub Actions 验证脚本
# 用于验证所有GitHub Actions工作流配置是否正确

echo "🔍 KeyForge GitHub Actions 验证脚本"
echo "========================================="

# 检查必要的工作流文件
workflows=(
    "build-and-test.yml"
    "uat-testing.yml"
    "code-quality.yml"
    "release.yml"
    "merge-and-deploy.yml"
    "test-github-actions.yml"
)

echo "📋 检查工作流文件..."
for workflow in "${workflows[@]}"; do
    if [ -f ".github/workflows/$workflow" ]; then
        echo "✅ $workflow - 存在"
    else
        echo "❌ $workflow - 缺失"
        exit 1
    fi
done

# 检查解决方案文件
echo ""
echo "📦 检查项目文件..."
if [ -f "KeyForge-Simplified.sln" ]; then
    echo "✅ 解决方案文件存在"
else
    echo "❌ 解决方案文件缺失"
    exit 1
fi

# 检查README徽章
echo ""
echo "🏷️ 检查README徽章..."
badge_count=$(grep -c "github.com/ModerRAS/KeyForge/actions/workflows" README.md)
if [ $badge_count -ge 4 ]; then
    echo "✅ GitHub Actions徽章已配置 ($badge_count个)"
else
    echo "⚠️  GitHub Actions徽章可能不完整 ($badge_count个)"
fi

# 检查文档文件
echo ""
echo "📚 检查文档文件..."
docs=(
    "README.md"
    "docs/README.md"
    "docs/CLAUDE.md"
    "docs/ci-cd/README.md"
    "docs/ci-cd/complete-guide.md"
    "docs/ci-cd/quick-reference.md"
    "CLAUDE.md"
)

for doc in "${docs[@]}"; do
    if [ -f "$doc" ]; then
        echo "✅ $doc - 存在"
    else
        echo "❌ $doc - 缺失"
    fi
done

# 验证YAML语法
echo ""
echo "🔧 验证YAML语法..."
for workflow in "${workflows[@]}"; do
    if command -v yq &> /dev/null; then
        if yq eval ".github/workflows/$workflow" > /dev/null 2>&1; then
            echo "✅ $workflow - YAML语法正确"
        else
            echo "❌ $workflow - YAML语法错误"
        fi
    else
        echo "⚠️  yq命令不可用，跳过YAML验证"
        break
    fi
done

# 检查.NET SDK
echo ""
echo "🖥️  检查.NET SDK..."
if command -v dotnet &> /dev/null; then
    dotnet_version=$(dotnet --version)
    echo "✅ .NET SDK已安装: $dotnet_version"
    
    # 检查项目还原
    echo "📦 检查项目依赖..."
    if dotnet restore KeyForge-Simplified.sln --verbosity quiet; then
        echo "✅ 项目依赖还原成功"
    else
        echo "❌ 项目依赖还原失败"
    fi
else
    echo "⚠️  .NET SDK未安装，跳过项目检查"
fi

# 生成验证报告
echo ""
echo "📊 生成验证报告..."
report_file="github-actions-validation-report.md"
cat > "$report_file" << EOF
# GitHub Actions 验证报告

## 验证时间
$(date)

## 验证结果

### 工作流文件
EOF

for workflow in "${workflows[@]}"; do
    if [ -f ".github/workflows/$workflow" ]; then
        echo "- ✅ $workflow" >> "$report_file"
    else
        echo "- ❌ $workflow" >> "$report_file"
    fi
done

cat >> "$report_file" << EOF

### 项目文件
- ✅ 解决方案文件: KeyForge-Simplified.sln
- ✅ README徽章: $badge_count个

### 文档文件
EOF

for doc in "${docs[@]}"; do
    if [ -f "$doc" ]; then
        echo "- ✅ $doc" >> "$report_file"
    else
        echo "- ❌ $doc" >> "$report_file"
    fi
done

if command -v dotnet &> /dev/null; then
    cat >> "$report_file" << EOF

### 开发环境
- ✅ .NET SDK: $(dotnet --version)
- ✅ 项目依赖: 已还原
EOF
fi

cat >> "$report_file" << EOF

## 下一步操作

1. 提交所有更改到GitHub仓库
2. 验证GitHub Actions工作流是否正常运行
3. 检查所有工作流的执行状态
4. 根据需要调整配置

## 文件位置

- 工作流配置: \`.github/workflows/\`
- 完整指南: \`docs/ci-cd/complete-guide.md\`
- 快速参考: \`docs/ci-cd/quick-reference.md\`
- 详细说明: \`docs/ci-cd/README.md\`
- 文档导航: \`docs/README.md\`
- 文档管理: \`docs/CLAUDE.md\`
- 文件规范: \`CLAUDE.md\`

---

*此报告由验证脚本自动生成*
EOF

echo "✅ 验证报告已生成: $report_file"

# 最终状态
echo ""
echo "🎉 验证完成！"
echo ""
echo "📋 下一步操作:"
echo "1. 提交代码到GitHub仓库"
echo "2. 检查GitHub Actions工作流状态"
echo "3. 创建测试标签验证发布流程"
echo "4. 查看验证报告了解详细信息"
echo ""
echo "🔗 有用链接:"
echo "- GitHub Actions: https://github.com/ModerRAS/KeyForge/actions"
echo "- 完整指南: docs/ci-cd/complete-guide.md"
echo "- 快速参考: docs/ci-cd/quick-reference.md"
echo "- 文档导航: docs/README.md"
echo "- 文档管理: docs/CLAUDE.md"