using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Patterns;
using Data_Classes;
using Managers;
using StaticClass;
using UnityEngine;
using EventType = Core.Events.EventType;


public class PoolingManager : Singleton<PoolingManager>
{
    [SerializeField] private PoolManagerData alwaysLoaded_PoolData;
    [SerializeField] private PoolManagerData sceneBased_NotesData;
    [SerializeField] private PoolManagerData sceneBased_HitMarksData;

    //private Dictionary<EventType, ObjectPool> _eventToPool = new();
    private Dictionary<EventType, PooledData> _eventToPooledData = new();
    
    private void Awake() {
        _eventToPooledData = _eventToPooledData.AddRange(alwaysLoaded_PoolData.eventToPooledData)
            .AddRange(sceneBased_NotesData.eventToPooledData).AddRange(sceneBased_HitMarksData.eventToPooledData);
        
        
        foreach(var kvp in _eventToPooledData){
            this.AddListener(kvp.Key, param => SpawnPooledObject(kvp.Key, (PooledObjectCallbackData) param));
        }
    }

    private void Start()
    {
        //Init For all note types in lane
        foreach (var kvp in sceneBased_NotesData.eventToPooledData) {
            foreach (var lane in LaneManager.Instance.LaneArray) {
                SetUpPool(kvp, lane.transform);
            }
        }

        foreach (var kvp in sceneBased_HitMarksData.eventToPooledData) {
            SetUpPool(kvp, ScoreManager.Instance.transform);
        }
    }

    private void SpawnPooledObject(EventType eventType, PooledObjectCallbackData data)
    {
        switch (eventType)
        {
            case EventType.SpawnNoteNormalGround:
                var normalData1 = (NoteInitData) data;
                SpawnNote(eventType, normalData1.orientation, data);
                break;
            case EventType.SpawnNoteNormalAir:
                var normalData2 = (NoteInitData) data;
                SpawnNote(eventType, normalData2.orientation, data);
                break;
            case EventType.SpawnNoteSlider:
                var sliderData = (NoteInitData) data;
                SpawnNote(eventType, sliderData.orientation, data);
                break;
            case EventType.NoteMissEvent:
                SpawnHitMark(eventType, data);
                break;
            case EventType.NoteHitEarlyEvent:
                SpawnHitMark(eventType, data);
                break;
            case EventType.NoteHitPerfectEvent:
                SpawnHitMark(eventType, data);
                break;
            case EventType.NoteHitLateEvent:
                SpawnHitMark(eventType, data);
                break;
            // default:
            //     throw new ArgumentOutOfRangeException();
        }
    }

    private void SpawnNote(EventType eventType, NoteData.LaneOrientation orientation, PooledObjectCallbackData data)
    {
        foreach (var lane in LaneManager.Instance.LaneArray) {
            if (lane.LaneOrientation != orientation) continue;
            foreach (var pool in lane.NotePools) {
                if (pool.PooledObjectEventType != eventType) continue;
                var obj = pool.Get();
                obj.Init(data, pool.Release);
            }
        }
    }

    private void SpawnHitMark(EventType eventType, PooledObjectCallbackData data)
    {
        foreach (var pool in ScoreManager.Instance.NotePools) {
            if (pool.PooledObjectEventType != eventType) continue;
            var obj = pool.Get();
            obj.Init(data, pool.Release);
        }
    }

    private void SetUpPool(KeyValuePair<EventType, PooledData> kvp, Transform parent) 
    {
        GameObject poolGO = new GameObject();
        poolGO.transform.position = new Vector3(0, -999, 0);
        poolGO.transform.parent = parent;
        poolGO.name = $"Pool {kvp.Value.prefab.name}";
        
        var pool = poolGO.AddComponent<ObjectPool>();
        pool.InitPool(kvp.Value.prefab, parent, kvp.Value.initialPoolSize, kvp.Value.maxPoolSize);
        pool.Init(kvp.Value.prefab, kvp.Key);
        for (var i = 0; i < kvp.Value.initialPoolSize; i++) {
            var obj = pool.CreateSetup();
            obj.transform.SetParent(poolGO.transform, false);
            pool.Release(obj);
        }
    }
}
