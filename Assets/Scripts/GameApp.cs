using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//游戏路口脚本
public class GameApp : MonoBehaviour
{
    void Start()
    {
        //初始化二进制存档系统（替代 PlayerPrefs，减少 JSON 序列化和 IO 开销）
        SaveFileManager.Load();

        //初始化资源缓存（预加载常用预制体，避免运行时 Resources.Load 重复 IO）
        ResourceCache.Init();

        //初始化对象池（减少 Instantiate/Destroy GC 压力）
        PoolManager.Init();

        //初始化配置表
        GameConfigManager.Instance.Init();

        //加载敌人ScriptableObject数据
        EnemyDataManager.Instance.LoadAllEnemyData();

        //初始化音频管理器
        AudioManager.Instance.Init();

        //初始化用户信息
        RoleManager.Instance.Init();
        RoleManager.Instance.ApplyUpgradesToDeck();

        //初始化药水（仅游戏启动一次）
        FightManager.Instance.InitPotions();

        //初始化遗物（仅游戏启动一次）
        FightManager.Instance.InitRelics();

        //显示loginUI 
        UIManager.Instance.ShowUI<LoginUI>("LoginUI");

        //播放BGM
        AudioManager.Instance.PlayBGM("bgm1");

    }
}
