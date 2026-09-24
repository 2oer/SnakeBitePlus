using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.AttackCards;

/*
卡牌名称：散蛇炮
卡牌描述：移除自身所有中毒。每移除一层中毒，对所有敌人施加{Poison:diff()}层中毒。立即触发一次所有敌人的身上的中毒效果。
升级效果：保留
*/
//已完成
public sealed class SBP_atk1 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    protected override IEnumerable<DynamicVar> CanonicalVars 
    => new[] { 
        new DynamicVar("Poison", 3m),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedPoison").WithMultiplier(CalculatePoisonAmount)
    };
    
    private static decimal CalculatePoisonAmount(CardModel card, Creature? _)
    {
        return (card.Owner.Creature.GetPower<PoisonPower>()?.Amount ?? 0) * card.DynamicVars["Poison"].BaseValue;
    }
    
    public SBP_atk1() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) { }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        AddKeyword(CardKeyword.Retain);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var poisonPower = base.Owner.Creature.GetPower<PoisonPower>();
        int playerPoisonStacks = poisonPower?.Amount ?? 0;
        // 获取所有敌人（在外部定义，以便后面代码可以访问）
        var aliveEnemies = base.CombatState.Enemies.Where(e => e.IsAlive).ToList();
        
        //存在中毒才会触发这个效果
        if (playerPoisonStacks >= 0)
        {
            // 移除中毒
            await PowerCmd.Remove<PoisonPower>(base.Owner.Creature);
            // 遍历敌人施加中毒
            if (aliveEnemies.Any())
            {
                for(int i = 0; i < playerPoisonStacks; i++)
                {
                    int poisonAmount = (int)base.DynamicVars["Poison"].BaseValue;
                    await PowerCmd.Apply<PoisonPower>(choiceContext, aliveEnemies, poisonAmount, base.Owner.Creature, this);
                }
            }
        }
        // 立即触发所有敌人的中毒效果（使用超光束特效）
        // 播放攻击动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", 0.5f);
        
        // 1. 对第一个敌人播放超光束特效
        if (aliveEnemies.Any())
        {
            NHyperbeamVfx hyperbeamVfx = NHyperbeamVfx.Create(base.Owner.Creature, aliveEnemies.First());
            if (hyperbeamVfx != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChild(hyperbeamVfx);
                // 等待超光束特效播放完成
                await Cmd.Wait(0.5f);
            }
            
            // 2. 对所有敌人播放冲击特效
            foreach (var enemy in aliveEnemies)
            {
                NHyperbeamImpactVfx impactVfx = NHyperbeamImpactVfx.Create(base.Owner.Creature, enemy);
                if (impactVfx != null)
                {
                    NCombatRoom.Instance?.CombatVfxContainer.AddChild(impactVfx);
                }
            }
        }
        
        // 对所有敌人同时造成伤害并减少一层中毒
        foreach (var enemy in aliveEnemies)
        {
            var enemyPoison = enemy.GetPower<PoisonPower>();
            if (enemyPoison != null && enemyPoison.Amount > 0)
            {
                await CreatureCmd.Damage(choiceContext, enemy, (decimal)enemyPoison.Amount, ValueProp.Unblockable | ValueProp.Unpowered, base.Owner.Creature, this);
                if (enemy.IsAlive)
                {
                    await PowerCmd.Decrement(enemyPoison);
                }
            }
        }
    }
}


