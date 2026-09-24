using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using CSharpSBP.SBP_Code.Cards;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：创蛇纪
// 卡牌描述：回合开始时升级手牌中的"蛇咬"。
public sealed class SnakeCreationPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        await base.AfterSideTurnStart(side, participants, combatState);
        
        if (side == base.Owner.Side)
        {
            // 获取手牌中的卡牌（使用 Owner.Player 获取玩家）
            var handCards = PileType.Hand.GetPile(base.Owner.Player).Cards;
            
            // 升级手牌中的蛇咬卡牌
            foreach (var card in handCards.Where(c => c is SnakeBitePlus_Card snakeCard && snakeCard.HasSnakeBiteTag && !c.IsUpgraded))
            {
                CardCmd.Upgrade(card);
            }
        }
    }
}   
