using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float levelDuration = 0f; // Default level time in seconds
    public TMP_Text timerText; // UI connection

    public float currentTime;
    public bool isTimerRunning;
    public TimeAllotted TA;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            timerText = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only run timer in level scenes (assuming main menu is index 0)
        if (scene.buildIndex > -1)
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }
    }

    void Update()
    {
        TA = FindAnyObjectByType<TimeAllotted>().GetComponent<TimeAllotted>();
        if (isTimerRunning)
        {
            currentTime += Time.deltaTime;

            // Update UI if available
            if (timerText != null)
            {
                timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}";
            }

            // Restart level when time expires
           
        }

        timerText = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();

        if (Input.GetKey(KeyCode.Escape))
        {
            ReloadLevel();
        }

        if (Input.GetKey (KeyCode.Tilde))
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
    }

    public void ReloadLevel()
    {
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        TA.TimeAlloted -= currentTime;
        TA.hasTracked = false;

    }

    // Existing scene management functions
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void NextLevel()
    {
        int nextBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            
            SceneManager.LoadScene(nextBuildIndex);
            TA.hasTracked = false;
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
        TA.TimeAlloted -= currentTime;
        TA.hasTracked = false;
    }

    public void Level(int levelBuildIndex)
    {
        if (levelBuildIndex >= 0 && levelBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(levelBuildIndex);
        }
        else
        {
            Debug.LogError("Invalid level build index: " + levelBuildIndex);
        }
    }

    public void Credits()
    {
        SceneManager.LoadScene("Credits",LoadSceneMode.Single);
    }

    public void EndGame()
    {
        Application.Quit();
    }
}