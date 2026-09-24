using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace CSharpSBP.SBP_Code.Cards.SkillCards;

/*
卡牌名称：蛇候未到
卡牌描述：消耗。移除自身至多{RemoveAmount:diff()}层中毒，每移除一层回复1点生命值。回合开始时，如果自身拥有超过20层中毒，将这张牌从消耗牌堆放回手牌，费用增加{Energy:energyIcons()}。
升级效果：最多可移除20层中毒
*/
public sealed class SBP_skill6 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Append(CardKeyword.Exhaust);

    public SBP_skill6() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new EnergyVar(2), new DynamicVar("RemoveAmount", 15m) };
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    => new[] { base.EnergyHoverTip };

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.DynamicVars["RemoveAmount"].UpgradeValueBy(5m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var poisonPower = base.Owner.Creature.GetPower<PoisonPower>();
        int poisonStacks = poisonPower?.Amount ?? 0;
        int maxRemove = (int)base.DynamicVars["RemoveAmount"].BaseValue;
        int removeAmount = System.Math.Min(poisonStacks, maxRemove);

        if (removeAmount > 0)
        {
            await PowerCmd.Remove<PoisonPower>(base.Owner.Creature);
            await PowerCmd.Apply<PoisonPower>(choiceContext, base.Owner.Creature, poisonStacks - removeAmount, base.Owner.Creature, this);
            await CreatureCmd.Heal(base.Owner.Creature, removeAmount);
        }

        await base.OnPlay(choiceContext, play);
    }

    // 参考 Bombardment.BeforeHandDraw 实现
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        CardPile? pile = base.Pile;
        if (pile != null && pile.Type == PileType.Exhaust && player == base.Owner)
        {
            // 检查是否中毒超过20层
            var poisonPower = base.Owner.Creature.GetPower<PoisonPower>();
            if (poisonPower != null && poisonPower.Amount > 20)
            {
                // 将费用增加2
                base.EnergyCost.UpgradeBy((int)base.DynamicVars["Energy"].IntValue);
                // 将这张牌从消耗牌堆放回手牌
                await CardPileCmd.Add(this, PileType.Hand);
            }
        }
    }
}

