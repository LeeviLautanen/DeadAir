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
    public float ResearchCost => data.ResearchCost;
    public string DisplayName => data.DisplayName;
    public static Action<UpgradeNode> OnUpgradeNodeClicked;

    private static readonly Logger log = new(nameof(UpgradeNode));
    [SerializeField] private UpgradeData data;
    private TMP_Text upgradeText;
    private Button upgradeButton;
    private Image upgradeBackground;
    [SerializeField] private bool isUnlocked;
    [SerializeField] private bool isResearched;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color researchedColor;

    private void Awake()
    {
        upgradeBackground = GetComponent<Image>();

        upgradeButton = GetComponent<Button>();
        upgradeButton.onClick.AddListener(() => OnUpgradeNodeClicked?.Invoke(this));

        upgradeText = GetComponentInChildren<TMP_Text>();

        Lock();
    }

    private void OnValidate()
    {
        // This shit is weird
        upgradeText = GetComponentInChildren<TMP_Text>();

        if (upgradeText && data)
        {
            upgradeText.text = data.DisplayName;
            gameObject.name = $"{data.DisplayName}_{transform.GetSiblingIndex()}";
        }
    }

    public void MarkAsResearched()
    {
        isResearched = true;
        upgradeButton.interactable = false;
        upgradeBackground.color = researchedColor;
    }

    public void MarkAsSelected()
    {
        upgradeButton.interactable = false;
        upgradeBackground.color = selectedColor;
    }

    public void Unlock()
    {
        isUnlocked = true;
        upgradeButton.interactable = true;
        upgradeBackground.color = Color.white;
        upgradeText.alpha = 1f;
    }

    public void Lock()
    {
        isUnlocked = false;
        upgradeButton.interactable = false;
        upgradeBackground.color = Color.gray;
        upgradeText.alpha = 0f;
    }
}
