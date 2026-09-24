using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using CSharpSBP.SBP_Code.Cards;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：蛇子程序
// 卡牌描述：每回合第一次打出"蛇咬"时，获得3能量，给予自身1层中毒。
public sealed class SnakegramPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public new IEnumerable<DynamicVar> DynamicVars => new[]
    {
        new EnergyVar(3)
    };

    // 使用布尔标志跟踪本回合是否已触发
    private bool _triggeredThisTurn = false;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 检查是否是玩家打出的卡牌且本回合未触发过
        if (cardPlay.Card.Owner != base.Owner.Player || _triggeredThisTurn)
        {
            return;
        }

        // 检查是否是蛇咬卡牌
        if (cardPlay.Card is SnakeBitePlus_Card snakeCard && snakeCard.HasSnakeBiteTag)
        {
            Flash();

            // 获得能量 = 3 * 层数
            await PlayerCmd.GainEnergy(3 * base.Amount, base.Owner.Player);

            // 给予中毒 = 1 * 层数
            await PowerCmd.Apply<PoisonPower>(context, base.Owner, base.Amount, base.Owner, null);

            // 标记本回合已触发
            _triggeredThisTurn = true;
        }
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 回合开始时重置触发状态
        if (side == base.Owner.Side)
        {
            _triggeredThisTurn = false;
        }

        return base.AfterSideTurnStart(side, participants, combatState);
    }
}
