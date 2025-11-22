# FireWithMoney - 项目结构

本项目采用模块化架构设计，参考了 Escape-From-Duckov-Coop-Mod-Preview 的项目结构。

## 📁 目录结构

```
FireWithMoney/
├── Main/
│   └── ModBehaviour.cs          # Mod 主入口，负责初始化和生命周期管理
├── Config/
│   └── BulletConfig.cs          # 子弹价格和等级配置
├── Models/
│   └── PaymentMode.cs           # 支付模式枚举
├── Managers/
│   ├── MoneyManager.cs          # 资金管理（银行卡/现金）
│   └── BulletManager.cs         # 子弹管理（获取、排序）
└── Patch/
    ├── Patch_GetBulletTypesInInventory.cs  # 显示所有子弹类型的补丁
    └── Patch_BeginReload.cs                # 装弹购买系统的补丁
```

## 📦 模块说明

### Main - 主模块
- **ModBehaviour.cs**: Mod 的主入口点
  - 初始化各个管理器
  - 应用 Harmony 补丁
  - 处理输入（Shift+B 切换支付模式）
  - 检查 Mod 兼容性

### Config - 配置模块
- **BulletConfig.cs**: 中心化的配置管理
  - 子弹价格配置表
  - 子弹等级排序配置
  - 提供查询方法

### Models - 数据模型
- **PaymentMode.cs**: 支付模式枚举
  - BankBalance: 银行卡支付
  - Cash: 现金支付

### Managers - 管理器模块
- **MoneyManager.cs**: 资金管理
  - 支付模式切换
  - 余额检查
  - 扣款/退款操作
  - 银行卡和现金的统一接口

- **BulletManager.cs**: 子弹管理
  - 获取指定口径的所有子弹
  - 按等级排序子弹类型

### Patch - Harmony 补丁模块
- **Patch_GetBulletTypesInInventory.cs**
  - 显示所有可用子弹类型（包括背包中没有的）
  - 按等级自动排序

- **Patch_BeginReload.cs**
  - 装弹时自动购买所需子弹
  - 余额不足时提示并购买最大可负担数量
  - 失败时自动退款

## 🎯 设计优势

1. **单一职责原则**: 每个类只负责一个功能模块
2. **易于维护**: 修改配置、管理器或补丁互不影响
3. **可扩展性**: 添加新功能时只需增加新的类
4. **代码复用**: 管理器可以被多个补丁共享使用
5. **清晰的依赖关系**: 
   - Main → Managers → Config/Models
   - Patch → Managers → Config/Models

## 🔧 扩展示例

### 添加新的子弹类型
只需修改 `Config/BulletConfig.cs` 中的配置表。

### 添加新的支付方式
1. 在 `Models/PaymentMode.cs` 添加枚举值
2. 在 `Managers/MoneyManager.cs` 添加对应逻辑

### 添加新的游戏功能补丁
在 `Patch/` 目录下创建新的补丁类，通过 `ModBehaviour.Instance` 访问管理器。

## 📝 使用说明

游戏中按 **Shift+B** 可以切换支付模式（银行卡/现金）。
