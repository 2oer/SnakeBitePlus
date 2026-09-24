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
卡牌名称：防咬
卡牌描述：获得{Block:diff()}点格挡。移除自身至多{RemoveAmount:diff()}层中毒。每移除一层中毒，额外获得{Block:diff()}点格挡。
升级效果：无（数值已在基础效果中体现）
*/
//已完成
public sealed class SBP_skill2 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    
    public SBP_skill2() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new BlockVar(3m, ValueProp.Move), new DynamicVar("RemoveAmount", 3m) };
    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.DynamicVars["RemoveAmount"].UpgradeValueBy(2m);
        AddKeyword(CardKeyword.Retain);
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // 基础格挡
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, play);
        
        var poisonPower = base.Owner.Creature.GetPower<PoisonPower>();
        int poisonStacks = poisonPower?.Amount ?? 0;
        
        int maxRemove = (int)base.DynamicVars["RemoveAmount"].BaseValue;
        int removeAmount = System.Math.Min(poisonStacks, maxRemove);
        
        // 逐次移除中毒，每次移除一层就获得一次格挡（保证能被敏捷效果多倍加持）
        for (int i = 0; i < removeAmount; i++)
        {
            var currentPoison = base.Owner.Creature.GetPower<PoisonPower>();
            if (currentPoison != null && currentPoison.Amount > 0)
            {
                await PowerCmd.Decrement(currentPoison);
                await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, play);
            }
        }
    }
}

