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
卡牌名称：蛇肩无视
卡牌描述：给予自身{SelfPoison:diff()}层中毒。获得{Block:diff()}点格挡。抽1张牌。
升级效果：格挡从8增加到12
*/
//已完成
public sealed class SBP_skill7 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    

    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new BlockVar(8m, ValueProp.Move), new DynamicVar("SelfPoison", 2m) };
    
    public SBP_skill7() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) 
    {
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.DynamicVars["SelfPoison"].UpgradeValueBy(-1m);
        base.DynamicVars.Block.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<PoisonPower>(choiceContext, base.Owner.Creature, 2, base.Owner.Creature, this);
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, play);
        await CardPileCmd.Draw(choiceContext, 1, base.Owner);
        
        await base.OnPlay(choiceContext, play);
    }
}

