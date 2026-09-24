using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace CSharpSBP.SBP_Code.Powers;

// 生成卡牌：至亮之咬
// 作为卡牌代价使用
// 卡牌描述：每回合开始时，给予自身{amount}层中毒。
public sealed class PoisonEachTurnPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        await base.AfterSideTurnStart(side, participants, combatState);

        if (side == base.Owner.Side)
        {
            // 触发能力效果
            Flash();
            // 给予自身中毒
            await PowerCmd.Apply<PoisonPower>(new ThrowingPlayerChoiceContext(), base.Owner, base.Amount, base.Owner, null);
        }
    }
}
