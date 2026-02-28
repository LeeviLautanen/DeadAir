using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public string DisplayName;
    public int researchCost;
    public List<UpgradeModifier> Modifiers = new();
}
