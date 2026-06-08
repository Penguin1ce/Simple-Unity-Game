using UnityEngine;
using TMPro;

public class QuestObjectiveUI : MonoBehaviour
{
    public TMP_Text questText;

    // 新增：把完成提示面板拖到这里
    public GameObject completionPanel;

    private MainQuestManager boundQuestManager;

    void OnEnable()
    {
        BindQuestManager();
    }

    void Start()
    {
        BindQuestManager();
    }

    void UpdateText(int stage, string message)
    {
        if (questText != null)
        {
            questText.text = "当前目标: " + message;
        }

        // 新增：检测任务是否完成
        if (completionPanel != null)
        {
            completionPanel.SetActive(stage == MainQuestManager.CompletedStage);
        }
    }

    void OnDisable()
    {
        if (boundQuestManager != null)
        {
            boundQuestManager.QuestStageChanged -= UpdateText;
            boundQuestManager = null;
        }
    }

    private void BindQuestManager()
    {
        if (questText == null)
        {
            questText = GetComponent<TMP_Text>();
        }

        MainQuestManager questManager = MainQuestManager.Instance;
        if (questManager == null || boundQuestManager == questManager)
        {
            return;
        }

        if (boundQuestManager != null)
        {
            boundQuestManager.QuestStageChanged -= UpdateText;
        }

        boundQuestManager = questManager;
        boundQuestManager.QuestStageChanged += UpdateText;
        UpdateText(boundQuestManager.QuestStage, boundQuestManager.GetCurrentQuestText());
    }
}
