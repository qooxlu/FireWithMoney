// encoding: utf-8

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Duckov.Modding;
using Duckov.Economy;
using ItemStatsSystem;
using HarmonyLib;
using UnityEngine;

namespace FireWithMoney
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        public static ModBehaviour Instance;
        
        // 默认消耗
        public int DefaultCostPerShot = 10;
        
        // 不同子弹类型的消耗配置（每发子弹的价格）
        public Dictionary<int, int> BulletTypeCosts = new Dictionary<int, int>
        {
            // S弹系列
            { 594, 1 },    // S弹 - 生锈弹
            { 595, 5 },    // S弹 - 普通弹
            { 597, 30 },   // S弹 - 穿甲弹
            { 598, 84 },   // S弹 - 高级穿甲弹
            { 691, 144 },  // S弹 - 特种穿甲弹

            // AR弹系列
            { 603, 1 },    // AR弹 - 生锈弹
            { 604, 7 },    // AR弹 - 普通弹
            { 606, 43 },   // AR弹 - 穿甲弹
            { 607, 120 },  // AR弹 - 高级穿甲弹
            { 694, 208 },  // AR弹 - 特种穿甲弹

            // L弹系列
            { 612, 2 },    // L弹 - 生锈弹
            { 613, 12 },   // L弹 - 普通弹
            { 615, 75 },   // L弹 - 穿甲弹
            { 616, 210 },  // L弹 - 高级穿甲弹
            { 698, 365 },  // L弹 - 特种穿甲弹

            // MAG弹系列
            { 640, 56 },    // MAG弹 - 普通弹
            { 708, 168 },   // MAG弹 - 穿甲弹
            { 709, 560 },   // MAG弹 - 高级穿甲弹
            { 710, 1664 },  // MAG弹 - 特种穿甲弹

            // 狙击弹系列
            { 621, 5 },     // 狙击弹 - 生锈弹
            { 622, 35 },    // 狙击弹 - 普通弹
            { 700, 105 },   // 狙击弹 - 穿甲弹
            { 701, 350 },   // 狙击弹 - 高级穿甲弹
            { 702, 1040},   // 狙击弹 - 特种穿甲弹

            // 霰弹系列
            { 630, 3 },     // 霰弹 - 生锈弹
            { 631, 21 },    // 霰弹 - 普通弹
            { 633, 126 },   // 霰弹 - 穿甲弹
            { 634, 360 },   // 霰弹 - 高级穿甲弹
            { 707, 624 },   // 霰弹 - 特种穿甲弹

            // 箭
            { 648, 3 },     // 木矢
            { 870, 300 },   // 低级穿甲箭
            { 871, 982 },   // 中级穿甲箭
            { 649, 1560 },  // 爆炸矢

            // 能量弹
            { 650,  26},    // 小能量弹
            { 1162, 186 },  // 强化小能量弹
            { 918,  52 },   // 大型能量弹

            // 火箭弹
            { 326, 520 },   // 火箭弹

            // 粑粑
            { 944, 1 },     // 粑粑弹
        };

        // 子弹等级排序（用于显示顺序）
        public Dictionary<int, int> BulletTierOrder = new Dictionary<int, int>
        {
            // S弹
            { 594, 0 }, { 595, 1 }, { 597, 2 }, { 598, 3 }, { 691, 4 },
            // AR弹
            { 603, 0 }, { 604, 1 }, { 606, 2 }, { 607, 3 }, { 694, 4 },
            // L弹
            { 612, 0 }, { 613, 1 }, { 615, 2 }, { 616, 3 }, { 698, 4 },
            // MAG弹
            { 640, 0 }, { 708, 1 }, { 709, 2 }, { 710, 3 },
            // 狙击弹
            { 621, 0 }, { 622, 1 }, { 700, 2 }, { 701, 3 }, { 702, 4 },
            // 霰弹
            { 630, 0 }, { 631, 1 }, { 633, 2 }, { 634, 3 }, { 707, 4 },
            // 箭
            { 648, 0 }, { 870, 1 }, { 871, 2 }, { 649, 3 },
            // 能量弹
            { 650, 0 }, { 1162, 1 }, { 918, 2 },
            // 火箭弹
            { 326, 0 },
            // 粑粑
            { 944, 0 },
        };

        private Harmony _harmony;

        protected override void OnAfterSetup()
        {
            Instance = this;
            _harmony = new Harmony("com.duckov.moneyfire");
            _harmony.PatchAll();
            Debug.Log("[FireWithMoney] Mod loaded! Bullet purchase system enabled");
            Debug.Log("[FireWithMoney] Bullet type prices configured:");
            foreach (var kvp in BulletTypeCosts)
            {
                Debug.Log($"  - Bullet {kvp.Key}: {kvp.Value} cash per round");
            }
        }

        protected override void OnBeforeDeactivate()
        {
            if (_harmony != null)
                _harmony.UnpatchAll("com.duckov.moneyfire");
            Debug.Log("[FireWithMoney] Mod unloaded");
        }

        public int GetCostForBulletType(int bulletTypeID)
        {
            if (BulletTypeCosts.TryGetValue(bulletTypeID, out int cost))
            {
                return cost;
            }
            return DefaultCostPerShot;
        }

        public bool HasEnoughMoney(int amount)
        {
            return EconomyManager.Money >= amount;
        }

        public bool TryDeductMoney(int amount)
        {
            long currentMoney = EconomyManager.Money;
            if (currentMoney >= amount)
            {
                var instance = EconomyManager.Instance;
                if (instance != null)
                {
                    var moneyField = typeof(EconomyManager).GetField("money", 
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    if (moneyField != null)
                    {
                        long newMoney = currentMoney - amount;
                        moneyField.SetValue(instance, newMoney);
                        
                        var onMoneyChangedEvent = typeof(EconomyManager).GetField("OnMoneyChanged",
                            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                        if (onMoneyChangedEvent != null)
                        {
                            var handler = onMoneyChangedEvent.GetValue(null) as Action<long, long>;
                            handler?.Invoke(currentMoney, newMoney);
                        }
                        
                        Debug.Log($"[FireWithMoney] Deducted {amount} from bank. Remaining: {newMoney}");
                        return true;
                    }
                }
            }
            return false;
        }

        public List<int> GetAllBulletTypesForCaliber(string caliber)
        {
            var allBullets = new List<int>();
            int caliberHash = "Caliber".GetHashCode();
            
            foreach (var bulletTypeID in BulletTypeCosts.Keys)
            {
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
                int tierA = BulletTierOrder.ContainsKey(a) ? BulletTierOrder[a] : 999;
                int tierB = BulletTierOrder.ContainsKey(b) ? BulletTierOrder[b] : 999;
                return tierA.CompareTo(tierB);
            });
            
            return allBullets;
        }
    }

    // Patch: 显示所有子弹类型并按等级排序
    [HarmonyPatch(typeof(ItemSetting_Gun), "GetBulletTypesInInventory")]
    public static class Patch_GetBulletTypesInInventory
    {
        static void Postfix(ItemSetting_Gun __instance, Inventory inventory, ref Dictionary<int, BulletTypeInfo> __result)
        {
            try
            {
                if (__instance == null || __instance.Item == null) return;
                var mod = ModBehaviour.Instance;
                if (mod == null) return;

                int caliberHash = "Caliber".GetHashCode();
                string caliber = __instance.Item.Constants.GetString(caliberHash, null);
                if (string.IsNullOrEmpty(caliber)) return;

                var allBulletTypes = mod.GetAllBulletTypesForCaliber(caliber);
                
                // 先添加缺失的子弹类型
                foreach (var bulletTypeID in allBulletTypes)
                {
                    if (!__result.ContainsKey(bulletTypeID))
                    {
                        var bulletInfo = new BulletTypeInfo
                        {
                            bulletTypeID = bulletTypeID,
                            count = 0
                        };
                        __result.Add(bulletTypeID, bulletInfo);
                    }
                }
                
                // 重新构建有序字典（按等级排序）
                var sortedDict = new Dictionary<int, BulletTypeInfo>();
                var sortedKeys = __result.Keys.ToList();
                sortedKeys.Sort((a, b) =>
                {
                    int tierA = mod.BulletTierOrder.ContainsKey(a) ? mod.BulletTierOrder[a] : 999;
                    int tierB = mod.BulletTierOrder.ContainsKey(b) ? mod.BulletTierOrder[b] : 999;
                    return tierA.CompareTo(tierB);
                });
                
                foreach (var key in sortedKeys)
                {
                    sortedDict[key] = __result[key];
                }
                
                __result = sortedDict;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FireWithMoney] GetBulletTypesInInventory error: {ex.Message}");
            }
        }
    }

    // Patch: 装弹前检查背包，没有就临时购买到背包
    [HarmonyPatch(typeof(ItemAgent_Gun), "BeginReload")]
    public static class Patch_BeginReload
    {
        static void Prefix(ItemAgent_Gun __instance)
        {
            try
            {
                var mod = ModBehaviour.Instance;
                if (mod == null) return;
                if (__instance.Holder != CharacterMainControl.Main) return;

                var gunSetting = __instance.GunItemSetting;
                if (gunSetting == null) return;

                int targetBulletID = gunSetting.TargetBulletID;
                if (targetBulletID == -1) return;

                var inventory = __instance.Holder.CharacterItem.Inventory;
                int bulletCountInInventory = gunSetting.GetBulletCountofTypeInInventory(targetBulletID, inventory);

                // 计算需要购买的数量
                int currentCount = gunSetting.BulletCount;
                int capacity = gunSetting.Capacity;
                int bulletsNeeded = capacity - currentCount - bulletCountInInventory;

                if (bulletsNeeded <= 0) return;

                int costPerBullet = mod.GetCostForBulletType(targetBulletID);
                int totalCost = costPerBullet * bulletsNeeded;

                if (!mod.HasEnoughMoney(totalCost))
                {
                    __instance.Holder.PopText($"余额不足！需要 {totalCost}", -1f);
                    return;
                }

                // 扣款并临时创建子弹到背包
                if (mod.TryDeductMoney(totalCost))
                {
                    Debug.Log($"[FireWithMoney] Purchasing {bulletsNeeded} x Bullet{targetBulletID} for {totalCost}");
                    
                    var bulletItem = ItemAssetsCollection.InstantiateSync(targetBulletID);
                    if (bulletItem != null && bulletItem.Stackable)
                    {
                        bulletItem.StackCount = bulletsNeeded;
                        
                        // 临时添加到背包，让游戏执行原装弹流程
                        bool added = inventory.AddAndMerge(bulletItem, 0);
                        
                        if (added)
                        {
                            __instance.Holder.PopText($"购买弹药 -{totalCost}", -1f);
                            Debug.Log($"[FireWithMoney] Added {bulletsNeeded} bullets to inventory for reload");
                        }
                        else
                        {
                            // 添加失败，退款
                            bulletItem.DestroyTree();
                            typeof(EconomyManager).GetField("money", BindingFlags.Instance | BindingFlags.NonPublic)
                                ?.SetValue(EconomyManager.Instance, EconomyManager.Money + totalCost);
                            Debug.LogError("[FireWithMoney] Failed to add bullets to inventory, refunded");
                        }
                    }
                    else
                    {
                        // 创建失败，退款
                        typeof(EconomyManager).GetField("money", BindingFlags.Instance | BindingFlags.NonPublic)
                            ?.SetValue(EconomyManager.Instance, EconomyManager.Money + totalCost);
                        Debug.LogError("[FireWithMoney] Failed to create bullet item, refunded");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FireWithMoney] BeginReload error: {ex.Message}");
            }
        }
    }
}
