using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace _Script.Managers
{
    public class DayManager : PersistentSingleton<DayManager>
    {
        private int _day = 1; // Start at day 1
        private int _secondsInDay = 2; // How many seconds in a day
        private float _currentTime; // Current time in the day
        
        public int GetDay() => _day;

        public UnityEvent OnNewDay = new UnityEvent();

        private void Update()
        {
            UpdateDayProgress();
        }

        private void UpdateDayProgress()
        {
            _currentTime += Time.deltaTime;

            if (_currentTime >= _secondsInDay)
            {
                NextDay();
                _currentTime = 0; // Reset day timer
            }
        }

        public void NextDay()
        {
            _day++;
            OnNewDay.Invoke();
            //Debug.Log($"Day {_day} has started!");
        }

        public float GetTimeOfDay()
        {
            return _currentTime / _secondsInDay; // Return progress as a percentage (0 to 1)
        }

        public int GetRemainingSeconds()
        {
            return Mathf.CeilToInt(_secondsInDay - _currentTime); // Remaining seconds in the current day
        }
    }
}