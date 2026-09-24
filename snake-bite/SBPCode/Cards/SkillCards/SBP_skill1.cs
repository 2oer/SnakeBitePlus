using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.SkillCards;

/*
卡牌名称：弹跳蛇咬
卡牌描述：给予自身{SelfPoison:diff()}层中毒，共3次。给予随机敌人{Poison:diff()}层中毒，共3次。
升级效果：中毒层数从7增加到10
*/
//已完成
public sealed class SBP_skill1 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => true;

    public SBP_skill1() : base(2, CardType.Skill, CardRarity.Common, TargetType.None) { }

    // 动态变量：中毒层数（与 SBP_atk1 风格一致）
    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[]
    {
        new DynamicVar("SelfPoison", 1m),
        new DynamicVar("Poison", 7m)
    };

    // 鼠标悬停提示：中毒
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    => new[] { HoverTipFactory.FromPower<PoisonPower>() };
    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        // 使用字典索引访问动态变量（与 SBP_atk1 风格一致）
        base.DynamicVars["Poison"].UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // 使用字典索引访问动态变量（与 SBP_atk1 风格一致）
        int poisonAmount = (int)base.DynamicVars["Poison"].BaseValue;
        int selfPoisonAmount = (int)base.DynamicVars["SelfPoison"].BaseValue;

        // 播放施法动画
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        for (int i = 0; i < 3; i++)
        {
            // 对自己施加中毒（疯狂蛇进加成由全局钩子统一处理）
            await PowerCmd.Apply<PoisonPower>(choiceContext, base.Owner.Creature, selfPoisonAmount, base.Owner.Creature, this);

            // 获取随机敌人
            Creature randomEnemy = base.Owner.RunState.Rng.CombatTargets.NextItem(base.CombatState.HittableEnemies);
            if (randomEnemy == null)
            {
                continue;
            }

            // 播放毒雾特效
            VfxCmd.PlayOnCreatureCenter(randomEnemy, "vfx/vfx_bite");

            // 对敌人施加中毒（疯狂蛇进加成由全局钩子统一处理）
            await PowerCmd.Apply<PoisonPower>(choiceContext, randomEnemy, poisonAmount, base.Owner.Creature, this);

            // 延迟以显示动画
            await Cmd.Wait(0.25f);
        }
    }
}

