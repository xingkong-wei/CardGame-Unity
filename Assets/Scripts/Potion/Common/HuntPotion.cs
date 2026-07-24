using UnityEngine;

/// <summary>
/// 狩猎药水 - 获得1层火、冰、电亲和度
/// </summary>
public class HuntPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int amount = data.effectValue;

        BuffManager.Instance.AddStatus(StatusType.FireAffinity, amount);
        BuffManager.Instance.AddStatus(StatusType.IceAffinity, amount);
        BuffManager.Instance.AddStatus(StatusType.LightningAffinity, amount);

        UIManager.Instance.ShowTip($"火、冰、电亲和 +{amount}",
            new Color(1f, 0.8f, 0.3f));
    }
}
