using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

public class TechManager : MonoBehaviour, IDragHandler, IScrollHandler
{
    public static event System.Action OnResearchCompleted;
    public bool IsVisible => isVisible;

    [Header("Prequisite line settings")]
    public Color lineColor = Color.black;
    public float lineWidth = 10f;

    [Header("Tree movement settings")]
    public RectTransform nodeContainerRect;
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f;
    public float maxZoom = 2f;

    private static readonly Logger log = new(true, LogLevel.Info);
    private GameObject lineContainer;
    private List<UpgradeNode> allNodes = new();
    private Dictionary<UpgradeNode, List<UpgradeNodeUILine>> nodeLines = new();
    private Dictionary<string, Dictionary<ModifierType, float>> flatModifiers = new();
    private Dictionary<string, Dictionary<ModifierType, float>> percentMultipliers = new();
    private bool isVisible;
    private Canvas upgradeCanvas;

    private void Start()
    {
        upgradeCanvas = GetComponent<Canvas>();
        SetVisible(false);

        GetComponentsInChildren(allNodes);
        lineContainer = GameObject.Find("LineContainer");
        UpgradeNode.OnUpgradeButtonClicked += HandleUpgradeButtonClicked;

        // Set up reserach view toggle button
        GameObject.Find("ResearchButton").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
        {
            SetVisible(!isVisible);
        });

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
                node.Unlock();
            else
                node.Lock();
        }
    }

    public void SetVisible(bool visible)
    {
        isVisible = visible;
        upgradeCanvas.enabled = visible;
    }

    public void OnDrag(PointerEventData eventData)
    {
        nodeContainerRect.anchoredPosition += eventData.delta;
    }

    public void OnScroll(PointerEventData eventData)
    {
        float oldScale = nodeContainerRect.localScale.x;
        float newScale = Mathf.Clamp(oldScale + eventData.scrollDelta.y * zoomSpeed, minZoom, maxZoom);
        nodeContainerRect.localScale = Vector3.one * newScale;
    }

    public float GetModifiedValue(float baseValue, ModifierType statType, string buildingId)
    {
        float flat = 0f;
        float percent = 1f;

        if (flatModifiers.ContainsKey(buildingId) && flatModifiers[buildingId].ContainsKey(statType))
            flat = flatModifiers[buildingId][statType];

        if (percentMultipliers.ContainsKey(buildingId) && percentMultipliers[buildingId].ContainsKey(statType))
            percent = percentMultipliers[buildingId][statType];

        return (baseValue + flat) * percent;
    }

    private void ApplyModifiers(List<UpgradeModifier> modifiers)
    {
        foreach (var mod in modifiers)
        {
            foreach (var target in mod.TargetBuildingIds)
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

    private void HandleUpgradeButtonClicked(UpgradeNode clickedNode)
    {
        if (!clickedNode.IsUnlocked || clickedNode.IsResearched)
            return;

        ApplyModifiers(clickedNode.Modifiers);
        clickedNode.MarkAsResearched();
        OnResearchCompleted?.Invoke();

        foreach (var node in allNodes)
        {
            if (node.prequisites.Contains(clickedNode) && IsNodeUnlocked(node))
                node.Unlock();
        }
    }

    private bool IsNodeUnlocked(UpgradeNode node)
    {
        foreach (var preq in node.prequisites)
            if (!preq.IsResearched)
                return false;

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
