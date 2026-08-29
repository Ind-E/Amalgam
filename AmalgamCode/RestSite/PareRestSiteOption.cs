using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Amalgam.AmalgamCode.RestSite;

public sealed class PareRestSiteOption(Player owner) : AmalgamRestSiteOption(owner)
{
    protected override string Id => "pare";

    private const int _cardsToRemove = 2;

    private const int _maxHpLoss = 5;

    public override LocString Description
    {
        get
        {
            if (IsEnabled)
            {
                LocString locString = new("rest_site_ui", "OPTION_" + OptionId + ".description");
                locString.Add("Cards", _cardsToRemove);
                locString.Add("MaxHp", _maxHpLoss);
                return locString;
            }
            else
            {
                LocString locString = new(
                    "rest_site_ui",
                    "OPTION_" + OptionId + ".descriptionDisabled"
                );
                locString.Add("Cards", _cardsToRemove);
                return locString;
            }
        }
    }

    public override bool IsEnabled => GetRemovableCardCount(Owner) >= _cardsToRemove;

    public override async Task<bool> OnSelect()
    {
        CardSelectorPrefs prefs = new(CardSelectorPrefs.RemoveSelectionPrompt, _cardsToRemove)
        {
            Cancelable = true,
            RequireManualConfirmation = true,
        };
        IEnumerable<CardModel> enumerable = await CardSelectCmd.FromDeckForRemoval(Owner, prefs);
        if (!enumerable.Any())
        {
            return false;
        }
        foreach (CardModel item in enumerable)
        {
            await CardPileCmd.RemoveFromDeck(item);
        }
        await CreatureCmd.LoseMaxHp(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            _maxHpLoss,
            false
        );
        return true;
    }

    private static int GetRemovableCardCount(Player player)
    {
        return PileType.Deck.GetPile(player).Cards.Count(c => c.IsRemovable);
    }
}
