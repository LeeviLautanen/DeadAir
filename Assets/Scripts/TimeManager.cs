using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public int CurrentDay => currentDay;

    private int currentDay = 1;
    private float dayProgress = 0f;

    public int GetHour()
    {
        return Mathf.FloorToInt(dayProgress * 24f);
    }

    public int GetMinute()
    {
        return Mathf.FloorToInt((dayProgress * 24f - GetHour()) * 60f);
    }

    public int GetSecond()
    {
        return Mathf.FloorToInt((((dayProgress * 24f - GetHour()) * 60f) - GetMinute()) * 60f);
    }

    private void Update()
    {
        dayProgress += Time.deltaTime / 60f; // 1 minute per day

        if (dayProgress >= 1f)
        {
            dayProgress = 0f;
            currentDay++;
        }
    }
}
