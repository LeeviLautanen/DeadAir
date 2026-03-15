using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    private TMP_Text clockText;
    private TMP_Text dayCounterText;
    private TMP_Text gameTimeMultText;
    private TimeManager timeManager;
    private int lastHour = -1;
    private int lastDay = -1;
    private float lastMult = -1f;

    private void Start()
    {
        clockText = GameObject.Find("Clock").GetComponent<TMP_Text>();
        dayCounterText = GameObject.Find("DayCounter").GetComponent<TMP_Text>();
        gameTimeMultText = GameObject.Find("GameTimeMult").GetComponent<TMP_Text>();
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

        float mult = timeManager.GameTimeMultiplier;
        if (mult != lastMult)
        {
            lastMult = mult;
            if (mult == 1f)
            {
                gameTimeMultText.text = "";
            }
            else if (mult == 0f)
            {
                gameTimeMultText.text = "(Paused)";
            }
            else
            {
                gameTimeMultText.text = $"(Speed x{mult:F1})";
            }
        }
    }
}
