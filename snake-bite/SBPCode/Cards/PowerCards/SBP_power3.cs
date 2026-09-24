using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using CSharpSBP.SBP_Code.Cards.AttackCards;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.PowerCards;

/*
卡牌名称：封咬蛇座
卡牌描述：每打出3张蛇系卡牌，将一张升级的王蛇咬剑加入手牌。
升级效果：获得[固有]关键词
说明：此卡为Ancient稀有度，作为战胜Boss后先古之民遗物奖励附带的卡牌
*/
//已完成
public sealed class SBP_power3 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    // “封咬蛇座”名字中不含“蛇咬”子串，属于“蛇”系但不属于“蛇咬”系
    public override bool HasSnakeBiteTag => false;
    // 悬浮提示：显示王蛇咬剑卡牌信息
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] {
        HoverTipFactory.FromCard<SBP_atk5>()
    };
    
    public SBP_power3() : base(1, CardType.Power, CardRarity.Ancient, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        // 初始层数为1（PowerCmd.Apply在amount==0时不会施加能力）
        // 实际计数由动态变量SnakeCount管理
        await PowerCmd.Apply<SnakeSwordPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        AddKeyword(CardKeyword.Innate);
        AddKeyword(CardKeyword.Retain);
    }
}

