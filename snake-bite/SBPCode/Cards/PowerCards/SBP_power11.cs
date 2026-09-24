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
卡牌名称：地咬狂蛇
卡牌描述：每当你抽到"蛇咬"时，自动将其对随机目标打出。}。
升级效果：降低{Energy:energyIcons()}。
*/
public sealed class SBP_power11 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    // “地咬狂蛇”名字中不含“蛇咬”子串，属于“蛇”系但不属于“蛇咬”系
    public override bool HasSnakeBiteTag => false;
    
    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new EnergyVar(1) };
    
    public SBP_power11() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self) { }
    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        // 将费用减少 1（从 2 费变为 1 费）
        base.EnergyCost.UpgradeBy(-1);
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // 施加地咬狂蛇能力（抽牌时自动打出蛇咬卡牌）
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SnakeRaiderPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
    }
}


