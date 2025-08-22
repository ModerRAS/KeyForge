using System;
using System.IO;
using System.Threading.Tasks;
using KeyForge.Domain.Common;
using KeyForge.Domain.Interfaces;

namespace KeyForge.Infrastructure.Services
{
    /// <summary>
    /// 文件存储服务实现
    /// 
    /// 原本实现：支持多种存储后端（本地文件系统、云存储、数据库等）
    /// 简化实现：基本的本地文件系统存储
    /// 
    /// 优化建议：
    /// 1. 添加云存储支持（Azure Blob Storage、AWS S3等）
    /// 2. 实现文件版本控制
    /// 3. 添加文件加密和压缩
    /// 4. 支持文件元数据存储
    /// </summary>
    public class FileStorageService : IFileStorageService
    {
        private readonly string _basePath;
        private readonly FileStorageOptions _options;

        public FileStorageService(string basePath, FileStorageOptions options)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            
            // 确保基础目录存在
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<Result<string>> SaveAsync(string fileName, byte[] data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return Result<string>.Failure("文件名不能为空");
                }

                if (data == null || data.Length == 0)
                {
                    return Result<string>.Failure("文件数据不能为空");
                }

                // 验证文件大小
                if (_options.MaxFileSize > 0 && data.Length > _options.MaxFileSize)
                {
                    return Result<string>.Failure($"文件大小超过限制: {data.Length} > {_options.MaxFileSize}");
                }

                // 验证文件扩展名
                if (_options.AllowedExtensions.Count > 0)
                {
                    var extension = Path.GetExtension(fileName).ToLowerInvariant();
                    if (!_options.AllowedExtensions.Contains(extension))
                    {
                        return Result<string>.Failure($"不允许的文件扩展名: {extension}");
                    }
                }

                // 创建安全的文件路径
                var safeFileName = GetSafeFileName(fileName);
                var filePath = Path.Combine(_basePath, safeFileName);
                
                // 确保目录存在
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 写入文件
                await File.WriteAllBytesAsync(filePath, data);

                return Result<string>.Success(filePath);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"保存文件失败: {ex.Message}");
            }
        }

        public async Task<Result<byte[]>> LoadAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return Result<byte[]>.Failure("文件名不能为空");
                }

                var filePath = Path.Combine(_basePath, fileName);
                
                if (!File.Exists(filePath))
                {
                    return Result<byte[]>.Failure($"文件不存在: {fileName}");
                }

                var data = await File.ReadAllBytesAsync(filePath);
                return Result<byte[]>.Success(data);
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Failure($"加载文件失败: {ex.Message}");
            }
        }

        public async Task<Result<bool>> ExistsAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return Result<bool>.Failure("文件名不能为空");
                }

                var filePath = Path.Combine(_basePath, fileName);
                var exists = File.Exists(filePath);
                
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"检查文件存在性失败: {ex.Message}");
            }
        }

        public async Task<Result> DeleteAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return Result.Failure("文件名不能为空");
                }

                var filePath = Path.Combine(_basePath, fileName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"删除文件失败: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<string>>> GetAllFilesAsync()
        {
            try
            {
                var files = Directory.GetFiles(_basePath, "*.*", SearchOption.AllDirectories);
                var relativeFiles = files.Select(f => GetRelativePath(f, _basePath));
                
                return Result<IEnumerable<string>>.Success(relativeFiles);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<string>>.Failure($"获取文件列表失败: {ex.Message}");
            }
        }

        public async Task<Result<long>> GetFileSizeAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return Result<long>.Failure("文件名不能为空");
                }

                var filePath = Path.Combine(_basePath, fileName);
                
                if (!File.Exists(filePath))
                {
                    return Result<long>.Failure($"文件不存在: {fileName}");
                }

                var fileInfo = new FileInfo(filePath);
                return Result<long>.Success(fileInfo.Length);
            }
            catch (Exception ex)
            {
                return Result<long>.Failure($"获取文件大小失败: {ex.Message}");
            }
        }

        public async Task<Result<DateTime>> GetFileLastModifiedAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return Result<DateTime>.Failure("文件名不能为空");
                }

                var filePath = Path.Combine(_basePath, fileName);
                
                if (!File.Exists(filePath))
                {
                    return Result<DateTime>.Failure($"文件不存在: {fileName}");
                }

                var fileInfo = new FileInfo(filePath);
                return Result<DateTime>.Success(fileInfo.LastWriteTime);
            }
            catch (Exception ex)
            {
                return Result<DateTime>.Failure($"获取文件修改时间失败: {ex.Message}");
            }
        }

        public async Task<Result> CreateDirectoryAsync(string directoryPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(directoryPath))
                {
                    return Result.Failure("目录路径不能为空");
                }

                var fullPath = Path.Combine(_basePath, directoryPath);
                
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"创建目录失败: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DirectoryExistsAsync(string directoryPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(directoryPath))
                {
                    return Result<bool>.Failure("目录路径不能为空");
                }

                var fullPath = Path.Combine(_basePath, directoryPath);
                var exists = Directory.Exists(fullPath);
                
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"检查目录存在性失败: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<string>>> GetDirectoriesAsync(string directoryPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(directoryPath))
                {
                    return Result<IEnumerable<string>>.Failure("目录路径不能为空");
                }

                var fullPath = Path.Combine(_basePath, directoryPath);
                
                if (!Directory.Exists(fullPath))
                {
                    return Result<IEnumerable<string>>.Failure($"目录不存在: {directoryPath}");
                }

                var directories = Directory.GetDirectories(fullPath);
                var relativeDirectories = directories.Select(d => GetRelativePath(d, _basePath));
                
                return Result<IEnumerable<string>>.Success(relativeDirectories);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<string>>.Failure($"获取目录列表失败: {ex.Message}");
            }
        }

        private string GetSafeFileName(string fileName)
        {
            // 移除路径遍历字符
            var safeName = fileName.Replace("..", "").Replace("\\", "/").Trim('/');
            
            // 移除非法字符
            var invalidChars = Path.GetInvalidFileNameChars();
            safeName = string.Join("_", safeName.Split(invalidChars));
            
            return safeName;
        }

        private string GetRelativePath(string fullPath, string basePath)
        {
            return fullPath.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar);
        }
    }

    /// <summary>
    /// 文件存储选项
    /// 
    /// 原本实现：复杂的配置选项和验证
    /// 简化实现：基本配置选项
    /// </summary>
    public class FileStorageOptions
    {
        public long MaxFileSize { get; set; } = 100 * 1024 * 1024; // 100MB
        public HashSet<string> AllowedExtensions { get; set; } = new();
        public bool EnableCompression { get; set; } = false;
        public bool EnableEncryption { get; set; } = false;
        public string EncryptionKey { get; set; } = string.Empty;
    }
}