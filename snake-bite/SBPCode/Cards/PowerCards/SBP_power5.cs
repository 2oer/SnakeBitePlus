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
卡牌名称：蛇洞
卡牌描述：每次给予自身中毒或去除自身中毒时，对所有敌人施加{Amount:diff}层中毒。
升级效果：获得[固有]关键词，每层中毒施加的层数从1增加到2
*/
// 已完成
public sealed class SBP_power5 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] 
    {
        new DynamicVar("Amount", 1m)
    };
    
    public SBP_power5() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int amount = IsUpgraded ? 2 : 1;
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SnakePoisonAuraPower>(choiceContext, base.Owner.Creature, amount, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        AddKeyword(CardKeyword.Innate);
        base.DynamicVars["Amount"].UpgradeValueBy(1m);
    }
}


