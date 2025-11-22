// encoding: utf-8
// FireWithMoney - Buy Ammo Anytime, Anywhere
// Copyright (c) 2025 Shadowrabbit
// Licensed under the MIT License

using System;
using System.Linq;
using System.Reflection;
using Duckov.Economy;
using ItemStatsSystem;
using UnityEngine;
using FireWithMoney.Models;

namespace FireWithMoney.Managers
{
    /// <summary>
    /// 资金管理器
    /// </summary>
    public class MoneyManager
    {
        /// <summary>
        /// 现金物品ID
        /// </summary>
        public const int CashItemID = 451;

        /// <summary>
        /// 当前支付模式
        /// </summary>
        public PaymentMode CurrentPaymentMode { get; set; } = PaymentMode.BankBalance;

        /// <summary>
        /// 检查是否有足够的钱
        /// </summary>
        public bool HasEnoughMoney(int amount)
        {
            if (CurrentPaymentMode == PaymentMode.BankBalance)
            {
                return EconomyManager.Money >= amount;
            }
            else
            {
                return GetCashInInventory() >= amount;
            }
        }

        /// <summary>
        /// 尝试扣款
        /// </summary>
        public bool TryDeductMoney(int amount)
        {
            if (CurrentPaymentMode == PaymentMode.BankBalance)
            {
                return DeductFromBank(amount);
            }
            else
            {
                return DeductFromInventory(amount);
            }
        }

        /// <summary>
        /// 从银行扣款
        /// </summary>
        private bool DeductFromBank(int amount)
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

        /// <summary>
        /// 获取背包中的现金总数
        /// </summary>
        public int GetCashInInventory()
        {
            if (CharacterMainControl.Main == null) return 0;
            
            var inventory = CharacterMainControl.Main.CharacterItem.Inventory;
            if (inventory == null) return 0;

            int totalCash = 0;
            foreach (var item in inventory.Content)
            {
                if (item != null && item.TypeID == CashItemID && item.Stackable)
                {
                    totalCash += item.StackCount;
                }
            }
            return totalCash;
        }

        /// <summary>
        /// 从背包扣款
        /// </summary>
        private bool DeductFromInventory(int amount)
        {
            if (CharacterMainControl.Main == null) return false;
            
            var inventory = CharacterMainControl.Main.CharacterItem.Inventory;
            if (inventory == null) return false;

            int totalCash = GetCashInInventory();
            if (totalCash < amount) return false;

            int remaining = amount;
            var cashItems = inventory.Content.Where(item => 
                item != null && item.TypeID == CashItemID && item.Stackable).ToList();

            foreach (var item in cashItems)
            {
                if (remaining <= 0) break;

                int stackCount = item.StackCount;
                if (stackCount <= remaining)
                {
                    remaining -= stackCount;
                    item.DestroyTree();
                }
                else
                {
                    item.StackCount = stackCount - remaining;
                    remaining = 0;
                }
            }

            Debug.Log($"[FireWithMoney] Deducted {amount} cash from inventory. Remaining: {totalCash - amount}");
            return remaining == 0;
        }

        /// <summary>
        /// 退款
        /// </summary>
        public void Refund(int amount)
        {
            if (CurrentPaymentMode == PaymentMode.BankBalance)
            {
                var moneyField = typeof(EconomyManager).GetField("money", 
                    BindingFlags.Instance | BindingFlags.NonPublic);
                moneyField?.SetValue(EconomyManager.Instance, EconomyManager.Money + amount);
                Debug.Log($"[FireWithMoney] Refunded {amount} to bank");
            }
            else
            {
                if (CharacterMainControl.Main == null) return;
                var inventory = CharacterMainControl.Main.CharacterItem.Inventory;
                if (inventory == null) return;

                var refundItem = ItemAssetsCollection.InstantiateSync(CashItemID);
                if (refundItem != null && refundItem.Stackable)
                {
                    refundItem.StackCount = amount;
                    inventory.AddAndMerge(refundItem, 0);
                    Debug.Log($"[FireWithMoney] Refunded {amount} cash to inventory");
                }
            }
        }

        /// <summary>
        /// 获取当前支付方式名称
        /// </summary>
        public string GetPaymentModeName()
        {
            return CurrentPaymentMode == PaymentMode.BankBalance ? "银行卡" : "现金";
        }

        /// <summary>
        /// 获取当前可用余额
        /// </summary>
        public long GetAvailableBalance()
        {
            return CurrentPaymentMode == PaymentMode.BankBalance ? EconomyManager.Money : GetCashInInventory();
        }

        /// <summary>
        /// 切换支付模式
        /// </summary>
        public void TogglePaymentMode()
        {
            CurrentPaymentMode = CurrentPaymentMode == PaymentMode.BankBalance 
                ? PaymentMode.Cash 
                : PaymentMode.BankBalance;
        }
    }
}
