# 战斗 / 药水 / 快捷栏 / 血条 — 完整搭建指南

## 一、涉及文件清单

### 主角侧
| 文件 | 路径 | 作用 |
|---|---|---|
| `PlayerHealth.cs` | `Assets/CS/` | 主角血量（扣血 / 回复 / HealthChanged 事件） |
| `PlayerHealthBar.cs` | `Assets/CS/` | 血条 HUD（锚点宽度 + 绿黄红变色） |
| `PlayerMana.cs` | `Assets/CS/` | 主角魔力（恢复 / ManaChanged 事件） |
| `PlayerManaBar.cs` | `Assets/CS/` | 蓝条 HUD（锚点宽度 + 蓝色渐变） |
| `PlayerAttack.cs` | `Assets/CS/` | 主角攻击（球形检测 Tag="Enemy"，延迟伤害） |
| `AttackBoostIndicator.cs` | `Assets/CS/` | 攻击提升提示（显示 "ATK +5"） |
| `PlayerPotionEffectController.cs` | `Assets/CS/Combat/` | 药水→属性桥接（监听 OnPotionUsed） |

### 快捷栏
| 文件 | 路径 | 作用 |
|---|---|---|
| `QuickSlotBar.cs` | `Assets/CS/` | 快捷道具栏（Z/X/C 键快速使用药水，自动构建 UI） |

### 敌人侧
| 文件 | 路径 | 作用 |
|---|---|---|
| `EnemyHealth.cs` | `Assets/CS/` | 正式敌人血量（扣血 / 死亡 / HealthChanged 事件） |
| `EnemyHealthBar.cs` | `Assets/CS/Combat/` | 敌人血条（世界空间 Canvas，自动构建，始终面朝摄像机） |
| `EnemyDummy.cs` | `Assets/CS/Shop/Test/` | 测试敌人（血量 + 金币掉落，同步 EnemyHealth） |
| `EnemyDummyHealthBar.cs` | `Assets/CS/Shop/Test/` | 测试敌人血条（锚点宽度，自动构建 Canvas） |

### 背包/商店
| 文件 | 路径 | 作用 |
|---|---|---|
| `InventoryManager.cs` | `Assets/CS/Inventory/` | 背包核心，`OnPotionUsed` 事件 |
| `InventoryItemData.cs` | `Assets/CS/Inventory/` | 药水定义（PotionEffectType + effectValue） |
| `CurrencyManager.cs` | `Assets/CS/Shop/` | 金币管理（初始 + 掉落 + 消费） |

---

## 二、数据流

```
[快捷栏 Z/X/C 或 背包使用按钮]
    │
    ▼
InventoryManager.UsePotion(itemId)
    ├─ RemoveItem(itemId, 1)
    └─ OnPotionUsed(itemId, effectType, effectValue)
         │
         ▼
PlayerPotionEffectController.HandlePotionUsed()
    │
    ├─ HealthRestore  → PlayerHealth.Heal(value)
    │     └─ HealthChanged → PlayerHealthBar 更新（宽度+颜色）
    │
    ├─ ManaRestore    → PlayerMana.RestoreMana(value)
    │     └─ ManaChanged   → PlayerManaBar 更新（宽度+颜色）
    │
    └─ AttackBoost    → PlayerAttack.ApplyTemporaryAttackBoost(value, 30s)
          └─ AttackBoostIndicator 每帧检测 → 显示 "ATK +N"


[鼠标左键攻击]
    │
    ▼
PlayerAttack.TryAttack()
    ├─ 球形检测（OverlapSphere），Tag="Enemy"
    └─ EnemyHealth.TakeDamage(damage)
         │
         ├─ 血量减少 → HealthChanged → EnemyHealthBar / EnemyDummyHealthBar 更新
         ├─ HP ≤ 0 → Died 事件 → 血条隐藏
         │
         ▼
    EnemyDummy 监听 EnemyHealth.Died
         └─ Died → CurrencyManager.AddGold(goldDrop)
```

---

## 三、Unity 设置步骤

### A. Player 组件

```
选中 Player（Cube，Tag=Player）
  → 添加组件 → PlayerHealth         （最大血量: 100）
  → 添加组件 → PlayerMana           （最大魔力: 100, 当前魔力: 设50便于测试药水）
  → 添加组件 → PlayerAttack         （伤害: 20）
  → 添加组件 → PlayerPotionEffectController
```

### B. HUD — 血条 + 蓝条

```
确保 Hierarchy 有 EventSystem（没有则 右键 → UI → EventSystem）

右键 → UI → Canvas → 重命名 "HUD"
```

**血条（HP）：**

```
右键 HUD → UI → Image → 重命名 "HP_BG"
  → 锚点 左上, 位置(10, -10), 宽240, 高22
  → 颜色: 黑色, Alpha 120

右键 HP_BG → UI → Image → 重命名 "HP_Fill"
  → 锚点 拉伸, 左1 右1 上1 下1, 中心点 X:0
  → 颜色: 绿色

右键 HP_BG → UI → Text → 重命名 "HP_Text"
  → 锚点 拉伸全铺
  → 文字: "100 / 100", 字号14, 白色, 居中

选中 HUD → 添加组件 → PlayerHealthBar
  → 玩家血量: 拖入 Player
  → 填充图片: HP_Fill
  → 血量文字: HP_Text
```

**蓝条（MP）— 放在血条下方：**

