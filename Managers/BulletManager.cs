// encoding: utf-8
// FireWithMoney - Buy Ammo Anytime, Anywhere
// Copyright (c) 2025 Shadowrabbit
// Licensed under the MIT License

using System.Collections.Generic;
using System.Linq;
using ItemStatsSystem;
using FireWithMoney.Config;

namespace FireWithMoney.Managers
{
    /// <summary>
    /// 子弹管理器
    /// </summary>
    public class BulletManager
    {
        /// <summary>
        /// 获取指定口径的所有子弹类型
        /// </summary>
        public List<int> GetAllBulletTypesForCaliber(string caliber)
        {
            var allBullets = new List<int>();
            int caliberHash = "Caliber".GetHashCode();
            
            foreach (var bulletTypeID in BulletConfig.BulletTypeCosts.Keys)
            {
                // 检查是否应该显示该子弹
                if (!BulletConfig.IsBulletShownInSwitchList(bulletTypeID))
                    continue;
                
                try
                {
                    var prefab = ItemAssetsCollection.GetPrefab(bulletTypeID);
                    if (prefab != null)
                    {
                        var bulletCaliber = prefab.Constants.GetString(caliberHash, null);
                        if (bulletCaliber == caliber)
                        {
                            allBullets.Add(bulletTypeID);
                        }
                    }
                }
                catch { }
            }
            
            // 按等级排序（生锈弹→普通弹→穿甲弹→高级穿甲弹→特种穿甲弹）
            allBullets.Sort((a, b) =>
            {
                int tierA = BulletConfig.GetTierOrder(a);
                int tierB = BulletConfig.GetTierOrder(b);
                return tierA.CompareTo(tierB);
            });
            
            return allBullets;
        }

        /// <summary>
        /// 对子弹类型字典按等级排序
        /// </summary>
        public Dictionary<int, BulletTypeInfo> SortBulletTypesByTier(Dictionary<int, BulletTypeInfo> bulletTypes)
        {
            var sortedDict = new Dictionary<int, BulletTypeInfo>();
            var sortedKeys = bulletTypes.Keys.ToList();
            sortedKeys.Sort((a, b) =>
            {
                int tierA = BulletConfig.GetTierOrder(a);
                int tierB = BulletConfig.GetTierOrder(b);
                return tierA.CompareTo(tierB);
            });
            
            foreach (var key in sortedKeys)
            {
                sortedDict[key] = bulletTypes[key];
            }
            
            return sortedDict;
        }
    }
}
