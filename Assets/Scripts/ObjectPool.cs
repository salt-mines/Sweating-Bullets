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

    /// <summary>
    ///     Prefab the pool's objects are based on.
    /// </summary>
    public GameObject Prefab { get; set; }

    /// <summary>
    ///     Game object under which created objects should be placed.
    /// </summary>
    public Transform Parent { get; set; }

    /// <summary>
    ///     Clear pool.
    /// </summary>
    public void Clear()
    {
        objectPool.Clear();
    }

    /// <summary>
    ///     Fill pool up to capacity.
    /// </summary>
    public void Fill()
    {
        var needed = Capacity - Count;
        for (var i = 0; i < needed; i++) objectPool.Add(CreateOne());
    }

    /// <summary>
    ///     Get a free inactive object, or creates one if none available.
    /// </summary>
    /// <returns></returns>
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