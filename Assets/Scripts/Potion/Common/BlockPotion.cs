/// <summary>
/// 格挡药水 - 获得12点护盾
/// </summary>
public class BlockPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        // 加护盾
        int amount = data.effectValue;
        FightManager.Instance.DefenseCount += amount;

        // 刷新UI
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        fightUI?.UpdateDefense();

        UIManager.Instance.ShowTip($"获得 {amount} 点护盾", new UnityEngine.Color(0.4f, 0.6f, 1f));
    }
}
