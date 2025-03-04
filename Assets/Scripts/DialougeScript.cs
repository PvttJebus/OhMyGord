using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DialougeScript : MonoBehaviour
{
    //Lists to put the dialoge text and determine who is talking 
    [TextArea]
    public List<string> Dialogs = new List<string>();

    [Tooltip("0 for narrator, 1 for Blimbo, 2 for Gord")]
    public List<int> characterSpeaking = new List<int>();

    //To control the dialog text and backgrounds/switching
    public int DialogsCount = 0;
    public TMP_Text dialogText;
    public TMP_Text dialogText2;
    public GameObject dialogBkg;
    public CanvasGroup canvasGroup;
    public bool dialogHasStarted;
    public bool mouseOverlap;

    public GameObject blimboSprite;
    public GameObject gordSprite;
    public SortingLayer layer;


    void Start()
    {

       
        dialogText = GetComponent<TMP_Text>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;

    }

    void Update()
    {
        if (dialogHasStarted == true)
        {
            if (characterSpeaking[DialogsCount] == 1)
            {
                blimboSprite.SetActive(true);
                gordSprite.SetActive(false);
            }

            else if (characterSpeaking[DialogsCount] == 2)
            {
                gordSprite.SetActive(true);
                blimboSprite.SetActive(false);
            }

            else
            {
                blimboSprite.SetActive(false);
                gordSprite.SetActive(false);
            }

            DialogBoxAppear();
        }

        if (dialogHasStarted == true && Input.GetMouseButtonDown(0))
        {
            if (DialogsCount < Dialogs.Count - 1)
            {
                DialogsCount++;
            }

            else
            {
                DialogBoxDissapear();
                dialogHasStarted = false;
                DialogsCount = 0;
            }
        }
    }


    public void DialogBoxAppear()
    {
        dialogText.text = Dialogs[DialogsCount];
        dialogText2.text = Dialogs[DialogsCount];
        canvasGroup.alpha = 1;
        dialogHasStarted = true;
        dialogBkg.SetActive(true);
    }

    public void DialogBoxDissapear()
    {
        dialogText.text = Dialogs[DialogsCount];
        dialogText2.text = Dialogs[DialogsCount];
        canvasGroup.alpha = 0;
        dialogHasStarted = true;
        dialogBkg.SetActive(true);
    }


}

