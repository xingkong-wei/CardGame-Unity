using UnityEngine;

/// <summary>
/// 元素法典 - 每场战斗开始时选择一种元素，首回合该元素叠加速度翻倍，其他减半
/// </summary>
public class ElementCodex : RelicBase
{
    private StatusType chosenAffinity;
    private bool isActive;

    public override void OnBattleStart()
    {
        isActive = false;

        ElementSelectUI selectUI = UIManager.Instance.ShowUI<ElementSelectUI>("ElementSelectUI") as ElementSelectUI;
        if (selectUI == null)
        {
            Debug.LogError("元素法典：找不到 ElementSelectUI");
            return;
        }

        selectUI.fireText?.gameObject.SetActive(false);
        selectUI.iceText?.gameObject.SetActive(false);
        selectUI.lightningText?.gameObject.SetActive(false);
        selectUI.fireBtn.interactable = true;
        selectUI.iceBtn.interactable = true;
        selectUI.lightningBtn.interactable = true;

        selectUI.ShowSelectForCodex(chosenType =>
        {
            chosenAffinity = chosenType;
            isActive = true;
            UIManager.Instance.ShowTip($"元素法典：选中{GetElementName(chosenType)}亲和度", Color.yellow);
        });
    }

    public override int ModifyAffinityGain(StatusType type, int stack)
    {
        if (!isActive) return stack;
        if (type == chosenAffinity) return stack * 2;
        return Mathf.Max(1, stack / 2);
    }

    public override void OnTurnEnd()
    {
        isActive = false;
    }

    private string GetElementName(StatusType type) => type switch
    {
        StatusType.FireAffinity => "火",
        StatusType.IceAffinity => "冰",
        StatusType.LightningAffinity => "雷",
        _ => ""
    };
}
