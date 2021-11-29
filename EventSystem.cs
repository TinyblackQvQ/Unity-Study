using System;
using System.Collections.Generic;

namespace Self.EventSystem
{
    public interface IEvent
    {
        string Name { get; }
        bool IsEnabled { get; set; }
        void Enable();
        void Disable();
        void Active();
    }
    public class Event : IEvent
    {
        public string Name { get; protected set; }
        public bool IsEnabled { get; set; }
        public event EventHandler Listeners;
        public Event(string name)
        {
            this.Name = name;
            this.IsEnabled = true;
        }
        public void Enable() => this.IsEnabled = true;
        public void Disable() => this.IsEnabled = false;
        protected virtual void Invoke()
        {
            if (this.IsEnabled)
                Listeners(this, EventArgs.Empty);
        }
        public virtual void Active() => Invoke();
    }

    public class Event<T> : IEvent
    {
        public string Name { get; protected set; }
        public bool IsEnabled { get; set; }
        public event EventHandler<T> Listeners;
        public void Enable() => this.IsEnabled = true;
        public void Disable() => this.IsEnabled = false;
        public Event(string name)
        {
            this.Name = name;
            this.IsEnabled = true;
        }
        protected void Invoke(T args)
        {
            if(this.IsEnabled)
                this.Listeners(this, args);
        }
        protected void Invoke()
        {
            this.Invoke(default);
        }
        public virtual void Active() => this.Invoke();
        public virtual void Active(T args) => this.Invoke(args);
    }
    public interface IEventGroupBase
    { 
        string Name { get; }
        bool IsEnabled { get; set; }

        IEvent this[string index] { get; set; }

        void Enable();
        void Disable();
        void Active();
        void Active<T>(T data);
        void Add(IEvent e);
        void Add(IEvent[] e);
        void Add(EventGroup e);
        void Add(EventGroup[] e);
        bool RemoveEvent(IEvent e);
        bool RemoveEventGroup(EventGroup e);
        bool RemoveEventByName(string name);
        bool RemoveEventGroupByName(string name);
        IEvent GetEvent(string name);
        EventGroup GetEventGroup(string name);
        Event<T> GetEvent<T>(string name);
    }

    public class EventAlreadyExistException : Exception
    {
        public EventAlreadyExistException(string message) : base(message)
        {
        }
    }
    public class EventNotFoundException : Exception
    {
        public EventNotFoundException(string message) : base(message)
        {
        }
    }
    public class EventGroupNotFoundException : Exception
    {
        public EventGroupNotFoundException(string message) : base(message)
        {
        }
    }
    public class PathFormatException : Exception
    {
        public PathFormatException(string message) : base(message)
        {
        }
    }

    public class EventGroup : IEventGroupBase
    {
        public string Name { get; private set; }
        public Dictionary<string, IEvent> ChildEvent;
        public Dictionary<string, EventGroup> ChildGroup;
        public bool IsEnabled { get; set; }

        public EventGroup(string name)
        {
            this.ChildEvent = new Dictionary<string, IEvent>();
            this.ChildGroup = new Dictionary<string, EventGroup>();
            this.IsEnabled = true;
            this.Name = name;
        }
        public virtual void Active()
        {
            this.Invoke();
        }

        public void Active<T>(T data)
        {
            this.Invoke(data);
        }

        public void Add(IEvent e)
        {
            try
            {
                this.ChildEvent.Add(e.Name, e);
            }
            catch (ArgumentException)
            {
                throw (new EventAlreadyExistException("EventGroup " + this.Name + " already have the Event " + e.Name));
            }
            finally
            { }
        }

        public void Add(IEvent[] e)
        {
            foreach (IEvent item in e)
            {
                try
                {
                    this.ChildEvent.Add(item.Name, item);
                }
                catch (ArgumentException)
                {
                    throw (new EventAlreadyExistException("EventGroup " + this.Name + " already have the Event " + item.Name));
                }
                finally
                { }
            }
                
        }

        public void Add(EventGroup e)
        {
            try
            {
                this.ChildGroup.Add(e.Name, e);
            }
            catch (ArgumentException)
            {
                throw (new EventAlreadyExistException("EventGroup " + this.Name + " already have the Event " + e.Name));
            }
            finally
            { }
        }

