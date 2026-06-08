using TMPro;
using UnityEngine;

public class QuestNpcInteraction : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    private bool isPlayerInTrigger;

    void Update()
    {
        if (!isPlayerInTrigger || !Input.GetKeyDown(interactKey))
        {
            return;
        }

        MainQuestManager questManager = MainQuestManager.Instance;
        if (questManager == null)
        {
            return;
        }

        if (questManager.QuestStage == MainQuestManager.NotAcceptedStage)
        {
            questManager.AcceptMainQuest();
            ShowDialogue(questManager.GetNpcDialogueText());
            return;
        }

        ShowDialogue(questManager.GetNpcDialogueText());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
        }
    }

    private void ShowDialogue(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }
}
