using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public static Timer instance;
    public float timeAllotted;

    private GameManager _gameManager;
    private GameObject _endCanvas;
    private TMP_Text _finalTimeText;
    private bool _hasTracked;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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
        _gameManager = FindObjectOfType<GameManager>();
        _hasTracked = false;

        switch (scene.name)
        {
            case "TrueLevel1":
                timeAllotted /= 2f;
                break;
            case "End Game":
                InitializeEndGameUI();
                break;
        }
    }

    private void Update()
    {
        if (_gameManager == null) return;

        if (!_gameManager.isTimerRunning && !_hasTracked)
        {
            timeAllotted += _gameManager.currentTime;
            _hasTracked = true;
        }
    }

    private void InitializeEndGameUI()
    {
        _endCanvas = GameObject.Find("End Canvas");
        if (_endCanvas == null)
        {
            Debug.LogError("End Canvas not found in End Game scene");
            return;
        }

        Transform timeTextTransform = _endCanvas.transform.Find("Time");
        if (timeTextTransform == null)
        {
            Debug.LogError("Time text object not found in End Canvas");
            return;
        }

        _finalTimeText = timeTextTransform.GetComponent<TMP_Text>();
        if (_finalTimeText == null)
        {
            Debug.LogError("TMP_Text component missing on Time object");
            return;
        }

        _finalTimeText.text = $"{timeAllotted:F2} Seconds";
    }
    public void HandleTimerStop(float currentTime)
    {
        if (!_hasTracked)
        {
            timeAllotted += currentTime;
            _hasTracked = true;
        }
    }

    public void ResetTimeTracking(float currentTime)
    {
        timeAllotted -= currentTime;
        _hasTracked = false;
    }
}