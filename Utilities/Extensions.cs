// encoding: utf-8
// FireWithMoney - Buy Ammo Anytime, Anywhere
// Copyright (c) 2025 Shadowrabbit
// Licensed under the MIT License

using ItemStatsSystem;

namespace FireWithMoney.Utilities
{
    /// <summary>
    /// 扩展方法工具类
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 简单的 PopText 包装，使用默认参数
        /// </summary>
        public static void PopTextSimple(this CharacterMainControl character, string text)
        {
            if (character != null)
            {
                character.PopText(text);
            }
        }
    }
}
