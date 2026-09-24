using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using CSharpSBP.SBP_Code.Cards.AttackCards;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：回蛇，SBP_skill4SBP_skill4
// 卡牌描述：下回合开始时将{amount}张"巨蛇咬"(SBP_stk7)卡牌放入手牌。
public sealed class IncomingSnakePower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        await base.AfterSideTurnStart(side, participants, combatState);

        if (side == base.Owner.Side)
        {
            // 触发能力效果
            Flash();

            // 创建 amount 张巨蛇咬卡牌放入手牌
            for (int i = 0; i < base.Amount; i++)
            {
                var snakeCard = combatState.CreateCard<SBP_atk7>(base.Owner.Player);
                await CardPileCmd.Add(snakeCard, PileType.Hand);
            }

            // 移除这个能力（只触发一次）
            await PowerCmd.Remove<IncomingSnakePower>(base.Owner);
        }
    }
}
