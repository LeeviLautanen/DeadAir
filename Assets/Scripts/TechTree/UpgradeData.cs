using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public string DisplayName;
    public string Description;
    public int ResearchCost;
    public List<UpgradeModifier> Modifiers = new();
}
