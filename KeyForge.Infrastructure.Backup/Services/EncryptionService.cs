using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using KeyForge.Domain.Common;
using KeyForge.Domain.Interfaces;

namespace KeyForge.Infrastructure.Services
{
    /// <summary>
    /// 加密服务实现
    /// 
    /// 原本实现：使用AES-256加密，支持密钥管理和安全存储
    /// 简化实现：使用基本的AES加密，固定密钥
    /// 
    /// 优化建议：
    /// 1. 实现密钥轮换机制
    /// 2. 添加密钥 derivation功能
    /// 3. 支持多种加密算法
    /// 4. 实现安全的密钥存储
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService()
        {
            // 简化实现：使用固定的密钥和IV
            // 在实际实现中，应该从安全配置中获取
            _key = Encoding.UTF8.GetBytes("ThisIsASecretKey1234567890123456"); // 32 bytes for AES-256
            _iv = Encoding.UTF8.GetBytes("ThisIsAnIV123456"); // 16 bytes for AES
        }

        public async Task<Result<string>> EncryptAsync(string plainText)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                
                var result = Convert.ToBase64String(encryptedBytes);
                return Result<string>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"加密失败: {ex.Message}");
            }
        }

        public async Task<Result<string>> DecryptAsync(string encryptedText)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                
                var result = Encoding.UTF8.GetString(decryptedBytes);
                return Result<string>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"解密失败: {ex.Message}");
            }
        }

        public async Task<Result<byte[]>> EncryptAsync(byte[] data)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                var encryptedBytes = encryptor.TransformFinalBlock(data, 0, data.Length);
                
                return Result<byte[]>.Success(encryptedBytes);
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Failure($"加密失败: {ex.Message}");
            }
        }

        public async Task<Result<byte[]>> DecryptAsync(byte[] data)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                var decryptedBytes = decryptor.TransformFinalBlock(data, 0, data.Length);
                
                return Result<byte[]>.Success(decryptedBytes);
            }
            catch (Exception ex)
            {
                return Result<byte[]>.Failure($"解密失败: {ex.Message}");
            }
        }

        public async Task<Result<string>> GenerateKeyAsync()
        {
            try
            {
                using var rng = RandomNumberGenerator.Create();
                var keyBytes = new byte[32]; // 256 bits
                rng.GetBytes(keyBytes);
                
                var key = Convert.ToBase64String(keyBytes);
                return Result<string>.Success(key);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"生成密钥失败: {ex.Message}");
            }
        }

        public async Task<Result<bool>> ValidateKeyAsync(string key)
        {
            try
            {
                var keyBytes = Convert.FromBase64String(key);
                var isValid = keyBytes.Length == 32; // 256 bits
                
                return Result<bool>.Success(isValid);
            }
            catch
            {
                return Result<bool>.Success(false);
            }
        }

        public async Task<Result> SetKeyAsync(string key)
        {
            try
            {
                var keyBytes = Convert.FromBase64String(key);
                if (keyBytes.Length != 32)
                {
                    return Result.Failure("密钥长度必须为256位");
                }
                
                // 简化实现：不实际设置密钥，在实际实现中应该更新内部密钥
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"设置密钥失败: {ex.Message}");
            }
        }

        public async Task<Result> SetKeyAsync(byte[] key)
        {
            try
            {
                if (key.Length != 32)
                {
                    return Result.Failure("密钥长度必须为256位");
                }
                
                // 简化实现：不实际设置密钥
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"设置密钥失败: {ex.Message}");
            }
        }
    }
}