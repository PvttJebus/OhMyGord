using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    // Random name configuration
    public static string[] initialNames = { "Scrimblo Blimbo" };
    public static string[] names = { "Scrimblo", "Blimbo", "Scrimblo Blimbo", "Blimbo Scrimblo", "Blimboff Scriminof",
        "Scrimblo Blimblam", "Scrimblab Blibblab", "Srimbly Bimbly", "Sclorby Dorby", "Scrobus the Blorbus",
        "Blorbus Butthead", "Wormbo Dormbo", "Scrumpus Dumpo", "Jumpboots Jamstrang", "Gwimbly", "Jimbo Blimbo",
        "Blurbee Durbee", "Tweedle Beatle", "John", "Paul", "George", "Ringo", "Jimblo", "The Scrunkly",
        "Scrimblim Blimble", "Superb Mairo", "Glup Shitto", "Cash Bannoca", "Glover" };

    // Dialogue content
    public string[] quips;
    private readonly string[] dead = { "You died." };

    // Scripted dialogue
    [TextArea] public List<string> Dialogs = new List<string>();
    [Tooltip("0 = Narrator, 1 = Scrimblo, 2 = Gord")]
    public List<int> characterSpeaking = new List<int>();

    // UI components
    public TMP_Text dialogText;
    public TMP_Text dialogText2;
    public GameObject dialogBkg;
    public CanvasGroup canvasGroup;
    public GameObject scrimbloSprite;
    public GameObject gordSprite;

    // State management
    [HideInInspector] public int dialogIndex;
    [HideInInspector] public bool dialogActive;
    private bool needsTextRefresh;

    private void Awake()
    {
        InitializeQuips();
    }

    private void Start()
    {
        InitializeComponents();
    }

    private void Update()
    {
        if (dialogActive)
        {
            HandleSpeakerVisuals();
            HandleDialogueProgression();
        }
    }

    private void InitializeQuips()
    {
        quips = new[]
        {
            "I'm a quip",
            $"I'm {GetRandomName()}"
        };
    }

    private void InitializeComponents()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        dialogText = GetComponent<TMP_Text>();
        canvasGroup.alpha = 0;
        dialogBkg.SetActive(false);
    }

    private void HandleSpeakerVisuals()
    {
        scrimbloSprite.SetActive(characterSpeaking[dialogIndex] == 1);
        gordSprite.SetActive(characterSpeaking[dialogIndex] == 2);

        if (needsTextRefresh)
        {
            UpdateDialogueText();
            needsTextRefresh = false;
        }
    }

    private void HandleDialogueProgression()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (dialogIndex < Dialogs.Count - 1)
            {
                dialogIndex++;
                needsTextRefresh = true;
            }
            else
            {
                EndDialogue();
            }
        }
    }

    public void StartDialogue()
    {
        dialogActive = true;
        dialogIndex = 0;
        canvasGroup.alpha = 1;
        dialogBkg.SetActive(true);
        UpdateDialogueText();
    }

    private void UpdateDialogueText()
    {
        var currentText = Dialogs[dialogIndex];

        if (characterSpeaking[dialogIndex] == 2) // Gord speaks
        {
            foreach (var name in initialNames)
            {
                currentText = currentText.Replace(name, GetRandomName());
            }
        }

        dialogText.text = currentText;
        dialogText2.text = currentText;
    }

    private void EndDialogue()
    {
        dialogActive = false;
        canvasGroup.alpha = 0;
        dialogBkg.SetActive(false);
        dialogIndex = 0;
    }

    // Static utility methods
    public static string GetRandomName() => names[Random.Range(0, names.Length)];
    public string GetRandomQuip() => quips[Random.Range(0, quips.Length)];
    public string GetRandomDeathMessage() => dead[Random.Range(0, dead.Length)];
}