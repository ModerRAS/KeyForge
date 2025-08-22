# KeyForge 架构重构实施指南

## 1. 重构概述

本指南提供了KeyForge项目架构重构的具体实施步骤，帮助开发团队有序地完成架构重构工作。

### 1.1 重构目标
- 解决Core层职责过重问题
- 统一重复定义的枚举和接口
- 建立清晰的分层架构
- 实现依赖倒置原则
- 提高代码的可维护性和可测试性

### 1.2 重构范围
- 项目结构调整
- 依赖关系重构
- 统一接口定义
- 代码迁移和清理

## 2. 实施步骤

### 2.1 第一阶段：准备工作

#### 2.1.1 备份现有代码
```bash
# 创建备份分支
git checkout -b architecture-refactor-backup

# 提交当前代码
git add .
git commit -m "备份当前代码状态"
```

#### 2.1.2 创建新的项目结构
```bash
# 创建新的目录结构
mkdir -p KeyForge.Domain/Common
mkdir -p KeyForge.Domain/Entities
mkdir -p KeyForge.Domain/ValueObjects
mkdir -p KeyForge.Domain/Aggregates
mkdir -p KeyForge.Domain/Events
mkdir -p KeyForge.Domain/Interfaces
mkdir -p KeyForge.Domain/Exceptions
mkdir -p KeyForge.Domain/Services

mkdir -p KeyForge.Application/Services
mkdir -p KeyForge.Application/DTOs
mkdir -p KeyForge.Application/Commands
mkdir -p KeyForge.Application/Queries
mkdir -p KeyForge.Application/Exceptions

mkdir -p KeyForge.Infrastructure/Data
mkdir -p KeyForge.Infrastructure/Repositories
mkdir -p KeyForge.Infrastructure/Services
mkdir -p KeyForge.Infrastructure/External
mkdir -p KeyForge.Infrastructure/Persistence
mkdir -p KeyForge.Infrastructure/DependencyInjection

mkdir -p docs
```

#### 2.1.3 更新解决方案文件
创建新的解决方案文件 `KeyForge.sln`，包含所有项目。

#### 2.1.4 配置项目依赖关系
按照架构设计文档中的依赖关系配置各项目的引用。

### 2.2 第二阶段：Domain层重构

#### 2.2.1 创建统一的枚举定义
**文件位置**: `KeyForge.Domain/Common/Enums.cs`

**操作步骤**:
1. 删除Core层中的重复枚举定义
2. 将所有枚举定义移动到Domain层
3. 更新所有引用这些枚举的代码

**需要删除的文件**:
- `KeyForge.Core/Domain/Common/Enums.cs`
- `KeyForge.Application/DTOs/Enums.cs`
- `KeyForge.Domain/Entities/GameAction.cs` 中的枚举定义
- `KeyForge.Core/Models/KeyAction.cs` 中的枚举定义

**需要更新的文件**:
- 所有引用这些枚举的文件需要更新命名空间引用

#### 2.2.2 创建统一的基类定义
**文件位置**: `KeyForge.Domain/Common/BaseTypes.cs`

**操作步骤**:
1. 删除Core层中的重复基类定义
2. 将所有基类定义移动到Domain层
3. 更新所有继承这些基类的代码

**需要删除的文件**:
- `KeyForge.Core/Domain/Common/BaseTypes.cs`
- `KeyForge.Domain/Base.cs`

#### 2.2.3 移动实体到Domain层
**操作步骤**:
1. 将 `KeyForge.Domain/Entities/GameAction.cs` 中的实体类移动到Domain层
2. 删除重复的实体定义
3. 更新所有引用

#### 2.2.4 创建统一的接口定义
**文件位置**: `KeyForge.Domain/Interfaces/IRepositories.cs`
**文件位置**: `KeyForge.Domain/Interfaces/IServices.cs`

**操作步骤**:
1. 将所有接口定义移动到Domain层
2. 删除重复的接口定义
3. 更新所有实现类

### 2.3 第三阶段：Application层重构

#### 2.3.1 创建应用服务
**文件位置**: `KeyForge.Application/Services/`

**操作步骤**:
1. 创建应用服务接口和实现
2. 实现业务逻辑编排
3. 添加依赖注入配置

#### 2.3.2 创建DTO和映射
**文件位置**: `KeyForge.Application/DTOs/`

**操作步骤**:
1. 创建数据传输对象
2. 实现对象映射
3. 添加验证逻辑

#### 2.3.3 实现命令查询分离
**文件位置**: `KeyForge.Application/Commands/` 和 `KeyForge.Application/Queries/`

**操作步骤**:
1. 创建命令和查询类
2. 实现处理器
3. 配置MediatR

### 2.4 第四阶段：Infrastructure层重构

#### 2.4.1 实现仓储模式
**文件位置**: `KeyForge.Infrastructure/Repositories/`

**操作步骤**:
1. 实现所有仓储接口
2. 添加数据访问层
3. 配置依赖注入

#### 2.4.2 重构数据访问
**文件位置**: `KeyForge.Infrastructure/Data/`

