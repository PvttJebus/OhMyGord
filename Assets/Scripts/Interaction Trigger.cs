using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractionTrigger : MonoBehaviour
{
    [Header("Interaction Settings")]
    public bool destroyOnEnter = false;
    public bool DialogueBox = false;
    public bool Quip = false;
    public bool Death = false;
    public bool NextLevel = false;

    [Header("Dependencies")]
    public Dialogue dialogue;
    public GameManager gm;
    public GameObject levelEndCanvas;
    public GameObject levelResults;
    public GameObject winUI;
    public GameObject loseUI;
    public TMP_Text endTime;
    public TMP_Text endScore;
    public TMP_Text endResult;

    private int playerScore;
    private List<string> originalDialogs = new List<string>();
    private List<int> originalSpeakers = new List<int>();

    void Awake()
    {
        GameObject dm = GameObject.Find("Dialog Manager");
        if (dm != null) dialogue = dm.GetComponent<Dialogue>();
        gm = FindObjectOfType<GameManager>();

        InitializeLevelEndComponents();
    }

    void InitializeLevelEndComponents()
    {
        levelEndCanvas = GameObject.Find("Level-End Canvas");

        if (levelEndCanvas != null)
        {
            levelResults = levelEndCanvas.transform.Find("Results")?.gameObject;
            winUI = levelEndCanvas.transform.Find("Win")?.gameObject;
            loseUI = levelEndCanvas.transform.Find("Lose")?.gameObject;
            endTime = levelResults?.transform.Find("Time")?.GetComponent<TMP_Text>();
            endScore = levelResults?.transform.Find("Score")?.GetComponent<TMP_Text>();
            endResult = levelResults?.transform.Find("End Text")?.GetComponent<TMP_Text>();
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger");

            if (DialogueBox) ExecuteDialogue();
            if (Quip) StartCoroutine(ExecuteQuip());
            if (Death) ExecuteDeath();
            if (NextLevel) ExecuteNextLevel();

            if (destroyOnEnter) Destroy(gameObject);
        }
    }

    public void ExecuteDialogue()
    {
        if (dialogue != null) dialogue.StartDialogue();
    }

    private IEnumerator ExecuteQuip()
    {
        if (dialogue != null)
        {
            // Backup original dialog data
            originalDialogs = new List<string>(dialogue.Dialogs);
            originalSpeakers = new List<int>(dialogue.characterSpeaking);

            // Set temporary quip dialog
            dialogue.Dialogs.Clear();
            dialogue.Dialogs.Add(dialogue.GetRandomQuip());
            dialogue.characterSpeaking.Clear();
            dialogue.characterSpeaking.Add(0); // Narrator

            dialogue.StartDialogue();

            // Wait for quip to complete
            yield return new WaitWhile(() => dialogue.dialogActive);

            // Restore original dialog data
            dialogue.Dialogs = new List<string>(originalDialogs);
            dialogue.characterSpeaking = new List<int>(originalSpeakers);
        }
    }

    public void ExecuteDeath()
    {
        if (levelEndCanvas == null) return;

        levelEndCanvas.SetActive(true);
        levelResults.SetActive(true);
        loseUI.SetActive(true);
        winUI.SetActive(false);

        gm.StopTimer();
        endResult.text = $"{Dialogue.GetRandomName()} is Died!";
        endResult.fontSize = 36;
        endTime.text = $"Time: {gm.currentTime:F2}";
        endTime.fontSize = 24;
        endScore.text = $"Score: {playerScore}";
        endScore.fontSize = 24;
    }

    public void ExecuteNextLevel()
    {
        if (levelEndCanvas == null) return;

        levelEndCanvas.SetActive(true);
        levelResults.SetActive(true);
        winUI.SetActive(true);
        loseUI.SetActive(false);

        gm.StopTimer();
        endResult.text = $"{Dialogue.GetRandomName()} Did Good!";
        endResult.fontSize = 36;
        endTime.text = $"Time: {gm.currentTime:F2}";
        endTime.fontSize = 24;
        endScore.text = $"Score: {playerScore}";
        endScore.fontSize = 24;
    }
}