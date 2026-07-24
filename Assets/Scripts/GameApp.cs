using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//游戏路口脚本
public class GameApp : MonoBehaviour
{
    void Start()
    {
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
