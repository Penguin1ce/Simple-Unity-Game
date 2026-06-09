# 主线任务系统协作接口清单

本文档用于地图、战斗、UI、背包同学交付资源前对齐接口。当前先不做正式场景接线；等对应资源完成后，任务系统按本文档接入。

## 当前任务系统边界

任务系统已有代码位于 `Assets/CS/Quest/`：

- `MainQuestManager`：主线任务状态管理器，会在场景加载后自动创建单例。
- `IMainQuestService`：任务系统对外接口。
- `EnemyType`：任务击杀目标类型，目前为 `Slime`、`Elite`、`Boss`。
- `QuestEnemyMarker`：挂在敌人对象上，用于标记该敌人属于哪种任务目标。
- `QuestNpcInteraction`：挂在任务 NPC 上，玩家靠近后按交互键接任务。
- `QuestObjectiveUI`：订阅任务状态变化并刷新任务文本。
- `QuestDebugTester`：临时调试入口，可手动推进任务阶段。

主线阶段约定：

| 阶段 | 常量 | 玩家目标 |
| --- | --- | --- |
| 0 | `NotAcceptedStage` | 与村口 NPC 对话 |
| 1 | `KillSlimeStage` | 击败史莱姆 |
| 2 | `KillEliteStage` | 击败精英怪 |
| 3 | `KillBossStage` | 击败 Boss |
| 4 | `CompletedStage` | 主线完成 |

## 公共接口

其他模块优先通过 `IMainQuestService` 对接，不直接改 `MainQuestManager` 内部状态。

```csharp
public interface IMainQuestService
{
    int QuestStage { get; }
    bool IsQuestCompleted { get; }

    event Action<int, string> QuestStageChanged;

    void AcceptMainQuest();
    void OnEnemyKilled(EnemyType enemyType);
    string GetCurrentQuestText();
    string GetNpcDialogueText();
}
```

获取当前服务：

```csharp
IMainQuestService questService = MainQuestManager.Instance;
```

约定：

- 接任务只调用 `AcceptMainQuest()`。
- 敌人死亡只调用 `OnEnemyKilled(enemyType)`。
- UI 文案只读 `GetCurrentQuestText()` 或订阅 `QuestStageChanged`。
- 其他模块不要调用 `SetQuestStageForDebug()`，该方法只给调试使用。

## 地图同学接口

地图侧需要提供任务系统可挂载的场景锚点和对象位置。

需要交付：

| 资源/对象 | 要求 |
| --- | --- |
| 主线起始 NPC 点位 | 场景中明确一个任务 NPC 位置，建议命名 `QuestNpc_Main_Start` |
| Slime 刷怪点 | 一个或多个史莱姆区域点位，建议命名 `Spawn_Quest_Slime_*` |
| Elite 刷怪点 | 一个或多个精英怪点位，建议命名 `Spawn_Quest_Elite_*` |
| Boss 刷怪点 | Boss 战斗点位，建议命名 `Spawn_Quest_Boss` |
| 导航/碰撞 | 玩家和敌人能正常到达任务目标区域，NavMesh 或碰撞需可用 |
| 任务路径提示点 | 如地图侧有路标/传送门/区域触发器，命名需稳定 |

任务系统接入方式：

- 在任务 NPC 对象上挂 `QuestNpcInteraction`。
- 在对应敌人实例或 prefab 上挂 `QuestEnemyMarker`。
- 如果地图使用动态刷怪，刷怪器需要在实例化敌人后设置对应 `EnemyType`，或直接使用已配置好的 prefab。

验收项：

- 玩家能从出生点走到任务 NPC。
- 玩家能到达 Slime、Elite、Boss 区域。
- 敌人追击和攻击路径不被地图阻挡。
- 场景对象命名稳定，后续替换资源不改变接口对象名。

## 战斗同学接口

战斗侧需要在敌人死亡时把击杀类型通知任务系统。

当前任务系统已在 `EnemyHealth.Die()` 中读取同对象上的 `QuestEnemyMarker`：

```csharp
QuestEnemyMarker marker = GetComponent<QuestEnemyMarker>();
MainQuestManager.Instance.OnEnemyKilled(marker.EnemyType);
```

需要交付：

| 敌人类型 | 任务枚举 | 要求 |
| --- | --- | --- |
| 史莱姆 | `EnemyType.Slime` | 死亡时调用 `EnemyHealth.Die()` 或等价死亡流程 |
| 精英怪 | `EnemyType.Elite` | 死亡对象上必须能提供 `QuestEnemyMarker` |
| Boss | `EnemyType.Boss` | Boss 死亡只通知一次，避免重复推进 |

战斗侧约定：

- 敌人对象需要 Tag 为 `Enemy`，保证玩家攻击检测能命中。
- 敌人死亡流程只触发一次，避免多次调用 `OnEnemyKilled()`。
- 如果战斗侧替换 `EnemyHealth`，需保留死亡事件或提供等价回调：

```csharp
MainQuestManager.Instance?.OnEnemyKilled(enemyType);
```

可选增强：

