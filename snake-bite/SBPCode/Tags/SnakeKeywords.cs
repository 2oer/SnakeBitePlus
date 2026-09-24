using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace CSharpSBP.SBP_Code.Tags;

public static class SnakeKeywords
{
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Snake;

    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword SnakeBite;
}
