using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes;
using CSharpSBP.SBP_Code.Cards.Nuke;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：核打击
// 效果：回合结束时对场上所有生物造成三次不可阻挡伤害（234，235和238）；回合开始时消耗所有手牌并将1张核掩体加入手牌
public sealed class NukeStrikePower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private const string _explosionSfx = "event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_eruption";

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        await base.AfterSideTurnStart(side, participants, combatState);

        if (side != base.Owner.Side)
        {
            return;
        }

        // 消耗所有手牌
        var handCards = PileType.Hand.GetPile(base.Owner.Player).Cards.ToList();
        foreach (var card in handCards)
        {
            await CardPileCmd.RemoveFromCombat(card);
            await CardPileCmd.Add(card, PileType.Exhaust);
        }

        // 将1张核掩体加入手牌
        CardModel nukeBunkerCard = base.CombatState.CreateCard<NukeBunker>(base.Owner.Player);
        await CardPileCmd.Add(nukeBunkerCard, PileType.Hand);
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await base.BeforeSideTurnEnd(choiceContext, side, participants);

        if (side != base.Owner.Side)
        {
            return;
        }

        // 检查是否有敌人
        if (!base.Owner.CombatState.Enemies.Any(e => e.IsAlive))
        {
            return;
        }

        decimal baseDamage = base.Amount;

        // 第一次爆炸
        await PerformExplosion(choiceContext, baseDamage - 1m);

        // 检测是否还有敌人存活
        if (!base.Owner.CombatState.Enemies.Any(e => e.IsAlive))
        {
            return;
        }

        await Task.Delay(600);

        // 第二次爆炸
        await PerformExplosion(choiceContext, baseDamage);

        // 检测是否还有敌人存活
        if (!base.Owner.CombatState.Enemies.Any(e => e.IsAlive))
        {
            return;
        }

        await Task.Delay(600);

        // 第三次爆炸
        await PerformExplosion(choiceContext, baseDamage + 3m);
    }

    private async Task PerformExplosion(PlayerChoiceContext choiceContext, decimal damageAmount)
    {
        List<Creature> aliveCreatures = GetAllAliveCreatures();

        // 播放爆炸音效（音量为原来的3倍）
        SfxCmd.Play(_explosionSfx, 3f);
        await Task.Delay(300);

        // 在第一个敌人中心播放组合爆炸特效
        Creature firstEnemy = aliveCreatures.FirstOrDefault(c => !c.IsPlayer);
        if (firstEnemy != null)
        {
            var creatureNode = NCombatRoom.Instance?.GetCreatureNode(firstEnemy);
            if (creatureNode != null)
            {
                Vector2 targetPosition = creatureNode.VfxSpawnPosition;

                // 1. 播放 NDoomVfx 末日特效
                NDoomVfx doomVfx = NDoomVfx.Create(
                    creatureNode.Visuals,
                    targetPosition,
                    new Vector2(100, 100),
                    shouldDie: false
                );
                NCombatRoom.Instance?.CombatVfxContainer.AddChild(doomVfx);

                // 延迟后播放火焰爆发特效
                await Task.Delay(150);

                // 2. 播放 3倍大小的 NFireBurstVfx 火焰爆发特效
                NFireBurstVfx fireBurstVfx = NFireBurstVfx.Create(firstEnemy, 2.7f);
                NCombatRoom.Instance?.CombatVfxContainer.AddChild(fireBurstVfx);

                // 延迟后对所有敌人播放地面火焰特效
                await Task.Delay(60);

                // 3. 对所有存活敌人播放 NGroundFireVfx 地面火焰特效
                foreach (var creature in aliveCreatures)
                {
                    if (!creature.IsPlayer && creature.IsAlive)
                    {
                        NGroundFireVfx groundFireVfx = NGroundFireVfx.Create(creature, VfxColor.Red);
                        NCombatRoom.Instance?.CombatVfxContainer.AddChild(groundFireVfx);
                    }
                }

                // 延迟后播放最终特效
                await Task.Delay(100);

                // 4. 同时播放 NSplashVfx 和 NGaseousImpactVfx（替代原烟雾特效）
                NSplashVfx splashVfx = NSplashVfx.Create(targetPosition, new Color("#ff4400"));
                NGaseousImpactVfx gaseousVfx = NGaseousImpactVfx.Create(firstEnemy, new Color("#9595995e"));

                NCombatRoom.Instance?.CombatVfxContainer.AddChild(splashVfx);
                NCombatRoom.Instance?.CombatVfxContainer.AddChild(gaseousVfx);
            }
        }

        // 延迟后造成伤害
        await Task.Delay(600);

        foreach (var creature in aliveCreatures)
        {
            if (creature.IsAlive)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    creature,
                    damageAmount,
                    ValueProp.Unblockable | ValueProp.Unpowered,
                    null,
                    null
                );
            }
        }
    }

    private List<Creature> GetAllAliveCreatures()
    {
        List<Creature> creatures = new List<Creature>();

        // 添加玩家方生物
        foreach (var player in base.Owner.CombatState.Players)
        {
            if (player.Creature != null && player.Creature.IsAlive)
            {
                creatures.Add(player.Creature);
            }
        }

        // 添加敌方生物
        creatures.AddRange(base.Owner.CombatState.Enemies.Where(e => e.IsAlive));

        return creatures;
    }
}
