using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtilitiesCode.GameSystems
{
    /// <summary>
    /// This is a pooling system that can be used by any other class in the project, should be attached to a game object 
    /// </summary>
    public class PoolingSystem : MonoBehaviour
    {
        private static PoolingSystem instance_;
        public static PoolingSystem Instance => instance_;

        private Dictionary<string, Queue<GameObject>> objectPool = new Dictionary<string, Queue<GameObject>>();

        public struct ObjectPoolProperties
        {
            public GameObject obj;
            public Transform parent;
            public Vector3 position;
            public Quaternion rotation;
            public bool activeState;
        }

        [Serializable]
        private struct ObjectsToSpawn
        {
            public GameObject obj;
            public int count;
        }

        [SerializeField,
         Tooltip("Should we setup and object pool at the start? If set to true 'initialSpawnList' should be setup")]
        private bool setupInitialPool;
        [SerializeField, Tooltip("List of objects that should be setup at the start")]
        private List<ObjectsToSpawn> initialSpawnList = new List<ObjectsToSpawn>();

        private void Awake()
        {
            instance_ = this;
            if (setupInitialPool) InitialPoolSetup();
        }

        /// <summary>
        /// This function sets up an object pool based on 'initialSpawnList' at the start if 'setupInitialPool' is true
        /// </summary>
        private void InitialPoolSetup()
        {
            foreach (var member in initialSpawnList)
            {
                for (int i = 0; i < member.count; i++)
                {
                    var o = Instantiate(member.obj, transform, true);
                    o.name = member.obj.name;
                    o.SetActive(false);
                    AddObjectToPool(o);
                }
            }
        }

        /// <summary>
        /// This function spawns an object if there's one available and if not instantiates a new once
        /// </summary>
        /// <param name="properties"> Properties of the object that needs to be spawned </param>
        /// <returns></returns>
        public GameObject Spawn(ObjectPoolProperties properties)
        {
            GameObject o;
            if (objectPool.TryGetValue(properties.obj.name, out Queue<GameObject> list))
            {
                o = list.Count > 0 ? list.Dequeue() : InstantiateNewObject(properties.obj);
                SetObjectProperties();
                return o;
            }

            o = InstantiateNewObject(properties.obj);
            SetObjectProperties();
            return o;

            void SetObjectProperties()
            {
                o.transform.SetPositionAndRotation(properties.position, properties.rotation);
                o.transform.SetParent(properties.parent);
                o.SetActive(properties.activeState);
            }
        }

        /// <summary>
        /// Instantiates the passed game object in case we don't have that object in pool
        /// </summary>
        /// <param name="obj"> The object that should be instantiated </param>
        /// <returns></returns>
        private GameObject InstantiateNewObject(GameObject obj)
        {
            var o = Instantiate(obj);
            o.name = obj.name;
            return o;
        }

        /// <summary>
        /// Adds the passed game object to the object pool
        /// </summary>
        /// <param name="obj"> The object that should be added to the object pool </param>
        private void AddObjectToPool(GameObject obj)
        {
            if (objectPool.TryGetValue(obj.name, out Queue<GameObject> list)) list.Enqueue(obj);
            else
            {
                var newList = new Queue<GameObject>();
                newList.Enqueue(obj);
                objectPool.Add(obj.name, newList);
            }
        }

        /// <summary>
        /// Deactivates and returns the passed object into the object pool
        /// </summary>
        /// <param name="obj"> The object that should be returned to the object pool </param>
        public void ReturnObjectToPool(GameObject obj)
        {
            AddObjectToPool(obj);
            obj.transform.SetParent(transform);
            obj.SetActive(false);
        }
    }
}