using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class TechManager : MonoBehaviour
{
    [Header("Prequisite line settings")]
    public Color lineColor = Color.black;
    public float lineWidth = 10f;
    public static event System.Action OnResearchCompleted;

    private static readonly Logger log = new(true, LogLevel.Info);
    private BuildingManager buildingManager;
    private GameObject lineContainer;
    private List<UpgradeNode> allNodes = new();
    private Dictionary<UpgradeNode, List<UpgradeNodeUILine>> nodeLines = new();
    private Dictionary<string, Dictionary<ModifierType, float>> flatModifiers = new();
    private Dictionary<string, Dictionary<ModifierType, float>> percentMultipliers = new();

    private void Start()
    {
        buildingManager = FindFirstObjectByType<BuildingManager>();

        GetComponentsInChildren(allNodes);
        lineContainer = GameObject.Find("LineContainer");
        UpgradeNode.OnUpgradeButtonClicked += HandleUpgradeButtonClicked;

        // Draw lines between prequisites nodes
        foreach (var node in allNodes)
        {
            nodeLines[node] = new();

            foreach (var preq in node.prequisites)
            {
                Vector2 nodePos = node.GetComponent<RectTransform>().anchoredPosition;
                Vector2 preqPos = preq.GetComponent<RectTransform>().anchoredPosition;
                var line = DrawLine(nodePos, preqPos, lineContainer.transform);
                nodeLines[node].Add(line);
            }

            if (IsNodeUnlocked(node))
            {
                node.Unlock();
            }
            else
            {
                node.Lock();
            }
        }
    }

    private void ApplyModifiers(List<UpgradeModifier> modifiers)
    {
        foreach (var mod in modifiers)
        {
            List<string> targetIds = mod.TargetBuildingIds;
            foreach (var target in targetIds)
            {
                if (mod.IsPercent)
                {
                    if (!percentMultipliers.ContainsKey(target))
                        percentMultipliers[target] = new Dictionary<ModifierType, float>();

                    if (!percentMultipliers[target].ContainsKey(mod.ModType))
                        percentMultipliers[target][mod.ModType] = 1f;

                    percentMultipliers[target][mod.ModType] *= 1f + mod.Value; // multiplicative stacking                    
                }
                else
                {
                    if (!flatModifiers.ContainsKey(target))
                        flatModifiers[target] = new Dictionary<ModifierType, float>();

                    if (!flatModifiers[target].ContainsKey(mod.ModType))
                        flatModifiers[target][mod.ModType] = 0f;

                    flatModifiers[target][mod.ModType] += mod.Value;
                }
            }
        }
    }

    public float GetModifiedValue(float baseValue, ModifierType statType, string buildingId)
    {
        float flat = 0f;
        float percent = 1f;

        // Apply modifiers for the specific building type
        if (flatModifiers.ContainsKey(buildingId) && flatModifiers[buildingId].ContainsKey(statType))
            flat = flatModifiers[buildingId][statType];

        if (percentMultipliers.ContainsKey(buildingId) && percentMultipliers[buildingId].ContainsKey(statType))
            percent = percentMultipliers[buildingId][statType];

        return (baseValue + flat) * percent;
    }

    private void HandleUpgradeButtonClicked(UpgradeNode clickedNode)
    {
        if (!clickedNode.IsUnlocked || clickedNode.IsResearched)
            return;

        // Apply modifiers
        ApplyModifiers(clickedNode.Modifiers);
        clickedNode.MarkAsResearched();
        OnResearchCompleted?.Invoke();

        // Unlock any nodes that have this node as a prequisite
        foreach (var node in allNodes)
        {
            if (!node.prequisites.Contains(clickedNode)) continue;

            if (IsNodeUnlocked(node))
            {
                node.Unlock();
            }
        }
    }

    private bool IsNodeUnlocked(UpgradeNode node)
    {
        foreach (var preq in node.prequisites)
        {
            if (!preq.IsResearched)
                return false;
        }
        return true;
    }

    private UpgradeNodeUILine DrawLine(Vector2 a, Vector2 b, Transform parent)
    {
        var go = new GameObject("Line", typeof(RectTransform), typeof(CanvasRenderer), typeof(UpgradeNodeUILine));
        go.transform.SetParent(parent, false);

        var line = go.GetComponent<UpgradeNodeUILine>();
        line.color = lineColor;
        line.a = a;
        line.b = b;
        line.thickness = lineWidth;
        line.SetVerticesDirty();

        return line;
    }
}
