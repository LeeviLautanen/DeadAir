using System.Collections.Generic;

public enum ModifierType
{
    MaxHealth,
    ProductionRate,
    ConsumptionRate,
    Capacity,
    Reservation,
    ResearchRate,
    TurretDamage
}

[System.Serializable]
public struct UpgradeModifier
{
    public List<string> TargetBuildingIds;
    public ModifierType ModType;
    public float Value;
    public bool IsPercent;
}
