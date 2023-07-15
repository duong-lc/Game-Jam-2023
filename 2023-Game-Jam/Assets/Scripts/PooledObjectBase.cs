using System;
using System.Collections;
using Core.Logging;
using UnityEngine;
using EventType = Core.Events.EventType;

public class PooledObjectCallbackData
{
    // public EventType eventType { get; protected set; }
    public Vector3 position { get; protected set; }
    public Transform parent { get; protected set; }
}


public class PooledObjectBase : MonoBehaviour, IPooledCore<PooledObjectBase>
{
    protected bool canRelease;
    protected Action<PooledObjectBase> KillAction;

    protected virtual void Start()
    {
        canRelease = false;
        // transform.position = new Vector3(999, 999, 999);
    }

    public virtual void Init(PooledObjectCallbackData data, Action<PooledObjectBase> killAction)
    {
        throw new NotImplementedException();
    }

    public virtual IEnumerator RunRoutine()
    {
        while (!canRelease) {
            yield return null;
        }
        // NCLogger.Log($"kill {gameObject.name}");
        KillAction(this);
        yield return null;
    }
}
