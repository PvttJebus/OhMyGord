using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Timer Settings")]
    public float levelDuration = 0f;
    public TMP_Text timerText;

    [Header("Game State")]
    public float currentTime;
    public bool isTimerRunning;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeTimerUI();
        ManageTimerState(scene);
    }

    private void InitializeTimerUI()
    {
        GameObject timerObject = GameObject.Find("Timer");
        if (timerObject != null)
        {
            timerText = timerObject.GetComponent<TMP_Text>();
            if (timerText == null)
            {
                Debug.LogWarning("Timer object missing TMP_Text component");
            }
        }
    }

    private void ManageTimerState(Scene scene)
    {
        bool isLevelScene = scene.buildIndex > 0; // Assuming main menu is index 0
        if (isLevelScene)
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }
    }

    private void Update()
    {
        UpdateTimer();
        HandleDebugInput();
    }

    private void UpdateTimer()
    {
        if (!isTimerRunning) return;

        currentTime += Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}";
        }
    }

    private void HandleDebugInput()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            ReloadLevel();
        }

        if (Input.GetKey(KeyCode.Tilde))
        {
            MainMenu();
        }
    }

    public void StartTimer()
    {
        currentTime = levelDuration;
        isTimerRunning = true;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        if (Timer.instance != null)
        {
            Timer.instance.HandleTimerStop(currentTime);
        }
    }

    public void ReloadLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
        ResetTimerTracking();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
        ResetTimerTracking();
    }

    public void NextLevel()
    {
        int nextBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextBuildIndex);
            ResetTimerTracking();
        }
        else
        {
            Debug.LogWarning("No more levels! Returning to Main Menu.");
            MainMenu();
        }
    }

    public void Restart()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
        ResetTimerTracking();
    }

    private void ResetTimerTracking()
    {
        if (Timer.instance != null)
        {
            Timer.instance.ResetTimeTracking(currentTime);
        }
    }

    public void Level(int levelBuildIndex)
    {
        if (levelBuildIndex >= 0 && levelBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(levelBuildIndex);
        }
        else
        {
            Debug.LogError($"Invalid level build index: {levelBuildIndex}");
        }
    }

    public void Credits()
    {
        SceneManager.LoadScene("Credits", LoadSceneMode.Single);
    }

    public void EndGame()
    {
        Application.Quit();
    }
}