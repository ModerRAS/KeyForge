using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL;

/// <summary>
/// 平台检测器 - 检测当前运行平台
/// </summary>
public static class PlatformDetector
{
    /// <summary>
    /// 检测当前平台
    /// </summary>
    /// <returns>平台信息</returns>
    public static PlatformInfo DetectPlatform()
    {
        var platform = DetectPlatformType();
        var version = GetPlatformVersion();
        var architecture = RuntimeInformation.ProcessArchitecture.ToString();
        var dotNetVersion = GetDotNetVersion();
        var is64Bit = Environment.Is64BitOperatingSystem;
        var systemName = GetSystemName();
        var hostName = Environment.MachineName;

        return new PlatformInfo
        {
            Platform = platform,
            Version = version,
            Architecture = architecture,
            DotNetVersion = dotNetVersion,
            Is64Bit = is64Bit,
            SystemName = systemName,
            HostName = hostName
        };
    }

    /// <summary>
    /// 检测平台类型
    /// </summary>
    /// <returns>平台类型</returns>
    private static Platform DetectPlatformType()
    {
        if (OperatingSystem.IsWindows())
        {
            return Platform.Windows;
        }
        else if (OperatingSystem.IsMacOS())
        {
            return Platform.MacOS;
        }
        else if (OperatingSystem.IsLinux())
        {
            return Platform.Linux;
        }
        else
        {
            return Platform.Unknown;
        }
    }

    /// <summary>
    /// 获取平台版本
    /// </summary>
    /// <returns>平台版本</returns>
    private static string GetPlatformVersion()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                return Environment.OSVersion.VersionString;
            }
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            {
                // 使用RuntimeInformation获取版本信息
                var release = File.Exists("/etc/os-release") 
                    ? File.ReadAllText("/etc/os-release") 
                    : string.Empty;
                
                if (!string.IsNullOrEmpty(release))
                {
                    var versionLine = release.Split('\n')
                        .FirstOrDefault(line => line.StartsWith("VERSION="));
                    if (versionLine != null)
                    {
                        return versionLine.Split('=')[1].Trim('"');
                    }
                }
                
                return Environment.OSVersion.VersionString;
            }
            
            return Environment.OSVersion.VersionString;
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取.NET版本
    /// </summary>
    /// <returns>.NET版本</returns>
    private static string GetDotNetVersion()
    {
        try
        {
            var version = RuntimeInformation.FrameworkDescription;
            return version.Replace(".NET ", "").Replace("Core", "");
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取系统名称
    /// </summary>
    /// <returns>系统名称</returns>
    private static string GetSystemName()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                return "Windows";
            }
            else if (OperatingSystem.IsMacOS())
            {
                return "macOS";
            }
            else if (OperatingSystem.IsLinux())
            {
                var release = File.Exists("/etc/os-release") 
                    ? File.ReadAllText("/etc/os-release") 
                    : string.Empty;
                
                if (!string.IsNullOrEmpty(release))
                {
                    var nameLine = release.Split('\n')
                        .FirstOrDefault(line => line.StartsWith("NAME="));
                    if (nameLine != null)
                    {
                        return nameLine.Split('=')[1].Trim('"');
                    }
                }
                
                return "Linux";
            }
            
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 检查平台是否受支持
    /// </summary>
    /// <param name="platform">平台类型</param>
    /// <returns>是否受支持</returns>
    public static bool IsPlatformSupported(Platform platform)
    {
        return platform switch
        {
            Platform.Windows => OperatingSystem.IsWindows(),
            Platform.MacOS => OperatingSystem.IsMacOS(),
            Platform.Linux => OperatingSystem.IsLinux(),
            _ => false
        };
    }

    /// <summary>
    /// 获取当前平台特性
    /// </summary>
    /// <returns>平台特性字典</returns>
    public static Dictionary<string, bool> GetPlatformCapabilities()
    {
        var capabilities = new Dictionary<string, bool>
        {
            ["IsWindows"] = OperatingSystem.IsWindows(),
            ["IsMacOS"] = OperatingSystem.IsMacOS(),
            ["IsLinux"] = OperatingSystem.IsLinux(),
            ["Is64Bit"] = Environment.Is64BitOperatingSystem,
            ["Is64BitProcess"] = Environment.Is64BitProcess,
            ["IsBrowser"] = OperatingSystem.IsBrowser(),
            ["IsAndroid"] = OperatingSystem.IsAndroid(),
            ["IsIOS"] = OperatingSystem.IsIOS(),
            ["IsFreeBSD"] = OperatingSystem.IsFreeBSD(),
            ["IsLinuxBased"] = OperatingSystem.IsLinux() || OperatingSystem.IsAndroid() || OperatingSystem.IsFreeBSD()
        };

        return capabilities;
    }
}