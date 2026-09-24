using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using CSharpSBP.SBP_Code.Cards;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：子蛇序
// 卡牌描述：每次打出"蛇咬"时获得1点能量，给予自身2层中毒 power。
public sealed class SnakeProgramPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public new IEnumerable<DynamicVar> DynamicVars => new List<DynamicVar>
    {
        new EnergyVar(base.Amount),
        new IntVar("PoisonAmount", 2 * base.Amount),
    };
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 检查是否是玩家打出的卡牌
        if (cardPlay.Card.Owner != base.Owner.Player)
        {
            return;
        }
        
        // 检查是否是蛇咬卡牌
        if (cardPlay.Card is SnakeBitePlus_Card snakeCard && snakeCard.HasSnakeBiteTag)
        {
            Flash();
            // 获得能量
            await PlayerCmd.GainEnergy(base.Amount, base.Owner.Player);

            // 给予自身中毒
            await PowerCmd.Apply<PoisonPower>(context, base.Owner, 2 * base.Amount, base.Owner, null);
        }
    }
}
