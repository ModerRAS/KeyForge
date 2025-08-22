using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL;

/// <summary>
/// HAL工厂类 - 用于创建平台特定的HAL实例（简化版）
/// 原本实现：包含完整的依赖注入和复杂的服务创建逻辑
/// 简化实现：只保留基本的工厂方法，确保项目能够编译
/// </summary>
public static class HALFactory
{
    /// <summary>
    /// 创建硬件抽象层实例
    /// </summary>
    /// <returns>HAL实例</returns>
    public static IHardwareAbstractionLayer CreateHAL()
    {
        return new HardwareAbstractionLayer();
    }
}