using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildMenuElement : MonoBehaviour
{
    public static event System.Action<BuildMenuElement> OnElementClicked;
    public bool IsSelected
    {
        get { return isSelected; }
        set
        {
            if (value == true)
                SelectElement();
            else
                UnselectElement();
        }
    }
    public string BuildingId => buildingId;
    public BuildingData Data => prefab.GetComponent<Building>().Data;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.gray7;

    private static readonly Logger log = new(nameof(BuildMenuElement));
    [Header("Element data")]
    [SerializeField] private GameObject prefab;
    string buildingId;
    private Sprite sprite;
    private bool isSelected;

    [Header("Internal references")]
    [SerializeField] private Image icon;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text indexNumber;

    private void Awake()
    {
        if (prefab == null)
            return;

        sprite = prefab.GetComponentInChildren<SpriteRenderer>().sprite;
        buildingId = prefab.GetComponent<Building>().Data.Id;

        if (sprite != null)
        {
            icon.sprite = sprite;
        }

        button.onClick.AddListener(() =>
        {
            OnElementClicked?.Invoke(this);
            SelectElement();
        });

        UnselectElement();
    }

    private void OnValidate()
    {
        if (prefab == null)
            return;

        sprite = prefab.GetComponentInChildren<SpriteRenderer>().sprite;
        buildingId = prefab.GetComponent<Building>().Data.Id;

        if (sprite != null)
        {
            icon.sprite = sprite;
        }

        if (buildingId.Length > 0)
        {
            gameObject.name = $"{buildingId}_element";
        }

        int index = transform.GetSiblingIndex();
        indexNumber.text = (index + 1).ToString();
    }

    public void SelectElement()
    {
        if (isSelected)
            return;

        isSelected = true;
        button.image.color = selectedColor;
    }

    public void UnselectElement()
    {
        if (!isSelected)
            return;

        isSelected = false;
        button.image.color = normalColor;
    }
}