```
右键 HUD → UI → Image → 重命名 "MP_BG"
  → 锚点 左上, 位置(10, -36), 宽240, 高17
  → 颜色: 黑色, Alpha 120

右键 MP_BG → UI → Image → 重命名 "MP_Fill"
  → 锚点 拉伸, 左1 右1 上1 下1, 中心点 X:0
  → 颜色: 蓝色

右键 MP_BG → UI → Text → 重命名 "MP_Text"
  → 锚点 拉伸全铺
  → 文字: "50 / 100", 字号12, 白色, 居中

选中 HUD → 添加组件 → PlayerManaBar
  → 玩家魔力: 拖入 Player
  → 填充图片: MP_Fill
  → 魔力文字: MP_Text
```

**攻击提升指示器（蓝条下方）：**

```
右键 HUD → Create Empty → 重命名 "ATK_Boost"
  → 锚点 左上, 位置(10, -56), 宽240, 高18

右键 ATK_Boost → UI → Text → 重命名 "ATK_Text"
  → 锚点 拉伸全铺
  → 文字: (留空), 颜色: 橙色, 字号14, 左对齐

选中 HUD → 添加组件 → AttackBoostIndicator
  → 玩家攻击: 拖入 Player
  → 指示器根物体: ATK_Boost
  → 加成文字: ATK_Text
```

### C. 快捷道具栏（Z/X/C 键使用药水）

```
右键 HUD → UI → Panel → 重命名 "QuickSlots"
  → 锚点 底部居中, 位置(0, 80)

选中 QuickSlots → 添加组件 → QuickSlotBar
  → 槽位边框: GUI_Parts/Gui_parts/Mini_frame0
  → 治疗药水图标: GUI_Parts/Icons/Healing Potion
  → 魔力药水图标: GUI_Parts/Icons/mana potion
  → 力量药水图标: GUI_Parts/Icons/Attack Potion
```

> 三个槽位、图标、数量文字、Z/X/C 快捷键提示全部由代码自动生成，无需手动创建子物体。

### D. 敌人 + 血条

```
创建 Enemy Cube → 位置(3, 0.5, 3) → Tag 设为 Enemy

  → 添加组件 → EnemyDummy
     最大血量: 100, 掉落金币: 10
  → 添加组件 → EnemyHealth
     （EnemyDummy 启动时自动同步 maxHealth）
  → 添加组件 → EnemyDummyHealthBar
     敌人: 拖入自身, 偏移: (0, 1.6, 0)

地面: 3D Object → Plane → 位置(0,0,0) → 缩放(5,1,5)
```

> 血条由代码自动构建 Canvas + 背景 + 填充条，无需手动创建。

---

## 四、操作方式汇总

| 操作 | 按键 | 说明 |
|---|---|---|
| 移动 | WASD | PlayerController 控制 |
| 攻击 | 鼠标左键 | PlayerAttack 球形检测，需靠近敌人 |
| 快捷用药(HP) | Z | 使用治理药水 |
| 快捷用药(MP) | X | 使用魔力药水 |
| 快捷用药(ATK) | C | 使用力量药水 |
| 打开背包 | B | 背包面板，可手动使用药水 |
| 商店互动 | E | 靠近 ShopNpc 后按 E |

---

## 五、HUD 最终布局

```
┌───────────────┐
│ HP 绿色 100/100│   ← PlayerHealthBar
│ MP 蓝色  50/100│   ← PlayerManaBar
│ ATK +5 (橙色)  │   ← AttackBoostIndicator (仅 boost 时可见)
└───────────────┘

┌─────┬─────┬─────┐
│ [Z] │ [X] │ [C] │   ← QuickSlotBar (底部居中)
│ 图标 │ 图标 │ 图标 │
│  x3  │  x2  │  x1  │
└─────┴─────┴─────┘
```

---

## 六、验证清单

| 测试项 | 操作 | 预期 |
|---|---|---|
| 血条显示 | Play → 看左上角 | 绿色条 "100 / 100" |
| 蓝条显示 | Play → 看血条下方 | 蓝色条 "50 / 100" |
| 快捷栏 | Play → 看底部 | 三个槽位，Z/X/C 图标 + "x0" |
| 快捷栏-加物品 | 背包 +1 治理药水 | Z 槽显示 "x1"，图标亮起 |
| 快捷栏-使用HP | 按 Z | 治理药水 -1，HP +30 |
| 快捷栏-使用MP | 按 X | 魔力药水 -1，MP +20 |
| 快捷栏-使用ATK | 按 C | 力量药水 -1，显示 "ATK +5" |
| 攻击敌人 | 靠近敌人按左键 | 敌人头顶血条缩短 |
| 敌人死亡 | 打到 HP=0 | 敌人消失，金币 +10，Console 输出日志 |
| 商店购买 | 靠近 ShopNpc 按 E | 商店弹出，可买药水 |


## 七、组件依赖关系

```
Player (Cube, Tag=Player) 上：
  PlayerController             WASD 移动
  PlayerHealth                 血量
  PlayerMana                   魔力
  PlayerAttack                 攻击
  PlayerPotionEffectController 药水→属性桥接

Enemy (Cube, Tag=Enemy) 上：
  EnemyDummy                   血量+金币掉落
  EnemyHealth                  正式血量（EnemyDummy 自动同步）
  EnemyDummyHealthBar          头顶血条

HUD (Canvas) 上：
  PlayerHealthBar              血条显示
  PlayerManaBar                蓝条显示
  AttackBoostIndicator         攻击提升显示
  QuickSlotBar                 快捷道具栏

自动创建（无需手动挂载）：
  InventoryManager / CurrencyManager / WeaponManager
```
