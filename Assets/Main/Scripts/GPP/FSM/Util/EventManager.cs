using System.Collections.Generic;

public class GameEvent{}

public class EventManager
{
    private object _queueLock;

    public delegate void EventDelegate<T> (T e) where T : GameEvent;
    private delegate void EventDelegate (GameEvent e);

    public EventManager() {
        _queueLock = new object();
        _delegates = new Dictionary<System.Type, EventDelegate>();
        _delegate_lookup = new Dictionary<System.Delegate, EventDelegate>();
        _queuedEvents = new List<GameEvent>();
    }

    private readonly Dictionary<System.Type, EventDelegate> _delegates;
    private readonly Dictionary<System.Delegate, EventDelegate> _delegate_lookup;
    private readonly List<GameEvent> _queuedEvents;

    public void AddHandler<T> (EventDelegate<T> del) where T : GameEvent {
        if (_delegate_lookup.ContainsKey(del)) {
            return;
        }

        EventDelegate internalDelegate = (e) => del((T)e);
        _delegate_lookup[del] = internalDelegate;

        EventDelegate tempDel;
        if (_delegates.TryGetValue(typeof(T), out tempDel)) {
            _delegates[typeof(T)] = tempDel += internalDelegate;
        } else {
            _delegates[typeof(T)] = internalDelegate;
        }
    }

    public void RemoveHandler<T>(EventDelegate<T> del) where T : GameEvent {
        EventDelegate internalDelegate;
        if (_delegate_lookup.TryGetValue(del, out internalDelegate)) {
            EventDelegate tempDel;
            if (_delegates.TryGetValue(typeof(T), out tempDel)) {
                tempDel -= internalDelegate;
                if (tempDel == null) {
                    _delegates.Remove(typeof(T));
                } else {
                    _delegates[typeof(T)] = tempDel;
                }
            }
            _delegate_lookup.Remove(del);
        }
    }

    public void Clear() {
        if (_queueLock != null) {
            lock (_queueLock) {
                if (_delegates != null) _delegates.Clear();
                if (_delegate_lookup != null) _delegate_lookup.Clear();
                if (_queuedEvents != null) _queuedEvents.Clear();
            }
            _queueLock = null;
        } else {
            if (_delegates != null) _delegates.Clear();
            if (_delegate_lookup != null) _delegate_lookup.Clear();
            if (_queuedEvents != null) _queuedEvents.Clear();
        }
    }

    public void Fire(GameEvent e) {
        EventDelegate del;
        if (_delegates.TryGetValue(e.GetType(), out del)) {
            del.Invoke(e);
        }
    }

    public void ProcessQueuedEvents() {
        List<GameEvent> events;
        lock (_queueLock)
        {
            if (_queuedEvents.Count > 0) {
                events = new List<GameEvent>(_queuedEvents);
                _queuedEvents.Clear();
            } else {
                return;
            }
        }
        foreach(var e in events) {
            Fire(e);
        }
    }

    public void Queue(GameEvent e) {
        lock (_queueLock)
        {
            _queuedEvents.Add(e);
        }
    }

}