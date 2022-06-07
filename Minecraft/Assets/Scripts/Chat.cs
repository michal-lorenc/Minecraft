using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Chat : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private TMP_InputField input;
    [SerializeField] private CanvasGroup inputCanvasGroup;

    private void Awake ()
    {
        
    }

    public void ToggleInput ()
    {
        input.text = "";
        inputCanvasGroup.interactable = !inputCanvasGroup.interactable;
        inputCanvasGroup.blocksRaycasts = !inputCanvasGroup.blocksRaycasts;

        if (!inputCanvasGroup.interactable)
        {
            inputCanvasGroup.alpha = 0;
        }
        else
        {
            input.Select();
            inputCanvasGroup.alpha = 1;
        }
    }

    public void SendChatMessage ()
    {
        SendChatMessage(input.text);
    }

    public void SendChatMessage (string message)
    {
        input.text = "";

        if (message.Length < 1)
            return;

        if (message[0] == '/')
        {
            message.TrimStart();
            ProcessCommand(message);
        }
        else
        {
            DisplayMessage(message, "Lori");
        }

        ToggleInput();
    }

    public void DisplayMessage (string message, string senderName = "INFO")
    {
        string timeString = "[" + System.DateTime.Now.ToString("HH:mm") + "]";
        text.text += "<b>" + timeString + " " + senderName + "</b>: " + message + "\n";
    }

    public void ProcessCommand (string command)
    {
        string[] commandWords = command.Split(' ');

        switch (commandWords[0])
        {
            case "time":
                break;
            default:
                goto Failure;
        }

        Failure:
            DisplayMessage("Unknown command");
    }
}
