    using UnityEngine;
using System;

public class MainQuestManager : MonoBehaviour
{
    public static MainQuestManager Instance { get; private set; }

    // 定义任务阶段常量
    public const int NotAcceptedStage = 0;
    public const int KillSlimeStage = 1;
    public const int KillEliteStage = 2;
    public const int KillBossStage = 3;
    public const int CompletedStage = 4;

    private int questStage = NotAcceptedStage;
    public int QuestStage => questStage;
    public bool IsQuestCompleted => questStage == CompletedStage;

    // 事件：当任务阶段改变时通知 UI
    public event Action<int, string> QuestStageChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 切换场景时不销毁
    }

    public void AcceptMainQuest()
    {
        SetQuestStage(KillSlimeStage);
    }

    public void OnEnemyKilled(EnemyType enemyType)
    {
        if (questStage == KillSlimeStage && enemyType == EnemyType.Slime)
        {
            SetQuestStage(KillEliteStage);
        }
        else if (questStage == KillEliteStage && enemyType == EnemyType.Elite)
        {
            SetQuestStage(KillBossStage);
        }
        else if (questStage == KillBossStage && enemyType == EnemyType.Boss)
        {
            SetQuestStage(CompletedStage);
        }
    }

    // 新增：调试用，直接设置阶段
    public void SetQuestStageForDebug(int stage)
    {
        SetQuestStage(stage);
    }

    private void SetQuestStage(int newStage)
    {
        questStage = newStage;
        QuestStageChanged?.Invoke(newStage, GetCurrentQuestText());
    }

    public string GetCurrentQuestText()
    {
        switch (questStage)
        {
            case NotAcceptedStage: return "与村口 NPC 对话";
            case KillSlimeStage: return "击败史莱姆";
            case KillEliteStage: return "击败精英怪";
            case KillBossStage: return "击败 Boss";
            case CompletedStage: return "主线任务已完成";
            default: return "未知任务";
        }
    }

    public string GetNpcDialogueText()
    {
        return "勇士，村里的史莱姆最近太猖狂了，你能帮帮我吗？";
    }
}