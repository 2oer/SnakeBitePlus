using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using CSharpSBP.SBP_Code.Cards.Nuke;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：灭世流程
// 效果：倒计时3回合，消耗所有手牌并将1张核打击加入手牌
// 在倒计时期间无法从卡牌获取格挡，回合结束时自动获得30点格挡
public sealed class ApocalypseProtocolPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        await base.AfterSideTurnStart(side, participants, combatState);

        if (side != base.Owner.Side) return;

        // 减少倒计时（倒数）
        await PowerCmd.Decrement(this);

        // 检查倒计时是否结束（达到0）
        if (base.Amount <= 0m)
        {
            // 获取玩家手牌
            var handCards = PileType.Hand.GetPile(base.Owner.Player).Cards.ToList();

            // 消耗所有手牌 - 使用移除卡牌并添加到消耗堆的方式
            foreach (var card in handCards)
            {
                await CardPileCmd.RemoveFromCombat(card);
                await CardPileCmd.Add(card, PileType.Exhaust);
            }

            // 将1张核打击加入手牌
            CardModel nukeStrikeCard = base.CombatState.CreateCard<NukeStrike>(base.Owner.Player);
            await CardPileCmd.Add(nukeStrikeCard, PileType.Hand);

            // 移除自身（一次性效果）
            await PowerCmd.Remove(this);
        }
    }

    // 在倒计时期间无法从卡牌获取格挡
    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != base.Owner)
        {
            return 1m;
        }
        if (props.HasFlag(ValueProp.Unpowered))
        {
            return 1m;
        }
        if (cardSource == null)
        {
            return 1m;
        }
        // 在倒计时期间（Amount > 0）阻止从卡牌获取格挡
        if (base.Amount > 0m)
        {
            return 0m;
        }
        return 1m;
    }

    // 回合结束时自动获得30格挡（在倒计时期间）
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await base.AfterSideTurnEnd(choiceContext, side, participants);

        if (side != base.Owner.Side) return;

        // 在倒计时期间（Amount > 0）回合结束时获得30格挡
        if (base.Amount > 0m)
        {
            await CreatureCmd.GainBlock(base.Owner, 30m, ValueProp.Unpowered, null);
        }
    }
}