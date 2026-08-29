using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace Amalgam.AmalgamCode.Relics;

[Pool(typeof(EventRelicPool))]
public class FrayedScarf : AmalgamRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    private int _cardsPlayedThisTurn;

    public override bool ShowCounter
    {
        get
        {
            if (CombatManager.Instance.IsInProgress)
            {
                return CardsPlayedThisTurn < DynamicVars.Cards.IntValue;
            }
            return false;
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public override int DisplayAmount => CardsPlayedThisTurn;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(4), new EnergyVar(1)];

    private int CardsPlayedThisTurn
    {
        get { return _cardsPlayedThisTurn; }
        set
        {
            AssertMutable();
            _cardsPlayedThisTurn = value;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        int intValue = DynamicVars.Cards.IntValue;
        Status = (CardsPlayedThisTurn == intValue - 1) ? RelicStatus.Active : RelicStatus.Normal;
        InvokeDisplayAmountChanged();
    }

    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost
    )
    {
        if (ShouldModifyCost(card))
        {
            modifiedCost = originalCost - DynamicVars.Energy.IntValue;
            return true;
        }

        modifiedCost = originalCost;
        return false;
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState
    )
    {
        if (side == Owner.Creature.Side)
        {
            CardsPlayedThisTurn = 0;
        }
        return Task.CompletedTask;
    }

    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        CardLocation cardLocation
    )
    {
        if (ShouldModifyCost(card) && !isAutoPlay)
        {
            return new CardLocation(cardLocation.player, PileType.Exhaust, cardLocation.position);
        }
        return cardLocation;
    }

    private bool ShouldModifyCost(CardModel card)
    {
        return CombatManager.Instance.IsInProgress
            && card.Owner.Creature == Owner.Creature
            && CardsPlayedThisTurn == DynamicVars.Cards.BaseValue - 1
            && card.Pile?.Type is PileType.Hand or PileType.Play;
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
        CardsPlayedThisTurn++;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        CardsPlayedThisTurn = 0;
        return Task.CompletedTask;
    }
}
