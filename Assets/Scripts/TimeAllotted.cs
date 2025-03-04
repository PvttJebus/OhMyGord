using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeAllotted : MonoBehaviour
{

    public TimeAllotted Instance;
    public float TimeAlloted;
    public GameManager GM;
    public GameObject EndCanvas;
    public GameObject TimeTMP;
    public TMP_Text finalTime;
    public bool hasTracked = false;

    // Start is called before the first frame update
    void Start()
    {


    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        GM = FindObjectOfType<GameManager>();
        if (GM.isTimerRunning == false && hasTracked == false)
        {
            TimeAlloted += GM.currentTime;
            hasTracked = true;
        }

       
        
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "TrueLevel1")
        {
            TimeAlloted /= 2;
        }

        if (currentScene == "End Game")
        {
            EndCanvas = GameObject.Find("End Canvas");
            TimeTMP = EndCanvas.transform.Find("Time")?.gameObject;
            finalTime = TimeTMP.GetComponent<TextMeshProUGUI>();

            finalTime.text = TimeAlloted.ToString("F2") + " Seconds";
        }

        Debug.Log(TimeAlloted);
    }
}
