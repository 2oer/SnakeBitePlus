using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using CSharpSBP.SBP_Code.Cards;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：疯狂蛇进
// 卡牌描述：这回合内，[gold]\"蛇咬\"[/gold]对[red]任意生物[/red]施加[gold]中毒[/gold]时，额外给予敌人中毒时，增加中毒量。
// 已完成
public sealed class PoisonBoostPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        await base.AfterSideTurnStart(side, participants, combatState);

        // Buff类型只能存在1回合，回合开始时移除
        if (side == base.Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }

    // 集中式中毒加成：任何“蛇咬”系卡牌由持有者施加中毒（层数增加）时，额外追加本能力层数的中毒。
    // 使用与原版 SneckoSkull（蛇骰骷髅）相同的 ModifyPowerAmountGivenAdditive 钩子，
    // 无需在每张施毒卡牌中手动读取本能力层数。
    public override decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        if (power is PoisonPower
            && amount > 0m
            && giver == base.Owner
            && cardSource is SnakeBitePlus_Card { HasSnakeBiteTag: true })
        {
            return base.Amount;
        }
        return 0m;
    }

    public override Task AfterModifyingPowerAmountGiven(PowerModel power)
    {
        if (power is PoisonPower)
        {
            Flash();
        }
        return Task.CompletedTask;
    }
}