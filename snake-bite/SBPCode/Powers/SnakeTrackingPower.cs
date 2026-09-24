using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using CSharpSBP.SBP_Code.Cards;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：追踪之蛇
// 卡牌描述：每回合打出的第一张"蛇咬"，会被打出两次。
public sealed class SnakeTrackingPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        // 检查是否是自己的卡牌
        if (card.Owner.Creature != base.Owner)
        {
            return playCount;
        }
        
        // 检查是否是蛇咬卡牌
        if (!(card is SnakeBitePlus_Card snakeCard && snakeCard.HasSnakeBiteTag))
        {
            return playCount;
        }
        
        // 检查本回合已经触发了多少次
        int num = CombatManager.Instance.History.CardPlaysStarted.Count(
            (CardPlayStartedEntry e) => e.Actor == base.Owner && 
                                       e.CardPlay.IsFirstInSeries && 
                                       e.HappenedThisTurn(base.CombatState)
        );
        
        // 如果还没达到触发次数限制，增加一次打出次数
        if (num < base.Amount)
        {
            return playCount + 1;
        }
        
        return playCount;
    }
    
    public override Task AfterModifyingCardPlayCount(CardModel card)
    {
        Flash();
        return Task.CompletedTask;
    }
}
