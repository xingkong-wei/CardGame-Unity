/// <summary>
/// 复制药水 - 本回合打出的下1张牌会额外打出一次
/// </summary>
public class DuplicatePotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int amount = data.effectValue;
        BuffManager.Instance.AddStatus(StatusType.Duplicate, amount);
    }
}
