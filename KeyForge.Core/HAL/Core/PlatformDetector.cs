using System;
using System.Runtime.InteropServices;
using KeyForge.HAL.Abstractions;

namespace KeyForge.HAL.Core
{
    /// <summary>
    /// 平台检测器
    /// </summary>
    public static class PlatformDetector
    {
        /// <summary>
        /// 检测当前平台
        /// </summary>
        /// <returns>平台类型</returns>
        public static Platform DetectPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Platform.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return Platform.MacOS;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Platform.Linux;
            else
                return Platform.Unknown;
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
                Platform.Windows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
                Platform.MacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
                Platform.Linux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                _ => false
            };
        }
        
        /// <summary>
        /// 获取平台信息
        /// </summary>
        /// <param name="platform">平台类型</param>
        /// <returns>平台信息</returns>
        public static PlatformInfo GetPlatformInfo(Platform platform)
        {
            return platform switch
            {
                Platform.Windows => GetWindowsInfo(),
                Platform.MacOS => GetMacOSInfo(),
                Platform.Linux => GetLinuxInfo(),
                _ => throw new PlatformNotSupportedException($"Platform {platform} is not supported")
            };
        }
        
        /// <summary>
        /// 获取当前平台信息
        /// </summary>
        /// <returns>平台信息</returns>
        public static PlatformInfo GetCurrentPlatformInfo()
        {
            var platform = DetectPlatform();
            return GetPlatformInfo(platform);
        }
        
        /// <summary>
        /// 获取Windows平台信息
        /// </summary>
        /// <returns>平台信息</returns>
        private static PlatformInfo GetWindowsInfo()
        {
            var capabilities = new PlatformCapabilities
            {
                SupportsGlobalHotkeys = true,
                SupportsLowLevelKeyboardHook = true,
                SupportsLowLevelMouseHook = true,
                SupportsMultipleDisplays = true,
                SupportsWindowTransparency = true,
                SupportsWindowTopmost = true,
                SupportsScreenRecording = true,
                SupportsAccessibility = true
            };
            
            return new PlatformInfo
            {
                Platform = Platform.Windows,
                Name = "Windows",
                Version = GetWindowsVersion(),
                Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                Capabilities = capabilities
            };
        }
        
        /// <summary>
        /// 获取macOS平台信息
        /// </summary>
        /// <returns>平台信息</returns>
        private static PlatformInfo GetMacOSInfo()
        {
            var capabilities = new PlatformCapabilities
            {
                SupportsGlobalHotkeys = true,
                SupportsLowLevelKeyboardHook = true,
                SupportsLowLevelMouseHook = true,
                SupportsMultipleDisplays = true,
                SupportsWindowTransparency = true,
                SupportsWindowTopmost = true,
                SupportsScreenRecording = true,
                SupportsAccessibility = true
            };
            
            return new PlatformInfo
            {
                Platform = Platform.MacOS,
                Name = "macOS",
                Version = GetMacOSVersion(),
                Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                Capabilities = capabilities
            };
        }
        
        /// <summary>
        /// 获取Linux平台信息
        /// </summary>
        /// <returns>平台信息</returns>
        private static PlatformInfo GetLinuxInfo()
        {
            var capabilities = new PlatformCapabilities
            {
                SupportsGlobalHotkeys = true,
                SupportsLowLevelKeyboardHook = true,
                SupportsLowLevelMouseHook = true,
                SupportsMultipleDisplays = true,
                SupportsWindowTransparency = true,
                SupportsWindowTopmost = true,
                SupportsScreenRecording = true,
                SupportsAccessibility = true
            };
            
            return new PlatformInfo
            {
                Platform = Platform.Linux,
                Name = "Linux",
                Version = GetLinuxVersion(),
                Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                Capabilities = capabilities
            };
        }
        
        /// <summary>
        /// 获取Windows版本
        /// </summary>
        /// <returns>版本字符串</returns>
        private static string GetWindowsVersion()
        {
            try
            {
                var version = Environment.OSVersion.Version;
                var versionString = version.ToString();
                
                // 添加Windows版本名称
                if (version.Major == 10 && version.Build >= 22000)
                    versionString += " (Windows 11)";
                else if (version.Major == 10)
                    versionString += " (Windows 10)";
                else if (version.Major == 6 && version.Minor == 3)
                    versionString += " (Windows 8.1)";
                else if (version.Major == 6 && version.Minor == 2)
                    versionString += " (Windows 8)";
                else if (version.Major == 6 && version.Minor == 1)
                    versionString += " (Windows 7)";
                else if (version.Major == 6 && version.Minor == 0)
                    versionString += " (Windows Vista)";
                else if (version.Major == 5 && version.Minor == 1)
                    versionString += " (Windows XP)";
                
                return versionString;
            }
            catch
            {
                return Environment.OSVersion.VersionString;
            }
        }
        
        /// <summary>
        /// 获取macOS版本
        /// </summary>
        /// <returns>版本字符串</returns>
        private static string GetMacOSVersion()
        {
            try
            {
                // 在macOS上，我们可以通过系统调用来获取版本信息
                // 这里使用Environment.OSVersion作为基础
                var version = Environment.OSVersion.Version;
                return $"macOS {version.Major}.{version.Minor}.{version.Build}";
            }
            catch
            {
                return Environment.OSVersion.VersionString;
            }
        }
        
        /// <summary>
        /// 获取Linux版本
        /// </summary>
        /// <returns>版本字符串</returns>
        private static string GetLinuxVersion()
        {
            try
            {
                // 尝试读取/proc/version文件
                if (System.IO.File.Exists("/proc/version"))
                {
                    var versionInfo = System.IO.File.ReadAllText("/proc/version");
                    var lines = versionInfo.Split('\n');
                    if (lines.Length > 0)
                    {
                        return lines[0].Trim();
                    }
                }
                
                // 尝试读取/etc/os-release文件
                if (System.IO.File.Exists("/etc/os-release"))
                {
                    var lines = System.IO.File.ReadAllLines("/etc/os-release");
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("PRETTY_NAME="))
                        {
                            var prettyName = line.Substring("PRETTY_NAME=".Length).Trim('"');
                            return prettyName;
                        }
                    }
                }
                
                return Environment.OSVersion.VersionString;
            }
            catch
            {
                return Environment.OSVersion.VersionString;
            }
        }
        
        /// <summary>
        /// 检查是否为管理员权限
        /// </summary>
        /// <returns>是否为管理员</returns>
        public static bool IsRunningAsAdmin()
        {
            try
            {
                var platform = DetectPlatform();
                return platform switch
                {
                    Platform.Windows => IsRunningAsAdminWindows(),
                    Platform.MacOS => IsRunningAsAdminMacOS(),
                    Platform.Linux => IsRunningAsAdminLinux(),
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 检查Windows管理员权限
        /// </summary>
        /// <returns>是否为管理员</returns>
        private static bool IsRunningAsAdminWindows()
        {
            using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
            {
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
        }
        
        /// <summary>
        /// 检查macOS管理员权限
        /// </summary>
        /// <returns>是否为管理员</returns>
        private static bool IsRunningAsAdminMacOS()
        {
            try
            {
                // 在macOS上，检查当前用户是否在admin组中
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "/usr/bin/id",
                        Arguments = "-Gn",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                return output.Contains("admin");
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 检查Linux管理员权限
        /// </summary>
        /// <returns>是否为管理员</returns>
        private static bool IsRunningAsAdminLinux()
        {
            try
            {
                // 在Linux上，检查当前用户是否为root
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "/usr/bin/id",
                        Arguments = "-u",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                return output.Trim() == "0";
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 获取平台特定的路径分隔符
        /// </summary>
        /// <returns>路径分隔符</returns>
        public static string GetPathSeparator()
        {
            var platform = DetectPlatform();
            return platform switch
            {
                Platform.Windows => "\\",
                Platform.MacOS => "/",
                Platform.Linux => "/",
                _ => "/"
            };
        }
        
        /// <summary>
        /// 获取平台特定的环境变量路径分隔符
        /// </summary>
        /// <returns>环境变量路径分隔符</returns>
        public static string GetEnvironmentPathSeparator()
        {
            var platform = DetectPlatform();
            return platform switch
            {
                Platform.Windows => ";",
                Platform.MacOS => ":",
                Platform.Linux => ":",
                _ => ":"
            };
        }
    }
}