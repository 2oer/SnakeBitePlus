using System.Threading.Tasks;
using CSharpSBP.SBP_Code.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.PowerCards;

/*
卡牌名称：死蛇形态
卡牌描述：每次给予敌人中毒时，给予敌人等量灾厄。
升级效果：降低为1费
*/
public sealed class SBP_power12 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    
    public SBP_power12() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }
    
    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.EnergyCost.UpgradeBy(-1);
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<DeathSnakePower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
    }
}


