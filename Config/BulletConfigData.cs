// encoding: utf-8
// FireWithMoney - Buy Ammo Anytime, Anywhere
// Copyright (c) 2025 Shadowrabbit
// Licensed under the MIT License

namespace FireWithMoney.Config
{
    /// <summary>
    /// 子弹配置数据
    /// </summary>
    public class BulletConfigData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public bool Display { get; set; }

        public BulletConfigData(int id, string name, int price, bool display)
        {
            Id = id;
            Name = name;
            Price = price;
            Display = display;
        }
    }
}
