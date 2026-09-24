using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;

namespace CSharpSBP.SBP_Code.Powers;

// 生成卡牌：地咬狂蛇
// 卡牌描述：减少{amount}点最大能量。=等效于回合开始时减少{amount}点能量。
public sealed class EnergyLossPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != base.Owner.Player)
        {
            return amount;
        }
        // 减少最大能量，确保不小于0
        decimal newAmount = amount - (decimal)base.Amount;
        return newAmount < 0 ? 0 : newAmount;
    }
}
