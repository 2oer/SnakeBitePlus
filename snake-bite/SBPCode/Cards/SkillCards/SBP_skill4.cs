using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using CSharpSBP.SBP_Code.Cards.AttackCards;
using CSharpSBP.SBP_Code.Powers;
using CSharpSBP.SBP_Code.Tags;

namespace CSharpSBP.SBP_Code.Cards.SkillCards;

/*
卡牌名称：回蛇
卡牌描述：消耗手牌中的一张"蛇系"卡牌。下回合开始时将3张"巨蛇咬"放入手牌。
升级效果：费用变为0
*/
//已完成
public sealed class SBP_skill4 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;

    // 悬浮提示：显示巨蛇咬卡牌信息
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] {
        HoverTipFactory.FromCard<SBP_atk7>()
    };
        
    public SBP_skill4() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        base.EnergyCost.SetCustomBaseCost(0);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var snakeCards = PileType.Hand.GetPile(base.Owner).Cards
            .Where(card => card.HasSnakeTag())
            .ToList();
        
        if (snakeCards.Any())
        {
            var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "CHOOSE_CARD"), 1);
            var chosenCards = await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, 
                card => card.HasSnakeTag(), this);
            
            var chosenCard = chosenCards.FirstOrDefault();
            
            if (chosenCard != null)
            {
                await CardCmd.Exhaust(choiceContext, chosenCard);
                await PowerCmd.Apply<IncomingSnakePower>(choiceContext, base.Owner.Creature, 3, base.Owner.Creature, this);
            }
        }
    }
}

