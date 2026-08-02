using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Map;

public class RestSiteUI : UIBase
{
    [Header("按钮")]
    public Button restBtn;
    public Button developBtn;

    [Header("描述文本")]
    public TextMeshProUGUI descText;

    private bool actionCompleted = false;

    private void Awake()
    {
        if (restBtn != null) restBtn.onClick.AddListener(OnRestClick);
        if (developBtn != null) developBtn.onClick.AddListener(OnDevelopClick);

        AddHoverTip(restBtn?.gameObject, "恢复你最大血量的30%");
        AddHoverTip(developBtn?.gameObject, "升级一张卡牌");
    }

    private void AddHoverTip(GameObject target, string tip)
    {
        if (target == null || descText == null) return;
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.AddComponent<EventTrigger>();
        EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => descText.text = tip);
        trigger.triggers.Add(enter);
        EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => descText.text = "");
        trigger.triggers.Add(exit);
    }

    private void OnRestClick()
    {
        if (actionCompleted) return;
        actionCompleted = true;
        DoRest();
    }

    private void OnDevelopClick()
    {
        if (actionCompleted) return;
        actionCompleted = true;
        DoDevelop();
    }

    private void DoRest()
    {
        int healAmount = Mathf.CeilToInt(FightManager.Instance.MaxHp * 0.3f);
        FightManager.HealPlayer(healAmount);

        if (Camera.main != null)
        {
            GameObject effect = Instantiate(ResourceCache.Get<GameObject>("Effects/HealOnce"));
            if (effect != null)
            {
                effect.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
                Destroy(effect, 2f);
            }
        }

        StartCoroutine(CompleteAction());
    }

    private void DoDevelop()
    {
        List<CardData> pool = new List<CardData>();
        if (RoleManager.Instance != null && RoleManager.Instance.cardList != null)
        {
            foreach (var dc in RoleManager.Instance.cardList)
            {
                var card = dc.cardData;
                if (card != null && card.upgradable && !dc.upgraded)
                    pool.Add(card);
            }
        }

        if (pool.Count == 0)
        {
            StartCoroutine(CompleteAction());
            return;
        }

        CardCollectionUI.ShowCardList(pool, "升级", true, (CardData selectedCard) =>
        {
            if (selectedCard != null)
            {
                foreach (var dc in RoleManager.Instance.cardList)
                {
                    if (dc.cardData == selectedCard && !dc.upgraded)
                    {
                        RoleManager.Instance.UpgradeCardInstance(dc);
                        break;
                    }
                }
            }
            StartCoroutine(CompleteAction());
        });

        // 隐藏 CardCollectionUI 的返回按钮（从 RestSiteUI 进入时不可返回）
        StartCoroutine(HideCollectionCloseBtn());
    }

    private IEnumerator HideCollectionCloseBtn()
    {
        yield return null; // 等一帧，确保 UI 已创建
        CardCollectionUI collectionUI = UIManager.Instance?.GetUI<CardCollectionUI>("CardCollectionUI");
        if (collectionUI != null && collectionUI.closeBtn != null)
            collectionUI.closeBtn.gameObject.SetActive(false);
    }

    private IEnumerator CompleteAction()
    {
        yield return new WaitForSeconds(0.3f);
        if (MapPlayerTracker.Instance != null) MapPlayerTracker.Instance.Locked = false;
        SlayTheSpireMapUI mapUI = UIManager.Instance?.GetUI<SlayTheSpireMapUI>("SlayTheSpireMapUI");
        if (mapUI != null) mapUI.Show();
        Close();
    }
}
