using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// 二进制存档文件管理器
/// 用一个二进制文件替代所有 PlayerPrefs 调用，减少 JSON 序列化和 IO 开销
/// 
/// 文件格式:
///   [int: entryCount]
///   for each entry:
///     [int: keyLength] [bytes: key]
///     [byte: type] (0=Int, 1=String)
///     [int: value]  or [int: valueLength][bytes: value]
/// </summary>
public static class SaveFileManager
{
    private static readonly string FilePath = Path.Combine(Application.persistentDataPath, "gamesave.bin");
    private static readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
    private static bool _loaded = false;

    #region 初始化

    /// <summary>从磁盘加载所有数据到内存缓存</summary>
    public static void Load()
    {
        _cache.Clear();
        _loaded = true;

        if (!File.Exists(FilePath)) return;

        try
        {
            using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs, Encoding.UTF8))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    int keyLen = reader.ReadInt32();
                    string key = Encoding.UTF8.GetString(reader.ReadBytes(keyLen));
                    byte type = reader.ReadByte();

                    if (type == 0) // Int
                    {
                        _cache[key] = reader.ReadInt32();
                    }
                    else // String
                    {
                        int valLen = reader.ReadInt32();
                        _cache[key] = Encoding.UTF8.GetString(reader.ReadBytes(valLen));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveFileManager.Load 失败: {e.Message}");
            _cache.Clear();
        }
    }

    /// <summary>将内存缓存写入磁盘</summary>
    public static void Save()
    {
        if (!_loaded) return;

        try
        {
            using (var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs, Encoding.UTF8))
            {
                writer.Write(_cache.Count);
                foreach (var kvp in _cache)
                {
                    byte[] keyBytes = Encoding.UTF8.GetBytes(kvp.Key);
                    writer.Write(keyBytes.Length);
                    writer.Write(keyBytes);

                    if (kvp.Value is int intVal)
                    {
                        writer.Write((byte)0);
                        writer.Write(intVal);
                    }
                    else if (kvp.Value is string strVal)
                    {
                        writer.Write((byte)1);
                        byte[] valBytes = Encoding.UTF8.GetBytes(strVal);
                        writer.Write(valBytes.Length);
                        writer.Write(valBytes);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveFileManager.Save 失败: {e.Message}");
        }
    }

    #endregion

    #region Int 存取

    public static int GetInt(string key, int defaultValue = 0)
    {
        if (!_loaded) Load();
        return _cache.TryGetValue(key, out object val) && val is int i ? i : defaultValue;
    }

    public static void SetInt(string key, int value)
    {
        if (!_loaded) Load();
        _cache[key] = value;
    }

    #endregion

    #region String 存取

    public static string GetString(string key, string defaultValue = "")
    {
        if (!_loaded) Load();
        return _cache.TryGetValue(key, out object val) && val is string s ? s : defaultValue;
    }

    public static void SetString(string key, string value)
    {
        if (!_loaded) Load();
        _cache[key] = value;
    }

    #endregion

    #region 辅助方法

    public static bool HasKey(string key)
    {
        if (!_loaded) Load();
        return _cache.ContainsKey(key);
    }

    public static void DeleteKey(string key)
    {
        if (!_loaded) Load();
        _cache.Remove(key);
    }

    /// <summary>删除所有以 prefix 开头的 Key</summary>
    public static void DeleteKeysByPrefix(string prefix)
    {
        if (!_loaded) Load();
        var toRemove = new List<string>();
        foreach (var key in _cache.Keys)
            if (key.StartsWith(prefix)) toRemove.Add(key);
        foreach (var key in toRemove) _cache.Remove(key);
    }

    /// <summary>清除所有数据</summary>
    public static void ClearAll()
    {
        _cache.Clear();
        if (File.Exists(FilePath))
        {
            try { File.Delete(FilePath); } catch { }
        }
    }

    /// <summary>强制保存并关闭（在 OnApplicationQuit 中调用）</summary>
    public static void Flush()
    {
        Save();
    }

    #endregion
}
