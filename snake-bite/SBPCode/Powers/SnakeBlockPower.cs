using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using CSharpSBP.SBP_Code.Tags;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：咬判
// 卡牌描述：打出蛇系卡牌时获得{Amount:diff()}点格挡。
public sealed class SnakeBlockPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(context, cardPlay);

        // 检查打出的卡牌是否是蛇系卡牌
        if (cardPlay.Card is ISnakeTagged snakeCard && snakeCard.HasSnakeTag)
        {
            // 获得 amount 的格挡
            await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
        }
    }
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        await base.AfterSideTurnStart(side, participants, combatState);

        // Buff类型只能存在1回合，回合开始时移除
        if (side == base.Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}
