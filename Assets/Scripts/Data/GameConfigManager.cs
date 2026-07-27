using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//整个游戏配置表的管理器
public class GameConfigManager
{
    public static GameConfigManager Instance = new GameConfigManager();

    //卡牌列表
    private List<CardData> cardDataList = new List<CardData>();
    private Dictionary<int, CardData> cardDataDict = new Dictionary<int, CardData>();

    //卡牌类型列表
    private List<CardTypeData> cardTypeList = new List<CardTypeData>();
    private Dictionary<int, CardTypeData> cardTypeDict = new Dictionary<int, CardTypeData>();

    //关卡表（保留原有逻辑）
    private GameConfigData levelData;
    private TextAsset textAsset;

    //初始化配置文件
    public void Init()
    {
        //加载所有卡牌数据
        LoadCardData();

        //加载所有卡牌类型
        LoadCardTypeData();

        //加载关卡数据（优先使用 ScriptableObject，兼容旧 txt）
        LevelConfigManager.Instance.Init();
        textAsset = Resources.Load<TextAsset>("Data/level");
        if (textAsset != null)
            levelData = new GameConfigData(textAsset.text);
    }

    //加载卡牌数据
    private void LoadCardData()
    {
        cardDataList.Clear();
        cardDataDict.Clear();

        CardData[] cards = Resources.LoadAll<CardData>("Date_Card/Card");
        foreach (CardData card in cards)
        {
            cardDataList.Add(card);
            if (!cardDataDict.ContainsKey(card.id))
            {
                cardDataDict.Add(card.id, card);
            }
            else
            {
                Debug.LogWarning($"CardData: 存在重复ID {card.id}，卡牌名称: {card.cardName}");
            }
        }
    }

    //加载卡牌类型数据
    private void LoadCardTypeData()
    {
        cardTypeList.Clear();
        cardTypeDict.Clear();

        CardTypeData[] types = Resources.LoadAll<CardTypeData>("Date_Card/CardType");
        foreach (CardTypeData type in types)
        {
            cardTypeList.Add(type);
            if (!cardTypeDict.ContainsKey(type.id))
            {
                cardTypeDict.Add(type.id, type);
            }
            else
            {
                Debug.LogWarning($"CardTypeData: 存在重复ID {type.id}，类型名称: {type.typeName}");
            }
        }
    }

    public List<CardData> GetCardDataList()
    {
        return cardDataList;
    }

    public List<CardTypeData> GetCardTypeList()
    {
        return cardTypeList;
    }

    public CardData GetCardById(int id)
    {
        if (cardDataDict.ContainsKey(id))
        {
            return cardDataDict[id];
        }
        return null;
    }

    //兼容旧接口
    public CardData GetCardById(string id)
    {
        int intId;
        if (int.TryParse(id, out intId))
        {
            return GetCardById(intId);
        }
        return null;
    }

    public CardTypeData GetCardTypeById(int id)
    {
        if (cardTypeDict.ContainsKey(id))
        {
            return cardTypeDict[id];
        }
        return null;
    }

    //兼容旧接口
    public CardTypeData GetCardTypeById(string id)
    {
        int intId;
        if (int.TryParse(id, out intId))
        {
            return GetCardTypeById(intId);
        }
        return null;
    }

    public List<Dictionary<string, string>> GetLevelLines()
    {
        return levelData.GetLines();
    }

    public Dictionary<string, string> GetLevelById(string id)
    {
        return levelData.GetOneById(id);
    }
}
