using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : UIBase
{
    public System.Action OnClosed;

    [Header("商品容器")]
    public Transform cardContainer;
    public Transform relicContainer;
    public Transform potionContainer;

    [Header("卡牌移除服务")]
    public Button cardRemoveBtn;
    public TextMeshProUGUI cardRemovePriceTxt;

    [Header("卡牌布局")]
    public int cardColumns = 3;
    public Vector2 cardSpacing = new Vector2(15, 15);
    public Vector2 cardPadding = new Vector2(20, 20);

    [Header("卡牌价格样式")]
    public TMP_FontAsset cardPriceFont;
    public Color cardPriceColor = new Color(1f, 0.85f, 0.298f);
    public float cardPriceFontSize = 16f;

    [Header("遗物/药水大小")]
    public float itemScale = 1f;

    [Header("遗物/药水价格样式")]
    public TMP_FontAsset itemPriceFont;
    public Color itemPriceColor = new Color(1f, 0.85f, 0.298f);
    public float itemPriceFontSize = 14f;
    public Vector2 itemPricePos = new Vector2(0, 10);
    public Vector2 itemPriceSize = new Vector2(100, 30);

    [Header("金币")]
    public TextMeshProUGUI goldTxt;

    [Header("Invemtory参数")]
    public Button mapBtn;
    public Button plotBtn;
    public Button cardBtn;
    public Button setBtn;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI cardCountText;
    public GameObject potionObj;

    [Header("按钮")]
    public Button leaveBtn;

    private List<GameObject> cardObjects = new List<GameObject>();
    private List<GameObject> relicObjects = new List<GameObject>();
    private List<GameObject> potionObjects = new List<GameObject>();

    private const int CARD_REMOVE_PRICE = 100;
    private bool removeUsed;
    private bool restoringFromSave; // 读档恢复时跳过 GenerateRandomShop

    /// <summary>标记为读档恢复（在 Show() 后调用，跳过下次存档覆盖）</summary>
    public void MarkRestoring() { restoringFromSave = true; }

    private void Start()
    {
        if (leaveBtn != null)
            leaveBtn.onClick.AddListener(OnLeaveClick);

        if (cardRemoveBtn != null)
        {
            if (cardRemovePriceTxt != null)
                cardRemovePriceTxt.text = CARD_REMOVE_PRICE.ToString();
            cardRemoveBtn.onClick.AddListener(OnCardRemoveClicked);
        }

        // Invemtory 按钮
        if (mapBtn != null) mapBtn.onClick.AddListener(FightUI.OpenNodeMapForObservation);
        if (plotBtn != null) plotBtn.onClick.AddListener(() => UIManager.Instance.ShowUI<PlotUI>("PlotUI"));
        if (cardBtn != null) cardBtn.onClick.AddListener(() => CardCollectionUI.ShowCardList(CardListType.Collection, "集卡簿"));
        if (setBtn != null) setBtn.onClick.AddListener(() =>
        {
            GameSettingUI ui = UIManager.Instance.ShowUI<GameSettingUI>("GameSettingUI") as GameSettingUI;
            if (ui != null) ui.SetPreviousUIName("ShopUI");
        });

        // 药水面板
        if (potionObj != null && potionObj.GetComponent<PotionPanelController>() == null)
            potionObj.AddComponent<PotionPanelController>();

        // 时间（每秒更新）
        InvokeRepeating(nameof(UpdateTimeDisplay), 0f, 1f);
    }

    private void UpdateTimeDisplay()
    {
        if (timeText == null) return;
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            int time = fightUI.GetBattleTime();
            timeText.text = string.Format("{0:00}:{1:00}", time / 60, time % 60);
        }
    }

    private void UpdateInvemtoryUI()
    {
        // 卡牌数量
        if (cardCountText != null)
            cardCountText.text = RoleManager.Instance.cardList.Count.ToString();

        // 金币
        RefreshGold();
    }

    public void GenerateRandomShop()
    {
        ClearAllItems();

        CardData[] allCards = Resources.LoadAll<CardData>("Data_Card");
        RelicData[] allRelics = Resources.LoadAll<RelicData>("Data_Relic");
        PotionData[] allPotions = Resources.LoadAll<PotionData>("Data_Potion");

        List<CardData> cards = PickRandomCards(allCards, 6);
        List<RelicData> relics = PickRandomRelics(allRelics, 3);
        List<PotionData> potions = PickRandomPotions(allPotions, 3);

        foreach (var card in cards)
            CreateCardItem(card, card.price > 0 ? card.price : GetCardPrice(card));

        foreach (var relic in relics)
            CreateRelicItem(relic, relic.price > 0 ? relic.price : GetRelicPrice(relic));

        foreach (var potion in potions)
            CreatePotionItem(potion, potion.buyPrice > 0 ? potion.buyPrice : GetPotionPrice(potion));
    }

    private List<CardData> PickRandomCards(CardData[] all, int count)
    {
        List<CardData> pool = new List<CardData>();
        foreach (var c in all)
        {
            if (c.rarity == CardRarity.Basic || c.rarity == CardRarity.Status ||
                c.rarity == CardRarity.Curse || c.rarity == CardRarity.Quest ||
                c.rarity == CardRarity.Generated)
                continue;
            pool.Add(c);
        }
        return PickRandom(pool, count);
    }

    private List<RelicData> PickRandomRelics(RelicData[] all, int count)
    {
        List<RelicData> pool = new List<RelicData>();
        foreach (var r in all)
        {
            if (r.rarity == RelicRarity.Starter) continue;
            pool.Add(r);
        }
        return PickWeightedRandom(pool, count);
    }

    /// <summary>
    /// 按权重不放回随机选取（已持有遗物权重降低）
    /// </summary>
    private List<RelicData> PickWeightedRandom(List<RelicData> pool, int count)
    {
        List<RelicData> result = new List<RelicData>();
        List<RelicData> remaining = new List<RelicData>(pool);

        for (int i = 0; i < count && remaining.Count > 0; i++)
        {
            RelicData picked = PotionDropManager.PickWeightedRelic(remaining);
            if (picked != null)
            {
                result.Add(picked);
                remaining.Remove(picked);
            }
        }
        return result;
    }

    private List<PotionData> PickRandomPotions(PotionData[] all, int count)
    {
        return PickRandom(new List<PotionData>(all), count);
    }

    private List<T> PickRandom<T>(List<T> pool, int count)
    {
        List<T> result = new List<T>();
        List<T> temp = new List<T>(pool);
        for (int i = 0; i < count && temp.Count > 0; i++)
        {
            int idx = Random.Range(0, temp.Count);
            result.Add(temp[idx]);
            temp.RemoveAt(idx);
        }
        return result;
    }

    private void CreateCardItem(CardData cardData, int price)
    {
        GameObject cardObj = Instantiate(ResourceCache.Get<GameObject>("UI/CardItem"), cardContainer);
        cardObj.name = cardData.scriptName; // 用 scriptName 便于存档/读档恢复

        CardItem[] items = cardObj.GetComponents<CardItem>();
        foreach (var ci in items) Destroy(ci);

        RectTransform cardRT = cardObj.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0, 1);
        cardRT.anchorMax = new Vector2(0, 1);
        cardRT.pivot = new Vector2(0, 1);

        int index = cardObjects.Count;
        int col = index % cardColumns;
        int row = index / cardColumns;
        float x = cardPadding.x + col * (cardRT.sizeDelta.x + cardSpacing.x);
        float y = -cardPadding.y - row * (cardRT.sizeDelta.y + cardSpacing.y);
        cardRT.anchoredPosition = new Vector2(x, y);

        Transform bg = cardObj.transform.Find("bg");
        if (bg != null)
        {
            Image bgImg = bg.GetComponent<Image>();
            if (bgImg != null && !string.IsNullOrEmpty(cardData.bgIcon))
                bgImg.sprite = ResourceCache.GetSprite(cardData.bgIcon);

            Material srcMat = ResourceCache.Get<Material>("Mats/outline");
            if (srcMat != null && bgImg != null)
            {
                Material outlineMat = Object.Instantiate(srcMat);
                outlineMat.SetColor("_lineColor", Color.black);
                outlineMat.SetFloat("_lineWidth", 1);
                bgImg.material = outlineMat;
            }

            Transform iconTf = bg.Find("icon");
            if (iconTf != null)
            {
                Image iconImg = iconTf.GetComponent<Image>();
                if (iconImg != null && !string.IsNullOrEmpty(cardData.icon))
                    iconImg.sprite = ResourceCache.GetSprite(cardData.icon);
            }

            TextMeshProUGUI nameTxt = bg.Find("nameTxt")?.GetComponent<TextMeshProUGUI>();
            if (nameTxt != null) nameTxt.text = cardData.cardName;

            TextMeshProUGUI msgTxt = bg.Find("msgTxt")?.GetComponent<TextMeshProUGUI>();
            if (msgTxt != null) msgTxt.text = cardData.GetFormattedDescription();

            TextMeshProUGUI costTxt = bg.Find("useTxt")?.GetComponent<TextMeshProUGUI>();
            if (costTxt != null)
            {
                int expend = cardData.upgradedExpend > 0 ? cardData.upgradedExpend : cardData.expend;
                costTxt.text = expend.ToString();
            }

            TextMeshProUGUI typeTxt = bg.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (typeTxt != null) typeTxt.text = cardData.GetTypeNames();
        }

        AddPriceLabel(cardObj, price, cardPriceFont, cardPriceColor, cardPriceFontSize, new Vector2(0, -5), new Vector2(100, 30));

        Button btn = cardObj.AddComponent<Button>();
        Image targetImg = cardObj.GetComponent<Image>() ?? bg?.GetComponent<Image>();
        if (targetImg != null) btn.targetGraphic = targetImg;
        CardData captured = cardData;
        btn.onClick.AddListener(() => OnCardClicked(captured, price, cardObj));

        cardObjects.Add(cardObj);
    }

    private void CreateRelicItem(RelicData relic, int price)
    {
        GameObject obj = Instantiate(ResourceCache.Get<GameObject>("UI/RelicIcon"), relicContainer);
        obj.name = relic.scriptName; // 用 scriptName 便于存档/读档恢复
        obj.transform.localScale = Vector3.one * itemScale;

        RelicIcon ri = obj.GetComponent<RelicIcon>();
        if (ri != null) ri.Setup(relic);

        AddPriceLabel(obj, price, itemPriceFont, itemPriceColor, itemPriceFontSize, itemPricePos, itemPriceSize);

        Button btn = obj.GetComponent<Button>() ?? obj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        Navigation nav = btn.navigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;
        btn.onClick.AddListener(() =>
        {
            if (FightManager.Instance.CoinAmount < price)
            {
                UIManager.Instance.ShowTip("金币不足", Color.red);
                return;
            }
            FightManager.Instance.AddCoin(-price);
            FightManager.Instance.AddRelic(relic);
            // 购买前先关闭并归位 TooltipPanel，防止因 SetParent 到根 Canvas 导致销毁后残留
            CleanupRelicIcon(obj);
            Destroy(obj);
            relicObjects.Remove(obj);
            UpdateInvemtoryUI();
            RefreshFightUI();
            RefreshPotionUI();
            SaveShopState();
        });

        relicObjects.Add(obj);
    }

    private void CreatePotionItem(PotionData potion, int price)
    {
        GameObject obj = Instantiate(ResourceCache.Get<GameObject>("UI/PotionIcon"), potionContainer);
        obj.name = potion.scriptName; // 用 scriptName 便于存档/读档恢复
        obj.transform.localScale = Vector3.one * itemScale;

        RelicIcon ri = obj.GetComponent<RelicIcon>();
        if (ri != null)
        {
            if (ri.iconImage != null && !string.IsNullOrEmpty(potion.icon))
            {
                ri.iconImage.sprite = ResourceCache.GetSprite(potion.icon);
                Color c = ri.iconImage.color;
                c.a = 1f;
                ri.iconImage.color = c;
            }
            if (ri.tooltipNameText != null) ri.tooltipNameText.text = potion.potionName;
            if (ri.tooltipDescText != null) ri.tooltipDescText.text = potion.description;
            if (ri.tooltipPanel != null) ri.tooltipPanel.SetActive(false);
            ri.InvokeAdjustTooltip();
        }

        AddPriceLabel(obj, price, itemPriceFont, itemPriceColor, itemPriceFontSize, itemPricePos, itemPriceSize);

        Button btn = obj.GetComponent<Button>() ?? obj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        Navigation nav = btn.navigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;
        btn.onClick.AddListener(() =>
        {
            if (FightManager.Instance.CoinAmount < price)
            {
                UIManager.Instance.ShowTip("金币不足", Color.red);
                return;
            }
            if (FightManager.Instance.potionList.Count >= 3)
            {
                UIManager.Instance.ShowTip("药水栏已满！", Color.red);
                return;
            }
            FightManager.Instance.AddCoin(-price);
            FightManager.Instance.potionList.Add(potion);
            CleanupRelicIcon(obj);
            Destroy(obj);
            potionObjects.Remove(obj);
            UpdateInvemtoryUI();
            RefreshFightUI();
            RefreshPotionUI();
            SaveShopState();
        });

        potionObjects.Add(obj);
    }

    private void AddPriceLabel(GameObject parent, int price, TMP_FontAsset font, Color color, float fontSize, Vector2 pos, Vector2 size)
    {
        GameObject priceObj = new GameObject("PriceTxt", typeof(RectTransform));
        priceObj.transform.SetParent(parent.transform, false);
        RectTransform prt = priceObj.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0);
        prt.anchorMax = new Vector2(0.5f, 0);
        prt.pivot = new Vector2(0.5f, 1);
        prt.anchoredPosition = pos;
        prt.sizeDelta = size;

        TextMeshProUGUI txt = priceObj.AddComponent<TextMeshProUGUI>();
        txt.text = price.ToString();
        txt.fontSize = fontSize;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = color;
        txt.raycastTarget = false;
        if (font != null) txt.font = font;
    }

    private int GetCardPrice(CardData card)
    {
        return card.rarity switch
        {
            CardRarity.Common => Random.Range(50, 70),
            CardRarity.Uncommon => Random.Range(70, 100),
            CardRarity.Rare => Random.Range(100, 150),
            _ => 50
        };
    }

    private int GetRelicPrice(RelicData relic)
    {
        return relic.rarity switch
        {
            RelicRarity.Common => Random.Range(100, 150),
            RelicRarity.Uncommon => Random.Range(150, 200),
            RelicRarity.Rare => Random.Range(200, 300),
            RelicRarity.Shop => Random.Range(100, 180),
            _ => 100
        };
    }

    private int GetPotionPrice(PotionData potion)
    {
        return potion.rarity switch
        {
            PotionRarity.Common => Random.Range(40, 60),
            PotionRarity.Uncommon => Random.Range(60, 90),
            PotionRarity.Rare => Random.Range(90, 130),
            _ => 50
        };
    }

    private void OnCardClicked(CardData card, int price, GameObject cardObj)
    {
        if (FightManager.Instance.CoinAmount < price)
        {
            UIManager.Instance.ShowTip("金币不足", Color.red);
            return;
        }
        FightManager.Instance.AddCoin(-price);
        RoleManager.Instance.AddCard(card);
        Destroy(cardObj);
        cardObjects.Remove(cardObj);
        UpdateInvemtoryUI();
        RefreshFightUI();
        SaveShopState();
    }

    private void OnCardRemoveClicked()
    {
        if (removeUsed)
        {
            UIManager.Instance.ShowTip("已使用过卡牌移除服务", Color.red);
            return;
        }
        if (FightManager.Instance.CoinAmount < CARD_REMOVE_PRICE)
        {
            UIManager.Instance.ShowTip("金币不足", Color.red);
            return;
        }
        FightManager.Instance.AddCoin(-CARD_REMOVE_PRICE);
        removeUsed = true;
        if (cardRemoveBtn != null) cardRemoveBtn.interactable = false;
        if (cardRemovePriceTxt != null) cardRemovePriceTxt.text = "售罄";
        UpdateInvemtoryUI();
        RefreshFightUI();
        RefreshPotionUI();
        SaveShopState();
        CardCollectionUI.ShowCardList(
            GetPlayerCardDataList(),
            "移除服务",
            true,
            (CardData selected) => RemoveSelectedCard(selected)
        );
    }

    private void RemoveSelectedCard(CardData card)
    {
        if (card == null) return;
        var cardList = RoleManager.Instance.cardList;
        for (int i = cardList.Count - 1; i >= 0; i--)
        {
            if (cardList[i] != null && cardList[i].cardData == card)
            {
                RoleManager.Instance.RemoveCard(i);
                return;
            }
        }
    }

    private List<CardData> GetPlayerCardDataList()
    {
        List<CardData> cards = new List<CardData>();
        if (RoleManager.Instance != null && RoleManager.Instance.cardList != null)
        {
            foreach (var dc in RoleManager.Instance.cardList)
                if (dc != null && dc.cardData != null)
                    cards.Add(dc.cardData);
        }
        return cards;
    }

    private void RefreshGold()
    {
        if (goldTxt != null)
            goldTxt.text = FightManager.Instance.CoinAmount.ToString();
    }

    private void RefreshFightUI()
    {
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        // 同步金币
        fightUI.UpdateCoinDisplay(FightManager.Instance.CoinAmount);
        // 同步卡牌数量
        fightUI.UpdateCollectionCount();
        // 同步遗物
        if (fightUI.relicsUI != null)
            fightUI.relicsUI.RefreshUI();
    }

    private void RefreshPotionUI()
    {
        // 刷新本地的药水面板
        if (potionObj != null)
        {
            PotionPanelController ppc = potionObj.GetComponent<PotionPanelController>();
            if (ppc != null) ppc.RefreshPotionButtons();
        }
        // 同步 FightUI 的药水面板
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null && fightUI.potionObj != null)
        {
            PotionPanelController fppc = fightUI.potionObj.GetComponent<PotionPanelController>();
            if (fppc != null) fppc.RefreshPotionButtons();
        }
    }

    /// <summary>
    /// 购买前清理 RelicIcon：关闭 Tooltip 并将其恢复到 obj 子层级，避免销毁后残留
    /// </summary>
    private void CleanupRelicIcon(GameObject obj)
    {
        if (obj == null) return;
        RelicIcon ri = obj.GetComponent<RelicIcon>();
        if (ri == null) return;
        // 触发 OnPointerExit 关闭 Tooltip 并将其归位
        ri.OnPointerExit(null);
    }

    private void ClearAllItems()
    {
        foreach (var obj in cardObjects) if (obj != null) Destroy(obj);
        foreach (var obj in relicObjects)
        {
            if (obj != null)
            {
                CleanupRelicIcon(obj);
                Destroy(obj);
            }
        }
        foreach (var obj in potionObjects)
        {
            if (obj != null)
            {
                CleanupRelicIcon(obj);
                Destroy(obj);
            }
        }
        cardObjects.Clear();
        relicObjects.Clear();
        potionObjects.Clear();
    }

    private void SaveShopState()
    {
        if (SaveManager.IsLoading || restoringFromSave) return;
        SaveManager.Save(SavePhase.Shop);
    }

    private void OnLeaveClick()
    {
        OnClosed?.Invoke();
        UIManager.Instance.CloseUI("ShopUI");
    }

    public override void Show()
    {
        base.Show();

        removeUsed = false;
        if (cardRemoveBtn != null) cardRemoveBtn.interactable = true;
        if (cardRemovePriceTxt != null) cardRemovePriceTxt.text = CARD_REMOVE_PRICE.ToString();
        GenerateRandomShop();
        UpdateInvemtoryUI();

        if (!SaveManager.IsLoading && !restoringFromSave)
            SaveManager.Save(SavePhase.Shop);
    }

    /// <summary>从存档恢复商店商品（先清空 Show() 生成的随机商品，再重建存档中的商品）</summary>
    public void RestoreFromSave(GameSaveData data)
    {
        ClearAllItems();

        removeUsed = data.shopRemoveUsed;
        if (cardRemoveBtn != null) cardRemoveBtn.interactable = !removeUsed;
        if (cardRemovePriceTxt != null) cardRemovePriceTxt.text = removeUsed ? "售罄" : CARD_REMOVE_PRICE.ToString();

        // 恢复卡牌商品（按 scriptName 匹配，因为文件名是 id_中文名 格式）
        if (data.shopCardIds.Count > 0)
        {
            CardData[] allCards = Resources.LoadAll<CardData>("Data_Card");
            for (int i = 0; i < data.shopCardIds.Count; i++)
            {
                CardData card = System.Array.Find(allCards, c => c.scriptName == data.shopCardIds[i]);
                if (card != null)
                {
                    int price = i < data.shopCardPrices.Count ? data.shopCardPrices[i] : 50;
                    CreateCardItem(card, price);
                }
            }
        }

        // 恢复遗物商品
        if (data.shopRelicIds.Count > 0)
        {
            RelicData[] allRelics = Resources.LoadAll<RelicData>("Data_Relic");
            for (int i = 0; i < data.shopRelicIds.Count; i++)
            {
                RelicData relic = System.Array.Find(allRelics, r => r.scriptName == data.shopRelicIds[i]);
                if (relic != null)
                {
                    int price = i < data.shopRelicPrices.Count ? data.shopRelicPrices[i] : 100;
                    CreateRelicItem(relic, price);
                }
            }
        }

        // 恢复药水商品
        if (data.shopPotionIds.Count > 0)
        {
            PotionData[] allPotions = Resources.LoadAll<PotionData>("Data_Potion");
            for (int i = 0; i < data.shopPotionIds.Count; i++)
            {
                PotionData potion = System.Array.Find(allPotions, p => p.scriptName == data.shopPotionIds[i]);
                if (potion != null)
                {
                    int price = i < data.shopPotionPrices.Count ? data.shopPotionPrices[i] : 50;
                    CreatePotionItem(potion, price);
                }
            }
        }

        UpdateInvemtoryUI();
        restoringFromSave = false; // 恢复完成后重置标志，允许后续购买触发正常存档
    }

    /// <summary>将商店状态写入存档</summary>
    public void WriteSaveData(GameSaveData data)
    {
        data.shopRemoveUsed = removeUsed;
        data.shopCardIds.Clear(); data.shopCardPrices.Clear();
        data.shopRelicIds.Clear(); data.shopRelicPrices.Clear();
        data.shopPotionIds.Clear(); data.shopPotionPrices.Clear();

        // 保存卡牌商品（通过 cardObjects 列表和价格标签）
        foreach (var obj in cardObjects)
        {
            if (obj == null) continue;
            string scriptName = obj.name;
            int price = GetPriceFromLabel(obj);
            data.shopCardIds.Add(scriptName);
            data.shopCardPrices.Add(price);
        }

        // 保存遗物商品
        foreach (var obj in relicObjects)
        {
            if (obj == null) continue;
            string scriptName = obj.name;
            int price = GetPriceFromLabel(obj);
            data.shopRelicIds.Add(scriptName);
            data.shopRelicPrices.Add(price);
        }

        // 保存药水商品
        foreach (var obj in potionObjects)
        {
            if (obj == null) continue;
            string scriptName = obj.name;
            int price = GetPriceFromLabel(obj);
            data.shopPotionIds.Add(scriptName);
            data.shopPotionPrices.Add(price);
        }
    }

    private int GetPriceFromLabel(GameObject obj)
    {
        Transform priceTf = obj.transform.Find("PriceTxt");
        if (priceTf != null)
        {
            TextMeshProUGUI txt = priceTf.GetComponent<TextMeshProUGUI>();
            if (txt != null && int.TryParse(txt.text, out int price))
                return price;
        }
        return 0;
    }
}
