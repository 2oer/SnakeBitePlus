using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using CSharpSBP.SBP_Code.Cards.AttackCards;

namespace CSharpSBP.SBP_Code.Cards.SkillCards;

/*
卡牌名称：蛇者苏生
卡牌描述：向你的手牌中加入3张"王蛇咬剑"。每有一层中毒费用减少1。
升级效果：保留
*/
public sealed class SBP_skill11 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;

    private const int BaseCost = 6;
    private const int MinCost = 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Append(CardKeyword.Exhaust);

    // 悬浮提示：显示王蛇咬剑卡牌信息
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] {
        HoverTipFactory.FromCard<SBP_atk5>()
    };

    public SBP_skill11() : base(BaseCost, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new EnergyVar(1) };

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        AddKeyword(CardKeyword.Retain);
    }

    protected override int CanonicalEnergyCost => BaseCost;

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this || base.IsClone)
        {
            return Task.CompletedTask;
        }
        UpdateCostBasedOnPoison();
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner)
        {
            return Task.CompletedTask;
        }
        UpdateCostBasedOnPoison();
        return Task.CompletedTask;
    }

    private void UpdateCostBasedOnPoison()
    {
        var poisonPower = base.Owner.Creature.GetPower<PoisonPower>();
        int poisonStacks = poisonPower?.Amount ?? 0;
        int costReduction = poisonStacks * (int)base.DynamicVars.Energy.BaseValue;
        int newCost = BaseCost - costReduction;
        newCost = System.Math.Max(newCost, MinCost);

        base.EnergyCost.SetThisCombat(newCost);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        for (int i = 0; i < 3; i++)
        {
            var card = base.Owner.Creature.CombatState.CreateCard<SBP_atk5>(base.Owner);
            await CardPileCmd.Add(card, PileType.Hand);
        }

        await base.OnPlay(choiceContext, play);
    }
}