**操作步骤**:
1. 创建DbContext
2. 配置实体映射
3. 实现数据迁移

#### 2.4.3 重构外部服务
**文件位置**: `KeyForge.Infrastructure/External/`

**操作步骤**:
1. 实现外部服务接口
2. 添加服务适配器
3. 配置服务注册

### 2.5 第五阶段：Core层清理

#### 2.5.1 删除Domain相关代码
**需要删除的文件**:
- `KeyForge.Core/Domain/` 目录下的所有文件
- `KeyForge.Core/Models/` 中的领域模型
- `KeyForge.Core/Domain/Interfaces/` 中的接口定义

#### 2.5.2 删除Application相关代码
**需要删除的文件**:
- `KeyForge.Core/Services/` 中的应用服务
- 应用层相关的业务逻辑

#### 2.5.3 保留通用工具类
**需要保留的文件**:
- 通用扩展方法
- 工具类
- 配置类

#### 2.5.4 更新依赖关系
**操作步骤**:
1. 更新项目引用
2. 移除不必要的依赖
3. 配置正确的依赖方向

### 2.6 第六阶段：UI层更新

#### 2.6.1 更新引用
**操作步骤**:
1. 更新项目引用
2. 更新命名空间引用
3. 添加新的依赖

#### 2.6.2 适配新的服务接口
**操作步骤**:
1. 更新服务调用
2. 适配新的接口
3. 更新依赖注入配置

#### 2.6.3 测试功能完整性
**操作步骤**:
1. 编译测试
2. 功能测试
3. 集成测试

## 3. 详细实施指南

### 3.1 删除重复定义的具体步骤

#### 3.1.1 删除Core层中的枚举定义
```bash
# 删除重复的枚举文件
rm KeyForge.Core/Domain/Common/Enums.cs
rm KeyForge.Application/DTOs/Enums.cs
```

#### 3.1.2 清理GameAction.cs中的重复定义
```csharp
// 删除KeyForge.Domain/Entities/GameAction.cs中的重复枚举定义
// 保留实体类定义，删除枚举定义
```

#### 3.1.3 清理KeyAction.cs中的重复定义
```csharp
// 删除KeyForge.Core/Models/KeyAction.cs中的重复枚举定义
// 保留模型类定义，删除枚举定义
```

### 3.2 更新引用的具体步骤

#### 3.2.1 更新命名空间引用
```csharp
// 在所有使用枚举的文件中，更新命名空间引用
// 从：
using KeyForge.Core.Domain.Common;
// 或
using KeyForge.Application.DTOs;

// 改为：
using KeyForge.Domain.Common;
```

#### 3.2.2 批量替换脚本
```bash
# 使用批量替换工具更新所有引用
find . -name "*.cs" -exec sed -i 's/KeyForge.Core.Domain.Common/KeyForge.Domain.Common/g' {} \;
find . -name "*.cs" -exec sed -i 's/KeyForge.Application.DTOs/KeyForge.Domain.Common/g' {} \;
```

### 3.3 依赖注入配置

#### 3.3.1 创建服务注册类
```csharp
// KeyForge.Infrastructure/DependencyInjection/ServiceRegistration.cs
public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // 注册仓储
        services.AddScoped<IScriptRepository, ScriptRepository>();
        services.AddScoped<IImageTemplateRepository, ImageTemplateRepository>();
        // ... 其他仓储
        
        // 注册服务
        services.AddScoped<IScriptPlayerService, ScriptPlayerService>();
        services.AddScoped<IImageRecognitionService, ImageRecognitionService>();
        // ... 其他服务
        
        return services;
    }
    
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 注册应用服务
        services.AddScoped<IScriptApplicationService, ScriptApplicationService>();
        // ... 其他应用服务
        
        // 注册MediatR
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(ServiceRegistration).Assembly);
        });
        
        return services;
    }
}
```

#### 3.3.2 更新Program.cs
```csharp
// 在启动程序中配置依赖注入
var services = new ServiceCollection();
services.AddInfrastructure();
services.AddApplication();

var serviceProvider = services.BuildServiceProvider();
```

## 4. 风险控制

### 4.1 编译错误处理

#### 4.1.1 常见编译错误及解决方案
1. **命名空间未找到**
   - 检查项目引用
   - 更新命名空间引用
   - 确保文件位置正确

2. **类型冲突**
   - 删除重复定义
   - 使用完全限定名
   - 检查using语句

3. **依赖循环**
   - 检查项目依赖关系
   - 使用接口解耦
   - 重构依赖结构

#### 4.1.2 编译测试脚本
```bash
# 编译测试脚本
#!/bin/bash

echo "开始编译测试..."

# 编译Domain层
dotnet build KeyForge.Domain/KeyForge.Domain.csproj
if [ $? -ne 0 ]; then
    echo "Domain层编译失败"
    exit 1
fi

# 编译Application层
dotnet build KeyForge.Application/KeyForge.Application.csproj
if [ $? -ne 0 ]; then
    echo "Application层编译失败"
    exit 1
fi

# 编译Infrastructure层
dotnet build KeyForge.Infrastructure/KeyForge.Infrastructure.csproj
if [ $? -ne 0 ]; then
    echo "Infrastructure层编译失败"
    exit 1
fi

# 编译Core层
dotnet build KeyForge.Core/KeyForge.Core.csproj
if [ $? -ne 0 ]; then
    echo "Core层编译失败"
    exit 1
fi

# 编译UI层
dotnet build KeyForge.UI/KeyForge.UI.csproj
if [ $? -ne 0 ]; then
    echo "UI层编译失败"
    exit 1
fi

echo "所有层编译成功"
```

