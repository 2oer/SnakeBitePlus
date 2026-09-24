using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace CSharpSBP.SBP_Code.Cards.SkillCards;

/*
卡牌名称：蛇频
卡牌描述：给予自身{SelfPoison:diff()}层中毒。抽{Card:diff()}张牌。
升级效果：抽牌数增加到4张
*/
//已完成
public sealed class SBP_skill9 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    
    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] {
        new DynamicVar("SelfPoison", 3m),
        new DynamicVar("Card", 3m)
      };
    
    public SBP_skill9() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) 
    {
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.DynamicVars["Card"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        
        int poisonAmount = (int)base.DynamicVars["SelfPoison"].BaseValue;
        await PowerCmd.Apply<PoisonPower>(choiceContext, base.Owner.Creature, poisonAmount, base.Owner.Creature, this);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Poison", base.Owner.Character.CastAnimDelay);
        await CardPileCmd.Draw(choiceContext, (int)base.DynamicVars["Card"].BaseValue, base.Owner);
        
        await base.OnPlay(choiceContext, play);
    }
}

