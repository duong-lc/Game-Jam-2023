using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Core
{
    /// <summary>
    /// A simple base class to simplify object pooling in Unity 2021.
    /// Derive from this class, call InitPool and you can Get and Release
    /// </summary>
    /// <typeparam name="T">A MonoBehaviour object you'd like to perform pooling on.</typeparam>
    public abstract class PoolBase<T> : MonoBehaviour where T : MonoBehaviour 
    {
        private T _prefab;
        private ObjectPool<T> _pool;
        private Transform _parent;
        
        private ObjectPool<T> Pool {
            get {
                if (_pool == null) throw new InvalidOperationException("You need to call InitPool before using it.");
                return _pool;
            }
            set => _pool = value;
        }

        public void InitPool(T prefab, Transform parent, int initial = 10, int max = 20, bool collectionChecks = false) {
            _parent = !parent ? parent : transform.parent;
            _prefab = prefab;
            Pool = new ObjectPool<T>(
                CreateSetup,
                GetSetup,
                ReleaseSetup,
                DestroySetup,
                collectionChecks,
                initial,
                max);
        }

        #region Overrides
        public virtual T CreateSetup() => Instantiate(_prefab, _parent);
        protected virtual void GetSetup(T obj)
        {
            obj.gameObject.SetActive(true);
            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        protected virtual void ReleaseSetup(T obj) => obj.gameObject.SetActive(false);
        protected virtual void DestroySetup(T obj) => Destroy(obj);
        #endregion

        #region Getters
        public virtual T Get() => Pool.Get();
        public virtual void Release(T obj) => Pool.Release(obj);
        #endregion
    }
}

