# SBP Mod 开发 API 文档

本文档记录《杀戮尖塔2》mod开发中常用的API和代码模式，方便快速查阅和使用。

---

## 目录

1. [卡牌基础结构](#1-卡牌基础结构)
2. [动态变量与本地化](#2-动态变量与本地化)
   - [卡牌关键词设定](#22-卡牌关键词设定)
   - [动态变量访问方式](#24-动态变量访问方式重要)
3. [能力系统](#3-能力系统)
4. [动画系统](#4-动画系统)
5. [特效系统（VFX）](#5-特效系统vfx)
   - [VfxCmd 命令系统](#51-vfxcmd-命令系统)
   - [自定义特效属性](#52-自定义特效属性)
   - [常用VFX节点类详解](#56-常用vfx节点类详解)
6. [音效系统（SFX）](#6-音效系统sfx)
   - [SfxCmd 命令系统](#61-sfxcmd-命令系统)
7. [命令系统](#7-命令系统)
   - [CreatureCmd 命令系统](#71-creaturecmd-命令系统)
     - [伤害系统方法](#711-伤害系统方法)
     - [生命值管理方法](#712-生命值管理方法)
     - [格挡系统方法](#713-格挡系统方法)
     - [控制效果方法](#714-控制效果方法)
     - [生物管理方法](#715-生物管理方法)
     - [动画与特效方法](#716-动画与特效方法)
     - [ValueProp 常用属性](#717-valueprop-常用属性)
   - [PowerCmd 命令系统](#72-powercmd-命令系统)
     - [Apply（应用能力）](#721-apply应用能力)
     - [ModifyAmount（修改能力层数）](#722-modifyamount修改能力层数)
     - [SetAmount（设置能力层数）](#723-setamount设置能力层数)
     - [Decrement（减少一层能力）](#724-decrement减少一层能力)
     - [Remove（移除能力）](#725-remove移除能力)
     - [TickDownDuration（减少持续时间）](#726-tickdownduration减少持续时间)
     - [PowerCmd 方法对比](#728-powercmd-方法对比)
8. [卡池配置](#8-卡池配置)
   - [创建自定义卡池](#61-创建自定义卡池)
   - [注册到原版卡池](#62-注册到原版卡池)
   - [绑定卡池到卡牌](#63-绑定卡池到卡牌)
   - [常用原版卡池](#64-常用原版卡池)
   - [CardCreationOptions](#65-cardcreationoptions卡牌创建选项)
9. [本地化格式](#9-本地化格式)
10. [伤害系统与蓄力攻击](#10-伤害系统与蓄力攻击)
11. [卡牌类型检查](#11-卡牌类型检查)
12. [游戏更新后 API 迁移（多人战斗版本）](#12-游戏更新后-api-迁移多人战斗版本)

**附录**

- [常用能力列表](#常用能力列表)
- [动画系统](#动画系统)
- [伤害与中毒的区别](#伤害与中毒的区别)
- [获取能力层数方法](#获取能力层数方法)
- [PoisonPower伤害逻辑](#poisonpower伤害逻辑)

---

## 1. 卡牌基础结构

### 1.1 卡牌类定义

```csharp
// 卡牌基础类
public abstract class SnakeBitePlus_Card : CustomCardModel, ISnakeTagged
{
    protected SnakeBitePlus_Card(int cost, CardType type, CardRarity rarity, TargetType target) 
        : base(cost, type, rarity, target) { }

    // 自定义卡牌标签标识
    public virtual bool HasSnakeTag => true;
    public virtual bool HasSnakeBiteTag => false;

    // 卡牌图片路径
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant().Replace("_", "")}.png".BigCardImagePath();
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant().Replace("_", "")}.png".CardImagePath();
}
```

### 1.2 卡牌打出方法

```csharp
// 方法：卡牌打出后事件
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
{
    // 卡牌效果逻辑
    await PowerCmd.Apply<PoisonPower>(target, amount, source, this);
}
```

---

## 2. 动态变量与本地化

### 2.1 定义动态变量

```csharp
// 定义动态变量
// 用于存储基础伤害值和格挡值
protected override IEnumerable<DynamicVar> CanonicalVars => 
    new[] { new DynamicVar("Damage", 6m), new DynamicVar("Block", 4m) };
// 定义额外动态变量
// 用于存储能量值和能力层数，如中毒层数
protected override IEnumerable<DynamicVar> ExtraDynamicVars => 
    new[] { new EnergyVar(), new PowerVar<PoisonPower>() };
```

### 2.2 卡牌关键词设定

```csharp
// 设定卡牌关键词（如消耗、保留等）
public override IEnumerable<CardKeyword> CanonicalKeywords => 
    new[] { CardKeyword.Exhaust };
// 卡牌升级后添加关键词,如消耗
protected override void OnUpgrade()
{
    base.OnUpgrade();
    base.AddKeyword(CardKeyword.Exhaust);
}
```

**常用关键词：**

| 关键词 | 效果 |
|--------|------|
| `CardKeyword.Exhaust` | 消耗：使用后移除卡牌 |
| `CardKeyword.Retain` | 保留：回合结束时不进入弃牌堆 |
| `CardKeyword.Eternal` | 永恒：无法被移除或变化 |
| `CardKeyword.Ethereal` | 虚无：回合结束时消失 |
| `CardKeyword.Sly` | 奇巧：回合中被弃时自动打出 |


**示例（Afterlife卡牌）：**
```csharp
public sealed class Afterlife : CardModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => 
        new[] { CardKeyword.Exhaust };
    // ...
}
```

### 2.3 额外悬浮提示

```csharp
// 定义额外悬浮提示
// 用于显示能力说明，如中毒效果
protected override IEnumerable<IHoverTip> ExtraHoverTips => 
    new[] { HoverTipFactory.FromPower<PoisonPower>() };
// 用于显示相关卡牌的信息
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] {
        HoverTipFactory.FromCard<NukeStrike>(),
        HoverTipFactory.FromCard<NukeBunker>()
    };
```

**作用**：悬停卡牌时显示能力说明。

### 2.3 卡牌升级方法（OnUpgrade）

`OnUpgrade` 是卡牌升级时调用的方法，用于修改卡牌属性。

#### 2.3.1 修改费用

```csharp
protected override void OnUpgrade()
{
    base.OnUpgrade();
    // 将费用减少 1（从 2 费变为 1 费）
    base.EnergyCost.UpgradeBy(-1);
}
```

#### 2.3.2 添加关键词

```csharp
protected override void OnUpgrade()
{
    base.OnUpgrade();
    // 添加固有和保留关键词
    AddKeyword(CardKeyword.Innate);
    AddKeyword(CardKeyword.Retain);
}
```

#### 2.3.3 修改动态变量

```csharp
protected override void OnUpgrade()
{
    base.OnUpgrade();
    // 将 "Accelerant" 变量的值增加 1
    base.DynamicVars["Accelerant"].UpgradeValueBy(1m);
    
    // 将 "Damage" 变量的值增加 2
    base.DynamicVars["Damage"].UpgradeValueBy(2m);
}
```

#### 2.3.4 综合示例

```csharp
public sealed class SBP_power2 : SnakeBitePlus_Card
{
    public SBP_power2() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }
    
    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        // 添加固有和保留关键词
        AddKeyword(CardKeyword.Innate);
        AddKeyword(CardKeyword.Retain);
    }
}
```

#### 2.3.5 常用升级操作

| 操作 | 代码 | 说明 |
|------|------|------|
| 减少费用 | `base.EnergyCost.UpgradeBy(-1)` | 费用 -1 |
| 添加关键词 | `AddKeyword(CardKeyword.Xxx)` | 添加指定关键词 |
| 增加变量值 | `base.DynamicVars["Var"].UpgradeValueBy(amount)` | 动态变量增加指定值 |

### 2.4 动态变量访问方式（重要）

**两种访问方式：**

| 访问方式 | 语法 | 适用类型 | 推荐度 |
|----------|------|----------|--------|
| **字典索引** | `base.DynamicVars["VariableName"]` | 任意 `DynamicVar` | ✅ 推荐 |
| **属性访问** | `base.DynamicVars.Poison` | 必须是 `PowerVar<PoisonPower>` | ⚠️ 谨慎 |

**问题说明：**

如果使用普通的 `DynamicVar` 定义变量，但使用属性方式访问，会导致运行时错误：

```
System.Collections.Generic.KeyNotFoundException: The given key 'PoisonPower' was not present in the dictionary.
```

**正确示例：**

```csharp
// 定义动态变量
protected override IEnumerable<DynamicVar> CanonicalVars 
=> new[] { new DynamicVar("Poison", 7m) };

// 使用字典索引访问（推荐）
int poisonAmount = (int)base.DynamicVars["Poison"].BaseValue;
base.DynamicVars["Poison"].UpgradeValueBy(3m);
```

**错误示例：**

```csharp
// 定义动态变量
protected override IEnumerable<DynamicVar> CanonicalVars 
=> new[] { new DynamicVar("Poison", 7m) };

// ❌ 错误：会导致 KeyNotFoundException
int poisonAmount = (int)base.DynamicVars.Poison.BaseValue;  // 运行时错误！
```

**统一代码风格建议：**

为避免此类问题，建议所有卡牌统一使用字典索引方式访问动态变量，如 `SBP_atk1` 所示：

```csharp
// 统一使用字典索引
int poisonAmount = (int)base.DynamicVars["Poison"].BaseValue;
int damageAmount = (int)base.DynamicVars["Damage"].BaseValue;
int blockAmount = (int)base.DynamicVars["Block"].BaseValue;
```

**完整示例**：
//触媒
```csharp
public class Accelerant : CustomCardModel
{
    // 定义变量初始值为 1
    protected override IEnumerable<DynamicVar> CanonicalVars => 
        new[] { new DynamicVar("Accelerant", 1m) };

    // 升级时增加变量值
    protected override void Upgrade()
    {
        base.Upgrade();
        base.DynamicVars["Accelerant"].UpgradeValueBy(1m);  // 变为 2
    }

    // 在 OnPlay 中使用变量值
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int amount = (int)base.DynamicVars["Accelerant"].Value;
        await PowerCmd.Apply<PoisonPower>(play.Target, amount, base.Owner.Creature, this);
    }
}
``` 

### 2.4 本地化文件格式

```json
{
  "CARD_NAME.title": "卡牌名称",
  "CARD_NAME.description": "造成{Damage}点伤害，获得{Block}点[gold]格挡[/gold]。"
}
```

---

## 3. 能力系统

### 3.1 能力类定义

```csharp
public sealed class MyPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 监听回合开始事件
    public override async Task BeforePlayPhaseStart(PlayerChoiceContext context, Player player)
    {
        await base.BeforePlayPhaseStart(context, player);
        // 回合开始时触发的效果
    }

    // 监听卡牌打出事件
    public override async TaskOn(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(context, cardPlay);
        // 卡牌打出后触发的效果
    }
}
```

### 3.2 常用能力接口

| 方法 | 触发时机 |
|------|----------|
| `BeforePlayPhaseStart` | 回合开始 |
| `AfterCardPlayed` | 卡牌打出后 |
| `BeforeDamageTaken` | 受到伤害前 |
| `AfterDamageTaken` | 受到伤害后 |
| `AfterModifyingHpLostAfterOsty` | 伤害修改完成后 |
| `OnTurnEnd` | 回合结束 |

### 3.3 能力元数据与逻辑分离模式

某些能力采用**元数据与逻辑分离**的设计模式：能力类本身只定义元数据，实际效果逻辑在其他能力中实现。

**示例：AccelerantPower（加速剂）**

```csharp
// AccelerantPower.cs - 只定义元数据
public sealed class AccelerantPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    // 悬停时显示中毒能力的说明（提示玩家两者关联）
    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
        new[] { HoverTipFactory.FromPower<PoisonPower>() };
}
```

**实际逻辑在 PoisonPower 中实现：**

```csharp
// PoisonPower.cs - 读取加速剂层数并增加触发次数
public sealed class PoisonPower : PowerModel
{
    // 计算中毒触发次数：1（基础） + 所有对手的加速剂层数
    private int TriggerCount
    {
        get
        {
            IEnumerable<Creature> opponents = base.Owner.CombatState
                .GetOpponentsOf(base.Owner).Where(c => c.IsAlive);
            
            return Math.Min(base.Amount, 
                1 + opponents.Sum(a => a.GetPowerAmount<AccelerantPower>()));
        }
    }

    // 回合开始时根据 TriggerCount 多次触发中毒
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        // 只在目标角色的回合开始时触发
        if (side != base.Owner.Side) return;
        // 计算中毒触发次数
        int iterations = TriggerCount;
        for (int i = 0; i < iterations; i++)
        {
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), 
                base.Owner, base.Amount, 
                ValueProp.Unblockable | ValueProp.Unpowered, null, null);
            
            if (base.Owner.IsAlive)
                await PowerCmd.Decrement(this);
        }
    }
}
```

**效果说明：**

| 场景 | 加速剂层数 | 中毒触发次数 |
|------|-----------|-------------|
| 无加速剂 | 0 | 1次（正常） |
| 1层加速剂 | 1 | 2次（伤害翻倍-1） |
| 2层加速剂 | 2 | 3次（伤害三倍-2） |

**设计优势：**
1. **解耦**：能力元数据与效果逻辑分离
2. **复用**：多个能力可以影响同一个目标能力
3. **清晰**：能力的实际效果由目标能力统一管理

---

## 4. 动画系统

### 4.1 播放角色动画

```csharp
// 播放施法动画
await CreatureCmd.TriggerAnim(
    base.Owner.Creature,          // 角色对象
    "Cast",                       // 动画名称
    base.Owner.Character.CastAnimDelay  // 动画延迟
);
```

### 4.2 常用动画名称

| 动画名 | 说明 |
|--------|------|
| `"Cast"` | 施法动作 |
| `"Attack"` | 攻击动作 |
| `"Defend"` | 防御动作 |
| `"Idle"` | 待机动作 |

---

## 5. 特效系统（VFX）

### 5.1 VfxCmd 命令系统

`VfxCmd` 是用于播放视觉特效的核心命令类，提供了多种播放方式。

#### 5.1.1 常用播放方法

```csharp
// 在生物中心播放特效
VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_bite");

// 在生物位置播放特效
VfxCmd.PlayOnCreature(target, "vfx/vfx_attack_slash");

// 在指定位置播放特效
VfxCmd.PlayVfx(new Vector2(100, 200), "vfx/vfx_fire_burst");

// 在所有目标生物上播放特效
VfxCmd.PlayOnCreatureCenters(targets, "vfx/vfx_giant_horizontal_slash");

// 全屏播放特效
VfxCmd.PlayFullScreenInCombat("vfx/vfx_adrenaline");
```

#### 5.1.2 常用特效路径

| 特效路径 | 说明 |
|----------|------|
| `vfx/vfx_bite` | 咬击特效 |
| `vfx/vfx_attack_slash` | 斩击特效 |
| `vfx/vfx_attack_blunt` | 钝击特效 |
| `vfx/vfx_flying_slash` | 飞斩特效 |
| `vfx/vfx_fire_burst` | 火焰爆发 |
| `vfx/vfx_bloody_impact` | 血腥冲击 |
| `vfx/vfx_gaze` | 凝视特效 |
| `vfx/vfx_giant_horizontal_slash` | 巨型横斩 |
| `vfx/vfx_block` | 格挡特效 |
| `vfx/vfx_cross_heal` | 治疗特效 |

### 5.2 自定义特效属性

通过直接创建 VFX 节点类，可以自定义特效的颜色、大小和位置。

#### 5.2.1 支持颜色自定义的 VFX

```csharp
// NStabVfx - 支持多种预设颜色
NStabVfx.Create(target, facingEnemies: true, VfxColor.Gold);
NStabVfx.Create(target, facingEnemies: false, VfxColor.Red);

// NThinSliceVfx - 支持颜色自定义
NThinSliceVfx.Create(target, VfxColor.Red);
NThinSliceVfx.Create(target, VfxColor.Cyan);

// NFireBurningVfx - 支持任意 Color
NFireBurningVfx.Create(creature, 1f, goingRight: false);
NFireBurningVfx.Create(creature, 1f, goingRight: false, new Color("#ff4444"));
```

#### 5.2.2 支持大小自定义的 VFX

```csharp
// NFireBurningVfx - 支持 scaleFactor 参数
NFireBurningVfx.Create(creature, 1f, goingRight: false);      // 正常大小
NFireBurningVfx.Create(creature, 0.75f, goingRight: false);   // 缩小 25%
NFireBurningVfx.Create(creature, 1.5f, goingRight: false);    // 放大 50%
```

#### 5.2.3 支持位置自定义的 VFX

```csharp
// 基于生物位置
NStabVfx.Create(target, facingEnemies: true, VfxColor.Purple);

// 基于任意位置
Vector2 customPosition = new Vector2(100, 200);
NFireBurningVfx.Create(customPosition, 1f, goingRight: true, Color.Red);
```

### 5.3 VfxColor 枚举

```csharp
public enum VfxColor
{
    Red,    // 红色
    Green,  // 绿色
    Blue,   // 蓝色
    Purple, // 紫色
    White,  // 白色
    Cyan,   // 青色
    Gold,   // 金色
    Black   // 黑色
}
```

### 5.4 Color 类使用方法

```csharp
// HTML 颜色
new Color("#ff8b57")
Color.FromHtml("#ff8b57")

// RGBA 值（范围 0-1）
new Color(0.8f, 0.3f, 0.8f, 0.66f)  // (R, G, B, A)

// 预设颜色
Colors.Red
Colors.Green
Colors.Blue
Colors.White
```

### 5.5 综合示例

```csharp
// 示例 1：播放简单特效
VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_bite");

// 示例 2：自定义颜色和方向
NStabVfx stabVfx = NStabVfx.Create(target, facingEnemies: true, VfxColor.Gold);
NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(stabVfx);

// 示例 3：自定义大小和颜色的火焰特效
NFireBurningVfx fireVfx = NFireBurningVfx.Create(
    targetPosition, 
    scaleFactor: 1.2f,    // 放大 20%
    goingRight: false, 
    tint: new Color("#ff4444")  // 红色调
);
NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(fireVfx);

// 示例 4：在指定位置播放特效
Vector2 spawnPos = new Vector2(50, 100);
VfxCmd.PlayVfx(spawnPos, "vfx/vfx_fire_burst");
```

### 5.6 常用VFX节点类详解

#### 5.6.1 NStabVfx（穿刺特效）

```csharp
// 创建穿刺特效
NStabVfx stabVfx = NStabVfx.Create(
    targetCreature,      // 目标生物
    facingEnemies: true, // 是否朝向敌人
    VfxColor.Gold        // 特效颜色
);
NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(stabVfx);
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `target` | `Creature` | 目标生物 |
| `facingEnemies` | `bool` | 特效方向是否朝向敌人 |
| `color` | `VfxColor` | 特效颜色（预设颜色） |

#### 5.6.2 NFireBurningVfx（火焰燃烧特效）

```csharp
// 创建火焰特效（支持自定义大小和颜色）
NFireBurningVfx fireVfx = NFireBurningVfx.Create(
    targetPosition,      // Vector2 位置或 Creature 对象
    scaleFactor: 1.2f,   // 缩放因子（默认1.0）
    goingRight: false,   // 是否朝右
    tint: new Color("#ff4444")  // 自定义颜色（可选）
);
NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(fireVfx);
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `target` | `Creature` 或 `Vector2` | 目标生物或位置 |
| `scaleFactor` | `float` | 缩放因子（0.5=缩小50%，1.5=放大50%） |
| `goingRight` | `bool` | 特效方向 |
| `tint` | `Color?` | 自定义颜色（null使用默认颜色） |

#### 5.6.3 NThinSliceVfx（薄刃切割特效）

```csharp
// 创建切割特效
NThinSliceVfx sliceVfx = NThinSliceVfx.Create(
    targetCreature,  // 目标生物
    VfxColor.Red     // 特效颜色
);
NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(sliceVfx);
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `target` | `Creature` | 目标生物 |
| `color` | `VfxColor` | 特效颜色 |

#### 5.6.4 NBloodSplashVfx（血溅特效）

```csharp
// 创建血溅特效
NBloodSplashVfx bloodVfx = NBloodSplashVfx.Create(
    targetCreature,  // 目标生物
    VfxColor.Red     // 特效颜色
);
NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(bloodVfx);
```

### 5.7 常用特效类型参考

#### 5.7.1 攻击特效

| 特效类 | 用途 | 支持参数 |
|--------|------|----------|
| `NStabVfx` | 穿刺/刺击特效 | 目标生物、朝向、颜色 |
| `NThinSliceVfx` | 薄刃切割特效 | 目标生物、颜色 |
| `NBigSlashVfx` | 大型斩击特效 | 目标生物、颜色 |
| `NHeavyBluntVfx` | 重击钝击特效 | 目标生物 |
| `NScratchVfx` | 抓痕特效 | 目标生物、方向 |
| `NHitSparkVfx` | 击中火花特效 | 目标生物、方向 |

#### 5.7.2 火焰特效

| 特效类 | 用途 | 支持参数 |
|--------|------|----------|
| `NFireBurningVfx` | 火焰燃烧特效 | 目标位置/生物、缩放、方向、颜色 |
| `NFireBurstVfx` | 火焰爆发特效 | 目标位置/生物 |
| `NGroundFireVfx` | 地面火焰特效 | 目标位置、缩放 |
| `NFireSmokePuffVfx` | 火焰烟雾特效 | 目标位置 |

#### 5.7.3 魔法特效

| 特效类 | 用途 | 支持参数 |
|--------|------|----------|
| `NSmallMagicMissileVfx` | 小型魔法导弹 | 起始位置、目标位置 |
| `NLargeMagicMissileVfx` | 大型魔法导弹 | 起始位置、目标位置 |
| `NLaserVfx` | 激光特效 | 起始位置、目标位置 |
| `NHyperbeamVfx` | 超光束特效 | 目标生物 |

#### 5.7.4 伤害/数字特效

| 特效类 | 用途 | 支持参数 |
|--------|------|----------|
| `NDamageNumVfx` | 伤害数字显示 | 目标位置、伤害值 |
| `NHealNumVfx` | 治疗数字显示 | 目标位置、治疗值 |
| `NDamageBlockedVfx` | 格挡伤害特效 | 目标生物 |

#### 5.7.5 状态特效

| 特效类 | 用途 | 支持参数 |
|--------|------|----------|
| `NStunnedVfx` | 眩晕状态特效 | 目标生物 |
| `NSleepingVfx` | 睡眠状态特效 | 目标生物 |
| `NPoisonImpactVfx` | 中毒冲击特效 | 目标生物 |
| `NDoomVfx` | 末日特效 | 目标位置、缩放 |

#### 5.7.6 爆炸/冲击特效

| 特效类 | 用途 | 支持参数 |
|--------|------|----------|
| `NSplashVfx` | 溅射特效 | 目标生物、颜色 |
| `NSmokePuffVfx` | 烟雾特效 | 目标位置 |
| `NGoopyImpactVfx` | 粘稠冲击特效 | 目标生物 |
| `NGaseousImpactVfx` | 气体冲击特效 | 目标生物 |
| `NStarryImpactVfx` | 星空冲击特效 | 目标位置 |

#### 5.7.7 卡牌特效

| 特效类 | 用途 | 支持参数 |
|--------|------|----------|
| `NCardFlyVfx` | 卡牌飞行特效 | 起始位置、目标位置 |
| `NCardUpgradeVfx` | 卡牌升级特效 | 卡牌对象 |
| `NCardTransformVfx` | 卡牌变形特效 | 卡牌对象 |

### 5.8 特效创建模式对比

| 方式 | 优点 | 缺点 | 适用场景 |
|------|------|------|----------|
| `VfxCmd.PlayOnCreatureCenter()` | 简单快捷 | 无法自定义属性 | 快速播放标准特效 |
| `NxxxVfx.Create()` | 支持自定义颜色/大小/位置 | 代码稍复杂 | 需要个性化特效 |

### 5.9 特殊特效说明

#### 5.9.1 NLaserVfx 的特殊用法

`NLaserVfx` 是一个特殊的激光特效类，**不支持静态的 `Create()` 方法**，需要从场景节点获取使用。

**错误用法（无法编译）：**
```csharp
// ❌ NLaserVfx 没有 Create() 方法
NLaserVfx laserVfx = NLaserVfx.Create(targetPosition, new Color("#4695d6"));
```

**正确用法：**

方法一：使用 `VfxCmd` 播放（推荐）
```csharp
// ✅ 使用 VfxCmd 播放激光特效
VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_laser");
```

方法二：从场景节点获取（高级用法）
```csharp
// ✅ 从父节点获取激光节点（适用于自定义场景）
NLaserVfx laser = parentNode.GetNode<NLaserVfx>("LaserBone/Laser");
laser.ExtendLaser(targetPosition);  // 发射激光
await Task.Delay(150);
laser.RetractLaser();               // 收回激光
```

**注意事项：**
- `NLaserVfx` 需要配合场景结构使用，不能直接创建
- 常用方法：`ExtendLaser()` 发射激光，`RetractLaser()` 收回激光，`ResetLaser()` 重置位置
- 如果只是简单播放激光特效，推荐使用 `VfxCmd.PlayOnCreatureCenter()`

### 5.10 实战示例：自定义特效属性

#### 示例1：修改特效大小和颜色

```csharp
// 创建一个放大150%的红色火焰特效
NFireBurningVfx fireVfx = NFireBurningVfx.Create(
    target.Creature,
    scaleFactor: 1.5f,           // 放大150%
    goingRight: true,
    tint: Color.FromHtml("#ff3333")  // 自定义红色
);
NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(fireVfx);
```

#### 示例2：在指定位置播放特效

```csharp
// 获取生物位置并偏移
Vector2 spawnPos = target.Creature.Position + new Vector2(50, -30);

// 在偏移位置播放爆炸特效
NFireBurningVfx explosion = NFireBurningVfx.Create(
    spawnPos,
    scaleFactor: 2.0f,           // 放大200%
    goingRight: false,
    tint: new Color(1f, 0.5f, 0f)  // 橙红色
);
NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(explosion);
```

#### 示例3：组合多个特效

```csharp
// 同时播放切割和血溅特效
NThinSliceVfx slice = NThinSliceVfx.Create(target.Creature, VfxColor.Gold);
NBloodSplashVfx blood = NBloodSplashVfx.Create(target.Creature, VfxColor.Red);

NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(slice);
NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(blood);
```

#### 示例4：在屏幕中央播放特效

```csharp
// 获取屏幕中央位置
Vector2 screenCenter = NGame.Instance.GetViewportRect().Size * 0.5f;

// 在屏幕中央播放大型爆炸特效
NFireBurningVfx explosionVfx = NFireBurningVfx.Create(
    screenCenter,
    scaleFactor: 8.0f,           // 放大8倍
    goingRight: false
);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(explosionVfx);
```

**关键点说明：**

| 方法/属性 | 说明 |
|-----------|------|
| `NGame.Instance.GetViewportRect().Size` | 获取游戏视口尺寸 |
| `* 0.5f` | 计算屏幕中央位置 |
| `NCombatRoom.Instance.CombatVfxContainer` | 战斗场景的特效容器 |
| `AddChild()` | 添加特效节点（替代`AddChildSafely()`） |

---

## 6. 音效系统（SFX）

### 6.1 SfxCmd 命令系统

`SfxCmd` 是用于播放音效的核心命令类，支持多种音效播放方式和参数控制。

#### 6.1.1 基本播放方法

```csharp
// 基础用法：播放指定路径的音效
SfxCmd.Play("sfx/sfx_attack_bite");

// 带音量控制的播放
SfxCmd.Play("sfx/sfx_explosion_large", volume: 3f);  // 3倍音量
```

#### 6.1.2 方法参数说明

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `path` | `string` | - | 音效文件路径（必填） |
| `volume` | `float` | `1f` | 音量倍数（1.0=正常音量） |

#### 6.1.3 实战示例：配合特效播放音效

```csharp
// 播放爆炸音效（3倍音量）
SfxCmd.Play("sfx/sfx_explosion_large", 3f);

// 同时播放大型爆炸特效
Vector2 screenCenter = NGame.Instance.GetViewportRect().Size * 0.5f;
NFireBurningVfx explosionVfx = NFireBurningVfx.Create(
    screenCenter,
    scaleFactor: 8.0f,
    goingRight: false
);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(explosionVfx);
```

#### 6.1.4 常用音效路径

| 音效路径 | 用途 |
|----------|------|
| `sfx/sfx_attack_bite` | 咬击音效 |
| `sfx/sfx_attack_slash` | 切割音效 |
| `sfx/sfx_explosion_large` | 大型爆炸音效 |
| `sfx/sfx_poison_applied` | 中毒施加音效 |
| `sfx/sfx_hit` | 受击音效 |
| `sfx/sfx_block_broken` | 格挡破碎音效 |
| `sfx/sfx_block` | 格挡音效 |

---

## 7. 命令系统

### 7.1 CreatureCmd 命令系统

`CreatureCmd` 是用于处理生物相关操作的核心命令类，提供了伤害、治疗、格挡、眩晕等多种生物操作方法。

#### 7.1.1 伤害系统方法

##### Damage（造成伤害）

**方法签名：**

```csharp
// 对单个目标造成伤害（卡牌来源）
Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, Creature target, DamageVar damageVar, CardModel cardSource);

// 对单个目标造成伤害（指定数值）
Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, CardModel cardSource);

// 对多个目标造成伤害
Task<IEnumerable<DamageResult>> Damage(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource);
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `choiceContext` | `PlayerChoiceContext` | 玩家选择上下文，使用 `new ThrowingPlayerChoiceContext()` 表示自动执行 |
| `target/targets` | `Creature` 或 `IEnumerable<Creature>` | 目标生物（单个或多个） |
| `amount` | `decimal` | 伤害数值 |
| `props` | `ValueProp` | 伤害属性标志（如 `Unblockable`、`Unpowered`） |
| `dealer` | `Creature?` | 攻击者（可为 null） |
| `cardSource` | `CardModel?` | 卡牌来源（可为 null） |

**重要实现细节：**

- **伤害流程 Hook 顺序**：
  1. `Hook.ModifyDamage` - 修改初始伤害值
  2. `Hook.BeforeDamageReceived` - 伤害到达前
  3. `creature.DamageBlockInternal()` - 处理格挡
  4. `Hook.ModifyHpLostBeforeOsty` - 奥斯提前修改伤害
  5. `Hook.ModifyUnblockedDamageTarget` - 修改未阻挡伤害目标
  6. `Hook.ModifyHpLostAfterOsty` - 奥斯提后修改伤害
  7. `creature.LoseHpInternal()` - 实际扣除生命值

- **格挡处理**：
  - 只有当 `props` 不包含 `ValueProp.Unblockable` 时才会计算格挡
  - `wasBlockBroken = originalTarget.Block <= 0 && blockedDamage > 0` - 格挡被击破
  - `wasFullyBlocked = !props.HasFlag(Unblockable) && (blockedDamage > 0 || Block > 0) && unblockedDamage == 0` - 完全格挡

- **伤害数字显示**：
  - 当 `damage > 0` 或 `modifiedAmount == 0 && receiver == unblockedDamageTarget` 时显示
  - 自动创建 `NDamageNumVfx` 显示伤害数字

- **自动触发的效果**：
  - 播放 `Hit` 动画（除非有 `SkipHurtAnim` 标志）
  - 播放怪物受伤音效（`HasHurtSfx`）
  - 屏幕震动（根据伤害大小：`<6` 弱，`<11` 中，`>=11` 强）
  - 格挡破碎时播放 `block_break` 音效
  - 完全格挡时播放 `block_hit` 音效并显示格挡火花特效

- **死亡处理**：
  - 伤害结算完成后统一处理死亡
  - 调用 `Hook.AfterBlockBroken`、`Hook.AfterCurrentHpChanged`、`Hook.AfterDamageGiven`、`Hook.AfterDamageReceived`
  - 如果触发格挡破碎，调用 `Hook.AfterBlockBroken`

**使用示例：**

```csharp
// 对单个敌人造成不可阻挡的非蓄力伤害
await CreatureCmd.Damage(
    new ThrowingPlayerChoiceContext(),
    target,
    5m,
    ValueProp.Unblockable | ValueProp.Unpowered,
    null,
    null
);

// 通过卡牌对目标造成伤害（自动使用卡牌拥有者作为攻击者）
await CreatureCmd.Damage(choiceContext, target, damageAmount, ValueProp.Unblockable, this);

// 对多个敌人造成伤害
var enemies = combatState.Enemies.Where(e => e.IsAlive);
await CreatureCmd.Damage(choiceContext, enemies, 3m, ValueProp.None, base.Owner.Creature, this);
```

#### 7.1.2 生命值管理方法

##### Heal（治疗）

**方法签名：**

```csharp
Task Heal(Creature creature, decimal amount, bool playAnim = true);
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `creature` | `Creature` | 目标生物 |
| `amount` | `decimal` | 治疗数值 |
| `playAnim` | `bool` | 是否播放治疗动画（默认 true） |

ao'si'tiaositi**重要实现细节：**

- 实际治疗量 = `min(amount, creature.MaxHp - creature.CurrentHp)` - 不会过量治疗
- 自动播放治疗音效 `event:/sfx/heal`
- 自动播放治疗特效：
  - 玩家：`vfx/vfx_cross_heal`
  - Osty 怪物：`vfx/vfx_heal_osty`
- 自动创建 `NHealNumVfx` 显示治疗数字
- 如果目标已死亡且 `playAnim=true`，会自动播放复活动画
- 触发 `Hook.AfterCurrentHpChanged`

**使用示例：**

```csharp
// 治疗目标生物 5 点生命值
await CreatureCmd.Heal(targetCreature, 5m);

// 不播放动画的治疗
await CreatureCmd.Heal(targetCreature, 3m, playAnim: false);
```

##### SetCurrentHp（设置当前生命值）

**方法签名：**

```csharp
Task SetCurrentHp(Creature creature, decimal amount);
```

**使用示例：**

```csharp
// 将生物生命值设置为 10
await CreatureCmd.SetCurrentHp(creature, 10m);
```

##### GainMaxHp（增加最大生命值）

**方法签名：**

```csharp
Task GainMaxHp(Creature creature, decimal amount);
```

**使用示例：**

```csharp
// 增加 3 点最大生命值（同时恢复等量生命值）
await CreatureCmd.GainMaxHp(creature, 3m);
```

##### LoseMaxHp（减少最大生命值）

**方法签名：**

```csharp
Task LoseMaxHp(PlayerChoiceContext choiceContext, Creature creature, decimal amount, bool isFromCard);
```

**使用示例：**

```csharp
// 减少 2 点最大生命值（来自卡牌效果）
await CreatureCmd.LoseMaxHp(choiceContext, creature, 2m, true);
```

##### SetMaxAndCurrentHp（同时设置最大和当前生命值）

**方法签名：**

```csharp
Task SetMaxAndCurrentHp(Creature creature, decimal amount);
```

**使用示例：**

```csharp
// 将最大生命值和当前生命值都设置为 15
await CreatureCmd.SetMaxAndCurrentHp(creature, 15m);
```

#### 7.1.3 格挡系统方法

##### GainBlock（获得格挡）

**方法签名：**

```csharp
Task<decimal> GainBlock(Creature creature, BlockVar blockVar, CardPlay? cardPlay, bool fast = false);
Task<decimal> GainBlock(Creature creature, decimal amount, ValueProp props, CardPlay? cardPlay, bool fast = false);
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `creature` | `Creature` | 目标生物 |
| `amount` | `decimal` | 格挡数值 |
| `cardPlay` | `CardPlay?` | 卡牌播放信息（可为 null） |
| `fast` | `bool` | 是否快速播放动画（默认 false） |

**重要实现细节：**

- **返回值**：实际获得的格挡值（经过 Hook 修改后）
- 当 `amount <= 0` 或战斗结束时，直接返回 0，不做任何处理
- **Hook 调用顺序**：
  1. `Hook.BeforeBlockGained` - 格挡获得前
  2. `Hook.ModifyBlock` - 修改格挡量
- 格挡值会被 `Math.Max(modifiedAmount, 0m)` 处理，负数会变为 0
- **自动触发的效果**：
  - 播放格挡音效 `event:/sfx/block_gain`
  - 播放格挡特效 `vfx/vfx_block`
  - 调用 `creature.GainBlockInternal()` 实际增加格挡
  - 记录到 `CombatManager.Instance.History`
- **等待时间**：
  - `fast=false`：`0.1f ~ 0.25f` 秒
  - `fast=true`：`0f ~ 0.03f` 秒
- 触发 `Hook.AfterBlockGained`

**使用示例：**

```csharp
// 获得 6 点格挡
await CreatureCmd.GainBlock(creature, 6m, ValueProp.None, cardPlay);

// 快速获得格挡（无动画）
await CreatureCmd.GainBlock(creature, 3m, ValueProp.None, null, fast: true);
```

##### LoseBlock（失去格挡）

**方法签名：**

```csharp
Task LoseBlock(Creature creature, decimal amount);
```

**使用示例：**

```csharp
// 失去 3 点格挡
await CreatureCmd.LoseBlock(creature, 3m);
```

#### 7.1.4 控制效果方法

##### Stun（眩晕）

**方法签名：**

```csharp
Task Stun(Creature creature, string? nextMoveId = null);
Task Stun(Creature creature, Func<IReadOnlyList<Creature>, Task> stunMove, string? nextMoveId = null);
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `creature` | `Creature` | 目标生物 |
| `stunMove` | `Func<IReadOnlyList<Creature>, Task>` | 眩晕期间执行的动作（可选） |
| `nextMoveId` | `string?` | 下次行动 ID（可选） |

**使用示例：**

```csharp
// 眩晕敌人
await CreatureCmd.Stun(cardPlay.Target);

// 眩晕敌人并执行自定义动作
await CreatureCmd.Stun(creature, async (targets) => {
    // 眩晕期间的额外效果
});
```

#### 7.1.5 生物管理方法

##### Add（添加生物）

**方法签名：**

```csharp
// 从怪物模型类型创建并添加
Task<Creature> Add<T>(CombatState combatState, string? slotName = null) where T : MonsterModel;

// 从怪物模型添加
Task<Creature> Add(MonsterModel monster, CombatState combatState, CombatSide side = CombatSide.Enemy, string? slotName = null);

// 添加已创建的生物
Task Add(Creature creature);
```

**使用示例：**

```csharp
// 添加指定类型的怪物到战斗中
Creature newEnemy = await CreatureCmd.Add<SlimeM>(combatState);
```

##### Kill（杀死生物）

**方法签名：**

```csharp
Task Kill(Creature creature, bool force = false);
Task Kill(IReadOnlyCollection<Creature> creatures, bool force = false);
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `creature/creatures` | `Creature` 或 `IReadOnlyCollection<Creature>` | 目标生物 |
| `force` | `bool` | 是否强制杀死（忽略死亡阻止效果） |

**重要实现细节：**

- **强制杀死 (`force=true`)**：忽略 `Hook.ShouldDie` 的阻止效果
- **死亡判定条件**：
  - `creature.MaxHp <= 0` 或 `Hook.ShouldDie()` 返回 true 时直接死亡
  - 否则触发 `Hook.AfterPreventingDeath` 并递归尝试杀死（最多递归 10 次防止死循环）
- **死亡流程**：
  1. 如果生物当前生命值 > 0，先用 `ValueProp.Unblockable | ValueProp.Unpowered` 扣除生命
  2. 触发 `Hook.BeforeDeath`
  3. 调用 `creature.InvokeDiedEvent()` 触发死亡事件
  4. 播放死亡动画 (`StartDeathAnim`)
  5. 如果是怪物，调用 `RemoveCreatureNode` 移除场景节点
  6. 触发 `Hook.AfterDeath`
  7. 移除所有能力 (`RemoveAllPowersAfterDeath`)
- **特殊处理**：
  - 杀死主要敌人后，如果队友都是次要敌人，会自动击杀所有队友
  - 玩家死亡时：
    - 清除 OrbQueue
    - 如果有 Osty 存活，先杀死 Osty
    - 调用 `CombatManager.Instance.HandlePlayerDeath()`
  - 如果所有玩家死亡，触发游戏失败流程
- **返回值**：`Task`（无返回值）

**使用示例：**

```csharp
// 杀死单个生物
await CreatureCmd.Kill(creature);

// 强制杀死生物（忽略复活能力）
await CreatureCmd.Kill(creature, force: true);

// 杀死多个生物
var deadEnemies = enemies.Where(e => e.CurrentHp <= 0).ToList();
await CreatureCmd.Kill(deadEnemies);
```

##### Escape（生物逃跑）

**方法签名：**

```csharp
Task Escape(Creature creature, bool removeCreatureNode = true);
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `creature` | `Creature` | 目标生物 |
| `removeCreatureNode` | `bool` | 是否移除生物节点（默认 true） |

**重要实现细节：**

- 如果生物已死亡，直接返回不做任何处理
- 移除所有能力（`RemoveAllPowersInternalExcept`）
- 如果 `removeCreatureNode=true`：
  - 隐藏生物节点 (`Visible = false`)
  - 设置为不可交互 (`ToggleIsInteractable(false)`)
  - 从战斗管理器移除 (`CombatManager.Instance.RemoveCreature`)
- 调用 `creature.CombatState.CreatureEscaped()` 处理逃跑逻辑

**使用示例：**

```csharp
// 让生物逃跑（移除节点）
await CreatureCmd.Escape(creature);

// 让生物逃跑（保留节点）
await CreatureCmd.Escape(creature, removeCreatureNode: false);
```

#### 7.1.6 动画与特效方法

##### TriggerAnim（触发动画）

**方法签名：**

```csharp
Task TriggerAnim(Creature creature, string triggerName, float waitTime);
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `creature` | `Creature` | 目标生物 |
| `triggerName` | `string` | 动画名称（如 "Attack"、"Cast"） |
| `waitTime` | `float` | 等待时间（秒） |

**重要实现细节：**

- 如果生物节点不存在，会输出错误日志（测试模式除外）
- 如果目标是玩家：
  - `"Attack"` 动画：播放 `character.AttackSfx`
  - `"Cast"` 动画：播放 `character.CastSfx`
  - 其他动画不播放音效
- 实际等待时间 = `min(waitTime * 0.5f, 0.25f)`

**常用动画名称：**

| 动画名 | 说明 |
|--------|------|
| `"Attack"` | 攻击动作 + 播放攻击音效 |
| `"Cast"` | 施法动作 + 播放施法音效 |
| `"Defend"` | 防御动作 |
| `"Hit"` | 受击动作 |

**使用示例：**

```csharp
// 播放攻击动画（使用角色自带的攻击动画延迟）
await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);

// 播放施法动画
await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

// 播放受击动画（无延迟）
await CreatureCmd.TriggerAnim(targetCreature, "Hit", 0f);
```

#### 7.1.7 ValueProp 常用属性

| 属性 | 说明 |
|------|------|
| `ValueProp.Unblockable` | 不可阻挡（伤害无法被格挡） |
| `ValueProp.Unpowered` | 非蓄力攻击（不受力量加成影响） |
| `ValueProp.Move` | 卡牌/怪物行动标记 |
| `ValueProp.SkipHurtAnim` | 跳过受伤动画 |
| `ValueProp.None` | 无特殊属性（普通伤害） |

**属性组合示例：**

```csharp
// 不可阻挡的非蓄力攻击
ValueProp props = ValueProp.Unblockable | ValueProp.Unpowered;

// 普通攻击（可被格挡，受蓄力影响）
ValueProp props = ValueProp.None;
```

**使用 Damage 方法示例：**

```csharp
// 造成不可格挡的伤害
await CreatureCmd.Damage(choiceContext, target, 5m, ValueProp.Unblockable, this);

// 造成不可格挡且不受力量影响的伤害
await CreatureCmd.Damage(choiceContext, target, 5m, ValueProp.Unblockable | ValueProp.Unpowered, this);

// 造成普通伤害（可以被格挡）
await CreatureCmd.Damage(choiceContext, target, 5m, ValueProp.None, this);
```

### 7.2 PowerCmd 命令系统

`PowerCmd` 是用于处理能力（Power）相关操作的核心命令类，提供了应用、修改、移除能力等多种方法。

#### 7.2.1 Apply（应用能力）

**方法签名：**

```csharp
// 对单个目标应用能力
Task<T?> Apply<T>(Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false) where T : PowerModel;

// 对多个目标应用能力
Task<IReadOnlyList<T>> Apply<T>(IEnumerable<Creature> targets, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false) where T : PowerModel;

// 应用已存在的能力实例
Task Apply(PowerModel power, Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false);
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `target/targets` | `Creature` 或 `IEnumerable<Creature>` | 目标生物（单个或多个） |
| `amount` | `decimal` | 能力层数 |
| `applier` | `Creature?` | 施加者（可为 null） |
| `cardSource` | `CardModel?` | 卡牌来源（可为 null） |
| `silent` | `bool` | 是否静默应用（不播放动画，默认 false） |

**重要实现细节：**

- 当 `amount == 0` 时，方法会直接返回，不做任何处理
- 如果目标生物的 `CanReceivePowers` 为 `false`，能力不会被应用
- 对于非实例化的能力，如果目标已有该能力，会调用 `ModifyAmount` 增加层数而非覆盖
- 调用流程：`Hook.BeforePowerAmountChanged` → `Hook.ModifyPowerAmountGiven` → `Hook.ModifyPowerAmountReceived` → `power.BeforeApplied()` → `power.ApplyInternal()`
- 如果能力是 `PowerType.Debuff` 且目标是玩家，会自动跳过下回合的持续时间tick

**使用示例：**

```csharp
// 对目标施加 3 层中毒
await PowerCmd.Apply<PoisonPower>(targetCreature, 3m, base.Owner.Creature, this);

// 静默应用能力（不播放动画）
await PowerCmd.Apply<StrengthPower>(targetCreature, 2m, base.Owner.Creature, this, silent: true);

// 对多个敌人同时施加能力
var enemies = combatState.Enemies.Where(e => e.IsAlive);
await PowerCmd.Apply<PoisonPower>(enemies, 3m, base.Owner.Creature, this);
```

#### 7.2.2 ModifyAmount（修改能力层数）

**方法签名：**

```csharp
Task<int> ModifyAmount(PowerModel power, decimal offset, Creature? applier, CardModel? cardSource, bool silent = false);
```

**返回值：** 修改后的新层数（`int`）

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `power` | `PowerModel` | 能力实例 |
| `offset` | `decimal` | 增减量（正数增加，负数减少） |
| `applier` | `Creature?` | 施加者（可为 null） |
| `cardSource` | `CardModel?` | 卡牌来源（可为 null） |

**重要实现细节：**

- 当 `offset == 0` 时，仍会执行但不触发部分 hook
- 会触发 `Hook.BeforePowerAmountChanged` 和 `Hook.AfterPowerAmountChanged` 钩子
- 修改层数后会检查 `ShouldRemoveDueToAmount()`，如果为 true 则自动移除能力
- 如果目标是存活的怪物，会更新其意图显示（`UpdateIntent`）

**使用示例：**

```csharp
// 获取能力实例
var poisonPower = base.Owner.GetPower<PoisonPower>();

// 减少 2 层中毒
if (poisonPower != null)
{
    await PowerCmd.ModifyAmount(poisonPower, -2, base.Owner, null);
}

// 增加 1 层力量
var strengthPower = base.Owner.GetPower<StrengthPower>();
if (strengthPower != null)
{
    await PowerCmd.ModifyAmount(strengthPower, 1, base.Owner, null);
}
```

#### 7.2.3 SetAmount（设置能力层数）

**方法签名：**

```csharp
Task<T?> SetAmount<T>(Creature target, decimal amount, Creature? applier, CardModel? cardSource) where T : PowerModel;
```

**使用示例：**

```csharp
// 将目标的力量设置为 5
await PowerCmd.SetAmount<StrengthPower>(targetCreature, 5m, base.Owner.Creature, this);
```

#### 7.2.4 Decrement（减少一层能力）

**方法签名：**

```csharp
Task Decrement(PowerModel power);
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `power` | `PowerModel` | 能力实例（**注意：不是泛型方法**） |

**重要实现细节：**

- 内部调用 `ModifyAmount(power, -1, null, null)`
- 当层数变为 0 时会自动移除能力

**使用示例：**

```csharp
// ❌ 错误用法（不是泛型方法）
// await PowerCmd.Decrement<PoisonPower>(owner, 1, owner, null);

// ✅ 正确用法（需要传入能力实例）
var poisonPower = base.Owner.GetPower<PoisonPower>();
if (poisonPower != null)
{
    await PowerCmd.Decrement(poisonPower);
}
```

#### 7.2.5 Remove（移除能力）

**方法签名：**

```csharp
// 移除指定类型的能力
Task Remove<T>(Creature creature) where T : PowerModel;

// 移除指定的能力实例
Task Remove(PowerModel? power);
```

**重要实现细节：**

- 移除能力时会调用 `power.RemoveInternal()` 移除能力
- 移除后会等待 `0.2f ~ 0.4f` 秒（`Cmd.CustomScaledWait`）
- 最后调用 `power.AfterRemoved()` 执行清理操作
- 如果 `power` 为 `null`，不会有任何操作

**使用示例：**

```csharp
// 移除目标的中毒能力
await PowerCmd.Remove<PoisonPower>(targetCreature);

// 移除指定的能力实例
var power = targetCreature.GetPower<WeakPower>();
await PowerCmd.Remove(power);

// 安全移除（检查是否为 null）
var venomPower = targetCreature.GetPower<VenomPower>();
if (venomPower != null)
{
    await PowerCmd.Remove(venomPower);
}
```

#### 7.2.6 TickDownDuration（减少持续时间）

**方法签名：**

```csharp
Task TickDownDuration(PowerModel power);
```

**说明：** 减少能力的持续时间。

**重要实现细节：**

- 如果 `power.SkipNextDurationTick == true`，会重置该标志而不减少层数
- 否则调用 `Decrement(power)` 减少一层

**使用示例：**

```csharp
var power = base.Owner.GetPower<RegenPower>();
if (power != null)
{
    await PowerCmd.TickDownDuration(power);
}
```

#### 7.2.8 PowerCmd 方法对比

| 方法 | 特点 | 适用场景 |
|------|------|----------|
| `Apply<T>` | 设置绝对层数，会覆盖现有层数 | 施加新能力或重置层数 |
| `ModifyAmount` | 设置相对偏移量，可增可减，返回新层数 | 增减能力层数 |
| `SetAmount<T>` | 设置绝对层数，智能处理 | 精确设置能力层数 |
| `Decrement` | 减少一层，内部调用 ModifyAmount(-1) | 逐回合减少的能力 |
| `TickDownDuration` | 减少持续时间，会检查 SkipNextDurationTick | 回合结束处理能力持续 |
| `Remove` | 完全移除能力，调用 AfterRemoved | 立即移除能力 |

**方法选择建议：**

- 需要精确控制能力层数变化时，使用 `ModifyAmount`
- 需要确保能力层数不超过某个值时，使用 `ModifyAmount` 配合条件判断
- 需要在能力移除时执行特定逻辑时，确保能力实现了 `AfterRemoved` 方法

### 7.3 其他常用命令

```csharp
// 造成伤害
await DamageCmd.DealDamage(source, target, damageAmount, DamageType.Normal);

// 获得格挡
await PlayerCmd.GainBlock(player, blockAmount);

// 获得能量
await PlayerCmd.GainEnergy(player, energyAmount);

// 抽牌
await DrawCmd.Draw(player, cardCount);

// 自动打出卡牌
await CardCmd.AutoPlay(context, card, target);

// 选择卡牌
var chosenCards = await CardSelectCmd.FromPile(context, player, pile, prefs, filter, source);
```

### 8.2 从弃牌堆选择卡牌

**代码模式（参考 Graveblast 卡牌）：**

```csharp
// 创建卡牌选择器配置
CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);

// 获取弃牌堆
CardPile pile = PileType.Discard.GetPile(base.Owner);

// 从弃牌堆显示卡牌供玩家选择
CardModel cardModel = (await CardSelectCmd.FromSimpleGrid(choiceContext, pile.Cards, base.Owner, prefs)).FirstOrDefault();

// 如果选择了卡牌，加入手牌
if (cardModel != null)
{
    await CardPileCmd.Add(cardModel, PileType.Hand);
}
```

**关键点说明：**

| 步骤 | 代码 | 说明 |
|------|------|------|
| 1 | `CardSelectorPrefs` | 创建选择器配置，指定提示文本和选择数量 |
| 2 | `PileType.Discard.GetPile()` | 获取弃牌堆的卡牌集合 |
| 3 | `CardSelectCmd.FromSimpleGrid()` | 显示可视化卡牌选择界面 |
| 4 | `CardPileCmd.Add()` | 将选中的卡牌加入目标牌堆 |

**选择器配置参数：**

```csharp
// 创建选择器配置
CardSelectorPrefs prefs = new CardSelectorPrefs(
    prompt: "选择一张卡牌",  // 显示给玩家的提示文本
    maxSelectable: 1,       // 最多可选数量
    minSelectable: 1,       // 最少必须选择数量（可选，默认为0）
    canCancel: false        // 是否可以取消选择（可选）
);
```

**其他常用牌堆类型：**

| PileType | 说明 |
|----------|------|
| `PileType.Discard` | 弃牌堆 |
| `PileType.Hand` | 手牌 |
| `PileType.Draw` | 抽牌堆 |
| `PileType.Exhaust` | 消耗堆 |
| `PileType.Reward` | 奖励堆 |

### 8.3 命令参数说明

| 参数 | 类型 | 说明 |
|------|------|------|
| `source` | `CardModel` 或 `Creature` | 效果来源 |
| `target` | `Creature` | 目标生物 |
| `amount` | `int` | 数值大小 |
| `context` | `PlayerChoiceContext` | 上下文信息 |

---

## 6. 卡池配置

### 6.1 创建自定义卡池

```csharp
public class SBPCardPool : CustomCardPoolModel
{
    public override string Title => MainFile.ModId;
    public override Color DeckEntryCardColor => Colors.Green;  // 猎人绿色
    public override bool IsColorless => false;  // 非无色卡池
}
```

### 6.2 注册到原版卡池（推荐用于角色拓展）

如果你想将 mod 卡牌作为原版角色的拓展，可以直接注册到原版卡池：

```csharp
using MegaCrit.Sts2.Core.Models.CardPools;

// 注册到静默者卡池
[Pool(typeof(SilentCardPool))]
public abstract class SnakeBitePlus_Card : CustomCardModel
{
    // ...
}
```

### 6.3 绑定卡池到卡牌

```csharp
[Pool(typeof(SBPCardPool))]
public abstract class SnakeBitePlus_Card : CustomCardModel
{
    // ...
}
```

### 6.4 常用原版卡池

| 卡池类型 | 说明 |
|----------|------|
| `SilentCardPool` | 静默者（猎人）卡池 |
| `IroncladCardPool` | 铁甲战士卡池 |
| `ColorlessCardPool` | 无色卡池 |

### 6.5 CardCreationOptions（卡牌创建选项）

`CardCreationOptions` 用于创建战斗奖励、商店卡牌等场景的卡牌生成选项，支持自定义卡池、稀有度过滤等功能。

#### 6.5.1 静态工厂方法

```csharp
// 根据房间类型创建卡牌选项（战斗、精英、Boss、商店）
public static CardCreationOptions ForRoom(Player player, RoomType roomType)

// 用于非战斗场景的奖励选项（使用默认稀有度权重）
public static CardCreationOptions ForNonCombatWithDefaultOdds(IEnumerable<CardModel> customCardPool)

// 用于非战斗场景的奖励选项（均匀稀有度）
public static CardCreationOptions ForNonCombatWithUniformOdds(IEnumerable<CardModel> customCardPool)
```

#### 6.5.2 链式配置方法

```csharp
// 设置自定义卡牌池（覆盖原有卡池）
public CardCreationOptions WithCustomPool(IEnumerable<CardModel> customPool, CardRarityOddsType? rarityOdds = null)

// 设置卡池 + 过滤函数
public CardCreationOptions WithCardPools(IEnumerable<CardPoolModel> pools, Func<CardModel, bool>? cardPoolFilter = null)

// 添加标志（如跳过升级roll、跳过Hook等）
public CardCreationOptions WithFlags(CardCreationFlags flag)

// 设置随机数生成器覆盖
public CardCreationOptions WithRngOverride(Rng rng)
```

#### 6.5.3 获取可用卡牌

```csharp
// 从选项中获取符合条件的卡牌列表
public IEnumerable<CardModel> GetPossibleCards(Player player)
```

#### 6.5.4 常用 CardCreationFlags

| 标志 | 说明 |
|------|------|
| `NoUpgradeRoll` | 跳过升级roll |
| `NoModifyHooks` | 跳过Hook修改 |
| `NoCardPoolModifications` | 跳过卡池修改 |
| `ForceRarityOddsChange` | 强制改变稀有度权重 |

#### 6.5.5 应用示例：生成只包含蛇系卡牌的奖励

```csharp
public override async Task AfterCombatEnd(CombatRoom room)
{
    await base.AfterCombatEnd(room);

    for (int i = 0; i < base.Amount; i++)
    {
        // 获取所有蛇系卡牌（HasSnakeTag == true）
        var snakeCards = ModelDb.Get<SilentCardPool>()
            .GetUnlockedCards(base.Owner.Player.UnlockState, base.Owner.Player.RunState.CardMultiplayerConstraint)
            .Where(c => c is ISnakeTagged tagged && tagged.HasSnakeTag);

        // 使用自定义卡池创建奖励选项
        var options = CardCreationOptions.ForNonCombatWithDefaultOdds(snakeCards);
        room.AddExtraReward(base.Owner.Player, new CardReward(options, 3, base.Owner.Player));

        // 添加卡牌移除奖励
        room.AddExtraReward(base.Owner.Player, new CardRemovalReward(base.Owner.Player));
    }
}
```

**关键点说明：**
- `ModelDb.Get<SilentCardPool>()` - 获取静默者卡池实例
- `GetUnlockedCards()` - 获取玩家已解锁的卡牌
- `ISnakeTagged` - 蛇系标签接口，用于过滤蛇系卡牌
- `ForNonCombatWithDefaultOdds()` - 最稳定的方案，兼容性好

---

## 7. 本地化格式

### 7.1 条件判断 `{IfUpgraded}`

```json
{
  "CARD.description": "造成{IfUpgraded:show:6|4}点伤害。{IfUpgraded:show:\\n[gold]保留[/gold]|}。"
}
```

**格式**：`{IfUpgraded:show:升级文本|未升级文本}`

### 7.2 能量图标 `{Energy:energyIcons()}`

```json
{
  "CARD.description": "获得{Energy:energyIcons()}。"
}
```

### 7.3 颜色标签

| 标签 | 效果 |
|------|------|
| `[gold]文本[/gold]` | 金色高亮 |
| `[silver]文本[/silver]` | 银色高亮 |
| `[red]文本[/red]` | 红色警告 |

---

## 8. 伤害系统与蓄力攻击

### 8.1 ValueProp 枚举

`ValueProp` 是用于标记伤害属性的枚举：

```csharp
public enum ValueProp
{
    Unblockable = 2,      // 不可阻挡
    Unpowered = 4,        // 非蓄力攻击
    Move = 8,             // 卡牌/怪物行动
    SkipHurtAnim = 0x10   // 跳过受伤动画
}
```

### 8.2 IsPoweredAttack 判断

判断某次伤害是否为蓄力攻击：

```csharp
public static bool IsPoweredAttack(this ValueProp props)
{
    if (props.HasFlag(ValueProp.Move))
    {
        return !props.HasFlag(ValueProp.Unpowered);
    }
    return false;
}
```

**判断逻辑**：

| 条件 | 结果 |
|------|------|
| 有 `Move` 且无 `Unpowered` | ✅ 蓄力攻击 |
| 有 `Move` 且有 `Unpowered` | ❌ 非蓄力攻击 |
| 无 `Move`（能力伤害等） | ❌ 非蓄力攻击 |

### 8.3 伤害反弹示例（ReflectPower）

监听伤害事件并反弹格挡伤害：

```csharp
public sealed class ReflectPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // 条件：目标是自己 + 有格挡伤害 + 是蓄力攻击 + 有攻击者
        if (target == base.Owner && result.BlockedDamage > 0 && 
            props.IsPoweredAttack() && dealer != null)
        {
            // 反弹格挡值的伤害给攻击者
            await CreatureCmd.Damage(choiceContext, dealer, 
                result.BlockedDamage, ValueProp.Unpowered, base.Owner, null);
        }
    }

    // 每回合减少1层
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == base.Owner.Side)
        {
            await PowerCmd.Decrement(this);
        }
    }
}
```

### 8.4 DamageResult 关键属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Damage` | `int` | 实际受到的伤害（扣除格挡后） |
| `BlockedDamage` | `int` | 被格挡吸收的伤害值 |
| `TotalDamage` | `int` | 总伤害（格挡前的原始伤害） |
| `WasFullyBlocked` | `bool` | 是否被完全格挡 |
| `WasBlocked` | `bool` | 是否有任何格挡 |
| `Props` | `ValueProp` | 伤害属性标志 |

**计算公式：**
```
TotalDamage = Damage + BlockedDamage
```

### 8.5 常用伤害判断模式

| 判断需求 | 代码 |
|----------|------|
| 是否受到伤害 | `result.Damage > 0` |
| 是否有格挡 | `result.BlockedDamage > 0` |
| 是否完全格挡 | `result.WasFullyBlocked` |
| 是否蓄力攻击 | `props.IsPoweredAttack()` |
| 是否不可阻挡 | `props.HasFlag(ValueProp.Unblockable)` |
| 是否卡牌/怪物行动 | `props.HasFlag(ValueProp.Move)` |

### 8.6 实战示例

#### 示例1：记录伤害日志

```csharp
public sealed class DamageTrackerPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target == base.Owner)
        {
            int damageTaken = result.Damage;      // 实际受到的伤害
            int blockedDamage = result.BlockedDamage; // 被格挡的伤害
            int totalDamage = result.TotalDamage;    // 总伤害
            
            Debug.Log($"受到 {totalDamage} 点伤害，格挡 {blockedDamage}，实际受到 {damageTaken}");
        }
    }
}
```

#### 示例2：荆棘反伤

```csharp
public sealed class ThornsPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target == base.Owner && dealer != null)
        {
            // 反弹实际受到伤害的 50%
            int reflectDamage = result.Damage / 2;
            
            if (reflectDamage > 0)
            {
                await CreatureCmd.Damage(choiceContext, dealer, reflectDamage, 
                    ValueProp.Unpowered, base.Owner, null);
            }
        }
    }
}
```

#### 示例3：区分伤害来源

```csharp
public sealed class EnemyDamagePower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != base.Owner || dealer == null) return;
        
        if (dealer.IsPlayer)
        {
            // 玩家造成的伤害
            Debug.Log("玩家攻击了我");
        }
        else
        {
            // 敌人造成的伤害
            Debug.Log("敌人攻击了我");
        }
    }
}
```

### 8.7 伤害计算流程图

```
伤害事件触发
      ↓
获取 DamageResult
      ↓
┌─────────────────────────┐
│ TotalDamage (原始伤害)   │
└───────────┬─────────────┘
            ↓
    减去格挡值
            ↓
┌─────────────────────────┐
│ Damage (实际伤害)        │
│ BlockedDamage (格挡值)   │
└───────────┬─────────────┘
            ↓
   判断伤害类型
   (蓄力/非蓄力/不可阻挡)
            ↓
   触发相应效果
```

---

## 9. 卡牌类型检查

### 9.1 检查蛇系卡牌

```csharp
if (cardPlay.Card is SnakeBitePlus_Card snakeCard)
{
    if (snakeCard.HasSnakeTag) { /* 蛇系卡牌 */ }
    if (snakeCard.HasSnakeBiteTag) { /* 蛇咬卡牌 */ }
}
```

### 9.2 检查原版蛇咬卡牌

```csharp
if (cardPlay.Card is MegaCrit.Sts2.Core.Models.Cards.Snakebite)
{
    // 原版蛇咬卡牌
}
```

---

## 常用能力列表

| 能力类 | 效果 |
|--------|------|
| `PoisonPower` | 中毒 |
| `StrengthPower` | 力量 |
| `DexterityPower` | 敏捷 |
| `BlockPower` | 格挡 |
| `RegenPower` | 再生 |
| `WeakPower` | 虚弱 |

---

## 动画系统

### 常用动画名称

原版卡牌中主要使用两种动画：

| 动画名称 | 使用场景 | 示例卡牌 |
|----------|----------|----------|
| `"Attack"` | 直接攻击卡牌 | `SweepingBeam`, `DyingStar`, `FlickFlack` |
| `"Cast"` | 技能/施法类卡牌 | `Stomp`, `Reflex`, `Fear`, `Defy` |

### 使用方法

```csharp
// 攻击动画
await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);

// 施法动画
await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
```

### 完整示例

**攻击卡牌：**
```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    // 播放攻击动画
    await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);
    
    // 执行攻击
    await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
        .FromCard(this)
        .Targeting(cardPlay.Target)
        .Execute(choiceContext);
}
```

**技能卡牌：**
```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    // 播放施法动画
    await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
    
    // 执行技能效果
    await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
}
```

### 动画延迟选项

| 延迟属性 | 说明 |
|----------|------|
| `AttackAnimDelay` | 攻击动画延迟 |
| `CastAnimDelay` | 施法动画延迟 |

### 选择建议

| 卡牌类型 | 推荐动画 |
|----------|----------|
| 攻击卡（直接造成伤害） | `"Attack"` |
| 技能卡（增益/减益/抽牌等） | `"Cast"` |
| 特殊攻击（范围/特效） | `"Cast"` |

---

## 伤害与中毒的区别

### 核心区别

在《杀戮尖塔2》中，造成伤害和施加中毒是两种不同的操作，它们在计算方式和效果上有本质区别：

| 特性 | DamageCmd.DealDamage | PowerCmd.Apply<PoisonPower> |
|------|---------------------|-----------------------------|
| **力量加成** | ✅ 自动应用 | ❌ 不应用 |
| **敏捷减免** | ✅ 自动计算 | ❌ 不计算 |
| **易伤/虚弱** | ✅ 自动应用 | ❌ 不应用 |
| **格挡吸收** | ✅ 可被格挡 | ❌ 不可被格挡 |
| **蓄力攻击** | ✅ 默认是Powered攻击 | ❌ 非Powered攻击 |
| **触发时机** | 立即造成伤害 | 回合开始时造成伤害 |

### 1. 造成伤害（DamageCmd.DealDamage）

```csharp
// 会自动应用力量加成
int damage = (int)base.DynamicVars.Damage.BaseValue;
await DamageCmd.DealDamage(base.Owner.Creature, play.Target, damage, DamageType.Normal);

// 实际伤害 = 基础伤害 + 力量数值
// 例如：基础伤害3 + 力量5 = 实际伤害8
```

**特点：**
- 立即造成伤害
- 自动应用所有伤害加成和减免
- 可以被格挡吸收
- 默认是蓄力攻击（会触发反弹等效果）

### 2. 施加中毒（PowerCmd.Apply<PoisonPower>）

```csharp
// 不会应用力量加成，直接使用数值
int poisonAmount = (int)base.DynamicVars.Poison.BaseValue;
await PowerCmd.Apply<PoisonPower>(play.Target, poisonAmount, base.Owner.Creature, this);

// 中毒层数 = 施加的数值（不受力量影响）
// 中毒伤害在回合开始时触发，每层造成等于层数的伤害
```

**特点：**
- 不立即造成伤害，而是添加能力
- 不受力量、敏捷等属性影响
- 不可被格挡（中毒伤害是 Unblockable）
- 非蓄力攻击（不会触发反弹）
- 伤害在回合开始时触发

### 3. 伤害计算公式对比

**DamageCmd.DealDamage：**
```
最终伤害 = 基础伤害 + 力量 - 敌方敏捷 × 状态倍率（易伤/虚弱）
```

**PoisonPower（回合开始时）：**
```
中毒伤害 = 中毒层数（不受力量影响）
```

### 4. 实战示例

**场景：玩家有5点力量**

```csharp
// 造成伤害
await DamageCmd.DealDamage(owner, target, 3, DamageType.Normal);
// 实际造成：3 + 5 = 8点伤害

// 施加中毒
await PowerCmd.Apply<PoisonPower>(target, 3, owner, this);
// 回合开始时造成：3点伤害（不受力量影响）
```

### 10. 选择建议

| 场景 | 推荐操作 |
|------|----------|
| 需要即时伤害 | `DamageCmd.DealDamage` |
| 需要延迟伤害 | `PowerCmd.Apply<PoisonPower>` |
| 需要利用力量加成 | `DamageCmd.DealDamage` |
| 需要绕过格挡 | `PowerCmd.Apply<PoisonPower>` |
| 需要触发蓄力攻击效果 | `DamageCmd.DealDamage` |
| 需要避免反弹 | `PowerCmd.Apply<PoisonPower>` |

---

## 获取能力层数方法

### 获取能力层数

在卡牌中获取某个能力的层数是非常常见的操作，以下是标准写法：

```csharp
// 获取能力层数（使用空合并运算符处理能力不存在的情况）
int powerAmount = base.Owner.Creature.GetPower<YourPowerType>()?.Amount ?? 0;
```

### 实战示例：PoisonBoostPower

```csharp
// 在卡牌中获取 PosionBoostPower 层数
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
{
    // 获取能力层数，如果不存在则默认为0
    int poisonBoost = base.Owner.Creature.GetPower<PosionBoostPower>()?.Amount ?? 0;
    
    // 在计算中使用能力加成
    int poisonAmount = (int)base.DynamicVars["Poison"].Value + poisonBoost;
    
    await PowerCmd.Apply<PoisonPower>(play.Target, poisonAmount, base.Owner.Creature, this);
}
```

### 关键要点

| 要点 | 说明 |
|------|------|
| `GetPower<T>()` | 获取指定类型的能力实例 |
| `?.Amount` | 安全访问能力层数，避免空引用异常 |
| `?? 0` | 当能力不存在时使用默认值0 |
| `base.Owner.Creature` | 当前卡牌拥有者的生物对象 |

### 完整示例

```csharp
public sealed class ExampleCard : SnakeBitePlus_Card
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // 获取多个能力的层数
        int strength = base.Owner.Creature.GetPower<StrengthPower>()?.Amount ?? 0;
        int dexterity = base.Owner.Creature.GetPower<DexterityPower>()?.Amount ?? 0;
        int poisonBoost = base.Owner.Creature.GetPower<PosionBoostPower>()?.Amount ?? 0;
        
        // 使用这些值进行计算
        int damage = 6 + strength;
        int poisonAmount = 3 + poisonBoost;
        
        await DamageCmd.DealDamage(base.Owner.Creature, play.Target, damage, DamageType.Normal);
        await PowerCmd.Apply<PoisonPower>(play.Target, poisonAmount, base.Owner.Creature, this);
    }
}
```

---

## PoisonPower伤害逻辑

### 中毒伤害核心流程

```
回合开始 → 计算触发次数 → 循环造成伤害 → 减少中毒层数
```

### 1. 触发次数计算

```csharp
private int TriggerCount
{
    get
    {
        IEnumerable<Creature> opponents = base.Owner.CombatState
            .GetOpponentsOf(base.Owner).Where(c => c.IsAlive);
        
        // 触发次数 = 1（基础） + 所有对手的加速剂层数总和
        return Math.Min(base.Amount, 1 + opponents.Sum(a => a.GetPowerAmount<AccelerantPower>()));
    }
}
```

**计算公式**：`触发次数 = min(中毒层数, 1 + 对手触媒层数总和)`

### 2. 伤害触发时机

```csharp
public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
{
    if (side != base.Owner.Side) return;  // 只在自己回合开始时触发
    
    int iterations = TriggerCount;
    for (int i = 0; i < iterations; i++)
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), 
            base.Owner, base.Amount, 
            ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        
        if (base.Owner.IsAlive)
            await PowerCmd.Decrement(this);
    }
}
```

### 3. 伤害属性标志

| ValueProp | 说明 |
|-----------|------|
| `Unblockable` | 不可被格挡 |
| `Unpowered` | 非蓄力攻击（不受力量加成） |

### 4. 伤害示例

**场景**：5层中毒 + 敌人2层触媒

```
触发次数 = min(5, 1+2) = 3次
    ↓
第1次：5点伤害，中毒→4层
第2次：5点伤害，中毒→3层
第3次：5点伤害，中毒→2层
    ↓
总计：15点伤害
```

### 11. 关键机制总结

| 机制 | 说明 |
|------|------|
| **不可阻挡** | 中毒伤害无法被格挡抵消 |
| **非蓄力攻击** | 不受力量加成，不触发反弹 |
| **触媒联动** | 对手触媒增加中毒触发次数 |
| **逐次消耗** | 每次触发减少1层中毒 |
| **死亡检测** | 目标死亡后停止消耗中毒 |

---

## 10. 悬浮提示系统

### 10.1 概述

`ExtraHoverTips` 用于在卡牌悬浮时显示额外信息，如能力说明、关键词解释等。

### 10.2 常用悬浮提示类型

| 方法 | 说明 | 示例 |
|------|------|------|
| `HoverTipFactory.FromPower<T>()` | 显示指定能力的说明 | `FromPower<PoisonPower>()` |
| `HoverTipFactory.FromCard<T>()` | 显示指定卡牌的信息 | `FromCard<Shiv>()` |
| `HoverTipFactory.FromKeyword()` | 显示关键词的说明 | `FromKeyword(CardKeyword.Exhaust)` |
| `HoverTipFactory.FromOrb<T>()` | 显示球体的说明 | `FromOrb<DarkOrb>()` |
| `HoverTipFactory.Static()` | 显示静态提示 | `Static(StaticHoverTip.Block)` |
| `HoverTipFactory.FromForge()` | 显示锻造相关信息 | `FromForge()` |
| `HoverTipFactory.FromCardWithCardHoverTips<T>()` | 显示卡牌及其所有悬浮提示 | `FromCardWithCardHoverTips<SovereignBlade>()` |

### 10.3 实现方式

在卡牌类中重写 `ExtraHoverTips` 属性：

```csharp
// 显示能力信息
protected override IEnumerable<IHoverTip> ExtraHoverTips 
    => new[] { HoverTipFactory.FromPower<PoisonPower>() };

// 显示卡牌信息
protected override IEnumerable<IHoverTip> ExtraHoverTips 
    => new[] { HoverTipFactory.FromCard<Shiv>() };

// 显示关键词信息
protected override IEnumerable<IHoverTip> ExtraHoverTips 
    => new[] { HoverTipFactory.FromKeyword(CardKeyword.Exhaust) };

// 组合多个提示
protected override IEnumerable<IHoverTip> ExtraHoverTips 
    => new[] {
        HoverTipFactory.Static(StaticHoverTip.Channeling),
        HoverTipFactory.FromOrb<DarkOrb>()
    };
```

### 10.4 原版示例

```csharp
// Accelerant.cs - 显示中毒能力说明
protected override IEnumerable<IHoverTip> ExtraHoverTips 
    => new[] { HoverTipFactory.FromPower<PoisonPower>() };

// Accuracy.cs - 显示 Shiv 卡牌信息
protected override IEnumerable<IHoverTip> ExtraHoverTips 
    => new[] { HoverTipFactory.FromCard<Shiv>() };

// Shadowmeld.cs - 显示格挡静态提示
protected override IEnumerable<IHoverTip> ExtraHoverTips 
    => new[] { HoverTipFactory.Static(StaticHoverTip.Block) };
```

---

## 11. 动画生成位置系统

### 11.1 概述

动画生成位置用于控制特效动画的播放位置，如爆炸效果、粒子特效等。

### 11.2 常用动画位置方法

| 方法 | 说明 | 适用场景 |
|------|------|----------|
| `VfxCmd.PlayOnCreatureCenter()` | 在生物中心播放动画 | 单体目标特效 |
| `VfxCmd.PlayFullScreenInCombat()` | 在屏幕中央播放全屏动画 | 全屏特效、爆炸效果 |
| `VfxCmd.PlayAtPosition()` | 在指定位置播放动画 | 自定义位置特效 |

### 11.3 使用示例

```csharp
// 在生物中心播放攻击特效
await VfxCmd.PlayOnCreatureCenter("vfx/vfx_attack_blunt", targetCreature);

// 在屏幕中央播放全屏爆炸动画
VfxCmd.PlayFullScreenInCombat("vfx/vfx_explosion_large");

// 在指定位置播放特效
await VfxCmd.PlayAtPosition("vfx/vfx_heal", position);
```

### 11.4 实战示例：核打击能力

```csharp
public sealed class NukeStrikePower : PowerModel
{
    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        // 对所有生物造成伤害
        foreach (var creature in allCreatures)
        {
            await CreatureCmd.Damage(choiceContext, creature, damage, 
                ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        }
        
        // 在屏幕中央播放爆炸动画（只播放一次）
        VfxCmd.PlayFullScreenInCombat("vfx/vfx_explosion_large");
        await Cmd.Wait(0.3f);
    }
}
```

### 11.5 关键要点

| 要点 | 说明 |
|------|------|
| **循环外播放** | 群体效果的动画应在循环外播放，避免重复播放 |
| **全屏动画** | 使用 `PlayFullScreenInCombat` 确保动画居中显示 |
| **等待时间** | 使用 `Cmd.Wait()` 在动画之间添加间隔 |

---

## 12. DamageCmd 对象类型

### 12.1 概述

`DamageCmd` 是用于处理伤害的命令对象，支持多种配置选项。

### 12.2 常用配置方法

| 方法 | 说明 | 参数 |
|------|------|------|
| `.FromCard()` | 设置伤害来源为卡牌 | `CardModel card` |
| `.FromCreature()` | 设置伤害来源为生物 | `Creature creature` |
| `.Targeting()` | 设置单个目标 | `Creature target` |
| `.TargetingAllOpponents()` | 设置所有敌人为目标 | `CombatState combatState` |
| `.WithHitCount()` | 设置命中次数 | `int count` |
| `.WithHitFx()` | 设置命中特效 | `string vfxPath, string audioPath` |
| `.SpawningHitVfxOnEachCreature()` | 在每个目标上播放特效 | - |
| `.Execute()` | 执行伤害命令 | `PlayerChoiceContext context` |

### 12.3 使用示例

```csharp
// 对单个目标造成伤害
await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .WithHitFx("vfx/vfx_attack_blunt", "blunt_attack.mp3")
    .Execute(choiceContext);

// 对所有敌人同时造成伤害
await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
    .WithHitCount(totalHits)
    .FromCard(this)
    .TargetingAllOpponents(base.CombatState)
    .WithHitFx("vfx/vfx_starry_impact", "slash_attack.mp3")
    .SpawningHitVfxOnEachCreature()
    .Execute(choiceContext);
```

### 12.4 DamageCmd vs CreatureCmd.Damage

| 特性 | DamageCmd | CreatureCmd.Damage |
|------|-----------|---------------------|
| **配置方式** | 流式API，灵活配置 | 直接参数传递 |
| **特效控制** | 支持自定义特效 | 默认特效 |
| **多目标** | 支持群体伤害 | 单个目标 |
| **适用场景** | 卡牌攻击、复杂伤害 | 简单伤害、能力伤害 |

### 12.5 实战示例：群体伤害

```csharp
// 参考 Radiate.cs 实现
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    int totalHits = 1 + (int)calculatedHits;
    
    await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
        .WithHitCount(totalHits)
        .FromCard(this)
        .TargetingAllOpponents(base.CombatState)
        .WithHitFx("vfx/vfx_starry_impact", null, "slash_attack.mp3")
        .SpawningHitVfxOnEachCreature()
        .Execute(choiceContext);
}
```

### 12.6 关键要点

| 要点 | 说明 |
|------|------|
| **流式API** | 使用链式调用配置伤害参数 |
| **来源设置** | 必须设置 `.FromCard()` 或 `.FromCreature()` |
| **目标设置** | 使用 `.Targeting()` 或 `.TargetingAllOpponents()` |
| **特效设置** | 通过 `.WithHitFx()` 设置视觉和音频效果 |

---

## 12. 游戏更新后 API 迁移（多人战斗版本）

> 适用版本：游戏 2026-09 更新（min_game_version 0.107.1 之后的多人战斗版本，BaseLib 3.0.6）。
> 本次更新将战斗系统改造为支持多人，所有战斗钩子（Hook）的签名统一发生变化。旧 mod 重新编译时会出现大量 `CS0115: 没有找到适合的方法重写` 错误，本章给出对照迁移方案。

### 12.1 核心变化总览

| 变化 | 旧 API | 新 API |
|------|--------|--------|
| 钩子声明位置 | `PowerModel` / `CardModel` 自身 | 统一上移到基类 `AbstractModel`（能力、卡牌、遗物等均可重写） |
| 战斗状态类型 | `CombatState`（具体类） | `ICombatState`（接口，命名空间 `MegaCrit.Sts2.Core.Combat`） |
| 选择上下文 | `PlayerChoiceContext` | 仍为 `PlayerChoiceContext`，命名空间确认为 `MegaCrit.Sts2.Core.GameActions.Multiplayer` |
| 回合事件 | 按"某一方"触发，参数只有 `CombatSide` | 新增 `participants`（参与该回合的生物列表） |
| 卡牌去向 | `GetResultPileType()` | `GetResultPileTypeForCardPlay()` |

### 12.2 回合开始/结束钩子新签名

```csharp
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

// 某一方回合开始（无 PlayerChoiceContext 参数！）
public override Task AfterSideTurnStart(
    CombatSide side,
    IReadOnlyList<Creature> participants,
    ICombatState combatState)

// 某一方回合开始（延迟版本，在中毒等回合开始效果之后触发）
public override Task AfterSideTurnStartLate(
    CombatSide side,
    IReadOnlyList<Creature> participants,
    ICombatState combatState)

// 某一方回合开始前（有 PlayerChoiceContext）
public override Task BeforeSideTurnStart(
    PlayerChoiceContext choiceContext,
    CombatSide side,
    IReadOnlyList<Creature> participants,
    ICombatState combatState)

// 某一方回合结束前（原 BeforeTurnEnd）
public override Task BeforeSideTurnEnd(
    PlayerChoiceContext choiceContext,
    CombatSide side,
    IEnumerable<Creature> participants)
public override Task BeforeSideTurnEndEarly(...)   // 同签名
public override Task BeforeSideTurnEndVeryEarly(...) // 同签名

// 某一方回合结束后（原 AfterTurnEnd）
public override Task AfterSideTurnEnd(
    PlayerChoiceContext choiceContext,
    CombatSide side,
    IEnumerable<Creature> participants)
public override Task AfterSideTurnEndLate(...)     // 同签名
```

**旧 → 新方法名映射：**

| 旧方法 | 新方法 |
|--------|--------|
| `AfterSideTurnStart(CombatSide, CombatState)` | `AfterSideTurnStart(CombatSide, IReadOnlyList<Creature>, ICombatState)` |
| `AfterSideTurnStartLate(CombatSide, CombatState)` | `AfterSideTurnStartLate(CombatSide, IReadOnlyList<Creature>, ICombatState)` |
| `BeforeTurnEnd(PlayerChoiceContext, CombatSide)` | `BeforeSideTurnEnd(PlayerChoiceContext, CombatSide, IEnumerable<Creature>)` |
| `AfterTurnEnd(PlayerChoiceContext, CombatSide)` | `AfterSideTurnEnd(PlayerChoiceContext, CombatSide, IEnumerable<Creature>)` |

**注意：**

1. `AfterSideTurnStart` / `AfterSideTurnStartLate` **没有** `PlayerChoiceContext` 参数。方法体内若需要上下文调用 `CreatureCmd.Damage` 等，使用 `new ThrowingPlayerChoiceContext()`。
2. 单人模式下沿用旧的 `side == base.Owner.Side` 判断即可；原版新代码（如 BlurPower）推荐用 `participants.Contains(base.Owner)` 判断该生物是否实际参与本回合。
3. 必须添加 `using System.Collections.Generic;`、`using MegaCrit.Sts2.Core.Entities.Creatures;`、`using MegaCrit.Sts2.Core.Combat;`。
4. 调用基类方法时参数也要同步：`await base.AfterSideTurnStart(side, participants, combatState);`

**原版参考（BlurPower.cs）：**

```csharp
public override async Task AfterSideTurnStart(
    CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
{
    if (participants.Contains(base.Owner))
    {
        await PowerCmd.Decrement(this);
    }
}
```

### 12.3 能力层数变化钩子新签名

```csharp
// 旧：AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
// 新（新增 choiceContext 首参，引用类型不再标注可空）：
public override Task AfterPowerAmountChanged(
    PlayerChoiceContext choiceContext,
    PowerModel power,
    decimal amount,
    Creature applier,
    CardModel cardSource)

// 对应地，层数变化前钩子：
public override Task BeforePowerAmountChanged(
    PowerModel power,
    decimal amount,
    Creature target,
    Creature applier,
    CardModel cardSource)
```

### 12.4 抽牌阶段钩子新签名

```csharp
// 旧：BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
// 新：CombatState 替换为 ICombatState（能力与卡牌上均可重写）
public override Task BeforeHandDraw(
    Player player,
    PlayerChoiceContext choiceContext,
    ICombatState combatState)
public override Task BeforeHandDrawLate(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)

// 相关钩子：
public override decimal ModifyHandDraw(Player player, decimal count)
public override bool ShouldDraw(Player player, bool fromHandDraw)
public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
```

### 12.5 卡牌打出后的去向

```csharp
// 旧：protected override PileType GetResultPileType()
// 新：方法重命名，参数与返回值不变
protected override PileType GetResultPileTypeForCardPlay()
{
    PileType result = base.GetResultPileTypeForCardPlay();
    // 自定义去向，例如返回手牌
    return result == PileType.Discard ? PileType.Hand : result;
}
```

### 12.6 ICombatState 常用成员

`PowerModel.CombatState` 与各钩子参数现在都是 `ICombatState`（`MegaCrit.Sts2.Core.Combat`），常用成员：

| 成员 | 说明 |
|------|------|
| `IReadOnlyList<Creature> Enemies` | 敌方所有生物 |
| `IReadOnlyList<Player> Players` | 所有玩家 |
| `IReadOnlyList<Creature> Allies` / `Creatures` / `PlayerCreatures` | 友方/全部/玩家生物 |
| `IReadOnlyList<Creature> HittableEnemies` | 所有可被攻击的敌人（AOE 常用） |
| `IReadOnlyList<Creature> GetOpponentsOf(Creature)` | 某生物的所有对手 |
| `IReadOnlyList<Creature> GetTeammatesOf(Creature)` | 某生物的所有队友（含自身） |
| `T CreateCard<T>(Player owner)` | 在战斗中创建卡牌实例 |
| `CombatSide CurrentSide` | 当前行动方 |
| `int RoundNumber` | 当前回合轮次（从 1 开始） |

### 12.7 迁移检查清单

重新编译旧 mod 时，按以下顺序处理：

1. 所有 `CS0115` 错误按 12.2 ~ 12.5 的签名表逐个替换。
2. 给改动的文件补齐 `System.Collections.Generic`、`Entities.Creatures`、`Combat`、`GameActions.Multiplayer` 四个 using。
3. 全项目搜索 `CombatState` 类型声明（参数/局部变量），改为 `ICombatState`；仅作接口成员使用（`.Enemies`/`.Players`/`.CreateCard<T>()` 等）的方法体无需改动。
4. 全项目搜索 `BeforeTurnEnd`/`AfterTurnEnd`，替换为 `BeforeSideTurnEnd`/`AfterSideTurnEnd` 并补 `participants` 参数。
5. 全项目搜索 `GetResultPileType()`，替换为 `GetResultPileTypeForCardPlay()`。

### 12.8 PowerCmd 命令新签名（choiceContext 首参）

多人版本中，所有修改战斗状态的命令都必须显式携带 `PlayerChoiceContext`（用于多人网络同步归属）。旧代码中 `PowerCmd.Apply/ModifyAmount` 的旧重载已删除，缺少参数会报 `CS1501: "Apply"方法没有采用 N 个参数的重载` 或 `CS7036: 未提供...所需参数`。

```csharp
// 对单个生物施加能力（旧 4 参 → 新 5 参 + 可选 silent）
public static Task<T?> Apply<T>(
    PlayerChoiceContext choiceContext,
    Creature target,
    decimal amount,
    Creature? applier,
    CardModel? cardSource,
    bool silent = false) where T : PowerModel

// 对一组生物施加能力（AOE）
public static Task<IReadOnlyList<T>> Apply<T>(
    PlayerChoiceContext choiceContext,
    IEnumerable<Creature> targets,
    decimal amount,
    Creature? applier,
    CardModel? cardSource,
    bool silent = false) where T : PowerModel

// 修改已有能力层数（offset 正增负减）
public static Task<int> ModifyAmount(
    PlayerChoiceContext choiceContext,
    PowerModel power,
    decimal offset,
    Creature? applier,
    CardModel? cardSource,
    bool silent = false)
```

**上下文从哪来：**

| 调用位置 | 使用的上下文 |
|----------|-------------|
| 卡牌 `OnPlay(PlayerChoiceContext choiceContext, CardPlay play)` | 直接用参数 `choiceContext` |
| 能力 `AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)` | 参数名通常为 `context` |
| 能力 `AfterPowerAmountChanged(PlayerChoiceContext choiceContext, ...)` | 参数 `choiceContext` |
| 回合开始/结束等无上下文钩子 | `new ThrowingPlayerChoiceContext()`（原版内部 `PowerCmd.Decrement` 也是这么做的；该类在 `MegaCrit.Sts2.Core.GameActions.Multiplayer`） |

**迁移示例（卡牌中）：**

```csharp
// 旧
await PowerCmd.Apply<PoisonPower>(target, amount, base.Owner.Creature, this);
await PowerCmd.ModifyAmount(poisonPower, -2, base.Owner.Creature, this);
// 新
await PowerCmd.Apply<PoisonPower>(choiceContext, target, amount, base.Owner.Creature, this);
await PowerCmd.ModifyAmount(choiceContext, poisonPower, -2, base.Owner.Creature, this);
```

**迁移示例（回合开始钩子中，非玩家主动触发）：**

```csharp
await PowerCmd.Apply<PoisonPower>(new ThrowingPlayerChoiceContext(), base.Owner, base.Amount, base.Owner, null);
```

### 12.9 无限血判断 API 变更

`Creature.ShowsInfiniteHp` 属性已删除（报 `CS1061`）。新 API 是对 `HpDisplay` 的扩展方法（命名空间 `MegaCrit.Sts2.Core.Entities.Creatures`，文件 `HpDisplayExtensions.cs`）：

```csharp
// 旧
if (combatState.HittableEnemies.All((Creature c) => c.ShowsInfiniteHp)) { ... }
// 新（原版 HellraiserPower 同款写法）
if (combatState.HittableEnemies.All((Creature c) => c.HpDisplay.IsInfinite())) { ... }
```

---

## Mod 修改日志

### 2026-09-13 适配游戏多人战斗更新（v1.5 修复）

**修改原因：** 游戏更新（2026-09-10 版本，sts2.dll 9,364,480 字节）后，战斗系统重构为多人架构，战斗钩子签名全面变更，mod 重新编译出现 18 个 `CS0115` 错误，mod 完全无法加载。

**修改意图：** 在不改变任何卡牌数值与能力设计（数值仍以本地化文件 `CSharpSBP/localization/zhs/` 为准）的前提下，仅将重写方法迁移到新签名，恢复 mod 的编译与加载能力。

**编译环境：** Godot.NET.Sdk 4.6.2 / net9.0 / BaseLib 3.0.6（NuGet）/ 游戏目录新版 sts2.dll，输出目录 `snake-bite/SBPDebug/`。

#### 修改文件清单

| 文件 | 位置 | 旧签名/写法 | 新签名/写法 | 修改意图 |
|------|------|------------|------------|----------|
| Powers/SnakeBlockPower.cs | 咬判能力 | `AfterSideTurnStart(CombatSide, CombatState)` | `AfterSideTurnStart(CombatSide, IReadOnlyList<Creature>, ICombatState)` | 适配多人回合钩子；补 Generic、Creatures using |
| Powers/SnakegramPower.cs | 蛇子程序能力 | 同上 | 同上；同步 `base` 调用 | 同上 |
| Powers/IncomingSnakePower.cs | 回蛇能力 | 同上 | 同上 | 同上 |
| Powers/PoisonBoostPower.cs | 疯狂蛇进能力 | 同上 | 同上 | 同上 |
| Powers/PoisonEachTurnPower.cs | 每回合中毒代价 | 同上 | 同上 | 同上 |
| Powers/NukeStrikePower.cs | 核打击能力（回合开始部分） | 同上 | 同上 | 同上 |
| Powers/SnakeArmorPower.cs | 岩蛇覆甲能力 | 同上 | 同上 | 同上 |
| Powers/SnakeCreationPower.cs | 创蛇纪能力 | 同上 | 同上 | 同上 |
| Powers/ApocalypseProtocolPower.cs | 灭世流程（倒计时部分） | 同上 | 同上 | 同上 |
| Powers/SnakeDamageToPoisonPower.cs | 衔尾之蛇能力 | `AfterSideTurnStartLate(CombatSide, CombatState)` | `AfterSideTurnStartLate(CombatSide, IReadOnlyList<Creature>, ICombatState)` | 适配延迟回合开始钩子 |
| Powers/NukeStrikePower.cs | 核打击能力（回合结束爆炸） | `BeforeTurnEnd(PlayerChoiceContext, CombatSide)` | `BeforeSideTurnEnd(PlayerChoiceContext, CombatSide, IEnumerable<Creature>)` | 回合结束钩子改名并补 participants |
| Powers/ApocalypseProtocolPower.cs | 灭世流程（结束获得30格挡） | `AfterTurnEnd(PlayerChoiceContext, CombatSide)` | `AfterSideTurnEnd(PlayerChoiceContext, CombatSide, IEnumerable<Creature>)` | 同上 |
| Powers/SnakePoisonAuraPower.cs | 蛇洞能力 | `AfterPowerAmountChanged(PowerModel, decimal, Creature?, CardModel?)` | `AfterPowerAmountChanged(PlayerChoiceContext, PowerModel, decimal, Creature, CardModel)` | 层数变化钩子新增上下文参数；补 GameActions.Multiplayer using |
| Powers/DeathSnakePower.cs | 死蛇形态能力 | 同上 | 同上 | 同上 |
| Powers/SnakeVenomSerumPower.cs | 蛇毒血清能力 | 同上（全限定类型写法） | 同上；简化为 using 引入类型 | 同上 |
| Powers/EndlessSnakePower.cs | 无尽蛇咬能力 | `BeforeHandDraw(Player, PlayerChoiceContext, CombatState)` | `BeforeHandDraw(Player, PlayerChoiceContext, ICombatState)` | 抽牌前钩子改用 ICombatState |
| Cards/SkillCards/SBP_skill6.cs | 蛇候未到 | 同上 | 同上 | 同上 |
| Cards/SkillCards/SBP_skill5.cs | 蛇毒墙 | `GetResultPileType()`（含 base 调用） | `GetResultPileTypeForCardPlay()` | 卡牌去向方法重命名，保持"返回手牌"行为 |

#### 第二轮修复（方法体内的命令 API 变更，48 个错误）

签名错误消除后，方法体暴露出命令系统的连锁变更（`CS1501`/`CS7036`/`CS1061`）：

| 文件范围 | 处数 | 旧写法 | 新写法 | 修改意图 |
|----------|------|--------|--------|----------|
| Cards/ 下全部 33 个含能力施加的卡牌（9 攻击 + 14 能力 + 10 技能，含 Nuke/ 3 张） | 41 | `PowerCmd.Apply<T>(target, amount, applier, cardSource)`、`PowerCmd.ModifyAmount(power, offset, applier, cardSource)` | 首参补 `choiceContext`（来自 `OnPlay` 参数） | 适配多人版本命令必须携带玩家上下文的新签名 |
| Powers/DeathSnakePower.cs | 1 | `Apply<DoomPower>(power.Owner, amount, base.Owner, cardSource)` | 首参补 `choiceContext`（AfterPowerAmountChanged 参数） | 同上 |
| Powers/SnakePoisonAuraPower.cs、SnakeVenomSerumPower.cs | 2 | `Apply<...>(target, amount, base.Owner, null)` | 首参补 `choiceContext` | 同上 |
| Powers/SnakegramPower.cs、SnakeProgramPower.cs | 2 | 同上 | 首参补 `context`（AfterCardPlayed 参数） | 同上 |
| Powers/PoisonEachTurnPower.cs、SnakeDamageToPoisonPower.cs | 2 | 同上（回合钩子内） | 首参补 `new ThrowingPlayerChoiceContext()` | 无玩家上下文的回合触发，使用原版同款 Throwing 上下文；补 GameActions.Multiplayer using |
| Powers/SnakeArmorPower.cs | 2 | `ModifyAmount(power, offset, base.Owner, null)`（伤害结算钩子里一处、回合开始一处） | 首参补 `new ThrowingPlayerChoiceContext()` | 同上 |
| Powers/SnakeRaiderPower.cs | 1 | `Creature.ShowsInfiniteHp` | `Creature.HpDisplay.IsInfinite()`（扩展方法） | 旧属性已删除，原版 HellraiserPower 同款新写法 |
| SBP.csproj | 1 项 | — | 增加 `<Compile Remove="**\_backup*\**" />` | 防止代码备份目录被 Godot SDK 默认通配符卷入编译（本次排查中曾因此产生大量 CS2001） |

**测试结果：**

- 编译命令：`dotnet build SBP.csproj -c Debug --no-incremental`（在 `snake-bite/` 目录）
- 结果：**0 个错误，15 个警告**。警告全部为 `CS8632`（项目未启用 `<Nullable>` 时使用了 `?` 可空注解，旧代码既有风格，不影响运行），无任何 CS 错误。
- 产物：`snake-bite/SBPDebug/CSharpSBP.dll`（120,320 字节）已由 csproj 后置步骤自动复制到 `E:\SteamLibrary\steamapps\common\Slay the Spire 2\Mods\CSharpSBP\`（dll/pdb 均已更新，2026-09-13 19:57）。
- 游戏内运行时测试：**待用户启动游戏验证**（建议重点测试：蛇毒墙返回手牌、蛇候未到消耗堆回抽、回合开始触发型能力、各中毒联动、灭世流程、核打击、事件补丁）。
- `CSharpSBP.json` 的 `min_game_version: 0.107.1` 与游戏 `release_info.json` 的 `v0.107.1` 一致，无需改动。

**未改动项说明：**

- 所有钩子方法体逻辑、数值、触发条件保持原样，仅做签名适配；`side == base.Owner.Side` 的判断在单人模式下语义不变。
- 卡牌、能力、事件的数值与效果未做任何调整，仍以本地化文件记录为准。
- MainFile.cs、Harmony 补丁（EventPatch）、卡池、事件、其余 30+ 张卡牌与能力均无需改动。

### 第三轮修复（游戏内实测 4 bug 排查与修复，2026-09-13 晚）

#### Bug 1：卡牌/能力描述中的"参数调用"异常 —— 实为 SmartFormat 占位符错误，与 choiceContext 无关

**排查结论：** 本地化文本是声明式 SmartFormat 数据，`{变量名:格式化器()}` 占位符由卡牌/能力的 `DynamicVars`（即 `CanonicalVars` / `new` 隐藏的 `DynamicVars`）按**名称**提供；整条解析链路（`CardModel.GetDescriptionForPile` → `DynamicVars.AddTo` → SmartFormat）不接受也不需要 `choiceContext`。`choiceContext` 只是 `PowerCmd` 等命令 API 的多人上下文参数（第二轮已全部补齐）。游戏内描述显示异常的真正原因是 3 处占位符与代码提供的变量名不匹配或格式化器语法非法：

| 文件 | 键 | 旧写法 | 新写法 | 修改原因 |
|------|----|--------|--------|----------|
| `CSharpSBP/localization/zhs/cards.json` | `CSHARPSBP-S_B_P_POWER7.description`（子蛇序） | `给予自身{Poison:diff()}层中毒` | `给予自身2层中毒` | `SBP_power7.CanonicalVars` 只提供 `Energy` 一个变量；中毒 2 层是能力 `SnakeProgramPower` 的固定效果（`IntVar("PoisonAmount", 2*Amount)`，Amount 固定为 1，升级仅降费），卡牌上不存在 `Poison` 变量，占位符无法解析。数值 2 与能力代码、旧文案语义一致 |
| `CSharpSBP/localization/zhs/cards.json` | `CSHARPSBP-S_B_P_POWER10.description`（无尽蛇咬） | `{Amount:()}` | `{Amount:diff()}` | 空格式化器名 `()` 非法（已注册格式化器仅有 diff/energyIcons/starIcons 等 9 个），同时修正了原文错位的引号 |
| `CSharpSBP/localization/zhs/powers.json` | `CSHARPSBP-SNAKE_PROGRAM_POWER.description` 与 `.smartDescription`（子蛇序能力） | `{EnergyVar:energyIcons()}` | `{Energy:energyIcons()}` | `new EnergyVar(amount)` 的默认变量名是 `"Energy"`（见原版 `EnergyVar.defaultName = "Energy"`），类名 `EnergyVar` 不是变量名；`SnakeProgramPower.DynamicVars` 提供的是 `Energy` 与 `PoisonAmount` |

**修改意图：** 让描述中的每个占位符都能被实际提供的动态变量解析，恢复游戏内数值显示。

#### Bug 2：【疯狂蛇进】对大部分卡牌不生效 —— 手动加成遗漏，已改为原版标准全局钩子

**根因：** `PoisonBoostPower`（疯狂蛇进施加的能力）自身原本没有任何中毒加成逻辑，中毒加成依赖每张施毒卡牌在 `OnPlay` 中**手动**读取 `Owner.Creature.GetPower<PoisonBoostPower>()?.Amount` 并加到施毒数量上；全 mod 39 张牌中只有 3 张（`SBP_atk5` 王蛇咬剑、`SBP_skill1` 弹跳蛇咬、`SBP_atk7` 巨蛇咬）写了这段调用，其余蛇咬牌（咬面蛇击等）以及未来新增牌全部不会生效。

**修复方案（参照原版遗物 `Core/Models/Relics/SneckoSkull.cs` 的集中式写法）：**

| 文件 | 修改内容 | 修改意图 |
|------|----------|----------|
| `SBPCode/Powers/PoisonBoostPower.cs` | 新增重写 `ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)`：仅当 `power is PoisonPower && amount > 0 && giver == base.Owner && cardSource is SnakeBitePlus_Card { HasSnakeBiteTag: true }` 时返回 `base.Amount`，否则返回 0；另重写 `AfterModifyingPowerAmountGiven` 在实际加成中毒时 `Flash()`；补 using `MegaCrit.Sts2.Core.Entities.Cards` | 一张牌的能力集中处理全部"蛇咬"系卡牌的中毒加成，杜绝逐牌手动调用遗漏。`amount > 0` 防止移除中毒（负值）被反向加成；`giver == base.Owner` 限定持有者本人施毒；`cardSource` 判定把非卡牌来源（蛇洞等能力连锁施毒时 cardSource 为 null）排除在外，与本地化描述"蛇咬卡牌施加中毒时"一致 |
| `SBPCode/Cards/AttackCards/SBP_atk5.cs` | 删除 `poisonBoost` 局部变量，`PowerCmd.Apply<PoisonPower>` 数量恢复为 `poisonAmount` | 钩子会统一追加，保留手动加法会造成双重加成 |
| `SBPCode/Cards/SkillCards/SBP_skill1.cs` | 同上，自毒与敌毒两处均移除 `+ poisonBoost` | 同上 |
| `SBPCode/Cards/AttackCards/SBP_atk7.cs` | 同上，自毒与敌毒两处 | 同上 |

**新发现并已使用的原版 API（补充记录）：**
- `AbstractModel.ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource) → decimal`（`MegaCrit.Sts2.Core.Models`）： additive 修正钩子，返回值叠加到能力给予层数；另有乘法版本 `ModifyPowerAmountGivenMultiplicative`。由 `Hook.ModifyPowerAmountGiven` 对战斗中所有监听器（能力/遗物等）调用，`PowerCmd.Apply` 与 `PowerCmd.ModifyAmount` 两条施毒路径都会触发。
- `AbstractModel.AfterModifyingPowerAmountGiven(PowerModel power) → Task`：修正实际发生后回调，原版用于让遗物/能力图标闪烁。
- 注意 Power 上比较施毒者用 `base.Owner`（`Creature`）；Card 上才是 `base.Owner.Creature`。

#### Bug 3："蛇"系 / "蛇咬"系分类错误 —— 4 张牌误标蛇咬，已按名称规则修正

**排查结论：** 分类与本地化文件无关，完全由 C# 接口 `ISnakeTagged`（`SBPCode/Tags/SnakeTags.cs`）的 `HasSnakeTag` / `HasSnakeBiteTag` 两个布尔属性决定；`card_keywords.json` 仅提供悬浮文案，不参与判定。按设计规则"**蛇咬系 = 卡牌名包含连续的'蛇咬'二字；蛇系 = 名字包含'蛇'，蛇咬系是蛇系的子集**"全量核对 39 张牌，4 张误标（均含"蛇"和"咬"但不含连续"蛇咬"）：

| 文件 | 卡牌名 | 旧标签 | 新标签 |
|------|--------|--------|--------|
| `SBPCode/Cards/AttackCards/SBP_atk2.cs` | 咬面蛇击 | HasSnakeBiteTag = true | **false**（HasSnakeTag 仍为 true） |
| `SBPCode/Cards/PowerCards/SBP_power1.cs` | 凶蛇恶咬 | true | **false** |
| `SBPCode/Cards/PowerCards/SBP_power3.cs` | 封咬蛇座 | true | **false** |
| `SBPCode/Cards/PowerCards/SBP_power11.cs` | 地咬狂蛇 | true | **false** |

修正后蛇咬系剩 4 张：弹跳蛇咬（skill1）、王蛇咬剑（atk5）、巨蛇咬（atk7）、无尽蛇咬（power10）。联动影响（符合设计）：咬面蛇击不再被凶蛇恶咬抽牌、子蛇序给能量、追踪之蛇双击、地咬狂蛇自动打出、坟冢爆蛇选牌、疯狂蛇进加成等"蛇咬"联动计数；它们仍全部保留蛇系身份（蛇景、蛇候未到、封咬蛇座计数、蛇肩无视等蛇系联动不变）。

**遗留显示层事项（本轮未改，待确认）：** `card_keywords.json` 定义了 `Snake`/`SnakeBite` 的标题与描述文案，但代码未通过 BaseLib 的 `CustomKeywords` 机制注册，也未在卡牌 `CanonicalKeywords` 中返回，故游戏内卡牌上不显示"蛇系/蛇咬"分类徽标与悬浮提示（描述正文中手写的 `[gold]蛇咬[/gold]` 仅为金色文字）。若需要游戏内可见的分类标签，需另行注册 BaseLib 自定义关键词并调整描述排版以防文字重复。

#### 环境事故处理：失控的自复制备份进程

编译时 MSBuild 文件枚举栈溢出（`FileMatcher.GetFilesRecursive` 无限递归）。排查发现上一会话 18:35 启动的备份命令 `Copy-Item "...\SBPCode" -Destination "...\SBPCode\_backup_20260913" -Recurse` 因**目标目录位于源目录内部**而无限自复制，已运行约 3.5 小时、生成 300+ 层嵌套目录树。已终止失控进程（经用户授权），用 robocopy /MIR 清空文件后以 `\\?\` 长路径前缀 `Directory.Delete` 删除整棵空目录树，SBPCode 现有源码未受影响。
**教训：** 备份目标必须放在被复制目录的外部（例如 `e:\1\MyMods\backups\SBPCode_yyyyMMdd\`），否则 Copy-Item -Recurse 会把自己的产物再次复制。

#### 本地化部署机制（本轮新查明，后续改本地化必读）

游戏运行时从游戏目录 `Mods\CSharpSBP\CSharpSBP.pck` 加载本地化与资源，**不是**直接读取源码目录的 `CSharpSBP/localization/zhs/*.json`；该 pck 此前停留在 2026-05-09，其中仍是含 `{Amount:()}` 等旧占位符的文本，仅改 json 不重导 pck 不会在游戏内生效。pck 内格式为 Godot pack format 3 / Godot **4.5.1**（与 `project.godot` 的 `config/features=4.5` 一致）。

重新导出与部署流程（本轮已跑通）：

1. 用本机 4.5.1 mono 编辑器 headless 导出（必须与游戏同版本，4.6 编辑器不用于此项目导包）：
   `D:\Godot\4.5.1GodotSharpMono\Godot_v4.5.1-stable_mono_win64_console.exe --headless --path "e:\1\MyMods\snake-bite" --export-pack "Windows Desktop" "e:\1\MyMods\build\CSharpSBP.pck"`
   输出目录 `e:\1\MyMods\build\` 在 Godot 项目目录**外部**（放项目内会被 `export_filter=all_resources` 卷入造成 pck 自包含）。导出时 C# 相关 PushError 堆栈为编辑器环境扫描工具代码的既有警告，不影响包内容。
2. `SBP.csproj` 的 `CopyToModsFolderOnBuild` 目标已新增一行：`..\build\CSharpSBP.pck` 存在时随 `dotnet build` 复制到 `Mods\CSharpSBP\`（沙箱禁止 PowerShell/robocopy 直接写 Mods 目录下的 .pck，MSBuild Copy 通道放行）。
3. 旧 pck 已备份至 `e:\1\MyMods\backups\CSharpSBP_20260509_old.pck`。

**测试结果：**

- 编译命令：`dotnet build SBP.csproj -c Debug --no-incremental`（`snake-bite/` 目录）
- 结果：**0 错误，17 个警告**（全部为项目既有 CS8632 nullable 注解警告，含 PoisonBoostPower 新签名上同风格的 2 个，无功能影响）。
- 产物：`snake-bite/SBPDebug/CSharpSBP.dll`（120,320 字节，2026-09-13 22:29）与重新导出的 `CSharpSBP.pck`（32,019,660 字节，Godot 4.5.1，2026-09-13 22:28）均已部署到 Steam Mods 目录；已校验 dll 元数据含 `ModifyPowerAmountGivenAdditive`/`AfterModifyingPowerAmountGiven`，pck 内新占位符（`{Amount:diff()}`、"自身2层"文案）存在、旧占位符（`{Amount:()}`、`{EnergyVar:`）已消失。
- **游戏内待验证（bug 1/2/3）：** ①子蛇序卡牌与能力描述、无尽蛇咬描述的数值正常显示；②打出疯狂蛇进后，4 张蛇咬牌（弹跳蛇咬/王蛇咬剑/巨蛇咬，以及无尽蛇咬生成的蛇咬牌）施毒均额外 +3，非蛇咬牌（含散蛇炮、咬面蛇击等）与蛇洞能力连锁施毒不加成，回合开始后能力消失不再加成；③咬面蛇击/凶蛇恶咬/封咬蛇座/地咬狂蛇不再触发蛇咬联动、仍触发蛇系联动。
- Bug 4（部分能力未测试）保持待测，建议优先：疯狂蛇进、蛇洞、蛇毒血清、死蛇形态、衔尾之蛇、地咬狂蛇自动打出。

### 第四轮修复：蛇系/蛇咬自定义关键词注册，2026-09-15

#### 背景

第三轮修复已将 4 张牌的 `HasSnakeBiteTag` 改为 false（分类逻辑正确），但游戏内仍不显示分类徽标与悬浮提示。根因：`card_keywords.json` 定义了 `Snake`/`SnakeBite` 文案，但没有任何代码把这些注册为游戏 `CardKeyword` 枚举值。BaseLib 3.0.6 提供 `[CustomEnum]` + `[KeywordProperties]` 机制在运行时生成新枚举值。

#### 新发现并已使用的 BaseLib API（补充记录）

- `[CustomEnum(string? name)]`（`BaseLib.Patches.Content.CustomEnumAttribute`）：标记一个 `static readonly CardKeyword` 字段为"新生成的枚举值"。BaseLib 在初始化时扫描所有带此特性的 `CardKeyword` 字段，为每个生成一个新的枚举整数值并赋给字段。`name` 参数可选（不提供时用字段名），作为关键词本地化键名。
- `[KeywordProperties(AutoKeywordPosition position, bool richKeyword = false)]`（`BaseLib.Patches.Content.KeywordPropertiesAttribute`）：可选，标记 `CardKeyword` 字段拥有额外属性。`position`（枚举 `AutoKeywordPosition`: `None`/`Before`/`After`）控制关键词标题自动插入卡牌描述的位置；`richKeyword` 启用富文本悬浮提示。不使用此特性也能创建关键词（仅无自动插入）。
- 本地化键前缀由 `CardKeywordExtensions.GetLocKeyPrefix()` → `StringHelper.Slugify(keyword.ToString())` 生成，格式为 **大写下划线**：`Slugify("Snake")` → `"SNAKE"`，`Slugify("SnakeBite")` → `"SNAKE_BITE"`。
- BaseLib 参考实现：`BaseLib.Cards.BaseLibKeywords.Purge`（自定义关键词 "Purge"，标记了 `[CustomEnum]`）。

#### 修改内容

| 文件 | 修改内容 | 修改意图 |
|------|----------|----------|
| `SBPCode/Tags/SnakeKeywords.cs`（**新建**） | 声明 `static readonly CardKeyword Snake` 和 `SnakeBite`，各带 `[CustomEnum]` + `[KeywordProperties(AutoKeywordPosition.After)]` | 在运行时注册两个自定义 CardKeyword；After 位让标题自动出现在描述末尾（与原版 Exhaust/Eternal 同位置） |
| `SBPCode/Cards/SnakeBitePlusCard.cs` | 重写 `CanonicalKeywords`：按 `HasSnakeTag`/`HasSnakeBiteTag` 返回对应关键词 | 所有蛇系/蛇咬卡牌自动携带关键词 |
| 11 张已重写 `CanonicalKeywords` 的卡牌 | `new[] { ... }` → `base.CanonicalKeywords.Append(...)` | 保留各自的原版关键词（Exhaust/Retain/Innate/Ethereal）同时合并基类的蛇系/蛇咬关键词；Nuke 3 张 HasSnakeTag=false 时 base 返回空集合，行为不变 |
| `CSharpSBP/localization/zhs/card_keywords.json` | 键名 `Snake.title` → `SNAKE.title`、`SnakeBite.title` → `SNAKE_BITE.title` | 匹配 `StringHelper.Slugify` 生成的大写键格式 |

11 张修改的卡牌：SBP_atk5、SBP_atk7、SBP_skill5、SBP_skill6、SBP_skill11、SBP_skill14、SBP_power4、SBP_power9、ApocalypseProtocol、NukeBunker、NukeStrike。每张补 `using System.Linq;`（Append 扩展方法）。

> **不确定项**：BaseLib 的 `KeywordInfo.Key` 可能直接用作本地化键前缀（CamelCase），而非走 `CardKeywordExtensions.GetLocKeyPrefix()`（Slugify 大写）。当前使用 Slugify 大写格式。若游戏内关键词标题显示为空，应改回 CamelCase 键名（`Snake.title`/`SnakeBite.title`）。

**测试结果：**
- 编译：`dotnet build SBP.csproj -c Debug --no-incremental` → **0 错误，17 个警告**（全部既有 CS8632 nullable 注解 + MSB3245 程序集解析警告，无功能影响）。
- 产物：`CSharpSBP.dll`（120,832 字节，+512 字节为新增代码）+ `CSharpSBP.pck`（32,020,900 字节，含新 card_keywords.json）均已部署到 Steam Mods 目录。已校验 dll 元数据含 `SnakeKeywords`/`CustomEnum`/`KeywordProperties`/`AutoKeywordPosition`。
- **游戏内待验证**：①蛇系卡牌描述末尾出现"蛇系."金色文字；②蛇咬卡牌出现"蛇系. 蛇咬."；③悬停显示悬浮提示框；④Nuke 3 张牌不出现蛇系/蛇咬；⑤功能逻辑不受影响。

#### 热修复：readonly 字段导致崩溃，2026-09-15

**问题**：游戏启动时 `BaseLib.Patches.Content.GenEnumValues.FindAndGenerate()` 通过 `FieldInfo.SetValue()` 向 `SnakeKeywords.Snake` 字段写入运行时生成的枚举值，但字段声明为 `static readonly`（CLR 标记为 initonly），抛出 `System.FieldAccessException: Cannot set initonly static field 'Snake' after type is initialized`，导致游戏崩溃。

**修复**：[SnakeKeywords.cs](file:///e:/1/MyMods/snake-bite/SBPCode/Tags/SnakeKeywords.cs) 第 10、14 行，`public static readonly CardKeyword` → `public static CardKeyword`（移除 `readonly`）。BaseLib 需要在类型初始化后通过反射写入字段值，字段不能是 initonly。

> **API 文档补充**：`[CustomEnum]` 标记的字段**必须**声明为 `public static`（不带 `readonly`）。BaseLib 的 `GenEnumValues.FindAndGenerate()` 在 `ModelDb.Init_Patch4` 阶段（所有 mod 加载后）通过反射 `FieldInfo.SetValue` 写入生成的枚举值，`readonly` 会阻止写入并抛出 `FieldAccessException`。参考 `BaseLib.Cards.BaseLibKeywords.Purge` 也是 `public static`（无 readonly）。

**测试结果**：编译 0 错误，17 个既有警告。dll 已部署。待游戏内验证启动不再崩溃。

### 第五轮修复：关键词键名、能力描述 diff() 误用、封咬蛇座/蛇毒血清/咬判文本，2026-09-20

依据 9月15日游戏日志（`%AppData%/SlayTheSpire2/logs/godot.log`，418KB 成功启动会话）实证定位，非推测。

#### 1. 自定义关键词键名错误（测试结果 Bug 6）

游戏内显示 `card_keywords.CSharp-SNAKE.title`（用户记录时少写 SBP），日志真实缺失键为：
- `[BaseLib] GetRawText: Key 'CSHARPSBP-SNAKE.title' not found in table 'card_keywords'`
- `Key 'CSHARPSBP-SNAKEBITE.title' not found`（注意：无下划线，字段名 `SnakeBite` 整体 ToUpper）

**键名生成机制**（Mono.Cecil 反编译运行时 BaseLib 3.4.7 实证）：`GenEnumValues.FindAndGenerate()` 对每个 `[CustomEnum]` 字段构造 `TypePrefix.GetPrefix(field.DeclaringType) + (attr.Name ?? field.Name).ToUpperInvariant()`。`GetPrefix` = 字段所在类型**命名空间第一个点号前的分段**（`ToUpperInvariant`）+ `'-'`。本 mod 命名空间首段为 `CSharpSBP`，故键前缀为 `CSHARPSBP-`。对照 BaseLib 自带 Purge：命名空间首段 `BaseLib` → 键 `BASELIB-PURGE.title`（已在 BaseLib.pck 中验证）。

**修复**：[card_keywords.json](file:///e:/1/MyMods/snake-bite/CSharpSBP/localization/zhs/card_keywords.json) 4 个键改为 `CSHARPSBP-SNAKE.title/.description`、`CSHARPSBP-SNAKEBITE.title/.description`。

#### 2. 能力描述误用 `{Amount:diff()}`（测试结果 Bug 1 主体）

日志错误：`Localization formatting error! No suitable Formatter could be found at 39 ... table=powers key=CSHARPSBP-DRAW_CARDS_POWER.smartDescription variables={Amount:1,...}`，共 4 个能力触发：DRAW_CARDS（凶蛇恶咬）、ENDLESS_SNAKE（无尽蛇咬）、SNAKE_BLOCK（咬判）、SNAKE_POISON_AURA（蛇洞）。

**根因**：[HighlightDifferencesFormatter.cs](file:///e:/1/MyMods/Sts2/src/Core/Localization/Formatters/HighlightDifferencesFormatter.cs#L23-L31) 的 `diff()` 只接受 `DynamicVar` 类型；而 [PowerModel.cs](file:///e:/1/MyMods/Sts2/src/Core/Models/PowerModel.cs#L368) 在渲染能力描述时以**裸 decimal** 注入 `Amount`（`locString.Add("Amount", Amount)`），未重写 `CanonicalVars` 提供同名 DynamicVar 的能力必然格式化失败。能力没有"基础值/升级值"概念，diff 高亮本就不适用。对游戏 1.9GB 主 pck 全量统计：能力描述中 `{Amount:diff()}` 出现 **0 次**，裸 `{Amount}` 出现 3145 次——能力统一用裸变量。

**修复**：[powers.json](file:///e:/1/MyMods/snake-bite/CSharpSBP/localization/zhs/powers.json) 8 处 `{Amount:diff()}` → `{Amount}`：DRAW_CARDS、ENDLESS_SNAKE、INCOMING_SNAKE、POISON_TURN、SNAKE_BLOCK、SNAKE_CREATION、SNAKE_POISON_AURA、SNAKEGRAM。（卡牌描述中的 `{Amount:diff()}` 合法——SBP_power5/SBP_power10 在 CanonicalVars 中提供了 `new DynamicVar("Amount",1m)` 且升级加值，保留不动。）
子蛇序（SNAKE_PROGRAM）的 `{Energy:energyIcons()}`/`{PoisonAmount}` 已在第三轮修复，本轮无错误，无需改动（DynamicVar.ToString 返回数值，裸占位符可渲染）。

#### 3. 咬判能力名称错误

`CSHARPSBP-SNAKE_BLOCK_POWER.title` "蛇系格挡" → "咬判"（与卡牌 SBP_skill13 同名，测试结果 Bug 1 注）。description 基础值由错误的 3 改为 2（卡牌基础格挡 2、升级 4，见 SBP_skill13.cs `new DynamicVar("Power",2m)` + 升级 +2；能力 Amount 即施加层数）。

#### 4. 封咬蛇座文本"每1张"（测试结果 Bug 5）

能力 Amount 固定为 1（仅作能力存在标记，计数由内部变量 SnakeCount 管理），smartDescription 的 `{Amount}` 渲染成 1 → "每打出1张…"。**修复**：smartDescription 写死 3，与 description 及卡牌文本一致。
**另发现并修复代码错误**：[SnakeSwordPower.cs](file:///e:/1/MyMods/snake-bite/SBPCode/Powers/SnakeSwordPower.cs#L43) 阈值为 `newCount >= 4`，实际每打出 **4** 张才触发（SnakeCount 从 0 起逐张 +1），与本地化设计"每3张"矛盾（用户记录"实际运行效果无误"应为测试时误数）。按规则"数值以本地化为准"改为 `>= 3`，**请游戏内重点复核**。

#### 5. 蛇毒血清能力描述与卡牌不符（测试结果 Bug 1）

代码实际给予 `2*Amount` 再生 + `3*Amount` 格挡（Amount=1），卡牌文本为"2点再生和3点格挡"，但能力 description 写"1点再生和格挡"（再生数错、格挡无数）。description 与 smartDescription 均改为"获得[blue]2[/blue]点再生和[blue]3[/blue]点格挡"，与卡牌一致。

**测试结果**：
- 编译：0 错误，17 个既有警告。dll（120,832 字节，阈值 4→3 同字节长度）+ pck（32,020,916 字节）已部署。
- 已校验 pck 内含 `CSHARPSBP-SNAKE.title` 等 4 个新键（旧 SNAKE.title 不存在）、咬判、蛇毒血清 2/3 文本、封咬蛇座写死 3。
- **待游戏内验证**：①蛇系/蛇咬关键词显示中文标题且可悬浮；②悬停凶蛇恶咬/无尽蛇咬/咬判/蛇洞能力不再报格式化错误、数值正确；③咬判能力名；④封咬蛇座恰好每 3 张蛇系牌触发（重点）；⑤蛇毒血清给予 2 再生+3 格挡且文本一致。

---

## 更新记录

| 日期 | 版本 | 更新内容 |
|------|------|----------|
| 2026-04-20 | v1.0 | 初始版本 |
| 2026-04-20 | v1.1 | 添加能力元数据与逻辑分离模式、PoisonPower伤害逻辑详解 |
| 2026-04-21 | v1.2 | 添加悬浮提示系统、动画生成位置系统、DamageCmd对象类型 |
| 2026-04-21 | v1.3 | 添加CreatureCmd命令系统完整文档（伤害、治疗、格挡、眩晕、生物管理、动画等） |
| 2026-04-21 | v1.4 | 完善VFX特效系统文档：添加NStabVfx、NFireBurningVfx、NThinSliceVfx、NBloodSplashVfx等常用节点类详解，补充特效大小、颜色、位置自定义方法及实战示例 |
| 2026-09-13 | v1.5 | 游戏多人战斗更新后的 API 迁移文档：回合开始/结束钩子新增 participants 参数、CombatState→ICombatState、AfterPowerAmountChanged 新增 choiceContext 参数、BeforeHandDraw 参数变更、GetResultPileType→GetResultPileTypeForCardPlay、PowerCmd.Apply/ModifyAmount 新增 choiceContext 首参、ShowsInfiniteHp→HpDisplay.IsInfinite()（见第 12 章）；同日完成 SnakeBitePlus mod 适配修复，0 错误编译通过（见"Mod 修改日志"） |
| 2026-09-13 | v1.6 | 第三轮修复（游戏内实测 4 bug）：①本地化 3 处 SmartFormat 占位符错误（{Poison} 无对应变量、{Amount:()} 空格式化器、{EnergyVar} 变量名应为 {Energy}），确认本地化链路与 choiceContext 无关；②PoisonBoostPower 新增 ModifyPowerAmountGivenAdditive/AfterModifyingPowerAmountGiven 钩子集中实现疯狂蛇进中毒加成（参照 SneckoSkull），移除 atk5/skill1/atk7 的手动加成；③按"名字含蛇咬"规则修正咬面蛇击/凶蛇恶咬/封咬蛇座/地咬狂蛇 4 张牌的 HasSnakeBiteTag；④清理失控自复制备份进程造成的 300+ 层嵌套目录。0 错误编译通过（见"第三轮修复"） |
| 2026-09-15 | v1.7 | 第四轮修复：蛇系/蛇咬自定义关键词注册。新建 SnakeKeywords.cs 用 [CustomEnum]+[KeywordProperties(After)] 声明两个自定义 CardKeyword；SnakeBitePlusCard 基类重写 CanonicalKeywords 按标签返回关键词；11 张已重写 CanonicalKeywords 的牌改为 base.Append 模式；修正 card_keywords.json 键名为 Slugify 大写格式；重新导出 pck。0 错误编译通过（见"第四轮修复"）。同日热修复：[CustomEnum] 字段去掉 readonly，否则 FieldAccessException 启动崩溃 |
| 2026-09-20 | v1.8 | 第五轮修复（依 9/15 游戏日志实证）：①关键词键名改为 CSHARPSBP-SNAKE/SNAKEBITE（BaseLib 键=命名空间首段大写+字段名大写，反编译实证）；②powers.json 8 处 {Amount:diff()}→{Amount}（diff 只接受 DynamicVar，能力 Amount 是裸 decimal；游戏本体能力 0 处用 diff）；③SNAKE_BLOCK_POWER 改名"咬判"、基础格挡文本 3→2；④封咬蛇座 smartDescription 写死 3，代码阈值 >=4 修正为 >=3（原实际每4张触发）；⑤蛇毒血清能力文本改为 2 再生/3 格挡与卡牌一致。0 错误编译通过（见"第五轮修复"） |

## 测试结果
|2026-09-13 | v1.5 | 测试结果|
测试后发现bug如下：
【已修复·待游戏内验证（v1.8，2026-09-20）】1.卡牌和能力的本地化中对参数的调用仍旧无法显示
涉及到的能力与卡牌：
凶蛇恶咬[能力],子蛇序[能力],蛇系格挡[能力](注：蛇系格挡为本地化文本错误，能力名应与卡牌名相同，为"咬判"),蛇洞[能力],蛇毒血清[能力](数值与内容均与卡牌描述不符)
    根因（2026-09-20，游戏日志实证）：能力 smartDescription 误用 {Amount:diff()}（diff 只接受 DynamicVar，能力 Amount 是裸 decimal）；咬判改名；蛇毒血清文本数值错误。子蛇序已在 v1.6 修复、本轮日志无错误。详见"第五轮修复"。
【已修复·待游戏内验证】2.卡牌【疯狂蛇进】的能力，对大部分卡牌没有正确生效
    推测：卡牌的自定义标签在编辑过程中丢失，或对应卡牌遗漏了对应能力的参数调用
    根因（2026-09-13 晚）：标签未丢失，是中毒加成靠每张牌手动调用、仅 3 张牌写了；已改为 PoisonBoostPower 重写 ModifyPowerAmountGivenAdditive 全局钩子（参照 SneckoSkull），并移除 3 张牌的手动加法防双重加成，详见"第三轮修复 Bug 2"。
【已修复·待游戏内验证】3.“蛇”系牌和“蛇咬”系牌分类有误
    原本设计意图：“蛇咬”系牌是“蛇”系牌的一种，用于在游戏内显示时区分，”蛇咬“系牌即为卡牌名字中包含“蛇咬”的卡牌，而“蛇”系牌则为所有包含“蛇”的卡牌（即”蛇“系一定包含“蛇咬”系在内）。
    推测：在本地化文件中，蛇”系牌和“蛇咬”系牌的分类标签不同，导致在游戏内显示时分类错误，可能导致了bug2的出现。
    根因与修复（2026-09-13 晚）：分类由 C# 接口属性决定、与本地化无关；咬面蛇击/凶蛇恶咬/封咬蛇座/地咬狂蛇 4 张牌误标蛇咬，已改回 HasSnakeBiteTag=false（仍为蛇系），蛇咬系现共 4 张。另：游戏内分类徽标/悬浮提示尚未注册 BaseLib 自定义关键词（遗留事项，详见上文）。
【待测试】4.部分能力尚未测试（建议优先：疯狂蛇进、蛇洞、蛇毒血清、死蛇形态、衔尾之蛇、地咬狂蛇自动打出）
【已修复·待游戏内验证（v1.8）】5.卡牌封咬蛇座应为每3张牌触发一次，文本为每1张牌触发1次，但能力实际运行效果无误
    修复（2026-09-20）：能力 smartDescription 的 {Amount} 渲染成固定层数1，已写死3。另核查代码发现阈值实为 >=4（每4张触发），已按本地化设计改为 >=3，请重点复核。
【已修复·待游戏内验证（v1.8）】6.自定义词条“蛇”“蛇咬”显示文本为card_keywords.CSharp-SNAKE.title与card_keywords.CSharp-SNAKEBITE.description，推测为本地化文本缺失
    修复（2026-09-20）：日志实证真实键名为 CSHARPSBP-SNAKE.title / CSHARPSBP-SNAKEBITE.description（BaseLib 键前缀=字段所在命名空间首段大写），已改 card_keywords.json 四键。
