using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using CSharpSBP.SBP_Code.Tags;

namespace CSharpSBP.SBP_Code.Cards.AttackCards;

/*
卡牌名称：坟冢爆蛇
卡牌描述：造成{Damage:diff()}点伤害。选择弃牌堆中的一张"蛇咬"卡牌打出。
升级效果：选择弃牌堆中的一张"蛇咬"卡牌升级后打出
*/
//已完成
public sealed class SBP_atk3 : SnakeBitePlus_Card
{
    public override bool HasSnakeTag => true;
    public override bool HasSnakeBiteTag => false;
    
    public SBP_atk3() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars
    => new[] { new DamageVar(3m, ValueProp.Move) };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
            return;
            
        // 造成伤害
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(choiceContext);
        
        // 获取弃牌堆中的"蛇咬"卡牌
        var discardPile = PileType.Discard.GetPile(base.Owner);
        var snakeBiteCards = discardPile.Cards
            .Where(card => card.HasSnakeBiteTag() || card.Id.Entry.ToLower().Contains("snakebite"))
            .ToList();
        
        if (snakeBiteCards.Any())
        {
            // 创建选择器配置：选择1张卡牌
            var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "CHOOSE_CARD"), 1);
            
            // 显示卡牌选择界面让玩家选择
            var chosenCard = (await CardSelectCmd.FromSimpleGrid(choiceContext, snakeBiteCards, base.Owner, prefs)).FirstOrDefault();
            
            if (chosenCard != null)
            {
                // 如果升级了且卡牌未升级，则升级该卡牌
                if (IsUpgraded && !chosenCard.IsUpgraded)
                {
                    chosenCard.UpgradeInternal();
                }
                
                // 加入手牌（会自动从弃牌堆移除）
                await CardPileCmd.Add(chosenCard, PileType.Hand);
                
                // 自动打出该卡牌
                await CardCmd.AutoPlay(choiceContext, chosenCard, null);
            }
        }
        else
        {
            // 如果弃牌堆中没有"蛇咬"卡牌，显示提示消息
            // 可以选择：什么都不做，或者显示一个视觉反馈
            // 这里选择不做任何操作，因为卡牌描述已经说明了效果
        }
    }
}
