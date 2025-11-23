// encoding: utf-8
// FireWithMoney - Buy Ammo Anytime, Anywhere
// Copyright (c) 2025 Shadowrabbit
// Licensed under the MIT License

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FireWithMoney.Config
{
    /// <summary>
    /// YAML 配置加载器（简化版，无需外部依赖）
    /// </summary>
    public static class YamlConfigLoader
    {
        /// <summary>
        /// 从 YAML 文件加载子弹配置
        /// </summary>
        public static List<BulletConfigData> LoadBulletConfig(string configPath)
        {
            var bulletConfigs = new List<BulletConfigData>();

            try
            {
                if (!File.Exists(configPath))
                {
                    Debug.LogError($"[FireWithMoney] 配置文件不存在: {configPath}");
                    return bulletConfigs;
                }

                string[] lines = File.ReadAllLines(configPath);
                
                int? currentId = null;
                string currentName = null;
                int? currentPrice = null;
                bool? currentDisplay = null;

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    
                    // 跳过注释和空行
                    if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                        continue;

                    // 检测新的配置项开始（以 "- id:" 开头）
                    if (trimmedLine.StartsWith("- id:"))
                    {
                        // 保存上一个配置项
                        if (currentId.HasValue && !string.IsNullOrEmpty(currentName) && 
                            currentPrice.HasValue && currentDisplay.HasValue)
                        {
                            bulletConfigs.Add(new BulletConfigData(
                                currentId.Value, 
                                currentName, 
                                currentPrice.Value, 
                                currentDisplay.Value
                            ));
                        }

                        // 重置当前配置项
                        currentId = ParseIntValue(trimmedLine, "- id:");
                        currentName = null;
                        currentPrice = null;
                        currentDisplay = null;
                    }
                    else if (trimmedLine.StartsWith("name:"))
                    {
                        currentName = ParseStringValue(trimmedLine, "name:");
                    }
                    else if (trimmedLine.StartsWith("price:"))
                    {
                        currentPrice = ParseIntValue(trimmedLine, "price:");
                    }
                    else if (trimmedLine.StartsWith("display:"))
                    {
                        currentDisplay = ParseBoolValue(trimmedLine, "display:");
                    }
                }

                // 保存最后一个配置项
                if (currentId.HasValue && !string.IsNullOrEmpty(currentName) && 
                    currentPrice.HasValue && currentDisplay.HasValue)
                {
                    bulletConfigs.Add(new BulletConfigData(
                        currentId.Value, 
                        currentName, 
                        currentPrice.Value, 
                        currentDisplay.Value
                    ));
                }

                Debug.Log($"[FireWithMoney] 成功加载 {bulletConfigs.Count} 个子弹配置");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FireWithMoney] 加载配置文件失败: {ex.Message}");
            }

            return bulletConfigs;
        }

        private static int? ParseIntValue(string line, string prefix)
        {
            try
            {
                string value = line.Substring(prefix.Length).Trim();
                if (int.TryParse(value, out int result))
                    return result;
            }
            catch { }
            return null;
        }

        private static string ParseStringValue(string line, string prefix)
        {
            try
            {
                string value = line.Substring(prefix.Length).Trim();
                // 移除引号
                value = value.Trim('"', '\'');
                return value;
            }
            catch { }
            return null;
        }

        private static bool? ParseBoolValue(string line, string prefix)
        {
            try
            {
                string value = line.Substring(prefix.Length).Trim().ToLower();
                if (value == "true")
                    return true;
                if (value == "false")
                    return false;
            }
            catch { }
            return null;
        }
    }
}
