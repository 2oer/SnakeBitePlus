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
卡牌名称：子蛇序
卡牌描述：每次打出"蛇咬"时获得{Energy:energyIcons()}。
升级效果：费用变为0
*/
//已完成
public sealed class SBP_power7 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    
    public SBP_power7() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) { }
    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new EnergyVar(1) };
    
    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        EnergyCost.SetCustomBaseCost(0);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SnakeProgramPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
    }
}


