using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using CSharpSBP.SBP_Code.Powers;

namespace CSharpSBP.SBP_Code.Cards.SkillCards;

/*
卡牌名称：疯狂蛇进
卡牌描述：本回合内，[gold]\"蛇咬\"[/gold]卡牌每次给予中毒时额外给予{Poison:diff()}层中毒。
升级效果：保留
*/
//已完成
public sealed class SBP_skill3 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    
    public SBP_skill3() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new DynamicVar("Poison", 3m) };

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        AddKeyword(CardKeyword.Retain);
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // 应用临时能力，显示给玩家
        return PowerCmd.Apply<PoisonBoostPower>(choiceContext, base.Owner.Creature, 3, base.Owner.Creature, this);
    }
}

