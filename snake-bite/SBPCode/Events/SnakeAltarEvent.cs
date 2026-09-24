using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using CSharpSBP.SBP_Code.Cards.PowerCards;
using CSharpSBP.SBP_Code.Cards.SkillCards;

namespace CSharpSBP.SBP_Code.Events;

/// <summary>
/// SnakeAltarEvent - 蛇神祭坛事件
/// 一层必定出现的随机事件，玩家可以选择获得先古卡牌
/// </summary>
public sealed class SnakeAltarEvent : EventModel
{
    [SavedProperty]
    private bool _hasTriggeredThisRun;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> options = new List<EventOption>();

        // 如果已经触发过，只显示离开选项
        if (_hasTriggeredThisRun)
        {
            options.Add(new EventOption(this, async () =>
            {
                SetEventFinished(L10NLookup("SNAKE_ALTAR.pages.INITIAL.options.leave_result"));
                await Task.CompletedTask;
            }, "SNAKE_ALTAR.pages.INITIAL.options.leave"));
            
            return options;
        }

        // 选项1：获得封咬蛇座
        options.Add(new EventOption(this, async () =>
        {
            await AddAncientCardToDeck<SBP_power3>();
            _hasTriggeredThisRun = true;
            SetEventFinished(L10NLookup("SNAKE_ALTAR.pages.INITIAL.options.power3_result"));
        }, "SNAKE_ALTAR.pages.INITIAL.options.power3"));

        // 选项2：获得至亮之咬
        options.Add(new EventOption(this, async () =>
        {
            await AddAncientCardToDeck<SBP_skill14>();
            _hasTriggeredThisRun = true;
            SetEventFinished(L10NLookup("SNAKE_ALTAR.pages.INITIAL.options.skill14_result"));
        }, "SNAKE_ALTAR.pages.INITIAL.options.skill14"));

        // 选项3：获得禁忌蛇典
        options.Add(new EventOption(this, async () =>
        {
            await AddAncientCardToDeck<SBP_power6>();
            _hasTriggeredThisRun = true;
            SetEventFinished(L10NLookup("SNAKE_ALTAR.pages.INITIAL.options.power6_result"));
        }, "SNAKE_ALTAR.pages.INITIAL.options.power6"));

        // 选项4：获得50金币
        options.Add(new EventOption(this, async () =>
        {
            await MegaCrit.Sts2.Core.Commands.PlayerCmd.GainGold(50, Owner);
            _hasTriggeredThisRun = true;
            SetEventFinished(L10NLookup("SNAKE_ALTAR.pages.INITIAL.options.gold_result"));
        }, "SNAKE_ALTAR.pages.INITIAL.options.gold"));

        return options;
    }

    private async Task AddAncientCardToDeck<T>() where T : CardModel
    {
        if (Owner != null)
        {
            var card = ModelDb.Card<T>().ToMutable();
            // 将卡牌升级
            card.UpgradeInternal();
            Owner.RunState.AddCard(card, Owner);
            await MegaCrit.Sts2.Core.Commands.CardPileCmd.Add(card, MegaCrit.Sts2.Core.Entities.Cards.PileType.Deck);
        }
    }
}


