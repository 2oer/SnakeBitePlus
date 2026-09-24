using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using CSharpSBP.SBP_Code.Cards;

namespace CSharpSBP.SBP_Code.Cards.Nuke;

// 卡牌名称：核掩体
// 卡牌描述：获得3层[gold]缓冲[/gold]。
// 回合结束时消耗。
// 类型：Status（状态牌）
public sealed class NukeBunker : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords
     => base.CanonicalKeywords.Append(CardKeyword.Exhaust).Append(CardKeyword.Retain);

    public NukeBunker() : base(0, CardType.Status, CardRarity.Curse, TargetType.Self) { }

    public override int MaxUpgradeLevel => 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得3层缓冲
        await PowerCmd.Apply<BufferPower>(choiceContext, base.Owner.Creature, 3m, base.Owner.Creature, this);
    }
}