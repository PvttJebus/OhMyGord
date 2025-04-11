using System;

// Dummy comment to force Unity recompilation
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the in-game property editing panel UI.
/// Dynamically generates UI controls for editable properties.
/// </summary>
public class PropertyPanelUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform contentContainer; // The vertical layout container inside the ScrollRect
    public TMP_Text titleText;
    public GameObject sliderPrefab; // Used for both float and int
    public GameObject togglePrefab;
    public GameObject resetButtonPrefab;

    private readonly List<GameObject> _activeControls = new();

    /// <summary>
    /// Populates the panel with editable properties of the target object.
    /// </summary>
    public void ShowProperties(object target)
    {
        Clear();

        if (target == null)
        {
            titleText.text = "No Selection";
            return;
        }

        // If target is a List<object> (multi-selection), show only common parameters
        if (target is IList<object> multiList && multiList.Count > 0)
        {
            var first = multiList[0];
            var commonMembers = new List<MemberInfo>();
            var firstMembers = first.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var member in firstMembers)
            {
                var attr = member.GetCustomAttribute<EditableInLevelEditorAttribute>();
                if (attr == null) continue;

                Type memberType = null;
                if (member is FieldInfo field)
                    memberType = field.FieldType;
                else if (member is PropertyInfo prop && prop.CanRead && prop.CanWrite)
                    memberType = prop.PropertyType;
                else
                    continue;

                bool allHave = true;
                foreach (var obj in multiList)
                {
                    var otherMember = obj.GetType().GetMember(member.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (otherMember.Length == 0) { allHave = false; break; }
                    if (memberType != null)
                    {
                        Type otherType = null;
                        if (otherMember[0] is FieldInfo f) otherType = f.FieldType;
                        else if (otherMember[0] is PropertyInfo p && p.CanRead && p.CanWrite) otherType = p.PropertyType;
                        if (otherType != memberType) { allHave = false; break; }
                    }
                }
                if (allHave)
                    commonMembers.Add(member);
            }

            titleText.text = $"{multiList.Count} Selected";

            foreach (var member in commonMembers)
            {
                var attr = member.GetCustomAttribute<EditableInLevelEditorAttribute>();
                Type memberType = null;
                object value = null;
                Action<object> setter = null;

                // Check if all values are the same
                bool allSame = true;
                object firstValue = null;
                foreach (var obj in multiList)
                {
                    object v = null;
                    if (member is FieldInfo field)
                    {
                        memberType = field.FieldType;
                        v = field.GetValue(obj);
                    }
                    else if (member is PropertyInfo prop && prop.CanRead && prop.CanWrite)
                    {
                        memberType = prop.PropertyType;
                        v = prop.GetValue(obj);
                    }
                    if (firstValue == null) firstValue = v;
                    else if (!Equals(firstValue, v)) { allSame = false; break; }
                }
                value = allSame ? firstValue : null;

                // Batch setter
                setter = v =>
                {
                    foreach (var obj in multiList)
                    {
                        if (member is FieldInfo field)
                            field.SetValue(obj, v);
                        else if (member is PropertyInfo prop && prop.CanRead && prop.CanWrite)
                            prop.SetValue(obj, v);
                    }
                };

                CreateControl(attr, memberType, value, setter);
            }
        }
        else
        {
            titleText.text = target.GetType().Name;

            var members = target.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var member in members)
            {
                var attr = member.GetCustomAttribute<EditableInLevelEditorAttribute>();
                if (attr == null) continue;

                Type memberType = null;
                object value = null;
                Action<object> setter = null;

                if (member is FieldInfo field)
                {
                    memberType = field.FieldType;
                    value = field.GetValue(target);
                    setter = v => field.SetValue(target, v);
                }
                else if (member is PropertyInfo prop && prop.CanRead && prop.CanWrite)
                {
                    memberType = prop.PropertyType;
                    value = prop.GetValue(target);
                    setter = v => prop.SetValue(target, v);
                }
                else
                {
                    continue;
                }

                CreateControl(attr, memberType, value, setter);
            }
        }
    }

    /// <summary>
    /// Clears all existing UI controls.
    /// </summary>
    public void Clear()
    {
        foreach (var go in _activeControls)
        {
            Destroy(go);
        }
        _activeControls.Clear();
    }

    /// <summary>
    /// Creates a UI control based on the property type and metadata.
    /// </summary>
    private void CreateControl(EditableInLevelEditorAttribute attr, Type type, object value, Action<object> setter)
    {
        // Placeholder: instantiate appropriate prefab and bind events
        // This is a stub; actual implementation will depend on your UI prefabs

        GameObject control = null;

        if (type == typeof(float) || type == typeof(int))
        {
            control = Instantiate(sliderPrefab, contentContainer);
            // Bind slider value and events here
            // For int, round the slider value before applying
        }
        else if (type == typeof(bool))
        {
            control = Instantiate(togglePrefab, contentContainer);
            // Bind toggle value and events here
        }
        else
        {
            // Unsupported type
            return;
        }

        _activeControls.Add(control);
    }
}