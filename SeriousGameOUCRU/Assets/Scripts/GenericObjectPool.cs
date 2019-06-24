using System.Collections.Generic;
using UnityEngine;

public abstract class GenericObjectPool<T> : MonoBehaviour where T : Component
{
    [SerializeField]
    private T prefab = null;
    [SerializeField]
    private int initCount = 0;

    private Queue<T> objects = new Queue<T>();

    /*** INSTANCE ***/

    public static GenericObjectPool<T> Instance { get; private set; }


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        Instance = this;
        AddObject(initCount);
    }

    public T Get()
    {
        if (objects.Count == 0)
            AddObject(1);
        return objects.Dequeue();
    }

    public void ReturnToPool(T objectToReturn)
    {
        objectToReturn.gameObject.SetActive(false);
        objects.Enqueue(objectToReturn);
    }

    private void AddObject(int count)
    {
        for (int i = 0 ; i < count; i++)
        {
            var newObject = GameObject.Instantiate(prefab);
            newObject.gameObject.SetActive(false);
            objects.Enqueue(newObject);
        }
    }
}
