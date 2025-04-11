using System.Collections.Generic;
// Ensure GroupButtons is defined in the global namespace and compiled before this script.
using UnityEngine;
using UnityEngine.UI;

public class GroupsPanelUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform groupButtonsContainer; // Parent for group buttons (e.g., a VerticalLayoutGroup)
    public GroupButtons groupButtonsPrefab; // Prefab for group buttons (edit/delete)
    public Button addGroupButton;           // Button to add a new group

    private List<GroupButtons> groupButtons = new List<GroupButtons>();

    void OnEnable()
    {
        RefreshGroupButtons();

        if (addGroupButton != null)
            addGroupButton.onClick.AddListener(OnAddGroupClicked);

        // Start real-time update coroutine
        StartCoroutine(RealtimeGroupRefresh());
    }

    public void RefreshGroupButtons()
    {
        // Clear old buttons
        foreach (var btn in groupButtons)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }
        groupButtons.Clear();

        // Find all InteractableGroups in the scene
        var groups = FindObjectsOfType<InteractableGroup>();
        foreach (var group in groups)
        {
            var btn = Instantiate(groupButtonsPrefab, groupButtonsContainer);
            btn.Initialize(group, this);
            groupButtons.Add(btn);
        }
    }

    public void OnGroupSelected(InteractableGroup group)
    {
        var editor = LevelEditor.Instance;
        if (editor != null && editor.controller != null)
        {
            var groupEditingState = editor.controller.GetGroupEditingState();
            if (groupEditingState != null)
                groupEditingState.SetActiveGroup(group);
            editor.controller.ChangeToGroupEditingState();
        }
        Debug.Log($"[GroupsPanelUI] Selected group: {group.name}");
    }

    // Coroutine for real-time group list updates
    private System.Collections.IEnumerator RealtimeGroupRefresh()
    {
        var wait = new WaitForSeconds(0.5f);
        var lastGroupCount = -1;
        while (gameObject.activeInHierarchy)
        {
            var groups = FindObjectsOfType<InteractableGroup>();
            if (groups.Length != lastGroupCount)
            {
                RefreshGroupButtons();
                lastGroupCount = groups.Length;
            }
            yield return wait;
        }
    }

    public void OnGroupDeleteRequested(InteractableGroup group)
    {
        if (group != null)
        {
            Destroy(group.gameObject);
            RefreshGroupButtons();
        }
    }

    private void OnAddGroupClicked()
    {
        // Create a new InteractableGroup in the scene
        var newGroupGO = new GameObject("InteractableGroup");
        var newGroup = newGroupGO.AddComponent<InteractableGroup>();
        // Optionally: set position, default members, etc.
        RefreshGroupButtons();
        Debug.Log("[GroupsPanelUI] Added new group.");
    }
}