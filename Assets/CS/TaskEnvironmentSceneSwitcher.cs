using UnityEngine;
using UnityEngine.SceneManagement;

public class TaskEnvironmentSceneSwitcher : MonoBehaviour
{
    private const string Task1SceneName = "Task1-GrassLand";
    private const string Task2SceneName = "Task2-LavaCave";
    private const string Task3SceneName = "Task3-SkullThrone";

    private static TaskEnvironmentSceneSwitcher instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
        {
            return;
        }

        TaskEnvironmentSceneSwitcher existing = FindObjectOfType<TaskEnvironmentSceneSwitcher>();
        if (existing != null)
        {
            instance = existing;
            DontDestroyOnLoad(existing.gameObject);
            return;
        }

        GameObject switcherObject = new GameObject(nameof(TaskEnvironmentSceneSwitcher));
        instance = switcherObject.AddComponent<TaskEnvironmentSceneSwitcher>();
        DontDestroyOnLoad(switcherObject);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            LoadTaskScene(Task1SceneName);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            LoadTaskScene(Task2SceneName);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            LoadTaskScene(Task3SceneName);
        }
    }

    private static void LoadTaskScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
