using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractionTrigger : MonoBehaviour
{
    public bool destroyOnEnter = false;
    public bool DialogueBox = false;
    public bool Quip = false;
    public bool Death = false;
    public bool NextLevel = false;
    public DialougeScript dialog;
    public GameManager gm;

    public GameObject levelEndCanvas;
    public GameObject levelResults;
    public GameObject winUI;
    public GameObject loseUI;

    public TMP_Text endTime;
    public TMP_Text endScore;
    public TMP_Text endResult;
    private int playerScore;
    public Dialogue name;


    void Awake()
    {
        GameObject dm = GameObject.Find("Dialog Manager");
        dialog = dm.GetComponent<DialougeScript>();
        gm = FindObjectOfType<GameManager>();

        levelEndCanvas = GameObject.Find("Level-End Canvas");
        
        if (levelEndCanvas != null)
        {
            levelResults = levelEndCanvas.transform.Find("Results")?.gameObject;
            winUI = levelEndCanvas.transform.Find("Win")?.gameObject;
            loseUI = levelEndCanvas.transform.Find("Lose")?.gameObject;
            endTime = levelResults.transform.Find("Time")?.GetComponent<TextMeshProUGUI>();
            endScore = levelResults.transform.Find("Score")?.GetComponent<TextMeshProUGUI>();
            endResult = levelResults.transform.Find("End Text")?.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("Does Level End Canvas even exist bro?");
        }
    }


    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Player entered trigger");

            if (DialogueBox) { ExecuteDialogue(); }
            if (Quip) { ExecuteQuip(); }
            if (Death) { ExecuteDeath(); }
            if (NextLevel) { ExecuteNextLevel(); }

            if (destroyOnEnter)
            {
                Destroy(gameObject);
            }
        }
    }

    public void ExecuteDialogue()
    {
        dialog.dialogHasStarted = true;
    }
    public void ExecuteQuip() { }
    public void ExecuteDeath()
    {
        levelEndCanvas.SetActive(true);
        levelResults.SetActive(true);
        loseUI.SetActive(true);
        winUI.SetActive(false);
        gm.StopTimer();
        endResult.text = $"{Dialogue.GetRandomName()} is Died!";
        endResult.fontSize = 36;
        endTime.text = $"Time: " + gm.currentTime.ToString("F2");
        endTime.fontSize = 24;
        endScore.text = $"Score: {playerScore}";
        endScore.fontSize = 24;
        
    }
    public void ExecuteNextLevel()
    {
        levelEndCanvas.SetActive(true);
        levelResults.SetActive(true);
        winUI.SetActive(true);
        loseUI.SetActive(false);
        gm.StopTimer();
        endResult.text = $"{Dialogue.GetRandomName()} Did Good!";
        endResult.fontSize = 36;
        endTime.text = $"Time: " + gm.currentTime.ToString("F2");
        endTime.fontSize = 24;
        endScore.text = $"Score: {playerScore}";
        endScore.fontSize = 24;
        

    }
}