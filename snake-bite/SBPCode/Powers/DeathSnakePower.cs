using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：死蛇形态
// 卡牌描述：每当你给予敌人中毒时，额外给予等量的灾厄。
public sealed class DeathSnakePower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DynamicVar("Doom", 1m) };

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature applier, CardModel cardSource)
    {
        // 检查条件：
        // 1. 能力类型是中毒 (PoisonPower)
        // 2. 中毒层数增加 (amount > 0)
        // 3. 目标是敌人 (power.Owner.IsEnemy)
        // 4. 施加者是自己 (applier == base.Owner)
        if (power is PoisonPower poisonPower && amount > 0m && power.Owner.IsEnemy && applier == base.Owner)
        {
            Flash();
            // 给予敌人与增加的中毒层数相同的灾厄
            // amount 参数表示增加的中毒层数，直接用它来施加等量的灾厄
            await PowerCmd.Apply<DoomPower>(choiceContext, power.Owner, amount, base.Owner, cardSource);
        }
    }
}
