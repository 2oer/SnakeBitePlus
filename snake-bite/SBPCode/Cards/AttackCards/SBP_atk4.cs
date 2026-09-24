using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace CSharpSBP.SBP_Code.Cards.AttackCards;

/*
卡牌名称：全蛇撞击
卡牌描述：对敌人造成自身中毒层数1.5倍的伤害。
升级效果：费用变为0
*/
//已完成
public sealed class SBP_atk4 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    
    public SBP_atk4() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
            return;
            
        decimal multiplier = 2m;
        
        var poisonPower = base.Owner.Creature.GetPower<PoisonPower>();
        int poisonStacks = poisonPower?.Amount ?? 0;
        if (IsUpgraded)
        {
            multiplier = 3m;
        }
        decimal dmg = poisonStacks * multiplier;
        await DamageCmd.Attack(dmg).FromCard(this).Targeting(play.Target).Execute(choiceContext);
    }
}


