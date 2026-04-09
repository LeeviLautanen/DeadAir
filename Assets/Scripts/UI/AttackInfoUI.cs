using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class AttackInfoUI : MonoBehaviour
{
    private MeteoriteWaveManager meteoriteWaveManager;
    private TMP_Text attackAmountText;

    private void Start()
    {
        meteoriteWaveManager = FindFirstObjectByType<MeteoriteWaveManager>();
        meteoriteWaveManager.OnNextWaveInfoUpdated += UpdateAttackInfo;

        attackAmountText = gameObject.GetComponent<TMP_Text>();

        if (meteoriteWaveManager)
            UpdateAttackInfo(meteoriteWaveManager.GetNextWaveData());
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
        attackAmountText.text = $"{amount}";
    }
}
