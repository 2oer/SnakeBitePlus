using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace CSharpSBP.SBP_Code.Cards.AttackCards;

/*
卡牌名称：辐蛇
卡牌描述：清除目标敌人身上中毒。敌人身上每有一层中毒被清除，就对其造成{Damage:diff()}点伤害。
升级效果：伤害+1
*/
public sealed class SBP_atk8 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    
    // 修改为单目标
    public SBP_atk8() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new DamageVar(1.5m, ValueProp.Move),
    };

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(0.5m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 检查目标是否有效
        if (cardPlay.Target == null)
            return;
            
        // 获取目标敌人的中毒层数
        var poisonPower = cardPlay.Target.GetPower<PoisonPower>();
        int poisonAmount = poisonPower?.Amount ?? 0;
        
        if (poisonAmount > 0)
        {
            // 移除所有中毒
            await PowerCmd.ModifyAmount(choiceContext, poisonPower, -poisonAmount, cardPlay.Target, this);
            // 根据移除的中毒层数造成伤害
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue * poisonAmount)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_starry_impact", null, "slash_attack.mp3")
                .Execute(choiceContext);
        }
    }
}
