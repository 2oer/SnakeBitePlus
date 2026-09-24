using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using CSharpSBP.SBP_Code.Cards;
using CSharpSBP.SBP_Code.Cards.Nuke;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.Nuke;

// 卡牌名称：灭世流程
// 卡牌描述：倒计时3回合，消耗所有手牌并将1张核打击加入手牌，期间无法从卡牌中获取格挡，但是回合结束时自动获得30点格挡。
// 关键词：虚无、消耗
// 升级效果：移除虚无关键词,获得固有关键词
// 已禁用：设置 CanBeGeneratedInCombat = false 这张卡无法在战斗中被生成
public sealed class ApocalypseProtocol : SnakeBitePlus_Card
{
    public override bool CanBeGeneratedInCombat => false;

    public override bool HasSnakeTag => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Append(CardKeyword.Exhaust).Append(CardKeyword.Ethereal);

    // 悬浮提示：显示核打击和核掩体卡牌信息
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] {
        HoverTipFactory.FromCard<NukeStrike>(),
        HoverTipFactory.FromCard<NukeBunker>()
    };

    public ApocalypseProtocol() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None) { }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        RemoveKeyword(CardKeyword.Ethereal);
        AddKeyword(CardKeyword.Innate);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 施加灭世流程能力，倒计时3回合后触发效果（初始值为3，回合开始时递减）
        await PowerCmd.Apply<ApocalypseProtocolPower>(choiceContext, base.Owner.Creature, 3m, base.Owner.Creature, this);
    }
}