using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer Settings")]
    [Tooltip("Initial time for the timer. Used as countdown start if countdownMode is true, or zero if false.")]
    public float initialTime = 0f;

    [Tooltip("If true, timer counts down from initialTime to zero. If false, counts up from zero.")]
    public bool countdownMode = false;

    public TMP_Text timerText;

    [Header("Game State")]
    public float currentTime;
    public bool isTimerRunning;

    private enum SceneIndex
    {
        MainMenu = 0,
        // Add other named scenes here as needed, e.g.,
        // Level1 = 1,
        // Level2 = 2,
        // Credits = 3
    }

    private const SceneIndex MainMenuScene = SceneIndex.MainMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameManager] Duplicate instance detected, destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // OnDestroy intentionally left empty; cleanup handled in OnDisable

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeTimerUI();
        ManageTimerState(scene);
    }

    private void InitializeTimerUI()
    {
        GameObject timerObject = GameObject.Find("Timer");
        Debug.Log($"[GameManager] InitializeTimerUI: Timer object found: {timerObject != null}");
        if (timerObject != null)
        {
            timerText = timerObject.GetComponent<TMP_Text>();
            Debug.Log($"[GameManager] InitializeTimerUI: timerText assigned: {timerText != null}");
            if (timerText == null)
            {
                Debug.LogWarning("Timer object missing TMP_Text component");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] InitializeTimerUI: Timer GameObject not found");
        }
    }

    private void ManageTimerState(Scene scene)
    {
        bool isLevelScene = scene.buildIndex > (int)MainMenuScene;
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

        if (countdownMode)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isTimerRunning = false;
                Debug.Log("[GameManager] Timer completed (countdown reached zero)");
                // TODO: Invoke timer complete event/callback here
            }
        }
        else
        {
            currentTime += Time.deltaTime;
            // Optionally, add a max time limit and completion logic here
        }

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
        currentTime = countdownMode ? initialTime : 0f;
        isTimerRunning = true;
        Debug.Log($"[GameManager] StartTimer called. Starting at time: {currentTime}");
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        if (Timer.instance != null)
        {
            Timer.instance.HandleTimerStop(currentTime);
        }
        else
        {
            Debug.LogWarning("[GameManager] StopTimer: Timer.instance is null");
        }
        Debug.Log($"[GameManager] StopTimer called. Final time: {currentTime}");
    }

    public void ReloadLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
        ResetTimerTracking();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene((int)MainMenuScene);
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
        else
        {
            Debug.LogWarning("[GameManager] ResetTimerTracking: Timer.instance is null");
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