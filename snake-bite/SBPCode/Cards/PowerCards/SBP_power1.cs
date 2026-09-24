using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpSBP.SBP_Code.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.PowerCards;

/*
卡牌名称：凶蛇恶咬
卡牌描述：每当打出一张"蛇咬"卡牌时，抽{Cards:diff()}张牌。
升级效果：抽牌从1增加到2
*/
//已完成
public sealed class SBP_power1 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    // “凶蛇恶咬”名字中不含“蛇咬”子串，属于“蛇”系但不属于“蛇咬”系
    public override bool HasSnakeBiteTag => false;
    
    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new DynamicVar("Cards", 1m) };
    
    public SBP_power1() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.DynamicVars["Cards"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<DrawCardsPower>(choiceContext, base.Owner.Creature, (int)base.DynamicVars["Cards"].BaseValue, base.Owner.Creature, this);
    }
}

