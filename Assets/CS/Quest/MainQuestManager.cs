using System;
using UnityEngine;

public class MainQuestManager : MonoBehaviour, IMainQuestService
{
    public const int NotAcceptedStage = 0;
    public const int KillSlimeStage = 1;
    public const int KillEliteStage = 2;
    public const int KillBossStage = 3;
    public const int CompletedStage = 4;

    private const string QuestStageSaveKey = "MainQuest.Stage";

    public static MainQuestManager Instance { get; private set; }

    [Header("Save Settings")]
    [SerializeField] private bool loadOnAwake = true;
    [SerializeField] private bool saveOnStageChanged = true;

    [Header("Debug")]
    [SerializeField, Range(NotAcceptedStage, CompletedStage)]
    private int questStage = NotAcceptedStage;

    public int QuestStage => questStage;
    public bool IsQuestCompleted => questStage >= CompletedStage;

    public event Action<int, string> QuestStageChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null || FindObjectOfType<MainQuestManager>() != null)
        {
            return;
        }

        new GameObject("MainQuestManager").AddComponent<MainQuestManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (loadOnAwake)
        {
            LoadQuestStage();
        }
    }

    private void Start()
    {
        NotifyQuestStageChanged();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AcceptMainQuest()
    {
        if (questStage != NotAcceptedStage)
        {
            return;
        }

        SetQuestStage(KillSlimeStage, true);
    }

    public void OnEnemyKilled(EnemyType enemyType)
    {
        int nextStage = questStage;

        if (questStage == KillSlimeStage && enemyType == EnemyType.Slime)
        {
            nextStage = KillEliteStage;
        }
        else if (questStage == KillEliteStage && enemyType == EnemyType.Elite)
        {
            nextStage = KillBossStage;
        }
        else if (questStage == KillBossStage && enemyType == EnemyType.Boss)
        {
            nextStage = CompletedStage;
        }

        if (nextStage != questStage)
        {
            SetQuestStage(nextStage, true);
        }
    }

    public string GetCurrentQuestText()
    {
        switch (questStage)
        {
            case NotAcceptedStage:
                return "当前任务：与村口 NPC 对话";
            case KillSlimeStage:
                return "当前任务：击败史莱姆";
            case KillEliteStage:
                return "当前任务：击败精英怪";
            case KillBossStage:
                return "当前任务：击败 Boss";
            case CompletedStage:
                return "任务完成：村庄危机已解除";
            default:
                return "当前任务：与村口 NPC 对话";
        }
    }

    public string GetNpcDialogueText()
    {
        switch (questStage)
        {
            case NotAcceptedStage:
                return "附近出现了很多怪物，请你先去击败史莱姆。";
            case KillSlimeStage:
                return "请先去击败史莱姆。";
            case KillEliteStage:
                return "史莱姆已经解决了，接下来去击败精英怪。";
            case KillBossStage:
                return "最后去击败 Boss，村庄就安全了。";
            case CompletedStage:
                return "村庄危机已经解除，谢谢你。";
            default:
                return "附近出现了很多怪物，请你先去击败史莱姆。";
        }
    }

    public void ResetQuest()
    {
        SetQuestStage(NotAcceptedStage, true);
    }

    public void SetQuestStageForDebug(int stage)
    {
        SetQuestStage(stage, true);
    }

    private void SetQuestStage(int stage, bool notify)
    {
        int clampedStage = Mathf.Clamp(stage, NotAcceptedStage, CompletedStage);
        if (questStage == clampedStage)
        {
            if (notify)
            {
                NotifyQuestStageChanged();
            }

            return;
        }

        questStage = clampedStage;

        if (saveOnStageChanged)
        {
            SaveQuestStage();
        }

        if (notify)
        {
            NotifyQuestStageChanged();
        }

        if (questStage == CompletedStage)
        {
            Debug.Log(GetCurrentQuestText(), this);
        }
    }

    private void NotifyQuestStageChanged()
    {
        QuestStageChanged?.Invoke(questStage, GetCurrentQuestText());
    }

    private void SaveQuestStage()
    {
        PlayerPrefs.SetInt(QuestStageSaveKey, questStage);
        PlayerPrefs.Save();
    }

    private void LoadQuestStage()
    {
        questStage = Mathf.Clamp(
            PlayerPrefs.GetInt(QuestStageSaveKey, NotAcceptedStage),
            NotAcceptedStage,
            CompletedStage
        );
    }
}
