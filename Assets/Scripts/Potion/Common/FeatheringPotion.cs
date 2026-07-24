/// <summary>
/// 羽化药水 - 本回合获得5点敏捷，回合结束时恢复
/// </summary>
public class FeatheringPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int amount = data.effectValue;
        int duration = data.duration;

        BuffManager.Instance.AddStatus(StatusType.Agility, amount, duration);

        UIManager.Instance.ShowTip($"获得 {amount} 点敏捷（本回合）",
            new UnityEngine.Color(0.3f, 0.8f, 1f));
    }
}
