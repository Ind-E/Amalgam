using Amalgam.Relics;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;

namespace Amalgam;

public class Ancient : CustomAncientModel
{
    public override string CustomMapIconPath => "res://Amalgam/map/ancient_node_amalgam.png";
    public override string CustomMapIconOutlinePath =>
        "res://Amalgam/map/ancient_node_amalgam_outline.png";

    public override bool IsValidForAct(ActModel act) => act.ActNumber() == 2;

    public override IEnumerable<EventOption> AllPossibleOptions =>
        [
            TungstenPaperweightOption,
            ParasiticShrympOption,
            SandPileOption,
            FakePaelsEyeOption,
            FakePaelsToothOption,
            AlphabetSoupOption,
            JunkDrawerOption,
            FrayedScarfOption,
            OminousRingOption,
            TinyHatchetOption,
            WornCleaverOption,
        ];

    private EventOption TungstenPaperweightOption => RelicOption<TungstenPaperweight>();

    private EventOption ParasiticShrympOption => RelicOption<ParasiticShrymp>();
    private EventOption SandPileOption => RelicOption<SandPile>();

    private EventOption FakePaelsToothOption => RelicOption<FakePaelsTooth>();
    private EventOption FakePaelsEyeOption => RelicOption<FakePaelsEye>();

    private EventOption AlphabetSoupOption => RelicOption<AlphabetSoup>();
    private EventOption JunkDrawerOption => RelicOption<JunkDrawer>();

    private EventOption FrayedScarfOption => RelicOption<FrayedScarf>();
    private EventOption OminousRingOption => RelicOption<OminousRing>();

    private EventOption TinyHatchetOption => RelicOption<TinyHatchet>();
    private EventOption WornCleaverOption => RelicOption<WornCleaver>();

    protected override OptionPools MakeOptionPools => new([]);

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> options = [TungstenPaperweightOption];

        IReadOnlyList<CardModel> cards = Owner!.Deck.Cards;
        if (Rng.NextBool() && SandPile.CanSpawn(cards))
            options.Add(SandPileOption);
        else
            options.Add(ParasiticShrympOption);

        if (Rng.NextBool() && cards.Count(c => c.IsRemovable) >= 4)
            options.Add(FakePaelsToothOption);
        else
            options.Add(FakePaelsEyeOption);

        if (Rng.NextBool() && AlphabetSoup.CanSpawn(cards))
            options.Add(AlphabetSoupOption);
        else
            options.Add(JunkDrawerOption);

        if (Rng.NextBool())
            options.Add(FrayedScarfOption);
        else
            options.Add(OminousRingOption);

        if (Rng.NextBool())
            options.Add(TinyHatchetOption);
        else
            options.Add(WornCleaverOption);

        Rng.Shuffle(options);
        return options[0..3];
    }
}
