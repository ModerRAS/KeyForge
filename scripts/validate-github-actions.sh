#!/bin/bash

# GitHub Actions 验证脚本
# 用于验证 GitHub Actions 配置修复是否正确

echo "🔍 GitHub Actions 配置验证脚本"
echo "================================="

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 检查函数
check_file() {
    if [ -f "$1" ]; then
        echo -e "${GREEN}✓${NC} $1 存在"
        return 0
    else
        echo -e "${RED}✗${NC} $1 不存在"
        return 1
    fi
}

check_version() {
    local file="$1"
    local expected_version="$2"
    
    # 检查直接使用的版本
    if grep -q "dotnet-version: '$expected_version'" "$file"; then
        echo -e "${GREEN}✓${NC} $file 使用正确的 .NET 版本: $expected_version"
        return 0
    # 检查环境变量方式
    elif grep -q "DOTNET_VERSION: '$expected_version'" "$file"; then
        echo -e "${GREEN}✓${NC} $file 使用正确的 .NET 环境变量版本: $expected_version"
        return 0
    # 检查矩阵方式
    elif grep -q "dotnet-version: \[$expected_version\]" "$file"; then
        echo -e "${GREEN}✓${NC} $file 使用正确的 .NET 矩阵版本: $expected_version"
        return 0
    else
        echo -e "${RED}✗${NC} $file .NET 版本不正确或未找到"
        return 1
    fi
}

check_artifact_version() {
    local file="$1"
    local expected_version="$2"
    
    if grep -q "actions/upload-artifact@$expected_version" "$file"; then
        echo -e "${GREEN}✓${NC} $file 使用正确的 upload-artifact 版本: $expected_version"
        return 0
    else
        echo -e "${RED}✗${NC} $file upload-artifact 版本不正确或未找到"
        return 1
    fi
}

check_download_artifact_version() {
    local file="$1"
    local expected_version="$2"
    
    if grep -q "actions/download-artifact@$expected_version" "$file"; then
        echo -e "${GREEN}✓${NC} $file 使用正确的 download-artifact 版本: $expected_version"
        return 0
    else
        echo -e "${RED}✗${NC} $file download-artifact 版本不正确或未找到"
        return 1
    fi
}

echo ""
echo "📋 检查 GitHub Actions 工作流文件..."
echo ""

# 检查所有工作流文件
workflow_files=(
    ".github/workflows/uat-testing.yml"
    ".github/workflows/ci-cd.yml"
    ".github/workflows/build-and-test.yml"
    ".github/workflows/merge-and-deploy.yml"
    ".github/workflows/release.yml"
)

total_checks=0
passed_checks=0

for file in "${workflow_files[@]}"; do
    echo -e "${YELLOW}检查 $file${NC}"
    
    if check_file "$file"; then
        ((total_checks++))
        ((passed_checks++))
        
        # 检查 .NET 版本
        ((total_checks++))
        if check_version "$file" "9.0.x"; then
            ((passed_checks++))
        fi
        
        # 检查 upload-artifact 版本
        ((total_checks++))
        if check_artifact_version "$file" "v4"; then
            ((passed_checks++))
        fi
        
        # 只检查实际需要 download-artifact 的文件
        if [[ "$file" == *release.yml ]]; then
            ((total_checks++))
            if check_download_artifact_version "$file" "v4"; then
                ((passed_checks++))
            fi
        else
            echo -e "${GREEN}✓${NC} $file 不需要 download-artifact"
        fi
    else
        ((total_checks++))
    fi
    
    echo ""
done

echo "📊 检查结果摘要"
echo "================"
echo "总检查项: $total_checks"
echo "通过项: $passed_checks"
echo "失败项: $((total_checks - passed_checks))"

if [ $passed_checks -eq $total_checks ]; then
    echo -e "${GREEN}🎉 所有检查都通过了！GitHub Actions 配置修复成功。${NC}"
    exit 0
else
    echo -e "${RED}❌ 有 $((total_checks - passed_checks)) 个检查失败，需要修复。${NC}"
    exit 1
fi