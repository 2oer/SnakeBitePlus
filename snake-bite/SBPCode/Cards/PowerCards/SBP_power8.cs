using System.Threading.Tasks;
using CSharpSBP.SBP_Code.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.PowerCards;

/*
卡牌名称：追踪之蛇
卡牌描述：每回合打出的第一张"蛇咬"，会被打出两次。
升级效果：费用从2减少到1
*/
public sealed class SBP_power8 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    
    public SBP_power8() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        EnergyCost.SetCustomBaseCost(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SnakeTrackingPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
    }
}


