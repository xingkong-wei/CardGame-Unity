using UnityEngine;

/// <summary>
/// 扫描仪 - 每回合第一张攻击类型牌伤害+50%
/// </summary>
public class Scanner : RelicBase
{
    private bool firstAttackThisTurn;
    private bool boostingThisCard;

    public override void OnTurnStart()
    {
        firstAttackThisTurn = true;
        boostingThisCard = false;
    }

    public override void OnCardPlayed(CardItem card)
    {
        if (card == null || card.data == null) return;
        if (!firstAttackThisTurn) return;
        if (!card.data.HasCardType("攻击")) return;

        boostingThisCard = true;
        firstAttackThisTurn = false;
    }

    public override int OnDealDamage(int damage)
    {
        if (!boostingThisCard) return damage;
        boostingThisCard = false;
        return Mathf.CeilToInt(damage * 1.5f);
    }

    public override float GetDamagePreviewMultiplier()
    {
        return firstAttackThisTurn ? 1.5f : 1f;
    }
}
