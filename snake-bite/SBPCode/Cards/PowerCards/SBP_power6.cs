using System.Threading.Tasks;
using CSharpSBP.SBP_Code.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.PowerCards;

/*
卡牌名称：禁忌蛇典
卡牌描述：战斗胜利后额外获得一组只有蛇系卡牌的卡牌奖励，可以选择一张非蛇系卡牌移除。
升级效果：额外的卡牌奖励变为升级状态
*/
//待测试
public sealed class SBP_power6 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    
    public SBP_power6() : base(1, CardType.Power, CardRarity.Ancient, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SnakeTreasurePower>(choiceContext, base.Owner.Creature, IsUpgraded ? 2 : 1, base.Owner.Creature, this);
    }
}


