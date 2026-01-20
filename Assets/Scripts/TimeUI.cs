using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    private GameObject clockTextGO;
    private GameObject dayCounterGO;
    private TMP_Text clockText;
    private TMP_Text dayCounterText;
    private TimeManager timeManager;
    private int lastHour = -1;
    private int lastDay = -1;

    private void Start()
    {
        clockTextGO = GameObject.Find("Clock");
        dayCounterGO = GameObject.Find("DayCounter");
        clockText = clockTextGO.GetComponent<TMP_Text>();
        dayCounterText = dayCounterGO.GetComponent<TMP_Text>();
        timeManager = FindFirstObjectByType<TimeManager>();
    }

    private void FixedUpdate()
    {
        int hour = timeManager.GetHour();
        if (hour != lastHour)
        {
            lastHour = hour;
            clockText.text = $"{hour:D2}:00";
        }

        int day = timeManager.CurrentDay;
        if (day != lastDay)
        {
            lastDay = day;
            dayCounterText.text = $"Day {day}";
        }
    }
}
