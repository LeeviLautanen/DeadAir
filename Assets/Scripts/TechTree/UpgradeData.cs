using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public string DisplayNameTemplate;
    public string DisplayName => InsertValuesIntoTemplate(DisplayNameTemplate);
    public string DescriptionTemplate;
    public string Description => InsertValuesIntoTemplate(DescriptionTemplate);
    public int ResearchCost;
    public List<UpgradeModifier> Modifiers = new();

    public string InsertValuesIntoTemplate(string template)
    {
        if (string.IsNullOrEmpty(template))
            return "No name";

        foreach (var m in Modifiers)
        {
            string key = "{" + m.ModType + "}";
            string valueText = m.IsPercent ? $"{m.Value}%" : $"{m.Value}";
            template = template.Replace(key, valueText);
        }

        return template;
    }
}
