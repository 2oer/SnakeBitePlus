using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace CSharpSBP.SBP_Code.Powers;
//SBP_power2
//使用中毒抵挡伤害，每回合额外开始时触发一次中毒伤害
//已完成
public sealed class SnakeArmorPower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 记录本回合已抵挡的伤害量，用于限制本回合抵挡上限
    private decimal _blockedThisTurn = 0;

    /// <summary>
    /// 获取本回合已被蛇甲抵挡的伤害值（用于其他能力读取）
    /// </summary>
    public decimal BlockedThisTurn => _blockedThisTurn;

    /// <summary>
    /// 使用中毒抵挡伤害
    /// 在奥术护盾计算之后，减少实际受到的伤害
    /// </summary>
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 只对自己生效
        if (target != base.Owner)
        {
            return amount;
        }

        // 不抵挡玩家自己造成的伤害（dealer 是玩家）
        if (dealer != null && dealer.IsPlayer)
        {
            return amount;
        }

        // 不抵挡中毒伤害（中毒伤害带有 Unpowered 属性，且 dealer 为 null）
        if (dealer == null && props.HasFlag(ValueProp.Unpowered))
        {
            return amount;
        }

        // 获取中毒层数
        var poisonPower = base.Owner.GetPower<PoisonPower>();
        if (poisonPower == null || poisonPower.Amount <= 0)
        {
            return amount;
        }

        // 用中毒抵挡伤害：最多减少剩余中毒层数量的伤害
        // poisonPower.Amount - _blockedThisTurn 是本回合剩余可抵挡的中毒层数
        decimal reduction = System.Math.Min(amount, poisonPower.Amount - _blockedThisTurn);

        // 记录抵挡的伤害量
        _blockedThisTurn += reduction;

        // 返回减少后的伤害值
        return Math.Max(0m, amount - reduction);
    }

    /// <summary>
    /// 在伤害修改完成后，立即消耗相应层数的中毒
    /// </summary>
    public override async Task AfterModifyingHpLostAfterOsty()
    {
        await base.AfterModifyingHpLostAfterOsty();

        // 如果本回合有抵挡伤害，立即消耗相应的中毒层数
        if (_blockedThisTurn > 0)
        {
            var poisonPower = base.Owner.GetPower<PoisonPower>();
            if (poisonPower != null && poisonPower.Amount > 0)
            {
                // 消耗中毒层数（使用 ModifyAmount 减少层数）
                await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), poisonPower, -_blockedThisTurn, base.Owner, null);
            }
            // 重置计数器
            _blockedThisTurn = 0;
        }
    }

    /// <summary>
    /// 回合开始时，重置抵挡计数器并额外触发一次中毒伤害
    /// </summary>
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        await base.AfterSideTurnStart(side, participants, combatState);

        // 回合开始时重置抵挡计数器
        _blockedThisTurn = 0;

        // 只在自己回合开始时触发额外中毒伤害
        if (side == base.Owner.Side)
        {
            var poisonPower = base.Owner.GetPower<PoisonPower>();
            if (poisonPower != null && poisonPower.Amount > 0)
            {
                // 造成中毒层数的伤害
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner, poisonPower.Amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
                // 消耗一层中毒（使用 ModifyAmount 减少层数）
                await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), poisonPower, -1, base.Owner, null);
            }
        }
    }
}
