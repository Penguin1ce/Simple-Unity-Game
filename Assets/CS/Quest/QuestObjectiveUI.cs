using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestObjectiveUI : MonoBehaviour
{
    [SerializeField] private Text questText;
    [SerializeField] private TMP_Text questTmpText;

    private IMainQuestService questService;

    private void OnEnable()
    {
        BindQuestService();
        Refresh();
    }

    private void Start()
    {
        BindQuestService();
        Refresh();
    }

    private void OnDisable()
    {
        if (questService != null)
        {
            questService.QuestStageChanged -= HandleQuestStageChanged;
            questService = null;
        }
    }

    private void BindQuestService()
    {
        IMainQuestService service = MainQuestManager.Instance;
        if (service == null || questService == service)
        {
            return;
        }

        if (questService != null)
        {
            questService.QuestStageChanged -= HandleQuestStageChanged;
        }

        questService = service;
        questService.QuestStageChanged += HandleQuestStageChanged;
    }

    private void HandleQuestStageChanged(int stage, string text)
    {
        SetText(text);
    }

    private void Refresh()
    {
        SetText(questService?.GetCurrentQuestText() ?? "当前任务：与村口 NPC 对话");
    }

    private void SetText(string text)
    {
        if (questText != null)
        {
            questText.text = text;
        }

        if (questTmpText != null)
        {
            questTmpText.text = text;
        }
    }
}