        public void Add(EventGroup[] e)
        {
            foreach (EventGroup item in e)
            {
                try
                {
                    this.ChildGroup.Add(item.Name, item);
                }
                catch (ArgumentException)
                {
                    throw (new EventAlreadyExistException("EventGroup " + this.Name + " already have the Event " + item.Name));
                }
                finally
                { }
            }
        }

        public void Disable()
        {
            this.IsEnabled = false;
        }

        public void Enable()
        {
            this.IsEnabled = true;
        }

        public IEvent GetEvent(string name)
        {
            try
            {
                return this.ChildEvent[name];
            }
            catch (KeyNotFoundException)
            {
                throw(new EventNotFoundException("EventGroup " + this.Name + " don't have the Event " + name));
            }
        }

        public EventGroup GetEventGroup(string name)
        {
            try
            {
                return this.ChildGroup[name];
            }
            catch (KeyNotFoundException)
            {
                throw (new EventNotFoundException("EventGroup " + this.Name + " don't have the EventGroup " + name));
            }
        }

        public Event<T> GetEvent<T>(string name)
        {
            try
            {
                return this.ChildEvent[name] as Event<T>;
            }
            catch (KeyNotFoundException)
            {
                throw (new EventNotFoundException("EventGroup " + this.Name + " don't have the Event " + name));
            }
        }

        private void Invoke()
        {
            foreach (KeyValuePair<string, IEvent> item in ChildEvent)
                item.Value.Active();
            foreach (KeyValuePair<string, EventGroup> item in ChildGroup)
                item.Value.Active();
        }

        public void Invoke<T>(T data)
        {
            foreach (KeyValuePair<string, IEvent> item in ChildEvent)
            {
                if (item.Value.GetType() is Event<T>)
                {
                    var _ = item.Value as Event<T>;
                    _.Active(data);
                }
                else
                    item.Value.Active();
            }

            foreach (KeyValuePair<string, EventGroup> item in ChildGroup)
                item.Value.Active();
        }

        public bool RemoveEvent(IEvent e)
        {
            foreach (KeyValuePair<string, IEvent> item in ChildEvent)
                if (item.Value == e)
                    return ChildEvent.Remove(item.Value.Name);
            return false;
        }

        public bool RemoveEventByName(string name)
        {
            foreach (KeyValuePair<string, IEvent> item in ChildEvent)
                if (item.Value.Name == name)
                    return ChildEvent.Remove(item.Value.Name);
            return false;
        }

        public bool RemoveEventGroup(EventGroup e)
        {
            foreach (KeyValuePair<string, EventGroup> item in ChildGroup)
                if (item.Value == e)
                    return ChildEvent.Remove(item.Value.Name);
            return false;
        }

        public bool RemoveEventGroupByName(string name)
        {
            foreach (KeyValuePair<string, EventGroup> item in ChildGroup)
                if (item.Value.Name == name)
                    return ChildEvent.Remove(item.Value.Name);
            return false;
        }
        public IEvent this[string index]
        {
            get
            {
                return GetEvent(index);
            }
            set
            {
                ChildEvent[index] = value;
            }
        }
        public static EventGroup operator-(EventGroup e, string name)
        {
            return e.GetEventGroup(name);
        }
    }
    public class EventSystem
    {
        public static EventGroup Root = new EventGroup("Root");
        public IEvent this[string index]
        {
            get
            {
                string[] pathMap = index.Split('/');
                EventGroup i;
                i = Root;
                foreach (string item in pathMap)
                {
                    if (pathMap[pathMap.Length - 1] == item)
                    {
                        var t = pathMap[pathMap.Length - 1].Split(':');
                        return i.GetEventGroup(t[0]).GetEvent(t[1]);
                    }
                    try
                    {
                        i = i.GetEventGroup(item);
                    }
                    catch (EventGroupNotFoundException)
                    {
                        throw (new EventGroupNotFoundException("Event " + item + " can not be found."));
                    }
                    finally
                    { }
                }
                return default;
            }
            set
            {
                string[] pathMap = index.Split('/');
                IEventGroupBase i;
                i = Root;
                foreach (string item in pathMap)
                {
                    if (pathMap[pathMap.Length - 1] == item)
                    {
                        var t = pathMap[pathMap.Length - 1].Split(':');
                        i.GetEventGroup(t[0])[t[1]] = value;
                    }
                    i = i.GetEventGroup(item);
                }
            }
        }
    }
}