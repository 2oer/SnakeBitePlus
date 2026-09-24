using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpSBP.SBP_Code.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.PowerCards;

/*
卡牌名称：衔尾之蛇
卡牌描述：在敌人行动回合，敌人的攻击不会使玩家损失生命值。作为代价，在玩家回合开始时给予玩家未被格挡的攻击伤害值的中毒。
升级效果：降低为2费
*/
//已完成，验证了和岩蛇覆甲的联动
public sealed class SBP_power4 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    
    public SBP_power4() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SnakeDamageToPoisonPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    base.CanonicalKeywords.Append(CardKeyword.Innate);
    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.EnergyCost.UpgradeBy(-1);
    }
}

