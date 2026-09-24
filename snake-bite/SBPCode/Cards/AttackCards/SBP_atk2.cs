using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace CSharpSBP.SBP_Code.Cards.AttackCards;

/*
卡牌名称：咬面蛇击
卡牌描述：本回合每打出一张"蛇咬"卡牌，对目标敌人造成{Damage:diff()}点伤害。{InCombat:\n（命中{CalculatedHits:diff()}次）|}
升级效果：伤害值+1
*/
//已完成
public sealed class SBP_atk2 : SnakeBitePlus_Card
{
    private const string _calculatedHitsKey = "CalculatedHits";
    
    public override bool HasSnakeTag => true;
    // “咬面蛇击”名字中不含“蛇咬”子串，属于“蛇”系但不属于“蛇咬”系
    public override bool HasSnakeBiteTag => false;
    
    public SBP_atk2() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
    {
        new DamageVar(7m, ValueProp.Move),
        new CalculationBaseVar(0m),  // 基础打击次数为0
        new CalculationExtraVar(1m),
        new CalculatedVar(_calculatedHitsKey).WithMultiplier(CountSnakeBiteCards)
    };
    private static decimal CountSnakeBiteCards(CardModel card, Creature? _)
    {
        return CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => 
            e.HappenedThisTurn(card.CombatState) && 
            e.CardPlay.Card.Owner == card.Owner &&
            ((e.CardPlay.Card is SnakeBitePlus_Card snakeCard && snakeCard.HasSnakeBiteTag) ||
             (e.CardPlay.Card is MegaCrit.Sts2.Core.Models.Cards.Snakebite))
        );
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
            return;
            
        int hitCount = (int)((CalculatedVar)base.DynamicVars[_calculatedHitsKey]).Calculate(cardPlay.Target);
        
        // 播放一次横扫光束动画
        NSweepingBeamVfx nSweepingBeamVfx = NSweepingBeamVfx.Create(base.Owner.Creature, new List<Creature> { cardPlay.Target });
        if (nSweepingBeamVfx != null)
        {
            NCombatRoom.Instance?.CombatVfxContainer.AddChild(nSweepingBeamVfx);
            await Task.Delay(100);
        }
        
        // 连续造成伤害（使用 WithHitCount 实现多次命中）
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
    }
}
