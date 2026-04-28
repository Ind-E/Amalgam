using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Amalgam.Relics;

[Pool(typeof(EventRelicPool))]
public class AlphabetSoup : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    private const string _momentumKey = "Momentum";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(_momentumKey, 8m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromEnchantment<Momentum>(DynamicVars[_momentumKey].IntValue);

    public override Task AfterObtained()
    {
        foreach (CardModel card in PileType.Deck.GetPile(Owner).Cards.Where(IsValidStrike))
        {
            CardCmd.Enchant<Momentum>(card, DynamicVars[_momentumKey].IntValue);
            if (NCardEnchantVfx.Create(card) is { } vfx)
            {
                NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(vfx);
            }
        }

        return Task.CompletedTask;
    }

    private static bool IsValidStrike(CardModel card) =>
        card.Tags.Contains(CardTag.Strike)
        && card.Rarity == CardRarity.Basic
        && ModelDb.Enchantment<Momentum>().CanEnchant(card);

    public static bool CanSpawn(IReadOnlyList<CardModel> cards) => cards.Any(IsValidStrike);
}