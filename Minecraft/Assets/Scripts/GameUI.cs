using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        Toggle();
    }

    public void Toggle()
    {
        if (canvasGroup.interactable)
            canvasGroup.alpha = 0;
        else
            canvasGroup.alpha = 1;

        canvasGroup.interactable = !canvasGroup.interactable;
        canvasGroup.blocksRaycasts = !canvasGroup.blocksRaycasts;
    }
}
