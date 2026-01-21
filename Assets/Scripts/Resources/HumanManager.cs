using UnityEngine;
using System.Collections;

public class HumanManager : MonoBehaviour
{
    private ResourceManager resourceManager;

    [Header("Settings")]
    [SerializeField] private float checkInterval = 1f; // How often to check (in seconds)
    [SerializeField] private int increaseRate = 5;   // How many humans to add/remove per check
    [SerializeField] private int decreaseRate = 5;   // How many humans to remove per check

    private Coroutine adjustmentCoroutine;

    private void Start()
    {
        resourceManager = GetComponent<ResourceManager>();

        // Start the periodic adjustment
        if (resourceManager != null)
        {
            adjustmentCoroutine = StartCoroutine(AdjustHumanCountPeriodically());
        }
        else
        {
            Debug.LogError("ResourceManager component not found!");
        }
    }

    private IEnumerator AdjustHumanCountPeriodically()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            AdjustHumanCount();
        }
    }

    private void AdjustHumanCount()
    {
        int currentHumans = resourceManager.GetResourceAmount("humans");
        int maxAmount = resourceManager.GetResourceMaxAmount("humans");

        if (currentHumans > maxAmount)
        {
            resourceManager.TryConsumeResource("humans", -decreaseRate);
        }
        else if (currentHumans < maxAmount)
        {
            resourceManager.AddResource("humans", increaseRate);
        }
    }

    private void OnDisable()
    {
        if (adjustmentCoroutine != null)
        {
            StopCoroutine(adjustmentCoroutine);
        }
    }

    private void OnEnable()
    {
        if (resourceManager != null && adjustmentCoroutine == null)
        {
            adjustmentCoroutine = StartCoroutine(AdjustHumanCountPeriodically());
        }
    }
}
