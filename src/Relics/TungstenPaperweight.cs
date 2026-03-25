using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;

namespace Amalgam.Relics;

[Pool(typeof(EventRelicPool))]
public class TungstenPaperweight : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override async Task AfterObtained()
    {
        CardCreationOptions options = new(
            [ModelDb.CardPool<ColorlessCardPool>()],
            CardCreationSource.Other,
            CardRarityOddsType.RegularEncounter
        );

        List<CardCreationResult> cards = [.. CardFactory.CreateForReward(Owner, 20, options)];
        foreach (
            CardModel item in await CardSelectCmd.FromSimpleGridForRewards(
                context: new BlockingPlayerChoiceContext(),
                cards: cards,
                player: Owner,
                prefs: new CardSelectorPrefs(L10NLookup(Id.Entry + ".selectionScreenPrompt"), 2)
            )
        )
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(item, PileType.Deck));
        }
    }

    public override string PackedIconOutlinePath =>
        "res://Amalgam/relics/tungsten_paperweight_outline.png";
    public override string PackedIconPath => "res://Amalgam/relics/tungsten_paperweight.png";
    public override string BigIconPath => "res://Amalgam/relics/tungsten_paperweight.png";
}
