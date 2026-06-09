using UnityEngine;
using TMPro;

public class QuestObjectiveUI : MonoBehaviour
{
    public TMP_Text questText;
    
    // 新增：把完成提示面板拖到这里
    public GameObject completionPanel;

    void Start()
    {
        if (questText == null)
        {
            questText = GetComponent<TMP_Text>();
        }

        if (MainQuestManager.Instance != null)
        {
            MainQuestManager.Instance.QuestStageChanged += UpdateText;
            UpdateText(MainQuestManager.Instance.QuestStage, "");
        }
    }

    void UpdateText(int stage, string message)
    {
        if (questText != null)
        {
            questText.text = "当前目标: " + MainQuestManager.Instance.GetCurrentQuestText();
        }

        // 新增：检测任务是否完成
        if (completionPanel != null)
        {
            if (stage == MainQuestManager.CompletedStage)
            {
                completionPanel.SetActive(true);
            }
            else
            {
                completionPanel.SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        if (MainQuestManager.Instance != null)
            MainQuestManager.Instance.QuestStageChanged -= UpdateText;
    }
}