using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Analytics;

public class PopupUI : BaseUI
{
    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;


    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform buttonRoot;
    [SerializeField] private PopUpButtonUI buttonPrefabs;


    private readonly List<PopUpButtonUI> spawnButtons = new List<PopUpButtonUI>();

    protected override void OnInitilze()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }
        base.OnInitilze();

        Hide();
    }


    public void ShowPopup(string title, string message, List<PopupButtonData> buttons, bool showCloseButton = true)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }

        if (messageText != null)
        { 
            messageText.text = message;
        }

        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(showCloseButton);
        }

        ClearButton();

        if (buttons != null)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                PopupButtonData buttondata = buttons[i];
                if (buttondata == null)
                {
                    continue;
                }
                PopUpButtonUI buttonUI = Instantiate(buttonPrefabs, buttonRoot);
                spawnButtons.Add(buttonUI);

                buttonUI.SetButton(buttondata.label, () =>
                {
                    Hide();
                    buttondata.onClick.Invoke();

                });

            }
        }
        
    }



    public void ShowMessage(string title, string message, string confrimText = "»Æ¿Œ", Action onConfrim = null)
    {
        var buttons = new List<PopupButtonData>
        {
            new PopupButtonData(confrimText,onConfrim)
        };

        ShowPopup(title, message, buttons, showCloseButton: true);
    }

    public void ShowChoice(string title, string message, string leftText, Action onLeft, string rightText, Action onRight, bool showCloseButton = false)
    {
        var buttons = new List<PopupButtonData>
        {
            new PopupButtonData(leftText, onLeft),
            new PopupButtonData(rightText, onRight)
        };

        ShowPopup(title, message, buttons, showCloseButton);
    }

    private void ClearButton()
    {
        for (int i = 0; i < spawnButtons.Count; i++)
        {
            if (spawnButtons[i] == null)
            {
                continue;
            }
            spawnButtons[i].Clear();
            Destroy(spawnButtons[i].gameObject);
        }
        spawnButtons.Clear();
    }

    protected override void OnHide()
    {
        ClearButton();
    }
}
