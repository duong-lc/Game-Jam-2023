using System;
using Core;
using Core.Logging;
using NoteClasses;
using UnityEngine;
using EventType = Core.Events.EventType;


public class ObjectPool : PoolBase<PooledObjectBase>
{
    private EventType _pooledObjectEventType;
    public EventType PooledObjectEventType {
        get => _pooledObjectEventType;
    }
        
    private PooledObjectBase _prefabRef;
    
    public void Init(PooledObjectBase prefab, EventType eventType)
    {
        _prefabRef = prefab;
        _pooledObjectEventType = eventType;
    }

    public override PooledObjectBase CreateSetup()
    {
        return Instantiate(_prefabRef, transform);
    }

    public override void Release(PooledObjectBase obj) {
        var note = obj as NoteBase;
        if(note != null) note.Collider.enabled = false;
        
        obj.transform.position = new Vector3(0, -999, 0);
        base.Release(obj);
    }

    public override PooledObjectBase Get()
    {
        var obj = base.Get();
        
        obj.transform.position = new Vector3(0, -999, 0);
        // foreach (Transform child in obj.transform) {
        //    child.position = new Vector3(0, -999, 0);
        // }
        // NCLogger.Log($"{obj.name} | {obj.transform.parent.parent} {obj.transform.position}");
        var note = obj as NoteBase;
        if (note != null)
        {
            // if(note as NoteSlider)Debug.Break();
            note.Collider.enabled = true;
        }
        return obj;
    } 
}