- 提供 `OnEnemyDied(EnemyType enemyType)` 事件，让任务系统订阅，后续减少任务逻辑对 `EnemyHealth` 的直接依赖。
- 如果敌人 prefab 由战斗同学维护，建议在 prefab 上直接配置 `QuestEnemyMarker`。

验收项：

- 未接任务时击杀敌人不错误完成后续阶段。
- 接任务后击杀 Slime 进入 Elite 阶段。
- 击杀 Elite 后进入 Boss 阶段。
- 击杀 Boss 后任务完成。
- 同一敌人死亡不会重复推进多个阶段。

## UI 同学接口

UI 侧需要显示当前任务目标，并在任务阶段变化时刷新。

当前可用接口：

```csharp
questService.QuestStageChanged += HandleQuestStageChanged;
string text = questService.GetCurrentQuestText();
```

需要交付：

| UI 元素 | 要求 |
| --- | --- |
| 任务目标文本 | 支持普通 `Text` 或 TextMeshPro `TMP_Text` |
| NPC 对话显示 | 可先读取 `GetNpcDialogueText()`，后续可替换为正式对话系统 |
| 任务完成提示 | 阶段进入 `CompletedStage` 后显示完成反馈 |
| 调试按钮 | 可选，仅开发期保留，调用 `QuestDebugTester` |

任务 UI 接入方式：

- 最小接法：在任务文本对象上挂 `QuestObjectiveUI`，绑定 `questText` 或 `questTmpText`。
- 正式接法：UI 管理器订阅 `IMainQuestService.QuestStageChanged`，自行控制面板、动效和提示。

UI 文案来源：

- 当前目标：`GetCurrentQuestText()`
- NPC 对话：`GetNpcDialogueText()`
- 阶段数值：`QuestStage`
- 是否完成：`IsQuestCompleted`

验收项：

- 进入场景后默认显示当前任务文本。
- 接任务、击杀目标、完成任务时文本能自动刷新。
- UI 不直接修改任务阶段。
- UI 对 `MainQuestManager.Instance == null` 有空值保护。

## 背包同学接口

当前任务系统还没有正式奖励发放逻辑。背包侧先预留奖励接口，等背包系统交付后接入任务完成奖励。

建议背包侧提供：

```csharp
public interface IInventoryService
{
    bool CanAddItem(string itemId, int count);
    bool AddItem(string itemId, int count);
}
```

任务系统期望调用点：

| 任务阶段 | 奖励触发 |
| --- | --- |
| 接任务时 | 默认不发奖励 |
| 击败 Slime | 可选小奖励，当前先不接 |
| 击败 Elite | 可选中间奖励，当前先不接 |
| 击败 Boss / 主线完成 | 发放主线完成奖励 |

背包侧需要交付：

| 内容 | 要求 |
| --- | --- |
| 物品 ID 表 | 奖励物品使用稳定字符串 ID，例如 `quest_main_reward_01` |
| 添加物品接口 | 返回是否添加成功，背包满时要可判断 |
| 奖励配置 | 奖励 ID、数量、图标、名称、描述 |
| 失败处理方案 | 背包满时是邮件补发、掉落地面、还是延迟领取 |

任务系统后续接入建议：

- 不把背包代码写进 `MainQuestManager` 主逻辑。
- 新增一个奖励适配器，例如 `QuestRewardAdapter`，订阅 `QuestStageChanged`。
- 当阶段变为 `CompletedStage` 时，调用背包接口发奖励。

验收项：

- 主线完成只发一次奖励。
- 背包满时不会吞奖励。
- 奖励 ID 和数量来自配置，不硬编码在 UI 文案里。

## 资源交付格式

各同学交付正式资源时，请同时给出以下信息：

| 模块 | 必填信息 |
| --- | --- |
| 地图 | 场景名、NPC 点位名、三个怪物点位名、是否已烘焙 NavMesh |
| 战斗 | 敌人 prefab 路径、敌人血量脚本、死亡回调位置、对应 `EnemyType` |
| UI | 任务目标文本对象路径、NPC 对话入口、完成提示入口 |
| 背包 | 背包服务入口、奖励物品 ID、奖励数量、背包满处理 |

资源路径建议：

- 地图场景：`Assets/Scenes/`
- 敌人 prefab：`Assets/Prefabs/Enemies/`
- NPC prefab：`Assets/Prefabs/NPC/`
- UI prefab：`Assets/Prefabs/UI/`
- 任务配置：`Assets/CS/Quest/` 或后续迁移到 `Assets/Configs/Quest/`

## 接入顺序

等正式资源交付后，按以下顺序接入：

1. 修 Build Settings，加入实际可玩的场景。
2. 接地图点位和任务 NPC。
3. 接敌人 prefab 的 `QuestEnemyMarker`。
4. 接任务 UI 文本刷新。
5. 跑通主线击杀流程。
6. 接背包奖励。
7. 做最终验收：接任务 -> 杀 Slime -> 杀 Elite -> 杀 Boss -> 完成任务 -> 发奖励。

## 当前暂缓项

以下内容等对应同学交付后再做：

- 不改正式地图场景摆放。
- 不替换敌人/NPC/UI 正式资源。
- 不新增背包奖励逻辑。
- 不把临时调试按钮当正式 UI。
