/// <summary>
/// 沧异脂浆 - 获得1层力量和1层敏捷
/// </summary>
public class StrangeTallowSlurryPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int amount = data.effectValue;
        BuffManager.Instance.AddStatus(StatusType.Strength, amount);
        BuffManager.Instance.AddStatus(StatusType.Agility, amount);
    }
}
