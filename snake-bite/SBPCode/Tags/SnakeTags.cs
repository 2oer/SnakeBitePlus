using MegaCrit.Sts2.Core.Models;

namespace CSharpSBP.SBP_Code.Tags;

public interface ISnakeTagged
{
    bool HasSnakeTag { get; }
    bool HasSnakeBiteTag { get; }
}

public static class SnakeTagHelper
{
    public static bool HasSnakeTag(this CardModel card)
    {
        return card is ISnakeTagged snakeCard && snakeCard.HasSnakeTag;
    }
    
    public static bool HasSnakeBiteTag(this CardModel card)
    {
        return card is ISnakeTagged snakeCard && snakeCard.HasSnakeBiteTag;
    }
}

