using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace CSharpSBP.SBP_Code.Cards.SkillCards;

/*
卡牌名称：咬借蛇间
卡牌描述：给予自身{SelfPoison:diff()}层中毒。获得{Energy:energyIcons()}点能量。
升级效果：中毒层数从3减少到2
*/
public sealed class SBP_skill12 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    
    public SBP_skill12() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) 
    {
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new DynamicVar("SelfPoison", 3m), new EnergyVar(2) };
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    => new[] { base.EnergyHoverTip, HoverTipFactory.FromPower<PoisonPower>() };
    
    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.DynamicVars["SelfPoison"].UpgradeValueBy(-1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.AttackAnimDelay);
        await PowerCmd.Apply<PoisonPower>(choiceContext, base.Owner.Creature, (int)base.DynamicVars["SelfPoison"].BaseValue, base.Owner.Creature, this);
        await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
        
        await base.OnPlay(choiceContext, play);
    }
}

