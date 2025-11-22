// encoding: utf-8
// FireWithMoney - Buy Ammo Anytime, Anywhere
// Copyright (c) 2025 Shadowrabbit
// Licensed under the MIT License

using System.Linq;
using ItemStatsSystem;
using UnityEngine;

namespace FireWithMoney.Managers
{
    /// <summary>
    /// 仓库管理器
    /// </summary>
    public class WarehouseManager
    {
        /// <summary>
        /// 获取仓库中指定类型物品的总数
        /// </summary>
        public int GetItemCountInWarehouse(int itemTypeID)
        {
            var warehouse = PlayerStorage.Inventory;
            if (warehouse == null) return 0;

            int totalCount = 0;
            foreach (var item in warehouse.Content)
            {
                if (item != null && item.TypeID == itemTypeID && item.Stackable)
                {
                    totalCount += item.StackCount;
                }
            }
            return totalCount;
        }

        /// <summary>
        /// 从仓库转移物品到背包
        /// </summary>
        /// <param name="itemTypeID">物品类型ID</param>
        /// <param name="amount">需要转移的数量</param>
        /// <param name="targetInventory">目标背包</param>
        /// <returns>实际转移的数量</returns>
        public int TransferFromWarehouse(int itemTypeID, int amount, Inventory targetInventory)
        {
            var warehouse = PlayerStorage.Inventory;
            if (warehouse == null || targetInventory == null) return 0;

            int transferred = 0;
            int remaining = amount;

            // 找出仓库中所有匹配的物品
            var warehouseItems = warehouse.Content.Where(item => 
                item != null && item.TypeID == itemTypeID && item.Stackable).ToList();

            foreach (var item in warehouseItems)
            {
                if (remaining <= 0) break;

                int stackCount = item.StackCount;
                
                if (stackCount <= remaining)
                {
                    // 整个堆叠都转移
                    // 先从仓库移除
                    warehouse.RemoveItem(item);
                    
                    // 添加到背包
                    bool added = targetInventory.AddAndMerge(item, 0);
                    if (added)
                    {
                        transferred += stackCount;
                        remaining -= stackCount;
                    }
                    else
                    {
                        // 添加失败，放回仓库
                        warehouse.AddAndMerge(item, 0);
                        break;
                    }
                }
                else
                {
                    // 部分转移
                    // 创建新的物品实例
                    var newItem = ItemAssetsCollection.InstantiateSync(itemTypeID);
                    if (newItem != null && newItem.Stackable)
                    {
                        newItem.StackCount = remaining;
                        
                        // 添加到背包
                        bool added = targetInventory.AddAndMerge(newItem, 0);
                        if (added)
                        {
                            // 从仓库中的物品减少数量
                            item.StackCount = stackCount - remaining;
                            transferred += remaining;
                            remaining = 0;
                        }
                        else
                        {
                            // 添加失败，销毁临时创建的物品
                            newItem.DestroyTree();
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (transferred > 0)
            {
                Debug.Log($"[FireWithMoney] Transferred {transferred} x Item{itemTypeID} from warehouse to inventory");
            }

            return transferred;
        }
    }
}
