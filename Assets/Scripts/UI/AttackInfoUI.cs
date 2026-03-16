using UnityEngine;
using TMPro;

public class AttackInfoUI : MonoBehaviour
{
    private MeteoriteWaveManager meteoriteWaveManager;
    private TMP_Text attackDateText;
    private TMP_Text attackAmountText;

    private void Start()
    {
        meteoriteWaveManager = FindFirstObjectByType<MeteoriteWaveManager>();
        meteoriteWaveManager.OnNextWaveInfoUpdated += UpdateAttackInfo;

        GameObject infoContainer = GameObject.Find("AttackInfoContainer");
        attackDateText = infoContainer.transform.Find("AttackDate").GetComponent<TMP_Text>();
        attackAmountText = infoContainer.transform.Find("AttackAmount").GetComponent<TMP_Text>();
    }

    private void OnDestroy()
    {
        if (meteoriteWaveManager != null)
            meteoriteWaveManager.OnNextWaveInfoUpdated -= UpdateAttackInfo;
    }

    private void UpdateAttackInfo((int, int, int)? waveData)
    {
        if (waveData == null)
        {
            return;
        }

        var (amount, day, hour) = waveData.Value;
        attackDateText.text = $"Day {day}, {hour:00}:00";
        attackAmountText.text = $"Meteorites: {amount}";
    }
}
