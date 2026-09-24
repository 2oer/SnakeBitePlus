using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpSBP.SBP_Code.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.PowerCards;

/*
卡牌名称：蛇毒血清
卡牌描述：每次给予自身中毒时获得2点再生和3点格挡。
升级效果：去除[虚无]关键词
*/
public sealed class SBP_power9 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Append(CardKeyword.Ethereal);

    public SBP_power9() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        RemoveKeyword(CardKeyword.Ethereal);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SnakeVenomSerumPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
    }
}


