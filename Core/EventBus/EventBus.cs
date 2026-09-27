using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra2DAction
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _listeners
            = new Dictionary<Type, List<Delegate>>();

        private static EventDebugger _debugger;

        public static void Subscribe<T>(Action<T> callback) where T : BaseEvent
        {
            Type eventType = typeof(T);
            if (!_listeners.ContainsKey(eventType))
                _listeners[eventType] = new List<Delegate>();
            if (!_listeners[eventType].Contains(callback))
                _listeners[eventType].Add(callback);

#if UNITY_EDITOR
            Debug.Log($"[EVENTBUS] Subscribed: {typeof(T).Name}");
#endif
        }

        public static void Unsubscribe<T>(Action<T> callback) where T : BaseEvent
        {
            Type eventType = typeof(T);
            if (_listeners.ContainsKey(eventType))
                _listeners[eventType].Remove(callback);
#if UNITY_EDITOR
            Debug.Log($"[EVENTBUS] Unsubscribed: {typeof(T).Name}");
#endif
        }

        public static void Raise<T>(T eventData) where T : BaseEvent
        {
            if (eventData == null)
            {
                Debug.LogError("[EVENTBUS] Intentaste lanzar un evento NULL.");
                return;
            }

            Type eventType = typeof(T);
            _debugger?.RegisterEvent(eventType.Name, eventData.SourceID, GetListenerCount<T>());
#if UNITY_EDITOR
            Debug.Log($"[EVENTBUS] Raised: {eventType.Name} | Source: {eventData.SourceID}");
#endif
            if (!_listeners.ContainsKey(eventType) || _listeners[eventType].Count == 0)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[EVENTBUS] No listeners for: {eventType.Name}");
#endif
                return;
            }

            var snapshot = new List<Delegate>(_listeners[eventType]);

            foreach (var listener in snapshot)
            {
                try
                {
                    ((Action<T>)listener).Invoke(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EVENTBUS] Error en listener de {eventType.Name}: {ex.Message}");
                }
            }
        }

        public static void RegisterDebugger(EventDebugger debugger)
        {
            _debugger = debugger;
        }

        public static int GetListenerCount<T>() where T : BaseEvent
        {
            Type eventType = typeof(T);
            return _listeners.ContainsKey(eventType) ? _listeners[eventType].Count : 0;
        }

        public static void Clear()
        {
            _listeners.Clear();
#if UNITY_EDITOR
            Debug.Log("[EVENTBUS] Listeners cleared.");
#endif
        }

        public static void PrintStatus()
        {
            Debug.Log("===== EVENTBUS STATUS =====");
            if (_listeners.Count == 0)
            {
                Debug.Log("No listeners registered.");
                return;
            }
            foreach (var kvp in _listeners)
                Debug.Log($"  {kvp.Key.Name} -> {kvp.Value.Count} listener(s)");
            Debug.Log("===========================");
        }
    }
}
