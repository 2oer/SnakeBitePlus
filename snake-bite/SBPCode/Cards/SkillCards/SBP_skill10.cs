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
卡牌名称：蛇上腺素
卡牌描述：只能在拥有超过5层中毒时打出。移除自身所有中毒。获得{Energy:energyIcons()}。抽2张牌。
升级效果：能量从1增加到2
*/
public sealed class SBP_skill10 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    
    private const int MinPoisonToPlay = 5;
    private const int DrawAmount = 2;
    
    public SBP_skill10() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) 
    {
    }
    
    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new EnergyVar(2) };
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    => new[] { base.EnergyHoverTip };

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.DynamicVars.Energy.UpgradeValueBy(1m);
    }

    protected override bool IsPlayable
    {
        get
        {
            var poisonPower = base.Owner?.Creature?.GetPower<PoisonPower>();
            int poisonStacks = poisonPower?.Amount ?? 0;
            return poisonStacks >= MinPoisonToPlay;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var poisonPower = base.Owner.Creature.GetPower<PoisonPower>();
        int poisonStacks = poisonPower?.Amount ?? 0;
        
        if (poisonStacks >= MinPoisonToPlay)
        {
            await PowerCmd.Remove<PoisonPower>(base.Owner.Creature);
            await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
            await CardPileCmd.Draw(choiceContext, DrawAmount, base.Owner);
        }
        else
        {
            return;
        }
        
        await base.OnPlay(choiceContext, play);
    }
}

