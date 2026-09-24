using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using CSharpSBP.SBP_Code.Cards;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：凶蛇恶咬SBP_power1
// 卡牌描述：每次打出一张"蛇咬"卡牌时，抽{amount:diff()}张牌。
public sealed class DrawCardsPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // 检查是否是玩家打出的卡牌
        if (cardPlay.Card.Owner != base.Owner.Player)
        {
            return;
        }
        
        // 检查是否是蛇咬卡牌
        if (cardPlay.Card is SnakeBitePlus_Card snakeCard && snakeCard.HasSnakeBiteTag )
        {
            // 抽牌
            await CardPileCmd.Draw(context, base.Amount, base.Owner.Player);
        }
    }
}
