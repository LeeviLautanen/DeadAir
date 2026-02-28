using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeNode : MonoBehaviour
{
    public List<UpgradeNode> prequisites = new();
    public bool IsUnlocked
    {
        get { return isUnlocked; }
        set
        {
            if (value == true)
                Unlock();
            else
                Lock();
        }
    }
    public bool IsResearched => isResearched;
    public List<UpgradeModifier> Modifiers => data.Modifiers;
    public static Action<UpgradeNode> OnUpgradeButtonClicked;

    private static readonly Logger log = new(true, LogLevel.Info);
    [SerializeField] private UpgradeData data;
    [SerializeField] private TMP_Text upgradeText;
    private Button upgradeButton;
    private Image upgradeBackground;
    [SerializeField] private bool isUnlocked;
    [SerializeField] private bool isResearched;

    private void Awake()
    {
        upgradeBackground = GetComponent<Image>();

        upgradeButton = GetComponent<Button>();
        upgradeButton.onClick.AddListener(() => OnUpgradeButtonClicked?.Invoke(this));

        Lock();
    }

    private void OnValidate()
    {
        if (upgradeText && data)
        {
            upgradeText.text = data.DisplayName;
            gameObject.name = data.DisplayName;
        }
    }

    public void MarkAsResearched()
    {
        isResearched = true;
        upgradeButton.interactable = false;
        upgradeBackground.color = Color.green;
    }

    public void Unlock()
    {
        isUnlocked = true;
        upgradeButton.interactable = true;
        upgradeBackground.color = Color.white;
        upgradeText.gameObject.SetActive(true);
    }

    public void Lock()
    {
        isUnlocked = false;
        upgradeButton.interactable = false;
        upgradeBackground.color = Color.gray;
        upgradeText.gameObject.SetActive(false);
    }
}
