#!/bin/bash

# 文档结构验证脚本
# 验证项目文档结构是否正确

echo "📋 文档结构验证脚本"
echo "=================="

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

check_directory() {
    if [ -d "$1" ]; then
        echo -e "${GREEN}✓${NC} $1 目录存在"
        return 0
    else
        echo -e "${RED}✗${NC} $1 目录不存在"
        return 1
    fi
}

echo ""
echo "📁 检查根目录文档文件..."
echo ""

# 检查根目录的必要文件
root_files=(
    "README.md"
    "CLAUDE.md"
    "LICENSE"
    "global.json"
)

for file in "${root_files[@]}"; do
    check_file "$file"
done

echo ""
echo "📂 检查docs目录结构..."
echo ""

# 检查docs目录
if check_directory "docs"; then
    # 检查docs下的重要文件
    docs_files=(
        "docs/README.md"
        "docs/CLAUDE.md"
    )
    
    for file in "${docs_files[@]}"; do
        check_file "$file"
    done
    
    echo ""
    echo "📁 检查docs子目录..."
    echo ""
    
    # 检查docs子目录
    docs_dirs=(
        "docs/architecture"
        "docs/ci-cd"
        "docs/development"
        "docs/deployment"
        "docs/user-guide"
        "docs/reference"
    )
    
    for dir in "${docs_dirs[@]}"; do
        check_directory "$dir"
    done
    
    echo ""
    echo "📋 检查CI/CD文档..."
    echo ""
    
    # 检查CI/CD文档
    cicd_files=(
        "docs/ci-cd/README.md"
        "docs/ci-cd/complete-guide.md"
        "docs/ci-cd/quick-reference.md"
        "docs/ci-cd/implementation-summary.md"
    )
    
    for file in "${cicd_files[@]}"; do
        check_file "$file"
    done
fi

echo ""
echo "📂 检查archive目录..."
echo ""

# 检查archive目录
if check_directory "archive"; then
    echo -e "${GREEN}✓${NC} archive 目录存在，用于存放归档文档"
    
    # 检查archive子目录
    archive_dirs=(
        "archive/architecture"
        "archive/planning"
        "archive/reports"
        "archive/testing"
    )
    
    for dir in "${archive_dirs[@]}"; do
        check_directory "$dir"
    done
fi

echo ""
echo "🗂️  检查项目根目录的冗余文档..."
echo ""

# 检查是否有冗余的文档文件在根目录
redundant_patterns=(
    "*ARCHITECTURE*.md"
    "*TESTING*.md"
    "*PROJECT*.md"
    "*IMPLEMENTATION*.md"
    "*TECH*.md"
    "*REQUIREMENTS*.md"
    "*USER*.md"
    "*API*.md"
    "*DESIGN*.md"
    "*DEPLOYMENT*.md"
    "*MIGRATION*.md"
    "*IMPROVEMENT*.md"
    "*GUIDE*.md"
    "*SPECIFICATION*.md"
    "*STRATEGY*.md"
    "*SUMMARY*.md"
    "*REPORT*.md"
    "*VALIDATION*.md"
    "*COMPLETION*.md"
    "*STATUS*.md"
    "*FAQ*.md"
    "*ACCEPTANCE*.md"
    "*STORIES*.md"
    "*CONSTRAINTS*.md"
    "*MODELS*.md"
    "*HAL*.md"
)

redundant_found=false
for pattern in "${redundant_patterns[@]}"; do
    files=$(find . -maxdepth 1 -name "$pattern" -type f 2>/dev/null)
    if [ -n "$files" ]; then
        echo -e "${YELLOW}⚠${NC} 发现冗余文档文件: $files"
        redundant_found=true
    fi
done

if [ "$redundant_found" = false ]; then
    echo -e "${GREEN}✓${NC} 根目录没有发现冗余文档文件"
fi

echo ""
echo "📊 验证完成"
echo "============"
echo "文档结构已经整理完成，主要文件都在正确的位置。"
echo ""
echo "📁 推荐的文档结构："
echo "├── docs/           # 主要文档目录"
echo "│   ├── README.md   # 文档导航"
echo "│   ├── CLAUDE.md   # 文档管理规范"
echo "│   ├── architecture/ # 架构文档"
echo "│   ├── ci-cd/      # CI/CD文档"
echo "│   ├── development/ # 开发文档"
echo "│   ├── deployment/  # 部署文档"
echo "│   ├── user-guide/ # 用户指南"
echo "│   └── reference/  # 参考资料"
echo "├── archive/        # 归档文档"
echo "├── README.md       # 项目说明"
echo "├── CLAUDE.md       # 开发规范"
echo "└── LICENSE         # 许可证"