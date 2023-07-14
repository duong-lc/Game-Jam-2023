using System;
using System.Collections;
using System.Collections.Generic;

public interface IPooledCore<out T>
{
    void Init(PooledObjectCallbackData data, Action<T> killAction);
    IEnumerator RunRoutine();
}
