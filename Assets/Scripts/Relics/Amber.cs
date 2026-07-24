using System.Linq;

/// <summary>
/// 琥珀 - 回合开始时，总和≥6则获得1层最低元素；差值≥4则改为获得3层
/// </summary>
public class Amber : RelicBase
{
    public override void OnTurnStart()
    {
        int fire = BuffManager.Instance.GetStack(StatusType.FireAffinity);
        int ice = BuffManager.Instance.GetStack(StatusType.IceAffinity);
        int lightning = BuffManager.Instance.GetStack(StatusType.LightningAffinity);

        int total = fire + ice + lightning;
        if (total < 6) return;

        int[] stacks = { fire, ice, lightning };
        StatusType[] types = { StatusType.FireAffinity, StatusType.IceAffinity, StatusType.LightningAffinity };

        int minStack = stacks.Min();
        StatusType lowest = StatusType.FireAffinity;
        for (int i = 0; i < 3; i++)
        {
            if (stacks[i] == minStack)
            {
                lowest = types[i];
                break;
            }
        }

        int diff = stacks.Max() - minStack;
        int amount = diff == 4 ? 3 : 1;
        BuffManager.Instance.AddStatus(lowest, amount);
    }
}
