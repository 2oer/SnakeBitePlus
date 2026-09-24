using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using CSharpSBP.SBP_Code.Cards.SkillCards;

namespace CSharpSBP.SBP_Code.Powers;

// 蛇景临时力量降低能力
// 类似于 PiercingWailPower，使敌人在本回合失去力量，回合结束时恢复
public sealed class SnakeSightTemporaryStrengthPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<SBP_skill8>();

    protected override bool IsPositive => false;
}
