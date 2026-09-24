using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.SkillCards;

/*
卡牌名称：咬判
卡牌描述：本回合每打出一张"蛇系"卡牌获得{Power:diff()}点格挡。
升级效果：格挡从2增加到4
*/
public sealed class SBP_skill13 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] 
    {
        new DynamicVar("Power", 2m)
    };
    
    public SBP_skill13() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) 
    {
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.DynamicVars["Power"].UpgradeValueBy(2m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SnakeBlockPower>(choiceContext, base.Owner.Creature, (int)base.DynamicVars["Power"].BaseValue, base.Owner.Creature, this);
        
        await base.OnPlay(choiceContext, play);
    }
}

