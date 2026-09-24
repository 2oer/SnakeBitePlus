using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace CSharpSBP.SBP_Code.Cards.SkillCards;

/*
卡牌名称：蛇毒墙
卡牌描述：自身拥有超过4层中毒才能被打出，去除{RemoveAmount:diff()}层中毒并获得{Block:diff()}点格挡，将此牌返回你的手牌。
升级效果：格挡从8增加到11，去除中毒层数从5增加到7
*/
public sealed class SBP_skill5 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    private const int MinPoisonToPlay = 4;
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    base.CanonicalKeywords.Append(CardKeyword.Innate);


    public SBP_skill5() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new BlockVar("Block", 8m, ValueProp.Move);
            yield return new DynamicVar("RemoveAmount", 5m);
        }
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.DynamicVars.Block.UpgradeValueBy(3m);
        base.DynamicVars["RemoveAmount"].UpgradeValueBy(2m);
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

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var poisonPower = base.Owner.Creature.GetPower<PoisonPower>();
        int poisonStacks = poisonPower?.Amount ?? 0;
        if (poisonStacks < MinPoisonToPlay || poisonPower == null)
        {
            return;
        }
        int removeAmount = Math.Min((int)base.DynamicVars["RemoveAmount"].BaseValue, poisonStacks);

        await PowerCmd.ModifyAmount(choiceContext, poisonPower, -removeAmount, base.Owner.Creature, this);
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);

        await base.OnPlay(choiceContext, cardPlay);
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        PileType resultPileType = base.GetResultPileTypeForCardPlay();
        if (resultPileType != PileType.Discard)
        {
            return resultPileType;
        }
        return PileType.Hand;
    }
}
