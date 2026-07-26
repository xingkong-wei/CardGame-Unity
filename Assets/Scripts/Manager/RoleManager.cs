using UnityEngine;
using System.Collections.Generic;

//用户信息管理器（拥有卡牌信息，金币信息等）
public class RoleManager
{
    private static RoleManager instance;
    public static RoleManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new RoleManager();
                instance.LoadUpgradedCards();
            }
            return instance;
        }
    }

    public List<DeckCard> cardList;//存储拥有的卡牌（每张独立实例）
    public List<int> upgradedCardIds = new List<int>();//[兼容旧存档] 已升级的卡牌模板ID

    // 解锁的最大岛屿索引
    private int maxUnlockedIsland = 0;

    // 已通关的岛屿索引（击败Boss后标记，不可再进入）
    private int completedIsland = -1;

    public void Init()
    {
        cardList = new List<DeckCard>();
        LoadUpgradedCards();

        // 从 GameConfig 读取初始卡牌
        var cfg = GameConfig.Instance;
        if (cfg.initialCards != null && cfg.initialCards.Count > 0)
        {
            cardList = cfg.GenerateCardList();
        }
        else
        {
            Debug.LogWarning("[RoleManager] GameConfig 中未配置初始卡牌！");
        }

        // 加载解锁进度
        LoadUnlockData();
    }

    public bool IsIslandUnlocked(int islandIndex)
    {
        return islandIndex <= maxUnlockedIsland;
    }

    /// <summary>
    /// 岛屿是否已通关（击败了该岛屿的 Boss）
    /// </summary>
    public bool IsIslandCompleted(int islandIndex)
    {
        return islandIndex <= completedIsland;
    }

    /// <summary>
    /// 标记当前岛屿为已通关（击败 Boss 后调用）
    /// </summary>
    public void MarkIslandCompleted(int islandIndex)
    {
        if (islandIndex > completedIsland)
        {
            completedIsland = islandIndex;
            SaveCompletedIsland();
        }
    }

    public void UnlockNextIsland()
    {
        if (maxUnlockedIsland + 1 < 23)
        {
            maxUnlockedIsland++;
            SaveUnlockData();
        }
    }

    public void AddCard(CardData card)
    {
        if (card != null)
        {
            cardList.Add(new DeckCard(card));
        }
    }

    public void RemoveCard(int index)
    {
        if (index >= 0 && index < cardList.Count)
        {
            cardList.RemoveAt(index);
        }
    }

    /// <summary>
    /// 升级指定 DeckCard（实例级别，不影响同ID其他张）
    /// </summary>
    public void UpgradeCardInstance(DeckCard target)
    {
        target.upgraded = true;
        SaveUpgradedCards();
    }

    /// <summary>
    /// [兼容旧存档] 升级——旧方法重定向到第一张未升级的实例
    /// </summary>
    public void UpgradeCard(int cardId)
    {
        foreach (var dc in cardList)
        {
            if (dc.cardData != null && dc.cardData.id == cardId && !dc.upgraded)
            {
                dc.upgraded = true;
                SaveUpgradedCards();
                return;
            }
        }
    }

    /// <summary>
    /// 获取某张卡牌是否升级（实例级别）
    /// </summary>
    public bool IsCardUpgraded(DeckCard deckCard)
    {
        return deckCard != null && deckCard.upgraded;
    }

    /// <summary>
    /// 是否有任意一张该模板ID的卡牌被升级
    /// </summary>
    public bool HasAnyUpgraded(int cardId)
    {
        if (cardList == null) return false;
        foreach (var dc in cardList)
            if (dc.cardData != null && dc.cardData.id == cardId && dc.upgraded)
                return true;
        return false;
    }

    private void SaveUnlockData()
    {
        PlayerPrefs.SetInt("MaxUnlockedIsland", maxUnlockedIsland);
        PlayerPrefs.Save();
    }

    private void SaveCompletedIsland()
    {
        PlayerPrefs.SetInt("CompletedIsland", completedIsland);
        PlayerPrefs.Save();
    }

    private void LoadUnlockData()
    {
        maxUnlockedIsland = PlayerPrefs.GetInt("MaxUnlockedIsland", 0);
        completedIsland = PlayerPrefs.GetInt("CompletedIsland", -1);
    }

    /// <summary>
    /// 保存已升级卡牌的 instanceId
    /// </summary>
    public void SaveUpgradedCards()
    {
        List<int> ids = new List<int>();
        foreach (var dc in cardList)
        {
            if (dc.upgraded)
                ids.Add(dc.instanceId);
        }
        PlayerPrefs.SetString("UpgradedCardIds", ids.Count > 0 ? string.Join(",", ids) : "");
        PlayerPrefs.Save();
    }

    private void LoadUpgradedCards()
    {
        string data = PlayerPrefs.GetString("UpgradedCardIds", "");
        if (string.IsNullOrEmpty(data)) return;
        // 旧格式兼容：纯数字=模板ID，用旧逻辑；含逗号=instanceId列表，等cardList加载后匹配
        _pendingUpgradedIds = new List<int>();
        foreach (string s in data.Split(','))
        {
            if (int.TryParse(s, out int id))
                _pendingUpgradedIds.Add(id);
        }
    }

    private List<int> _pendingUpgradedIds;

    /// <summary>
    /// Init后调用，将升级标记应用到DeckCard
    /// </summary>
    public void ApplyUpgradesToDeck()
    {
        if (_pendingUpgradedIds == null || _pendingUpgradedIds.Count == 0) return;
        foreach (var dc in cardList)
        {
            // 优先匹配instanceId（新格式），否则按模板ID匹配（旧兼容）
            if (_pendingUpgradedIds.Contains(dc.instanceId))
                dc.upgraded = true;
            // 兼容旧：如果_pendingUpgradedIds含模板id且这张没被标记，标记第一张
            else if (_pendingUpgradedIds.Contains(dc.cardData.id) && !HasUpgraded(dc.cardData.id))
                dc.upgraded = true;
        }
        _pendingUpgradedIds = null;
        SaveUpgradedCards();
    }

    private bool HasUpgraded(int cardId)
    {
        foreach (var dc in cardList)
            if (dc.cardData != null && dc.cardData.id == cardId && dc.upgraded)
                return true;
        return false;
    }
}
