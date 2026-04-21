using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    public float DeltaTime => deltaTime;
    public float LastDeltaBeforePause => lastDeltaBeforePause;
    public float DayDeltaTime => deltaTime / (DayLengthSeconds / 24f);
    public bool IsPaused { get => isPaused; set => SetPause(value); }
    public int CurrentDay => currentDay;
    public float GameTimeMultiplier = 1f;
    public float MinGameTimeMultiplier = 0.25f;
    public float MaxGameTimeMultiplier = 10f;
    public float DayLengthSeconds = 60f;
    public Light SunLight;
    public float DayBrightness = 1f;
    public float NightBrightness = 0.2f;

    private static readonly Logger log = new(nameof(TimeManager));
    private int currentDay = 1;
    private float dayProgress = 0f;
    private readonly List<TimedEvent> scheduledEvents = new();
    private bool isPaused;
    private float unpausedMultiplier = 1f;
    private float deltaTime = 0f;
    private float lastDeltaBeforePause = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SetTime(1, 6, 0, 0); // Start at day 1, 6.00
    }

    private void Update()
    {
        deltaTime = Time.deltaTime * GameTimeMultiplier;
        if (!isPaused)
        {
            lastDeltaBeforePause = deltaTime;
        }

        dayProgress += deltaTime / DayLengthSeconds;

        float newIntensity = Mathf.Lerp(NightBrightness, DayBrightness, Mathf.Sin(dayProgress * Mathf.PI * 2f - Mathf.PI / 2f) * 0.5f + 0.5f);
        log.Info($"Updating sun intensity to {newIntensity:F2} (Day Progress: {dayProgress:F2})");
        if (SunLight != null) SunLight.intensity = newIntensity;

        ProcessScheduledEvents();

        if (dayProgress >= 1f)
        {
            dayProgress %= 1f;
            currentDay++;
        }
    }

    private void OnValidate()
    {
        MinGameTimeMultiplier = Mathf.Max(0.1f, MinGameTimeMultiplier);
        MaxGameTimeMultiplier = Mathf.Max(MinGameTimeMultiplier, MaxGameTimeMultiplier);
        if (!isPaused)
        {
            GameTimeMultiplier = Mathf.Clamp(GameTimeMultiplier, MinGameTimeMultiplier, MaxGameTimeMultiplier);
        }
        DayLengthSeconds = Mathf.Max(1f, DayLengthSeconds);
    }

    public void StepGameTimeMultiplier(float step)
    {
        if (Mathf.Approximately(step, 0f))
            return;

        SetGameTimeMultiplier(GameTimeMultiplier + step);
    }

    public void SetGameTimeMultiplier(float newValue)
    {
        if (isPaused)
        {
            return;
        }

        GameTimeMultiplier = Mathf.Clamp(newValue, MinGameTimeMultiplier, MaxGameTimeMultiplier);
    }

    public void SetPause(bool pause)
    {
        if (isPaused == pause)
        {
            return;
        }

        isPaused = pause;

        if (isPaused)
        {
            unpausedMultiplier = Mathf.Clamp(GameTimeMultiplier, MinGameTimeMultiplier, MaxGameTimeMultiplier);
            GameTimeMultiplier = 0f;
        }
        else
        {
            GameTimeMultiplier = Mathf.Clamp(unpausedMultiplier, MinGameTimeMultiplier, MaxGameTimeMultiplier);
        }
    }

    public void ScheduleEvent(System.Action action, int day, int hour, int minute = 0, int second = 0)
    {
        day = Mathf.Max(1, day);
        float triggerDayProgress = ConvertTimeToDayProgress(hour, minute, second);
        TimedEvent newEvent = new(day, triggerDayProgress, action);

        // Insert event in sorted order
        int index = scheduledEvents.FindIndex(e => day < e.TriggerDay || (day == e.TriggerDay && triggerDayProgress < e.TriggerDayProgress));
        if (index == -1)
            scheduledEvents.Add(newEvent);
        else
            scheduledEvents.Insert(index, newEvent);

        log.Info($"Scheduled event for Day {day} at {hour:00}:{minute:00}:{second:00}");
    }

    public void SetTime(int day, int hour, int minute = 0, int second = 0)
    {
        currentDay = Mathf.Max(1, day);
        dayProgress = ConvertTimeToDayProgress(hour, minute, second);
        log.Info($"Time set to Day {currentDay} at {hour:00}:{minute:00}:{second:00}");
    }

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

    private float ConvertTimeToDayProgress(int hour, int minute, int second)
    {
        hour = Mathf.Clamp(hour, 0, 23);
        minute = Mathf.Clamp(minute, 0, 59);
        second = Mathf.Clamp(second, 0, 59);
        return (hour / 24f) + (minute / 1440f) + (second / 86400f);
    }

    private void ProcessScheduledEvents()
    {
        while (scheduledEvents.Count > 0)
        {
            TimedEvent nextEvent = scheduledEvents[0];
            bool isPastCurrentDay = currentDay > nextEvent.TriggerDay;
            bool isSameDayButPastTime = currentDay == nextEvent.TriggerDay && dayProgress >= nextEvent.TriggerDayProgress;

            if (isPastCurrentDay || isSameDayButPastTime)
            {
                log.Info($"Triggering event scheduled for Day {nextEvent.TriggerDay} at {Mathf.FloorToInt(nextEvent.TriggerDayProgress * 24f):00}:{Mathf.FloorToInt((nextEvent.TriggerDayProgress * 24f - Mathf.FloorToInt(nextEvent.TriggerDayProgress * 24f)) * 60f):00}");
                nextEvent.Action.Invoke();
                scheduledEvents.RemoveAt(0);
            }
            else
            {
                break;
            }
        }
    }

    private class TimedEvent
    {
        public int TriggerDay;
        public float TriggerDayProgress;
        public System.Action Action;

        public TimedEvent(int day, float dayProgress, System.Action action)
        {
            TriggerDay = day;
            TriggerDayProgress = dayProgress;
            Action = action;
        }
    }
}
