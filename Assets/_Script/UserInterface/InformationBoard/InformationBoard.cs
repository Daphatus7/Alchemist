// Author : Peiyu Wang @ Daphatus
// 13 02 2025 02 00

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Script.UserInterface.InformationBoard
{
    /// <summary>
    /// Board that displays information in a queue.
    /// </summary>
    public sealed class InformationBoard : Singleton<InformationBoard>
    {
        private readonly Queue<InformationContext> _informationQueue = new Queue<InformationContext>();
    
        public event Action<InformationContext> onDisplayNewContext;
        public event Action<InformationContext> onRemoveContext;

        public Queue<InformationContext> InformationQueue => _informationQueue;
    
        private Coroutine _displayTimer;
        private float _nextDisplayTime;
    
        public void AddInformation(InformationContext information)
        {
            if (information == null)
            {
                Debug.LogWarning("Attempted to add a null InformationContext.");
                return;
            }

            _informationQueue.Enqueue(information);
        
            // If a context is already being displayed, simply enqueue the new context.
            if (_displayTimer != null) return;
        
            _nextDisplayTime = _informationQueue.Peek().DisplayTime;
            _displayTimer = StartCoroutine(DisplayTimer(_informationQueue.Dequeue()));
        }
    
    
        private IEnumerator DisplayTimer(InformationContext context)
        {
            // Display the context
            OnDisplayNewContext(context);
            yield return new WaitForSeconds(context.DisplayTime);
        
            // Remove the context
            OnRemoveContext(context);
        
            // Check if there is more information to display
            if (_informationQueue.Count > 0)
            {
                var contextToDisplay = _informationQueue.Dequeue();
                _nextDisplayTime = contextToDisplay.DisplayTime;
                _displayTimer = StartCoroutine(DisplayTimer(contextToDisplay));
            }
            else
            {
                _displayTimer = null;
            }
        }

        private void OnDisplayNewContext(InformationContext context)
        {
            onDisplayNewContext?.Invoke(context);
        }

        private void OnRemoveContext(InformationContext context)
        {
            onRemoveContext?.Invoke(context);
        }
    }


    [Serializable]
    public abstract class InformationContext
    {
        private string _informationText; public string InformationText => _informationText;
        private float _displayTime; public float DisplayTime => _displayTime;

        protected InformationContext(string informationText, float displayTime)
        {
            _informationText = informationText;
            _displayTime = displayTime;
        }
    }
    [Serializable]
    public class SimpleMessageContext : InformationContext
    {
        public SimpleMessageContext(string message, float displayTime) : base(message, displayTime)
        {
            
        }
    }
}