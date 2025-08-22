using System;
using System.Collections.Generic;

namespace KeyForge.Core.Domain.Automation
{
    /// <summary>
    /// 脚本仓库接口
    /// </summary>
    public interface IScriptRepository
    {
        /// <summary>
        /// 获取所有脚本
        /// </summary>
        IEnumerable<Script> GetAllScripts();

        /// <summary>
        /// 根据ID获取脚本
        /// </summary>
        Script GetScriptById(Guid scriptId);

        /// <summary>
        /// 保存脚本
        /// </summary>
        void SaveScript(Script script);

        /// <summary>
        /// 保存脚本到指定文件
        /// </summary>
        void SaveScript(Script script, string filePath);

        /// <summary>
        /// 从文件加载脚本
        /// </summary>
        Script LoadScript(string filePath);

        /// <summary>
        /// 删除脚本
        /// </summary>
        void DeleteScript(Guid scriptId);

        /// <summary>
        /// 检查脚本是否存在
        /// </summary>
        bool ScriptExists(Guid scriptId);
    }
}