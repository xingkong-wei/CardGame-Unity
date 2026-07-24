using UnityEngine;

/// <summary>
/// 不灭灵汁 - 获得5层生命再生，每回合开始时减少1层
/// </summary>
public class EternalSpiritElixirPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int amount = data.effectValue;

        BuffManager.Instance.AddStatus(StatusType.Regeneration, amount);

        UIManager.Instance.ShowTip($"获得 {amount} 层生命再生",
            new Color(0.3f, 1f, 0.3f));
    }
}
