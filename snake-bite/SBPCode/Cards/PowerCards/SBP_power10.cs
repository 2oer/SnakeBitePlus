using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using CSharpSBP.SBP_Code.Cards.AttackCards;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.PowerCards;

/*
卡牌名称：无尽蛇咬
卡牌描述：回合开始时将{Amount:diff}"张"巨蛇咬"卡牌加入手牌。
升级效果：获得[固有]关键词
*/
public sealed class SBP_power10 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => true;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] 
    {
        new DynamicVar("Amount", 1m)
    };

    // 悬浮提示：显示巨蛇咬卡牌信息
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] {
        HoverTipFactory.FromCard<SBP_atk7>()
    };
    
    public SBP_power10() : base(0, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        AddKeyword(CardKeyword.Innate);
        base.DynamicVars["Amount"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int amount = IsUpgraded ? 2 : 1;
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<EndlessSnakePower>(choiceContext, base.Owner.Creature, amount, base.Owner.Creature, this);
    }
}


