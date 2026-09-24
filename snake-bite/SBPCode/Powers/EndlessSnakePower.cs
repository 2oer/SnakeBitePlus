using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using CSharpSBP.SBP_Code.Cards.AttackCards;

namespace CSharpSBP.SBP_Code.Powers;

// 能力名称：无尽蛇咬
// 卡牌描述：回合开始时将指定数量的"蛇咬"卡牌加入手牌。
public sealed class EndlessSnakePower : SnakeBitePlusPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player == base.Owner.Player)
        {
            Flash();
            // 创建 SBP_atk7 卡牌到手牌，数量为能力层数
            for (int i = 0; i < base.Amount; i++)
            {
                var snakeCard = combatState.CreateCard<SBP_atk7>(base.Owner.Player);
                await CardPileCmd.Add(snakeCard, PileType.Hand);
            }
        }
    }
}
