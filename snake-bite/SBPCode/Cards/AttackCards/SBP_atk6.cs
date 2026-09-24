using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace CSharpSBP.SBP_Code.Cards.AttackCards;

/*
卡牌名称：蛇击
卡牌描述：造成{Damage:diff()}点伤害。移除自身至多3层中毒。每移除一层中毒，额外对所有敌人造成{Damage:diff()}点伤害。
升级效果：最多可移除4层中毒
*/
//已完成
public sealed class SBP_atk6 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    
    public SBP_atk6() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[]{new DamageVar(3m, ValueProp.Move), new DynamicVar("RemoveAmount", 3m)};

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.DynamicVars["RemoveAmount"].UpgradeValueBy(2m);
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
            return;
            
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(choiceContext);
        
        var poisonPower = base.Owner.Creature.GetPower<PoisonPower>();
        int poisonStacks = poisonPower?.Amount ?? 0;
        
        int maxRemove = (int)base.DynamicVars["RemoveAmount"].BaseValue;
        int removeAmount = System.Math.Min(poisonStacks, maxRemove);
        
        if (removeAmount > 0)
        {
            // 移除指定层数的中毒（至多3层，升级后4层），而不是移除全部
            await PowerCmd.ModifyAmount(choiceContext, poisonPower, -removeAmount, base.Owner.Creature, null);
            // 使用 Radiate 动画效果：对全体敌人造成多次伤害
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .WithHitCount(removeAmount)
                .FromCard(this)
                .TargetingAllOpponents(base.CombatState)
                .WithHitFx("vfx/vfx_starry_impact", null, "slash_attack.mp3")
                .SpawningHitVfxOnEachCreature()
                .Execute(choiceContext);
        }
    }
}


