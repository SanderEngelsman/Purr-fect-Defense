using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ButtonClickAudioManager : MonoBehaviour
{
    public static ButtonClickAudioManager Instance { get; private set; }

    [SerializeField, Tooltip("Buttons that play the button click sound when clicked")]
    private List<Button> buttonsWithClickSound = new List<Button>();

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            return;
        }

        // Add click listeners to assigned buttons
        foreach (var button in buttonsWithClickSound)
        {
            if (button != null)
            {
                button.onClick.AddListener(() => AudioManager.Instance?.PlayButtonClickSound());
                Debug.Log($"Added click sound listener to button: {button.name}", button);
            }
        }
    }

    private void OnValidate()
    {
        // Warn about null or duplicate buttons
        for (int i = 0; i < buttonsWithClickSound.Count; i++)
        {
            if (buttonsWithClickSound[i] == null)
            {
                Debug.LogWarning($"Button at index {i} in ButtonsWithClickSound is null.", this);
            }
            else if (buttonsWithClickSound.IndexOf(buttonsWithClickSound[i]) != i)
            {
                Debug.LogWarning($"Duplicate button {buttonsWithClickSound[i].name} at index {i} in ButtonsWithClickSound.", this);
            }
        }
    }
}