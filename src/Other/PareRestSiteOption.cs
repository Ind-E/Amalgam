using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Amalgam.Other;

public sealed class PareRestSiteOption : RestSiteOption
{
    private const int _cardsToRemove = 2;

    private const int _maxHpLose = 6;

    public override string OptionId => "AMALGAM-PARE";

    public override LocString Description
    {
        get
        {
            if (IsEnabled)
            {
                LocString locString = new LocString(
                    "rest_site_ui",
                    "OPTION_" + OptionId + ".description"
                );
                locString.Add("Cards", _cardsToRemove);
                locString.Add("MaxHp", _maxHpLose);
                return locString;
            }
            return new LocString("rest_site_ui", "OPTION_" + OptionId + ".descriptionDisabled");
        }
    }

    public PareRestSiteOption(Player owner)
        : base(owner)
    {
        base.IsEnabled = GetRemovableCardCount(owner) >= _cardsToRemove;
    }

    public override async Task<bool> OnSelect()
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(
            CardSelectorPrefs.RemoveSelectionPrompt,
            _cardsToRemove
        )
        {
            Cancelable = true,
            RequireManualConfirmation = true,
        };
        IEnumerable<CardModel> enumerable = await CardSelectCmd.FromDeckForRemoval(
            base.Owner,
            prefs
        );
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
            _maxHpLose,
            false
        );
        return true;
    }

    private static int GetRemovableCardCount(Player player)
    {
        return PileType.Deck.GetPile(player).Cards.Count((CardModel c) => c.IsRemovable);
    }
}
