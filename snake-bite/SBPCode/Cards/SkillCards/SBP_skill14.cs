using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using CSharpSBP.SBP_Code.Powers;
using CSharpSBP.SBP_Code.Tags;

namespace CSharpSBP.SBP_Code.Cards.SkillCards;

/*
卡牌名称：至亮之咬
卡牌描述：获得{Energy:energyIcons()}。抽{Cards:diff()}张牌。回合开始时给予自身2层中毒。
升级效果：能量从2增加到3，抽牌从2增加到3，保留
稀有度：Ancient（先古）
*/
public sealed class SBP_skill14 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => false;
    public override bool HasSnakeBiteTag => false;
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Append(CardKeyword.Exhaust);
    
    public SBP_skill14() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self) { }
    
    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] {
                new EnergyVar(2), 
                new DynamicVar("Cards", 2m) };
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
        new[] { base.EnergyHoverTip, HoverTipFactory.FromPower<PoisonPower>() };

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.DynamicVars.Energy.UpgradeValueBy(1m);
        base.DynamicVars["Cards"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // 获得能量
        await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
        
        // 抽牌
        await CardPileCmd.Draw(choiceContext, (int)base.DynamicVars.Cards.BaseValue, base.Owner);
        
        // 施加每回合开始给予中毒的能力
        await PowerCmd.Apply<PoisonEachTurnPower>(choiceContext, base.Owner.Creature, 2, base.Owner.Creature, this);
        
        await base.OnPlay(choiceContext, play);
    }
}