### 4.2 功能完整性保证

#### 4.2.1 测试用例清单
1. **脚本管理功能**
   - 创建脚本
   - 编辑脚本
   - 删除脚本
   - 执行脚本

2. **图像识别功能**
   - 模板匹配
   - 颜色识别
   - 文本识别

3. **状态机功能**
   - 状态转换
   - 规则评估
   - 事件处理

4. **热键功能**
   - 注册热键
   - 执行热键
   - 注销热键

#### 4.2.2 集成测试脚本
```csharp
// 集成测试示例
[TestClass]
public class ArchitectureIntegrationTests
{
    [TestMethod]
    public async Task ScriptCreation_ShouldWorkWithNewArchitecture()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var scriptService = serviceProvider.GetRequiredService<IScriptApplicationService>();
        
        // Act
        var result = await scriptService.CreateScriptAsync(new CreateScriptCommand
        {
            Name = "Test Script",
            Description = "Test Description"
        });
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
    }
    
    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure();
        services.AddApplication();
        return services.BuildServiceProvider();
    }
}
```

## 5. 验收标准

### 5.1 架构验收标准

#### 5.1.1 分层清晰性检查
- [ ] Domain层不依赖任何其他层
- [ ] Application层只依赖Domain层
- [ ] Infrastructure层只依赖Domain层
- [ ] Presentation层依赖Application和Domain层
- [ ] 每层职责明确，无职责过重

#### 5.1.2 依赖关系检查
- [ ] 遵循依赖倒置原则
- [ ] 无循环依赖
- [ ] 接口定义在Domain层
- [ ] 实现在Infrastructure层

#### 5.1.3 接口统一性检查
- [ ] 无重复的枚举定义
- [ ] 无重复的接口定义
- [ ] 接口契约清晰
- [ ] 命名规范统一

### 5.2 代码质量标准

#### 5.2.1 编译标准
- [ ] 所有项目编译通过
- [ ] 无编译警告
- [ ] 无冗余引用
- [ ] 命名空间正确

#### 5.2.2 测试标准
- [ ] 单元测试覆盖率 > 80%
- [ ] 集成测试通过
- [ ] 功能测试通过
- [ ] 性能测试通过

#### 5.2.3 代码规范
- [ ] 符合C#编码规范
- [ ] 注释完整
- [ ] 文档齐全
- [ ] 代码复用性良好

## 6. 实施时间表

### 6.1 第一周：准备阶段
- 周一：备份代码，创建新项目结构
- 周二：更新解决方案文件
- 周三：配置项目依赖关系
- 周四：准备测试环境
- 周五：准备阶段验收

### 6.2 第二周：Domain层重构
- 周一：创建统一枚举定义
- 周二：创建统一基类定义
- 周三：移动实体到Domain层
- 周四：创建统一接口定义
- 周五：Domain层验收

### 6.3 第三周：Application层重构
- 周一：创建应用服务
- 周二：创建DTO和映射
- 周三：实现命令查询分离
- 周四：添加MediatR支持
- 周五：Application层验收

### 6.4 第四周：Infrastructure层重构
- 周一：实现仓储模式
- 周二：重构数据访问
- 周三：重构外部服务
- 周四：添加依赖注入配置
- 周五：Infrastructure层验收

### 6.5 第五周：Core层清理
- 周一：删除Domain相关代码
- 周二：删除Application相关代码
- 周三：保留通用工具类
- 周四：更新依赖关系
- 周五：Core层验收

### 6.6 第六周：UI层更新
- 周一：更新引用
- 周二：适配新的服务接口
- 周三：测试功能完整性
- 周四：性能测试
- 周五：最终验收

## 7. 注意事项

### 7.1 开发规范
- 严格遵循SOLID原则
- 使用依赖注入模式
- 编写单元测试
- 保持代码简洁

### 7.2 版本控制
- 每个阶段创建单独的分支
- 定期提交代码
- 编写详细的提交信息
- 及时合并代码

### 7.3 团队协作
- 明确分工
- 定期沟通
- 代码审查
- 知识共享

## 8. 总结

本实施指南提供了KeyForge项目架构重构的详细步骤和最佳实践。通过严格按照本指南执行，可以确保重构过程的顺利进行，最终实现一个架构清晰、职责明确、易于维护和扩展的系统。

重构成功后，KeyForge项目将具备以下优势：
- 清晰的分层架构
- 统一的接口定义
- 良好的可测试性
- 高度的可扩展性
- 优秀的可维护性

请团队成员仔细阅读本指南，并在实施过程中遇到问题时及时沟通解决。