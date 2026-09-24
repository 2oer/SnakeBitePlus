using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using CSharpSBP.SBP_Code.Cards;
using CSharpSBP.SBP_Code.Cards.Nuke;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.Nuke;

// 卡牌名称：核打击
// 卡牌描述：打出时加入3张核掩体。回合结束时对场上所有生物造成三次的不可阻挡伤害（234，235和238）。
// 升级效果：消耗2点能量
// 特性：必须优先打出（类似受缚），保留（不会消耗）
public sealed class NukeStrike : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => false;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DamageVar(235m, ValueProp.Move) };

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Append(CardKeyword.Retain);

    public override int MaxUpgradeLevel => 0;

    protected override bool ShouldGlowRedInternal => true;

    // 悬浮提示：显示核掩体卡牌信息
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] {
        HoverTipFactory.FromCard<NukeBunker>()
    };

    public NukeStrike() : base(3, CardType.Attack, CardRarity.Token, TargetType.None) { }
    
    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.EnergyCost.SetCustomBaseCost(2);
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        // 必须优先打出（类似受缚卡牌）
        return true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 加入1张核掩体token到手牌
        CardModel bunkerCard = base.CombatState.CreateCard<NukeBunker>(base.Owner);
        await CardPileCmd.Add(bunkerCard, PileType.Hand);

        // 施加核打击能力，用于在回合结束时造成伤害
        decimal damage = base.DynamicVars.Damage.BaseValue;
        await PowerCmd.Apply<NukeStrikePower>(choiceContext, base.Owner.Creature, damage, base.Owner.Creature, this);
    }
}