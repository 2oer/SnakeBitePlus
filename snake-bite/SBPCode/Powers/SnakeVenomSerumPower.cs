using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：蛇毒血清
// 卡牌描述：每次给予自身中毒时获得2点再生和3点格挡。
public sealed class SnakeVenomSerumPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature applier, CardModel cardSource)
    {
        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
        
        // 检查是否是中毒能力、是否是自身、且层数增加
        if (!(power is PoisonPower) || power.Owner != base.Owner || amount <= 0) return;
        
        Flash();
        
        // 获得2点再生
        await PowerCmd.Apply<RegenPower>(choiceContext, base.Owner, 2m * base.Amount, base.Owner, null);
        
        // 获得3点格挡
        await CreatureCmd.GainBlock(base.Owner, 3m * base.Amount, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, null);
    }
}
