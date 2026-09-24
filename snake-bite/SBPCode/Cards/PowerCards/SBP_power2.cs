using System.Threading.Tasks;
using CSharpSBP.SBP_Code.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.PowerCards;

/*
卡牌名称：岩蛇覆甲
卡牌描述：你身上的中毒可以抵挡敌人攻击造成的伤害。回合开始时，你身上的中毒会额外触发一次。
升级效果：获得 固有 ，保留 关键词
*/
//已完成
public sealed class SBP_power2 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    
    public SBP_power2() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        AddKeyword(CardKeyword.Innate);
        AddKeyword(CardKeyword.Retain);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        
        // 检测是否具有 SnakeDamageToPoisonPower
        var poisonToDamagePower = base.Owner.Creature.GetPower<SnakeDamageToPoisonPower>();
        int poisonToDamageAmount = poisonToDamagePower?.Amount ?? 0;
        
        // 如果有，先移除该能力
        if (poisonToDamagePower != null)
        {
            await PowerCmd.Remove(poisonToDamagePower);
        }
        
        // 添加 SnakeArmorPower
        await PowerCmd.Apply<SnakeArmorPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
        
        // 如果之前有 SnakeDamageToPoisonPower，重新添加确保触发顺序
        if (poisonToDamageAmount > 0)
        {
            await PowerCmd.Apply<SnakeDamageToPoisonPower>(choiceContext, base.Owner.Creature, poisonToDamageAmount, base.Owner.Creature, this);
        }
    }
}
