// encoding: utf-8
// FireWithMoney - Buy Ammo Anytime, Anywhere
// Copyright (c) 2025 Shadowrabbit
// Licensed under the MIT License

using System;
using HarmonyLib;
using ItemStatsSystem;
using UnityEngine;
using FireWithMoney.Managers;

namespace FireWithMoney
{
    /// <summary>
    /// FireWithMoney Mod 主类
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        public static ModBehaviour Instance;

        /// <summary>
        /// 资金管理器
        /// </summary>
        public MoneyManager MoneyManager { get; private set; }

        /// <summary>
        /// 子弹管理器
        /// </summary>
        public BulletManager BulletManager { get; private set; }

        /// <summary>
        /// 仓库管理器
        /// </summary>
        public WarehouseManager WarehouseManager { get; private set; }

        private Harmony _harmony;

        protected override void OnAfterSetup()
        {
            Instance = this;
            
            // 初始化管理器
            MoneyManager = new MoneyManager();
            BulletManager = new BulletManager();
            WarehouseManager = new WarehouseManager();
            
            // 应用 Harmony 补丁
            _harmony = new Harmony("com.duckov.firewithmoney");
            
            try
            {
                _harmony.PatchAll();
                Debug.Log("[FireWithMoney] Mod loaded! Bullet purchase system enabled");
                Debug.Log("[FireWithMoney] Press Shift+B to toggle payment mode (Bank/Cash)");
                
                // 检查潜在冲突
                CheckCompatibility();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FireWithMoney] Failed to load mod: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查与其他 Mod 的兼容性
        /// </summary>
        private void CheckCompatibility()
        {
            // 检查是否有其他 mod 也 patch 了相同方法
            var gunPatches = Harmony.GetPatchInfo(AccessTools.Method(typeof(ItemSetting_Gun), "GetBulletTypesInInventory"));
            if (gunPatches != null)
            {
                int postfixCount = gunPatches.Postfixes.Count;
                if (postfixCount > 1)
                {
                    Debug.LogWarning($"[FireWithMoney] Detected {postfixCount} Postfix patches on GetBulletTypesInInventory. Possible mod conflicts.");
                }
            }
            
            var reloadPatches = Harmony.GetPatchInfo(AccessTools.Method(typeof(ItemAgent_Gun), "BeginReload"));
            if (reloadPatches != null)
            {
                int prefixCount = reloadPatches.Prefixes.Count;
                if (prefixCount > 1)
                {
                    Debug.LogWarning($"[FireWithMoney] Detected {prefixCount} Prefix patches on BeginReload. Possible mod conflicts.");
                }
            }
        }

        private void Update()
        {
            // 监听 Shift+B 键切换支付模式
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.B))
            {
                MoneyManager.TogglePaymentMode();
                string mode = MoneyManager.GetPaymentModeName();
                
                if (CharacterMainControl.Main != null)
                {
                    CharacterMainControl.Main.PopText($"[{mode}] 支付");
                    Debug.Log($"[FireWithMoney] Payment mode switched to: {mode}");
                }
            }
        }

        protected override void OnBeforeDeactivate()
        {
            _harmony?.UnpatchAll("com.duckov.firewithmoney");
            Debug.Log("[FireWithMoney] Mod unloaded");
        }
    }
}
