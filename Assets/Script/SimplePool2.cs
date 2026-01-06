using UnityEngine;
using System.Collections.Generic;

public static class SimplePool2
{






    private const int DEFAULT_POOL_SIZE = 3;




    public class Pool
    {


        private int _nextId = 1;






        private readonly Queue<GameObject> _inactive;



        public readonly HashSet<int> MemberIDs;


        private readonly GameObject _prefab;

        public int StackCount
        {
            get { return _inactive.Count; }
        }


        public Pool(GameObject prefab, int initialQty)
        {
            _prefab = prefab;



            _inactive = new Queue<GameObject>(initialQty);
            MemberIDs = new HashSet<int>();
        }

        public void Preload(int initialQty, Transform parent = null)
        {
            for (int i = 0; i < initialQty; i++)
            {

                var obj = GameObject.Instantiate(_prefab, parent);
                obj.name = string.Format("{0} ({1})", _prefab.name, _nextId++);


                MemberIDs.Add(obj.GetInstanceID());

                obj.SetActive(false);

                _inactive.Enqueue(obj);
            }
        }


        public GameObject Spawn(Vector3 pos, Quaternion rot)
        {
            while (true)
            {
                GameObject obj;
                if (_inactive.Count == 0)
                {


                    obj = GameObject.Instantiate(_prefab, pos, rot);
                    obj.name = string.Format("{0} ({1})", _prefab.name, _nextId++);


                    MemberIDs.Add(obj.GetInstanceID());
                }
                else
                {

                    obj = _inactive.Dequeue();

                    if (obj == null)
                    {








                        continue;
                    }
                }
                obj.transform.position = pos;
                obj.transform.rotation = rot;
                obj.SetActive(true);
                return obj;
            }
        }

        public T Spawn<T>(Vector3 pos, Quaternion rot)
        {
            return Spawn(pos, rot).GetComponent<T>();
        }






        public void Despawn(GameObject obj)
        {
            if (!obj.activeSelf)
                return;
            obj.SetActive(false);





            _inactive.Enqueue(obj);
        }
    }


    public static Dictionary<int, Pool> _pools;




    private static void Init(GameObject prefab = null, int qty = DEFAULT_POOL_SIZE)
    {
        if (_pools == null)
            _pools = new Dictionary<int, Pool>();

        if (prefab != null)
        {


            var prefabID = prefab.GetInstanceID();
            if (!_pools.ContainsKey(prefabID))
                _pools[prefabID] = new Pool(prefab, qty);
        }
    }

    public static void PoolPreLoad(GameObject prefab, int qty, Transform newParent = null)
    {
        Init(prefab, 1);
        _pools[prefab.GetInstanceID()].Preload(qty, newParent);
    }









    public static GameObject[] Preload(GameObject prefab, int qty = 1, Transform newParent = null)
    {
        Init(prefab, qty);

        var obs = new GameObject[qty];
        for (int i = 0; i < qty; i++)
        {
            obs[i] = Spawn(prefab, Vector3.zero, Quaternion.identity);
            if (newParent != null)
                obs[i].transform.SetParent(newParent);
        }


        for (int i = 0; i < qty; i++)
            Despawn(obs[i]);
        return obs;
    }









    public static GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        Init(prefab);

        return _pools[prefab.GetInstanceID()].Spawn(pos, rot);
    }

    public static GameObject Spawn(GameObject prefab)
    {
        return Spawn(prefab, Vector3.zero, Quaternion.identity);
    }

    public static T Spawn<T>(T prefab) where T : Component
    {
        return Spawn(prefab, Vector3.zero, Quaternion.identity);
    }

    public static T Spawn<T>(T prefab, Vector3 pos, Quaternion rot) where T : Component
    {
        Init(prefab.gameObject);
        return _pools[prefab.gameObject.GetInstanceID()].Spawn<T>(pos, rot);
    }




    public static void Despawn(GameObject obj)
    {
        Pool p = null;
        foreach (var pool in _pools.Values)
        {
            if (pool.MemberIDs.Contains(obj.GetInstanceID()))
            {
                p = pool;
                break;
            }
        }

        if (p == null)
        {
            Debug.LogFormat("Object '{0}' wasn't spawned from a pool. Destroying it instead.", obj.name);
            Object.Destroy(obj);
        }
        else
        {
            p.Despawn(obj);
        }
    }

    public static int GetStackCount(GameObject prefab)
    {
        if (_pools == null)
            _pools = new Dictionary<int, Pool>();
        if (prefab == null) return 0;
        return _pools.ContainsKey(prefab.GetInstanceID()) ? _pools[prefab.GetInstanceID()].StackCount : 0;
    }

    public static void ClearPool()
    {
        if (_pools != null)
        {
            _pools.Clear();
        }
    }
}