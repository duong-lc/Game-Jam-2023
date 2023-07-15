using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using EventType = Core.Events.EventType;

[Serializable]
public class PooledData
{
    public PooledObjectBase prefab;
    public int initialPoolSize = 10;
    public int maxPoolSize = 50;
}

[CreateAssetMenu(fileName = "PoolManagerData_ObjectType", menuName = "Pool Manager Data", order = 0)]
public class PoolManagerData : SerializedScriptableObject
{
    public Dictionary<EventType, PooledData> eventToPooledData = new();
}
