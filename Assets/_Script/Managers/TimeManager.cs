using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Script.Managers
{
    public class TimeManager : PersistentSingleton<TimeManager>
    {
        [Header("Time Settings")]
        [SerializeField] private int _day = 1; // Start at day 1
        [SerializeField] private int _dayLength = 10;   // length of the "day" portion in seconds
        [SerializeField] private int _nightLength = 20; // length of the "night" portion in seconds

        private int _secondsInCycle;   // total seconds in a full day/night cycle
        private float _currentTime;    // current time into the cycle

        // Events
        public UnityEvent onNewDay = new UnityEvent();

        public UnityEvent onNightStart = new UnityEvent();
        
        [Header("Visual Settings")]
        [SerializeField] private Image dayNightOverlay;    // Assign the UI Image used for day/night overlay
        [SerializeField] private Color dayOverlayColor = new Color(0f, 0f, 0f, 0f);
        [SerializeField] private Color nightOverlayColor = new Color(0f, 0f, 0f, 0.6f);

        // Use an AnimationCurve to control how quickly darkness sets in.
        // For example, an ease-in curve will keep things brighter for longer, then gradually ramp up darkness.
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        // You can adjust this curve in the Unity Inspector:
        // - Key at (0,0): Start fully bright at day start.
        // - Key at (1,1): Fully dark at the end of the cycle.
        // Add more keys or modify tangents for a more "immersive" feel.

        private void Start()
        {
            _secondsInCycle = _dayLength + _nightLength;
            UpdateOverlayColor(0f); // Initialize overlay at the start of the cycle (full day)
        }

        private void Update()
        {
            UpdateDayProgress();
            UpdateOverlay();
        }

        private void UpdateDayProgress()
        {
            _currentTime += Time.deltaTime;
            
            if (IsNight())
            {
                onNightStart.Invoke();
            }
            
            // If we've passed a full cycle (day + night):
            if (_currentTime >= _secondsInCycle)
            {
                NextDay();
                _currentTime = 0; // Reset cycle timer
            }
        }

        private bool IsDayTime()
        {
            return _currentTime < _dayLength;
        }
        
        

        public void NextDay()
        {
            _day++;
            onNewDay.Invoke();
        }

        public bool IsNight()
        {
            return !IsDayTime();
        }
        
        /// <summary>
        /// Returns a normalized value (0 to 1) indicating how far we are into the full day/night cycle.
        /// 0 = start of day
        /// 1 = end of night (just before next day starts)
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

            // For t < dayFraction: in the "day" portion (more bright)
            // For t > dayFraction: in the "night" portion (more dark)

            float nightBlend = Mathf.InverseLerp(dayFraction, 1f, t);

            // Instead of a direct Lerp, we use the transitionCurve to smooth the blending:
            float curvedBlend = transitionCurve.Evaluate(nightBlend);

            UpdateOverlayColor(curvedBlend);
        }

        private void UpdateOverlayColor(float curvedBlend)
        {
            // Lerp between dayOverlayColor and nightOverlayColor based on curvedBlend
            if (dayNightOverlay != null)
            {
                dayNightOverlay.color = Color.Lerp(dayOverlayColor, nightOverlayColor, curvedBlend);
            }
        }
    }
}
