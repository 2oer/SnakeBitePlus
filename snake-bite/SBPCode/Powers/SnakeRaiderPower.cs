using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using CSharpSBP.SBP_Code.Cards;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：地咬狂蛇
// 卡牌描述：每当你抽到"蛇咬"时，自动将其对随机目标打出。
public sealed class SnakeRaiderPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    private const int MaxAutoplayPerChain = 10;
    
    private HashSet<CardModel>? _autoplayingCards;
    private int _autoplayChainCount;
    
    private HashSet<CardModel> AutoplayingCards
    {
        get
        {
            if (_autoplayingCards == null)
            {
                _autoplayingCards = new HashSet<CardModel>();
            }
            return _autoplayingCards;
        }
    }
    
    public override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        // 检查防卡死限制：单次连锁最多打出10张牌
        if (_autoplayChainCount >= MaxAutoplayPerChain)
        {
            return;
        }
        
        // 检查是否是自己的卡牌
        if (card.Owner.Creature != base.Owner)
        {
            return;
        }
        
        // 检查是否是蛇咬卡牌
        if (!(card is SnakeBitePlus_Card snakeCard && snakeCard.HasSnakeBiteTag))
        {
            return;
        }
        
        // 检查是否有可攻击的敌人
        if (base.Owner.CombatState.HittableEnemies.All((Creature c) => c.HpDisplay.IsInfinite()))
        {
            return;
        }
        
        // 自动打出卡牌
        Flash();
        AutoplayingCards.Add(card);
        
        // 增加连锁计数
        _autoplayChainCount++;
        try
        {
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
        finally
        {
            // 减少连锁计数（无论成功与否都执行）
            _autoplayChainCount--;
            AutoplayingCards.Remove(card);
        }
    }
}
