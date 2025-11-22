// encoding: utf-8
// FireWithMoney - Buy Ammo Anytime, Anywhere
// Copyright (c) 2025 Shadowrabbit
// Licensed under the MIT License

using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using ItemStatsSystem;
using Duckov.Economy;
using UnityEngine;
using FireWithMoney.Config;

namespace FireWithMoney.Patch
{
    /// <summary>
    /// 装弹前检查背包，没有就临时购买到背包
    /// </summary>
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

                // 计算需要的总数量
                int currentCount = gunSetting.BulletCount;
                int capacity = gunSetting.Capacity;
                int bulletsNeeded = capacity - currentCount - bulletCountInInventory;

                if (bulletsNeeded <= 0) return;

                // 步骤1: 首先尝试从仓库转移子弹到背包（仅在基地可用）
                int transferredFromWarehouse = 0;
                if (bulletsNeeded > 0 && PlayerStorage.Inventory != null)
                {
                    int warehouseCount = mod.WarehouseManager.GetItemCountInWarehouse(targetBulletID);
                    if (warehouseCount > 0)
                    {
                        int toTransfer = Mathf.Min(bulletsNeeded, warehouseCount);
                        transferredFromWarehouse = mod.WarehouseManager.TransferFromWarehouse(targetBulletID, toTransfer, inventory);
                        bulletsNeeded -= transferredFromWarehouse;
                        
                        if (transferredFromWarehouse > 0)
                        {
                            Debug.Log($"[FireWithMoney] Transferred {transferredFromWarehouse} bullets from warehouse to inventory");
                            __instance.Holder.PopText($"从仓库转移 {transferredFromWarehouse} 发子弹");
                        }
                    }
                }

                // 步骤2: 如果仍然不够，则使用金钱购买
                if (bulletsNeeded <= 0) return;

                int costPerBullet = BulletConfig.GetCostForBulletType(targetBulletID);
                int totalCost = costPerBullet * bulletsNeeded;

                // 如果余额不足，尝试购买能负担的最大数量
                if (!mod.MoneyManager.HasEnoughMoney(totalCost))
                {
                    long availableMoney = mod.MoneyManager.GetAvailableBalance();
                    int affordableBullets = (int)(availableMoney / costPerBullet);
                    
                    if (affordableBullets <= 0)
                    {
                        string paymentType = mod.MoneyManager.GetPaymentModeName();
                        int needCost = (int)(totalCost - availableMoney);
                        Debug.LogWarning($"[FireWithMoney] Insufficient {paymentType} balance. Need additional {needCost} units.");
                        
                        // 使用协程延迟显示，避免被游戏默认提示覆盖
                        __instance.Holder.StartCoroutine(DelayedPopText(
                            __instance.Holder, 
                            $"{paymentType}余额不足！还需要 {needCost} 元 [按 Shift+B 切换支付方式]", 
                            0.0f));
                        return;
                    }
                    
                    // 调整为可负担的数量
                    bulletsNeeded = affordableBullets;
                    totalCost = costPerBullet * bulletsNeeded;
                    
                    string paymentTypeInfo = mod.MoneyManager.GetPaymentModeName();
                    Debug.LogWarning($"[FireWithMoney] Insufficient funds. Adjusted purchase to {bulletsNeeded} bullets costing {totalCost} units.");
                    
                    // 使用协程延迟显示
                    __instance.Holder.StartCoroutine(DelayedPopText(
                        __instance.Holder, 
                        $"{paymentTypeInfo}余额不足，购买 {bulletsNeeded} 发 [按 Shift+B 切换支付方式]",
                        0.0f));
                }

                // 扣款并临时创建子弹到背包
                if (mod.MoneyManager.TryDeductMoney(totalCost))
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
                            string paymentType = mod.MoneyManager.GetPaymentModeName();
                            __instance.Holder.PopText($"{paymentType} -{totalCost} 元");
                            Debug.Log($"[FireWithMoney] Added {bulletsNeeded} bullets to inventory for reload");
                        }
                        else
                        {
                            // 添加失败，退款
                            bulletItem.DestroyTree();
                            mod.MoneyManager.Refund(totalCost);
                            Debug.LogError("[FireWithMoney] Failed to add bullets to inventory, refunded");
                        }
                    }
                    else
                    {
                        // 创建失败，退款
                        mod.MoneyManager.Refund(totalCost);
                        Debug.LogError("[FireWithMoney] Failed to create bullet item, refunded");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FireWithMoney] BeginReload error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 延迟显示 PopText，避免被游戏默认提示覆盖
        /// </summary>
        private static IEnumerator DelayedPopText(CharacterMainControl character, string text, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (character != null)
            {
                character.PopText(text);
            }
        }
    }
}
