using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.AttackCards;

/*
卡牌名称：巨蛇咬
卡牌描述：给予自身{SelfPoison:diff()}层中毒。给予目标敌人{Poison:diff()}层中毒。消耗。
升级效果：中毒层数从9层增加到12层
定位：消耗版Snakebite，拥有更高的数值更低的费用，作为代价会给予自身中毒，适合初期过渡使用
*/
//已完成
public sealed class SBP_atk7 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => base.CanonicalKeywords.Append(CardKeyword.Exhaust);

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DynamicVar("SelfPoison", 2m),
        new DynamicVar("Poison", 9m)
    };

    public SBP_atk7() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        // 使用字典索引访问动态变量（与 SBP_atk1 风格一致）
        base.DynamicVars["Poison"].UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        // 给予自身中毒（疯狂蛇进加成由全局钩子统一处理）
        await PowerCmd.Apply<PoisonPower>(choiceContext, base.Owner.Creature, (int)base.DynamicVars["SelfPoison"].BaseValue, base.Owner.Creature, this);

        // 给予目标敌人中毒（疯狂蛇进加成由全局钩子统一处理）
        if (play.Target != null)
        {
            VfxCmd.PlayOnCreatureCenter(play.Target, "vfx/vfx_bite");
            await PowerCmd.Apply<PoisonPower>(choiceContext, play.Target, (int)base.DynamicVars["Poison"].BaseValue, base.Owner.Creature, this);
        }
    }
}
