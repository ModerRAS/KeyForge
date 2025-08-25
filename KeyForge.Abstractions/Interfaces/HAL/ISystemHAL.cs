namespace KeyForge.Abstractions.Interfaces.HAL
{
    /// <summary>
    /// 系统硬件抽象层基础接口
    /// 【优化实现】定义了跨平台系统功能的硬件抽象层接口
    /// 原实现：系统功能直接调用平台API，缺乏统一抽象
    /// 优化：通过HAL抽象，实现跨平台系统功能的统一接口
    /// </summary>
    public interface ISystemHAL : IDisposable
    {
        /// <summary>
        /// 初始化系统HAL
        /// </summary>
        Task<bool> InitializeAsync();
        
        /// <summary>
        /// 获取HAL类型
        /// </summary>
        HALType HALType { get; }
        
        /// <summary>
        /// 获取HAL版本
        /// </summary>
        Version Version { get; }
        
        /// <summary>
        /// HAL状态
        /// </summary>
        HALStatus Status { get; }
    }
    
    /// <summary>
    /// 系统信息硬件抽象层接口
    /// </summary>
    public interface ISystemInfoHAL : ISystemHAL
    {
        /// <summary>
        /// 获取操作系统信息
        /// </summary>
        Task<OSInfo> GetOSInfoAsync();
        
        /// <summary>
        /// 获取硬件信息
        /// </summary>
        Task<HardwareInfo> GetHardwareInfoAsync();
        
        /// <summary>
        /// 获取内存信息
        /// </summary>
        Task<MemoryInfo> GetMemoryInfoAsync();
        
        /// <summary>
        /// 获取CPU信息
        /// </summary>
        Task<CPUInfo> GetCPUInfoAsync();
        
        /// <summary>
        /// 获取磁盘信息
        /// </summary>
        Task<List<DiskInfo>> GetDiskInfosAsync();
        
        /// <summary>
        /// 获取网络信息
        /// </summary>
        Task<List<NetworkInfo>> GetNetworkInfosAsync();
    }
    
    /// <summary>
    /// 进程管理硬件抽象层接口
    /// </summary>
    public interface IProcessHAL : ISystemHAL
    {
        /// <summary>
        /// 启动进程
        /// </summary>
        Task<int> StartProcessAsync(string filePath, string arguments = "");
        
        /// <summary>
        /// 停止进程
        /// </summary>
        Task<bool> StopProcessAsync(int processId);
        
        /// <summary>
        /// 获取进程信息
        /// </summary>
        Task<ProcessInfo> GetProcessInfoAsync(int processId);
        
        /// <summary>
        /// 获取所有进程
        /// </summary>
        Task<List<ProcessInfo>> GetAllProcessesAsync();
        
        /// <summary>
        /// 查找进程
        /// </summary>
        Task<ProcessInfo?> FindProcessAsync(string processName);
        
        /// <summary>
        /// 等待进程退出
        /// </summary>
        Task<bool> WaitForProcessExitAsync(int processId, int timeoutMs = -1);
    }
    
    /// <summary>
    /// 系统事件硬件抽象层接口
    /// </summary>
    public interface ISystemEventHAL : ISystemHAL
    {
        /// <summary>
        /// 设置系统事件钩子
        /// </summary>
        Task<bool> SetSystemEventHookAsync(SystemEventType eventType, SystemEventCallback callback);
        
        /// <summary>
        /// 移除系统事件钩子
        /// </summary>
        Task<bool> RemoveSystemEventHookAsync(SystemEventType eventType);
        
        /// <summary>
        /// 发送系统事件
        /// </summary>
        Task<bool> SendSystemEventAsync(SystemEvent systemEvent);
        
        /// <summary>
        /// 监控系统事件
        /// </summary>
        Task<bool> MonitorSystemEventsAsync(SystemEventType[] eventTypes);
    }
    
    /// <summary>
    /// 性能监控硬件抽象层接口
    /// </summary>
    public interface IPerformanceHAL : ISystemHAL
    {
        /// <summary>
        /// 获取CPU使用率
        /// </summary>
        Task<double> GetCPUUsageAsync();
        
        /// <summary>
        /// 获取内存使用率
        /// </summary>
        Task<double> GetMemoryUsageAsync();
        
        /// <summary>
        /// 获取磁盘使用率
        /// </summary>
        Task<double> GetDiskUsageAsync(string driveLetter);
        
        /// <summary>
        /// 获取网络使用率
        /// </summary>
        Task<NetworkUsage> GetNetworkUsageAsync();
        
        /// <summary>
        /// 获取系统启动时间
        /// </summary>
        Task<DateTime> GetSystemStartTimeAsync();
        
        /// <summary>
        /// 获取系统运行时间
        /// </summary>
        Task<TimeSpan> GetSystemUptimeAsync();
    }
    
    /// <summary>
    /// 系统事件回调委托
    /// </summary>
    public delegate void SystemEventCallback(SystemEvent systemEvent);
    
    /// <summary>
    /// 操作系统信息
    /// </summary>
    public record OSInfo
    {
        public string Name { get; init; } = string.Empty;
        public string Version { get; init; } = string.Empty;
        public string Architecture { get; init; } = string.Empty;
        public string Locale { get; init; } = string.Empty;
        public DateTime InstallDate { get; init; }
    }
    
    /// <summary>
    /// 硬件信息
    /// </summary>
    public record HardwareInfo
    {
        public string Manufacturer { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public string SerialNumber { get; init; } = string.Empty;
        public string BIOSVersion { get; init; } = string.Empty;
    }
    
    /// <summary>
    /// 内存信息
    /// </summary>
    public record MemoryInfo
    {
        public long TotalPhysicalMemory { get; init; }
        public long AvailablePhysicalMemory { get; init; }
        public long TotalVirtualMemory { get; init; }
        public long AvailableVirtualMemory { get; init; }
        public double MemoryUsagePercentage { get; init; }
    }
    
    /// <summary>
    /// CPU信息
    /// </summary>
    public record CPUInfo
    {
        public string Name { get; init; } = string.Empty;
        public int Cores { get; init; }
        public int LogicalProcessors { get; init; }
        public double MaxClockSpeed { get; init; }
        public double CurrentClockSpeed { get; init; }
        public double UsagePercentage { get; init; }
    }
    
    /// <summary>
    /// 磁盘信息
    /// </summary>
    public record DiskInfo
    {
        public string DriveLetter { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public long TotalSize { get; init; }
        public long FreeSpace { get; init; }
        public string FileSystem { get; init; } = string.Empty;
        public double UsagePercentage { get; init; }
    }
    
    /// <summary>
    /// 网络信息
    /// </summary>
    public record NetworkInfo
    {
        public string Name { get; init; } = string.Empty;
        public string IPAddress { get; init; } = string.Empty;
        public string MACAddress { get; init; } = string.Empty;
        public bool IsConnected { get; init; }
        public double Speed { get; init; }
    }
    
    /// <summary>
    /// 进程信息
    /// </summary>
    public record ProcessInfo
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string FilePath { get; init; } = string.Empty;
        public long MemoryUsage { get; init; }
        public double CPUUsage { get; init; }
        public DateTime StartTime { get; init; }
        public ProcessState State { get; init; }
    }
    
    /// <summary>
    /// 网络使用情况
    /// </summary>
    public record NetworkUsage
    {
        public long BytesReceived { get; init; }
        public long BytesSent { get; init; }
        public double ReceiveRate { get; init; }
        public double SendRate { get; init; }
    }
    
    /// <summary>
    /// 系统事件
    /// </summary>
    public record SystemEvent
    {
        public SystemEventType Type { get; init; }
        public DateTime Timestamp { get; init; }
        public string Message { get; init; } = string.Empty;
        public object? Data { get; init; }
    }
}