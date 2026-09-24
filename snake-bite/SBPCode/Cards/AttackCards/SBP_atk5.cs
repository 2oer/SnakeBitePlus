using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using CSharpSBP.SBP_Code.Powers;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Helpers;

namespace CSharpSBP.SBP_Code.Cards.AttackCards;

/*
卡牌名称：王蛇咬剑
卡牌描述：给予所有敌人10层中毒。触发目标敌人的中毒效果。(保留,消耗)
升级效果：费用变为1
说明：此卡为Token卡牌，只能由其他卡牌生成（如封咬蛇座）
*/
//已完成
public sealed class SBP_atk5 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => true;

    public SBP_atk5() : base(2, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
    {
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.EnergyCost.SetCustomBaseCost(1);
    }
    // 提供卡牌的悬浮提示，显示中毒能力效果
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    => new[] { HoverTipFactory.FromPower<PoisonPower>() };

    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new DynamicVar("Poison", 10m) };

    public override IEnumerable<CardKeyword> CanonicalKeywords
    => base.CanonicalKeywords.Append(CardKeyword.Exhaust).Append(CardKeyword.Retain);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // 使用字典索引访问动态变量（与 SBP_atk1 风格一致）
        int poisonAmount = (int)base.DynamicVars["Poison"].BaseValue;

        // 对所有存活敌人施加中毒（疯狂蛇进的额外层数由 PoisonBoostPower.ModifyPowerAmountGivenAdditive 统一追加）
        var aliveEnemies = base.CombatState.Enemies.Where(e => e.IsAlive).ToList();

        // 播放攻击动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);

        // 同时对所有敌人播放斩击特效
        VfxCmd.PlayOnCreatureCenters(aliveEnemies, "vfx/vfx_giant_horizontal_slash");

        // 施加中毒（疯狂蛇进加成由全局钩子统一处理）
        await PowerCmd.Apply<PoisonPower>(choiceContext, aliveEnemies, poisonAmount, base.Owner.Creature, this);

        // 立刻触发目标敌人的中毒效果（使用单体特效）
        var enemyPoison = play.Target.GetPower<PoisonPower>();
        if (enemyPoison != null && enemyPoison.Amount > 0)
        {
            var bigSlashVfx = MegaCrit.Sts2.Core.Nodes.Vfx.NBigSlashVfx.Create(play.Target);
            var bigSlashImpactVfx = MegaCrit.Sts2.Core.Nodes.Vfx.NBigSlashImpactVfx.Create(play.Target);
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(bigSlashVfx);
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(bigSlashImpactVfx);
            await Cmd.Wait(0.1f);

            await CreatureCmd.Damage(choiceContext, play.Target, (decimal)enemyPoison.Amount, ValueProp.Unblockable | ValueProp.Unpowered, base.Owner.Creature, this);
            // 减少1层中毒
            if (play.Target.IsAlive)
                await PowerCmd.Decrement(enemyPoison);
        }
    }
}

