using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：蛇洞SBP_power5
// 卡牌描述：每次给予自身中毒或去除自身中毒时，对所有敌人施加Amount层的中毒。
public sealed class SnakePoisonAuraPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature applier, CardModel cardSource)
    {
        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);

        // 检查是否是中毒能力
        if (!(power is PoisonPower))
        {
            return;
        }

        // 检查是否是发生在自己身上的变化
        if (power.Owner != base.Owner)
        {
            return;
        }

        // 检查是否有层数变化（增加或减少）
        if (amount == 0)
        {
            return;
        }

        // 对所有敌人施加 Amount 层中毒
        Flash();
        foreach (var enemy in base.Owner.CombatState.GetOpponentsOf(base.Owner))
        {
            if (enemy.IsAlive)
            {
                await PowerCmd.Apply<PoisonPower>(choiceContext, enemy, base.Amount, base.Owner, null);
            }
        }
    }
}
