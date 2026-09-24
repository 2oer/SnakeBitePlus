using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using CSharpSBP.SBP_Code.Tags;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：禁忌蛇典
// 卡牌描述：战斗胜利后额外获得一组只有蛇系卡牌的卡牌奖励。可以选择一张非蛇系卡牌移除。
public sealed class SnakeTreasurePower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await base.AfterCombatEnd(room);
        
        for (int i = 0; i < base.Amount; i++)
        {
            // 获取所有蛇系卡牌（HasSnakeTag == true）
            var snakeCards = ModelDb.CardPool<SilentCardPool>()
                .GetUnlockedCards(base.Owner.Player.UnlockState, base.Owner.Player.RunState.CardMultiplayerConstraint)
                .Where(c => c is ISnakeTagged tagged && tagged.HasSnakeTag);

            // 使用自定义卡池创建奖励选项
            var options = CardCreationOptions.ForNonCombatWithDefaultOdds(snakeCards);
            room.AddExtraReward(base.Owner.Player, new CardReward(options, 3, base.Owner.Player));

            // 让玩家选择移除一张非蛇系卡牌（可跳过）
            var prefs = new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 0, 1)
            {
                Cancelable = true
            };

            var selectedCards = await CardSelectCmd.FromDeckForRemoval(base.Owner.Player, prefs,
                c => !(c is ISnakeTagged tagged && tagged.HasSnakeTag));

            var cardToRemove = selectedCards.FirstOrDefault();
            if (cardToRemove != null)
            {
                await CardPileCmd.RemoveFromDeck(cardToRemove);
            }
        }
    }
}