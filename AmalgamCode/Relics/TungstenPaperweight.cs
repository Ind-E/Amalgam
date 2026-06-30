using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;

namespace Amalgam.AmalgamCode.Relics;

public class TungstenPaperweight : AmalgamRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    private const string _maxCardsKey = "MaxCards";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2), new DynamicVar(_maxCardsKey, 20)];

    public override async Task AfterObtained()
    {
        CardCreationOptions options = new(
            [ModelDb.CardPool<ColorlessCardPool>()],
            CardCreationSource.Other,
            CardRarityOddsType.RegularEncounter
        );

        List<CardCreationResult> cards =
        [
            .. CardFactory.CreateForReward(Owner, DynamicVars[_maxCardsKey].IntValue, options),
        ];
        foreach (
            CardModel item in await CardSelectCmd.FromSimpleGridForRewards(
                context: new BlockingPlayerChoiceContext(),
                cards: cards,
                player: Owner,
                prefs: new CardSelectorPrefs(
                    L10NLookup(Id.Entry + ".selectionScreenPrompt"),
                    DynamicVars.Cards.IntValue
                )
            )
        )
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(item, PileType.Deck));
        }
    }
}
