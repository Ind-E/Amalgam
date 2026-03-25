using Amalgam.Enchantments;
using Amalgam.Other;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Amalgam.Relics;

[Pool(typeof(EventRelicPool))]
public class SandPile : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromEnchantment<Scattered>();

    private bool _usedUp;

    public override bool IsUsedUp => UsedUp;

    [SavedProperty]
    public bool UsedUp
    {
        get { return _usedUp; }
        set
        {
            AssertMutable();
            _usedUp = value;
            if (_usedUp)
            {
                Status = RelicStatus.Disabled;
            }
        }
    }

    public override Task AfterObtained()
    {
        IEnumerable<CardModel> enumerable = PileType
            .Deck.GetPile(Owner)
            .Cards.Where(card => card.IsUpgradable && card.Enchantment is null)
            .ToList()
            .StableShuffle(Owner.RunState.Rng.Niche)
            .Take(6);
        NRun.Instance?.GlobalUi.GridCardPreviewContainer.ForceMaxColumnsUntilEmpty(3);
        foreach (CardModel card in enumerable)
        {
            CardCmd.Enchant<Scattered>(card, 1m);
            var vfx = NCardEnchantVfx.Create(card);
            NRun.Instance?.GlobalUi.GridCardPreviewContainer.AddChildSafely(vfx);
        }
        return Task.CompletedTask;
    }

    public override bool TryModifyRestSiteOptions(
        Player player,
        ICollection<RestSiteOption> options
    )
    {
        if (player != Owner || UsedUp)
        {
            return false;
        }
        options.Add(new CollectRestSiteOption(player));
        options.Add(new ScatterRestSiteOption(player));
        return true;
    }

    public override string PackedIconOutlinePath => "res://Amalgam/relics/sand_pile_outline.png";
    public override string PackedIconPath => "res://Amalgam/relics/sand_pile.png";
    public override string BigIconPath => "res://Amalgam/relics/sand_pile.png";

    public static bool CanSpawn(IReadOnlyList<CardModel> cards)
    {
        // use Swift because it has no override for CanEnchant. Can't use Imbued because it only allows skills by default
        return cards.Count(c => ModelDb.Enchantment<Swift>().CanEnchant(c)) >= 6;
    }
}
