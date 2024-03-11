using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Algorithm1Script : MonoBehaviour
{
    public ToggleGroup toggleGroup;

    void Start()
    {
        // Register this toggle with the toggle group
        var toggle = GetComponent<Toggle>();
        toggle.group = toggleGroup;

        // Add listener to detect toggle changes
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("Toggle " + gameObject.name + " is now selected.");
            // Perform any necessary actions based on the selected option
        }
    }
}
