using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_DialogueUI : MonoBehaviour
{
    public UnityEngine.UI.Text dialogueWindow;
    public UnityEngine.UI.InputField inputField;

    public void OnMessageInput(string message)
    {
        if(!string.IsNullOrWhiteSpace(message))
        {
            dialogueWindow.text += $"User: { message }\n";
            inputField.text = string.Empty;
            inputField.ActivateInputField();
        }
    }
}
