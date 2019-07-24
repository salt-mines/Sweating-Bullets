using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly List<GameObject> objectPool = new List<GameObject>(4);

    public int Capacity
    {
        get => objectPool.Capacity;
        set => objectPool.Capacity = value;
    }

    public int Count => objectPool.Count;

    public GameObject Prefab { get; set; }

    public Transform Parent { get; set; }

    public void Clear()
    {
        objectPool.Clear();
    }

    public void Fill()
    {
        var needed = Capacity - Count;
        for (var i = 0; i < needed; i++) objectPool.Add(CreateOne());
    }

    public GameObject GetOne()
    {
        foreach (var gameObject in objectPool)
            if (!gameObject.gameObject.activeSelf)
                return gameObject;

        var o = CreateOne();
        objectPool.Add(o);
        return o;
    }

    private GameObject CreateOne()
    {
        var o = Object.Instantiate(Prefab, Parent);
        o.SetActive(false);
        return o;
    }
}