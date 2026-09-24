using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpSBP.SBP_Code.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.PowerCards;

/*
卡牌名称：蛇子程序
卡牌描述：每回合第一次打出"蛇咬"时，获得(3){Energy:energyIcons()}，给予自身1层中毒。
升级效果：费用变为0
*/
public sealed class SBP_power13 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    
    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new EnergyVar(3) };
    
    public SBP_power13() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        EnergyCost.SetCustomBaseCost(0);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SnakegramPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
    }
}


