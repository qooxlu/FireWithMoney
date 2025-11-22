// encoding: utf-8
// FireWithMoney - Buy Ammo Anytime, Anywhere
// Copyright (c) 2025 Shadowrabbit
// Licensed under the MIT License

using System;
using System.Collections.Generic;
using HarmonyLib;
using ItemStatsSystem;
using UnityEngine;

namespace FireWithMoney.Patch
{
    /// <summary>
    /// 显示所有子弹类型并按等级排序
    /// </summary>
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

                var allBulletTypes = mod.BulletManager.GetAllBulletTypesForCaliber(caliber);
                
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
                __result = mod.BulletManager.SortBulletTypesByTier(__result);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FireWithMoney] GetBulletTypesInInventory error: {ex.Message}");
            }
        }
    }
}
