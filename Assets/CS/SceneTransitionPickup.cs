using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionPickup : MonoBehaviour
{
    public enum TriggerMode
    {
        PressInteractKey,
        AutoOnEnter
    }

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "Task2-LavaCave";

    [Header("Interaction")]
    [SerializeField] private TriggerMode triggerMode = TriggerMode.PressInteractKey;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float triggerRadius = 1.5f;
    [SerializeField] private GameObject promptUI;

    private bool playerInRange;
    private bool isLoading;

    private void Awake()
    {
        EnsureTriggerCollider();
        ShowPrompt(false);
    }

    private void Update()
    {
        if (triggerMode != TriggerMode.PressInteractKey || !playerInRange || isLoading)
        {
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            LoadNextScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;

        if (triggerMode == TriggerMode.AutoOnEnter)
        {
            LoadNextScene();
            return;
        }

        ShowPrompt(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;
        ShowPrompt(false);
    }

    private void LoadNextScene()
    {
        if (isLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("[SceneTransitionPickup] Next scene name is empty.", this);
            return;
        }

        if (SceneManager.GetActiveScene().name == nextSceneName)
        {
            return;
        }

        isLoading = true;
        SceneManager.LoadScene(nextSceneName);
    }

    private void EnsureTriggerCollider()
    {
        Collider[] colliders = GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].isTrigger)
            {
                return;
            }
        }

        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = triggerRadius;
    }

    private void ShowPrompt(bool show)
    {
        if (promptUI != null)
        {
            promptUI.SetActive(show);
        }
    }
}
