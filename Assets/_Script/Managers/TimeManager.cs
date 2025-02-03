using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Script.Managers
{
    [DefaultExecutionOrder(-600)]
    public class TimeManager : PersistentSingleton<TimeManager>, IPausable
    {
        [Header("Time Settings")]
        [SerializeField] private int _day = 1;             // Start at day 1
        [SerializeField] private int _dayLength = 10;        // Length of the "day" portion in seconds
        [SerializeField] private int _nightLength = 20;      // Length of the "night" portion in seconds

        private int _secondsInCycle;   // Total seconds in a full day/night cycle
        private float _currentTime;    // Current time into the cycle

        // Accumulator to track seconds during the night.
        private float _nightTickAccumulator = 0f;

        // Flag to trigger OnNightStart only once at the transition.
        private bool _hasTriggeredNightStart = false;

        // Pause flag (can be set by your pause system).
        private bool _isPaused = false;
        public void Pause(bool pause)
        {
            _isPaused = pause;
        }

        // Events
        public event Action OnNewDay;
        
        /// <summary>
        /// Triggered once at the moment the night begins.
        /// </summary>
        public event Action OnNightStart;
        
        /// <summary>
        /// Ticks every second during the night.
        /// </summary>
        public event Action OnUpdateNight;

        [Header("Visual Settings")]
        [SerializeField] private Image dayNightOverlay;  // Assign the UI Image used for day/night overlay
        [SerializeField] private Color dayOverlayColor = new Color(0f, 0f, 0f, 0f);
        [SerializeField] private Color nightOverlayColor = new Color(0f, 0f, 0f, 0.6f);

        // AnimationCurve to control how quickly darkness sets in.
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private void Start()
        {
            _secondsInCycle = _dayLength + _nightLength;
            UpdateOverlayColor(0f); // Initialize overlay (full day brightness)
        }

        private void Update()
        {
            // Do nothing if the game is paused.
            if (_isPaused)
                return;
            
            UpdateDayProgress();
            UpdateOverlay();

            // Check for transition to night: fire OnNightStart once when the day switches to night.
            if (IsNight() && !_hasTriggeredNightStart)
            {
                OnNightStart?.Invoke();
                _hasTriggeredNightStart = true;
            }
            else if (IsDayTime())
            {
                // Reset the flag when it's day so the next night transition can trigger OnNightStart again.
                _hasTriggeredNightStart = false;
            }

            // During the night, accumulate time and trigger OnUpdateNight once per second.
            if (IsNight())
            {
                _nightTickAccumulator += Time.deltaTime;
                if (_nightTickAccumulator >= 1f)
                {
                    int ticks = Mathf.FloorToInt(_nightTickAccumulator);
                    for (int i = 0; i < ticks; i++)
                    {
                        OnUpdateNight?.Invoke();
                    }
                    _nightTickAccumulator -= ticks;
                }
            }
            else
            {
                // Reset the accumulator during daytime.
                _nightTickAccumulator = 0f;
            }
        }

        private void UpdateDayProgress()
        {
            _currentTime += Time.deltaTime;
            
            // When the full day/night cycle is complete, trigger a new day.
            if (_currentTime >= _secondsInCycle)
            {
                NextDay();
                _currentTime = 0; // Reset the cycle timer.
            }
        }

        private bool IsDayTime()
        {
            return _currentTime < _dayLength;
        }
        
        public void NextDay()
        {
            _day++;
            OnNewDay?.Invoke();
        }

        public bool IsNight()
        {
            return !IsDayTime();
        }
        
        /// <summary>
        /// Returns a normalized value (0 to 1) indicating progress through the day/night cycle.
        /// 0 = start of day, 1 = end of night (just before the next day begins).
        /// </summary>
        public float GetTimeOfDay()
        {
            return _currentTime / _secondsInCycle;
        }

        public int GetDay()
        {
            return _day;
        }

        public int GetRemainingSeconds()
        {
            return Mathf.CeilToInt(_secondsInCycle - _currentTime);
        }

        private void UpdateOverlay()
        {
            float t = GetTimeOfDay();
            float dayFraction = (float)_dayLength / _secondsInCycle;
            // Calculate a blend factor for the night overlay.
            float nightBlend = Mathf.InverseLerp(dayFraction, 1f, t);
            // Smooth the blend with the transition curve.
            float curvedBlend = transitionCurve.Evaluate(nightBlend);
            UpdateOverlayColor(curvedBlend);
        }

        private void UpdateOverlayColor(float curvedBlend)
        {
            if (dayNightOverlay != null)
            {
                dayNightOverlay.color = Color.Lerp(dayOverlayColor, nightOverlayColor, curvedBlend);
            }
        }
    }
    
    public interface IPausable
    {
        void Pause(bool pause);
    }
}