using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 通用 GameObject 对象池
/// 用 Get()/Release() 替代 Instantiate/Destroy，减少 GC
/// </summary>
public class ObjectPool
{
    private readonly Queue<GameObject> _pool = new Queue<GameObject>();
    private readonly GameObject _prefab;
    private readonly Transform _parent;
    private readonly int _preloadCount;

    /// <summary>池名（用于调试）</summary>
    public string Name { get; }

    public ObjectPool(GameObject prefab, Transform parent, int preloadCount = 5, string name = null)
    {
        _prefab = prefab;
        _parent = parent;
        _preloadCount = preloadCount;
        Name = name ?? prefab.name;

        for (int i = 0; i < preloadCount; i++)
        {
            GameObject obj = CreateNew();
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    private GameObject CreateNew()
    {
        GameObject obj = Object.Instantiate(_prefab, _parent);
        obj.name = _prefab.name;
        return obj;
    }

    /// <summary>
    /// 从池中取出一个对象（若池空或对象已销毁则创建新对象）
    /// </summary>
    public GameObject Get()
    {
        GameObject obj = null;
        while (_pool.Count > 0)
        {
            obj = _pool.Dequeue();
            if (obj != null) break;
        }

        if (obj == null)
            obj = CreateNew();

        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// 归还对象到池中
    /// </summary>
    public void Release(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        obj.transform.SetParent(_parent, false); // 归还到池根节点，防止场景切换时被销毁
        _pool.Enqueue(obj);
    }

    /// <summary>
    /// 延迟归还对象到池中
    /// </summary>
    public void ReleaseDelayed(GameObject obj, float delay)
    {
        if (obj == null) return;
        // 使用临时协程宿主
        var host = obj.GetComponent<PooledObject>() ?? obj.AddComponent<PooledObject>();
        host.StartCoroutine(ReleaseAfterDelay(obj, delay));
    }

    private System.Collections.IEnumerator ReleaseAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Release(obj);
    }

    /// <summary>
    /// 清空池中所有对象
    /// </summary>
    public void Clear()
    {
        while (_pool.Count > 0)
        {
            GameObject obj = _pool.Dequeue();
            if (obj != null) Object.Destroy(obj);
        }
    }

    /// <summary>池中可用对象数</summary>
    public int AvailableCount => _pool.Count;
}

/// <summary>
/// 挂到预制体上，支持延迟归还的协程宿主
/// </summary>
public class PooledObject : MonoBehaviour
{
    /// <summary>对象所属的池（归还时使用）</summary>
    public ObjectPool Pool { get; set; }

    /// <summary>归还到池中</summary>
    public void ReturnToPool()
    {
        if (Pool != null)
            Pool.Release(gameObject);
        else
            gameObject.SetActive(false);
    }

    /// <summary>延迟归还</summary>
    public void ReturnToPoolDelayed(float delay)
    {
        if (Pool != null)
            Pool.ReleaseDelayed(gameObject, delay);
        else
            StartCoroutine(ReturnAfterDelay(delay));
    }

    private System.Collections.IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
