// encoding: utf-8
// FireWithMoney - Buy Ammo Anytime, Anywhere
// Copyright (c) 2025 Shadowrabbit
// Licensed under the MIT License

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FireWithMoney.Config
{
    /// <summary>
    /// 子弹价格和等级配置
    /// </summary>
    public static class BulletConfig
    {
        private static bool _isConfigLoaded = false;
        private static Dictionary<int, BulletConfigData> _bulletConfigCache = new Dictionary<int, BulletConfigData>();

        /// <summary>
        /// 默认每发子弹消耗
        /// </summary>
        public const int DefaultCostPerShot = 10;

        /// <summary>
        /// 不同子弹类型的消耗配置（每发子弹的价格）
        /// </summary>
        public static Dictionary<int, int> BulletTypeCosts
        {
            get
            {
                LoadConfigIfNeeded();
                return _bulletConfigCache.ToDictionary(kv => kv.Key, kv => kv.Value.Price);
            }
        }

        /// <summary>
        /// 加载配置文件（如果尚未加载）
        /// </summary>
        private static void LoadConfigIfNeeded()
        {
            if (_isConfigLoaded)
                return;

            _isConfigLoaded = true;
            _bulletConfigCache.Clear();

            // 构建配置文件路径
            string modDirectory = Path.GetDirectoryName(typeof(BulletConfig).Assembly.Location);
            string configPath = Path.Combine(modDirectory, "bullet_config.yaml");

            Debug.Log($"[FireWithMoney] 尝试加载配置文件: {configPath}");

            // 如果配置文件不存在，则复制一份配置文件模板过去
            if (!File.Exists(configPath))
            {
                string templatePath = Path.Combine(modDirectory, "bullet_config_template.yaml");
                if (File.Exists(templatePath))
                {
                    File.Copy(templatePath, configPath);
                    Debug.Log($"[FireWithMoney] 配置文件不存在，已复制模板到: {configPath}");
                }
                else
                {
                    Debug.LogWarning($"[FireWithMoney] 配置文件和模板均不存在: {configPath}，{templatePath}");
                }
            }

            // 加载 YAML 配置
            var bulletConfigs = YamlConfigLoader.LoadBulletConfig(configPath);
            
            if (bulletConfigs != null && bulletConfigs.Count > 0)
            {
                foreach (var config in bulletConfigs)
                {
                    _bulletConfigCache[config.Id] = config;
                }
                Debug.Log($"[FireWithMoney] 配置加载完成，共 {_bulletConfigCache.Count} 个子弹类型");
            }
            else
            {
                // 使用后备硬编码配置
                Debug.LogWarning("[FireWithMoney] YAML 配置加载失败或为空，使用后备硬编码配置");
                foreach (var kv in _fallbackBulletTypeCosts)
                {
                    _bulletConfigCache[kv.Key] = new BulletConfigData(kv.Key, $"Bullet_{kv.Key}", kv.Value, true);
                }
                Debug.Log($"[FireWithMoney] 后备配置加载完成，共 {_bulletConfigCache.Count} 个子弹类型");

            }
        }

        // 以下为后备硬编码配置（如果 YAML 加载失败）
        private static readonly Dictionary<int, int> _fallbackBulletTypeCosts = new Dictionary<int, int>
        {
            // S弹系列
            { 594, 1 },    // S弹 - 生锈弹
            { 595, 5 },    // S弹 - 普通弹
            { 597, 30 },   // S弹 - 穿甲弹
            { 598, 84 },   // S弹 - 高级穿甲弹
            { 691, 144 },  // S弹 - 特种穿甲弹
            // 鸭区突围
            { 4300003, 186 },   // S-超压穿甲手枪弹 
            // J-lab扩展包
            { 20250626, 50 },   // S-肉伤弹
            { 20250612, 90 },   // S-碎甲弹
            { 20250627, 280 },  // S-钝伤弹
            // T-7
            { 1380011, 3 },     // S-量产穿甲弹
            { 1380047, 46 },    // S-猎杀者穿甲弹
            { 1380411, 2080 },  // S-特种爆破弹

            // AR弹系列
            { 603, 1 },    // AR弹 - 生锈弹
            { 604, 7 },    // AR弹 - 普通弹
            { 606, 43 },   // AR弹 - 穿甲弹
            { 607, 120 },  // AR弹 - 高级穿甲弹
            { 694, 208 },  // AR弹 - 特种穿甲弹
            // 鸭区突围
            { 4300001, 436 },   // AR-碳化钨芯穿甲弹
            // J-lab扩展包
            { 20250661, 80 },   // AR-肉伤弹
            { 20250658, 150 },  // AR-碎甲弹
            { 20250655, 400 },  // AR-钝伤弹
            // T-7
            { 1380031, 37 }, // AR-猎杀者穿甲弹

            // L弹系列
            { 612, 2 },    // L弹 - 生锈弹
            { 613, 12 },   // L弹 - 普通弹
            { 615, 75 },   // L弹 - 穿甲弹
            { 616, 210 },  // L弹 - 高级穿甲弹
            { 698, 365 },  // L弹 - 特种穿甲弹
            // 鸭区突围
            { 4300002, 456 },   // BR-硬化钢芯穿甲弹
            // J-lab扩展包
            { 20250663, 200 },  // L-肉伤弹
            { 20250660, 350 },  // L-碎甲弹
            { 20250656, 700 },  // L-钝伤弹
            // T-7
            { 1380023, 52 },    // L-猎杀者穿甲弹

            // MAG弹系列
            { 640, 56 },    // MAG弹 - 普通弹
            { 708, 168 },   // MAG弹 - 穿甲弹
            { 709, 560 },   // MAG弹 - 高级穿甲弹
            { 710, 1664 },  // MAG弹 - 特种穿甲弹
            // 鸭区突围
            { 4300006, 1248 },   // MAG-全被甲硬心穿甲弹
            // T-7
            { 1380013, 2080 },   // MAG-重型穿甲弹

            // 狙击弹系列
            { 621, 5 },     // 狙击弹 - 生锈弹
            { 622, 35 },    // 狙击弹 - 普通弹
            { 700, 105 },   // 狙击弹 - 穿甲弹
            { 701, 350 },   // 狙击弹 - 高级穿甲弹
            { 702, 1040},   // 狙击弹 - 特种穿甲弹
            // 鸭区突围
            { 4300005, 1040 },   // SNP-实心铜空尖弹头穿甲弹
            // T-7
            { 1380043, 47 },     // 猎杀者穿甲狙击弹

            // 霰弹系列
            { 630, 3 },     // 霰弹 - 生锈弹
            { 631, 21 },    // 霰弹 - 普通弹
            { 633, 126 },   // 霰弹 - 穿甲弹
            { 634, 360 },   // 霰弹 - 高级穿甲弹
            { 707, 624 },   // 霰弹 - 特种穿甲弹
            // 鸭区突围
            { 4300004, 768 },   // SHT-钨芯散射穿甲霰弹
            // J-lab扩展包
            { 20250662, 180 },  // SHT-肉伤弹：霰弹
            { 20250659, 400 },  // SHT-碎甲弹：霰弹
            { 20250657, 1200 }, // SHT-钝伤弹：霰弹
            // T-7
            { 1380045, 47 },    // 猎杀者穿甲霰弹

            // 箭
            { 648, 3 },     // 木矢
            { 870, 98 },    // 低级穿甲箭
            { 871, 326 },   // 中级穿甲箭
            { 649, 520 },   // 爆炸矢
            // J-lab扩展包
            { 20250616, 2000 }, // 穿透箭矢

            // 能量弹
            { 650,  26},    // 小能量弹
            { 1162, 186 },  // 强化小能量弹
            { 918,  52 },   // 大型能量弹
            // T-7
            { 1380049, 4 },     // 量产小型能量弹
            { 1380035, 5 },     // 量产大型能量弹

            // 火箭弹
            { 326, 520 },   // 火箭弹
            // T-7
            { 1380019, 1560 },  // 加强火箭弹

            // 粑粑
            { 944, 1 },     // 粑粑弹
            
            // J-lab扩展包 - Candy
            { 20250647, 20 },   // 糖豆子弹
        };

        /// <summary>
        /// 获取指定子弹是否显示在 T 键的弹药切换列表中
        /// </summary>
        public static bool IsBulletShownInSwitchList(int bulletTypeID)
        {
            LoadConfigIfNeeded();
            
            if (_bulletConfigCache.TryGetValue(bulletTypeID, out var config))
            {
                return config.Display;
            }
            
            // 如果配置中没有，默认显示
            return true;
        }

        /// <summary>
        /// 获取指定子弹类型的价格
        /// </summary>
        public static int GetCostForBulletType(int bulletTypeID)
        {
            LoadConfigIfNeeded();
            
            if (_bulletConfigCache.TryGetValue(bulletTypeID, out var config))
            {
                return config.Price;
            }
            
            // 如果配置中没有，尝试后备配置
            if (_fallbackBulletTypeCosts.TryGetValue(bulletTypeID, out int cost))
            {
                return cost;
            }
            
            return DefaultCostPerShot;
        }

        /// <summary>
        /// 获取指定子弹的等级排序
        /// </summary>
        public static int GetTierOrder(int bulletTypeID)
        {
            LoadConfigIfNeeded();
            
            if (_bulletConfigCache.TryGetValue(bulletTypeID, out var config))
            {
                return config.Price; // 使用价格作为排序依据
            }
            
            // 如果配置中没有，尝试后备配置
            if (_fallbackBulletTypeCosts.TryGetValue(bulletTypeID, out int tier_of_cost))
            {
                return tier_of_cost;
            }
            
            return 9999;
        }
    }
}
