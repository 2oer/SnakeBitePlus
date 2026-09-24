using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using CSharpSBP.SBP_Code.Extensions;
using CSharpSBP.SBP_Code.Tags;

namespace CSharpSBP.SBP_Code.Cards;

[Pool(typeof(SilentCardPool))]
public abstract class SnakeBitePlus_Card : CustomCardModel, ISnakeTagged
{
    protected SnakeBitePlus_Card(int cost, CardType type, CardRarity rarity, TargetType target) 
        : base(cost, type, rarity, target) { }

    // 是否有蛇系标签
    public virtual bool HasSnakeTag => true;

    // 是否有蛇咬标签
    public virtual bool HasSnakeBiteTag => false;

    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant().Replace("_", "")}.png".BigCardImagePath();

    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant().Replace("_", "")}.png".CardImagePath();

    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant().Replace("_", "")}.png".CardImagePath();

    // 根据蛇系/蛇咬标签自动注册自定义关键词，使游戏内显示分类徽标与悬浮提示
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            var keywords = new List<CardKeyword>();
            if (HasSnakeTag) keywords.Add(SnakeKeywords.Snake);
            if (HasSnakeBiteTag) keywords.Add(SnakeKeywords.SnakeBite);
            return keywords;
        }
    }
}


