using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：衔尾之蛇
// 卡牌描述：在敌人行动回合，敌人的攻击不会使玩家损失生命值。作为代价，在玩家回合开始时给予玩家未被格挡的攻击伤害值的中毒。
public sealed class SnakeDamageToPoisonPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 存储本回合未被格挡的伤害值（将转化为中毒）
    private decimal _unblockedDamage = 0;

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 只处理对自己的伤害
        if (target != base.Owner)
        {
            return amount;
        }

        // 只在敌人攻击时生效
        if (dealer != null && dealer.IsEnemy && !dealer.IsPlayer)
        {
            // 检查是否存在 SnakeArmorPower
            var snakeArmorPower = base.Owner.GetPower<SnakeArmorPower>();
            decimal blockedByArmor = 0;

            if (snakeArmorPower != null)
            {
                // 获取被蛇甲抵挡的伤害
                blockedByArmor = snakeArmorPower.BlockedThisTurn;
                // 记录未被蛇甲抵挡的部分（即原本应该承受但没被抵挡的伤害）
                _unblockedDamage += Math.Max(0m, amount - blockedByArmor);
            }
            else
            {
                // 如果没有蛇甲，直接记录所有伤害
                _unblockedDamage += amount;
            }

            Flash();
            // 使用 NDamageNumVfx 播放未被格挡的伤害数字（紫色，表示下回合的中毒量）
            var creatureNode = NCombatRoom.Instance?.GetCreatureNode(base.Owner);
            if (creatureNode != null && amount > 0)
            {
                Vector2 targetPosition = creatureNode.VfxSpawnPosition;
                NDamageNumVfx damageNumVfx = NDamageNumVfx.Create(targetPosition, (int)amount);
                damageNumVfx.Modulate = Colors.Purple;
                NCombatRoom.Instance?.CombatVfxContainer.AddChild(damageNumVfx);
            }

            // 阻止生命值损失（返回 0）
            return 0;
        }

        return amount;
    }

    public override async Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        await base.AfterSideTurnStartLate(side, participants, combatState);

        if (side == base.Owner.Side)
        {
            // 在玩家回合开始后期（中毒触发之后），将记录的伤害转化为中毒
            if (_unblockedDamage > 0)
            {
                Flash();
                await PowerCmd.Apply<PoisonPower>(new ThrowingPlayerChoiceContext(), base.Owner, _unblockedDamage, base.Owner, null);
                _unblockedDamage = 0;
            }
        }
    }
}
