using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GroupButtons : MonoBehaviour
{
    [Header("UI References")]
    public Button editButton;
    public Button deleteButton;
    public TMP_Text groupNameText;

    private InteractableGroup group;
    private GroupsPanelUI parentPanel;

    public void Initialize(InteractableGroup group, GroupsPanelUI parentPanel)
    {
        this.group = group;
        this.parentPanel = parentPanel;
        if (groupNameText != null)
            groupNameText.text = group.name;

        if (editButton != null)
            editButton.onClick.AddListener(OnEditClicked);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    private void OnEditClicked()
    {
        parentPanel.OnGroupSelected(group);
    }

    private void OnDeleteClicked()
    {
        parentPanel.OnGroupDeleteRequested(group);
    }
}