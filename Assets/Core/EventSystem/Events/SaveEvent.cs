using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    // Base interface for save events
    public interface ISaveEvent : IEvent { }

    // Lifecycle: Before writing to disk (Sync your state to SaveManager here)
    public struct BeforeWriteSaveEvent : ISaveEvent
    {
        public string SlotName;
        public bool IsGlobal;

        public BeforeWriteSaveEvent(string slotName, bool isGlobal = false)
        {
            SlotName = slotName;
            IsGlobal = isGlobal;
        }
    }

    // Lifecycle: After writing to disk
    public struct AfterWriteSaveEvent : ISaveEvent
    {
        public string SlotName;
        public bool IsGlobal;
        public bool Success;

        public AfterWriteSaveEvent(string slotName, bool success, bool isGlobal = false)
        {
            SlotName = slotName;
            Success = success;
            IsGlobal = isGlobal;
        }
    }

    // Lifecycle: Before loading from disk
    public struct BeforeLoadSaveEvent : ISaveEvent
    {
        public string SlotName;
        public bool IsGlobal;

        public BeforeLoadSaveEvent(string slotName, bool isGlobal = false)
        {
            SlotName = slotName;
            IsGlobal = isGlobal;
        }
    }

    // Lifecycle: Data is loaded into memory, objects should restore state
    public struct OnLoadSaveEvent : ISaveEvent
    {
        public SaveContainer Container;
        public bool IsGlobal;

        public OnLoadSaveEvent(SaveContainer container, bool isGlobal = false)
        {
            Container = container;
            IsGlobal = isGlobal;
        }
    }

    // Lifecycle: After loading process is fully complete
    public struct AfterLoadSaveEvent : ISaveEvent
    {
        public string SlotName;
        public bool IsGlobal;
        public bool Success;

        public AfterLoadSaveEvent(string slotName, bool success, bool isGlobal = false)
        {
            SlotName = slotName;
            Success = success;
            IsGlobal = isGlobal;
        }
    }

    // Notification: A specific value was updated in memory
    public struct SaveValueUpdatedEvent : ISaveEvent
    {
        public string Key;
        public object Value;
        public bool IsGlobal;

        public SaveValueUpdatedEvent(string key, object value, bool isGlobal = false)
        {
            Key = key;
            Value = value;
            IsGlobal = isGlobal;
        }
    }

    // External trigger to request a save
    public struct SaveRequestEvent : ISaveEvent
    {
        public string SlotName;
        public bool IsGlobal;

        public SaveRequestEvent(string slotName = null, bool isGlobal = false)
        {
            SlotName = slotName;
            IsGlobal = isGlobal;
        }
    }

    // Keep compatibility for now or remove if not needed
    public struct SaveEvent : IEvent
    {
        public SaveEvent(string slot, bool success)
        {
            Slot = slot;
            Success = success;
        }

        public string Slot { get; }
        public bool Success { get; }
    }

    public struct LoadEvent : IEvent
    {
        public LoadEvent(string slot, bool success)
        {
            Slot = slot;
            Success = success;
        }

        public string Slot { get; }
        public bool Success { get; }
    }
}
