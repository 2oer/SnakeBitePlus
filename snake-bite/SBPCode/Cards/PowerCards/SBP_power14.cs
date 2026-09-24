using System.Threading.Tasks;
using CSharpSBP.SBP_Code.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.PowerCards;

/*
卡牌名称：创蛇纪
卡牌描述：回合开始时升级手牌中的"蛇咬"。
升级效果：获得[固有]关键词
*/
public sealed class SBP_power14 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    
    public SBP_power14() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        AddKeyword(CardKeyword.Innate);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SnakeCreationPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
    }
}


