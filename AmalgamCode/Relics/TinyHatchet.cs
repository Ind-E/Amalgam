using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Amalgam.AmalgamCode.Relics;

public class TinyHatchet : AmalgamRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(13), new RepeatVar(2)];

    private bool UsedThisCombat
    {
        get { return CardsPlayedThisCombat >= DynamicVars.Cards.IntValue; }
    }

    private int _cardsPlayedThisCombat;

    private int CardsPlayedThisCombat
    {
        get { return _cardsPlayedThisCombat; }
        set
        {
            AssertMutable();
            _cardsPlayedThisCombat = value;
            UpdateDisplay();
        }
    }

    public override int DisplayAmount => CardsPlayedThisCombat;

    public override bool ShowCounter
    {
        get
        {
            if (CombatManager.Instance.IsInProgress)
            {
                return CardsPlayedThisCombat < DynamicVars.Cards.IntValue;
            }
            return false;
        }
    }

    private void UpdateDisplay()
    {
        int intValue = DynamicVars.Cards.IntValue;
        Status = (CardsPlayedThisCombat == intValue - 1) ? RelicStatus.Active : RelicStatus.Normal;
        InvokeDisplayAmountChanged();
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (
            !CombatManager.Instance.IsInProgress
            || cardPlay.IsAutoPlay
            || cardPlay.Card.Owner != Owner
        )
        {
            return Task.CompletedTask;
        }
        CardsPlayedThisCombat++;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        CardsPlayedThisCombat = 0;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (UsedThisCombat || card.Owner != Owner || Status != RelicStatus.Active)
        {
            return playCount;
        }
        return playCount + DynamicVars.Repeat.IntValue;
    }

    public override Task AfterModifyingCardPlayCount(CardModel card)
    {
        Flash();
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }
}
