using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.SkillCards;

/*
卡牌名称：蛇景
卡牌描述：给予目标敌人{Poison:diff()}层中毒。使其在本回合失去{Power:diff()}点力量。自身获得1层虚弱。
升级效果：力量削弱从9增加到11
*/
//已完成
public sealed class SBP_skill8 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    
    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new DynamicVar("Power", 9m), new DynamicVar("Poison", 3m) };
    
    public SBP_skill8() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy) 
    {
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.DynamicVars["Power"].UpgradeValueBy(2m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        
        // 给予目标敌人中毒
        await PowerCmd.Apply<PoisonPower>(choiceContext, play.Target, (int)base.DynamicVars["Poison"].BaseValue, base.Owner.Creature, this);
        
        // 使用临时力量降低能力，使敌人本回合失去力量（回合结束时恢复）
        await PowerCmd.Apply<SnakeSightTemporaryStrengthPower>(choiceContext, play.Target, base.DynamicVars["Power"].BaseValue, base.Owner.Creature, this);
        
        // 自身获得1层虚弱
        await PowerCmd.Apply<WeakPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
        
        await base.OnPlay(choiceContext, play);
    }
}
