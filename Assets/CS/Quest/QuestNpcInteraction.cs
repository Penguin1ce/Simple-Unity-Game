using UnityEngine;

public class QuestNpcInteraction : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private Transform player;

    private IMainQuestService QuestService => MainQuestManager.Instance;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void Update()
    {
        if (player == null || QuestService == null)
        {
            return;
        }

        if (Vector3.Distance(transform.position, player.position) > interactDistance)
        {
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    public void Interact()
    {
        IMainQuestService questService = QuestService;
        if (questService == null)
        {
            return;
        }

        Debug.Log(questService.GetNpcDialogueText(), this);

        if (questService.QuestStage == MainQuestManager.NotAcceptedStage)
        {
            questService.AcceptMainQuest();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}
