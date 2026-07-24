/// <summary>
/// 狂暴魔药 - 本回合获得5点力量，回合结束时恢复
/// </summary>
public class ViolentPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int amount = data.effectValue;
        int duration = data.duration;

        BuffManager.Instance.AddStatus(StatusType.Strength, amount, duration);

        UIManager.Instance.ShowTip($"获得 {amount} 点力量（本回合）",
            new UnityEngine.Color(1f, 0.5f, 0f));
    }
}
