using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using CSharpSBP.SBP_Code.Cards;
using CSharpSBP.SBP_Code.Cards.AttackCards;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：封咬蛇座
// 卡牌描述：每打出3张蛇系卡牌，将一张升级的王蛇咬剑加入手牌。
public sealed class SnakeSwordPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 定义动态变量用于计数
    protected override IEnumerable<DynamicVar> CanonicalVars => 
        new[] { new DynamicVar("SnakeCount", 0m) };

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 检查是否是玩家打出的卡牌
        if (cardPlay.Card.Owner != base.Owner.Player)
        {
            return;
        }
        
        // 检查是否是蛇系卡牌
        if (cardPlay.Card is SnakeBitePlus_Card snakeCard && snakeCard.HasSnakeTag)
        {
            // 获取当前计数（使用动态变量）
            int currentCount = (int)base.DynamicVars["SnakeCount"].BaseValue;
            
            // 增加计数
            int newCount = currentCount + 1;
            
            // 每3张蛇系卡牌生成一张升级的王蛇咬剑
            if (newCount >= 3)
            {
                Flash();
                // 创建升级的王蛇咬剑卡牌并加入手牌
                var swordCard = base.Owner.CombatState.CreateCard<SBP_atk5>(base.Owner.Player);
                CardCmd.Upgrade(swordCard);
                await CardPileCmd.Add(swordCard, PileType.Hand);
                
                // 重置计数器为0
                newCount = 0;
            }
            
            // 更新动态变量
            base.DynamicVars["SnakeCount"].BaseValue = newCount;
        }
    }
}
