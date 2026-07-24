using UnityEngine;

/// <summary>
/// 法杖 - 每打出一张法术牌，30%概率获得1点能量
/// </summary>
public class Staff : RelicBase
{
    private const float TRIGGER_CHANCE = 0.3f;

    public override void OnCardPlayed(CardItem card)
    {
        if (card == null || !card.IsSpellCard()) return;
        if (Random.value > TRIGGER_CHANCE) return;

        FightManager.Instance.CurPowerCount += 1;
    }
}
