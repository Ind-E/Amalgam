using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace Amalgam.AmalgamCode.Relics;

public class FakePaelsEye : AmalgamRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    private bool _anyCardsPlayedThisTurn;

    private bool AnyCardsPlayedThisTurn
    {
        get { return _anyCardsPlayedThisTurn; }
        set
        {
            AssertMutable();
            _anyCardsPlayedThisTurn = value;
        }
    }

    private bool _usedThisCombat;

    private bool UsedThisCombat
    {
        get { return _usedThisCombat; }
        set
        {
            AssertMutable();
            _usedThisCombat = value;
        }
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (
            !CombatManager.Instance.IsInProgress
            || AnyCardsPlayedThisTurn
            || UsedThisCombat
            || cardPlay.Card.Owner != Owner
        )
        {
            return Task.CompletedTask;
        }
        Status = RelicStatus.Normal;
        AnyCardsPlayedThisTurn = true;
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState
    )
    {
        if (side != Owner.Creature.Side || UsedThisCombat)
        {
            return Task.CompletedTask;
        }
        Status = RelicStatus.Active;
        AnyCardsPlayedThisTurn = false;
        return Task.CompletedTask;
    }

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants
    )
    {
        if (UsedThisCombat || AnyCardsPlayedThisTurn || side != CombatSide.Player)
        {
            return;
        }

        await PowerCmd.Apply<ImprovementPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Cards.BaseValue,
            Owner.Creature,
            null
        );

        Flash();
        Status = RelicStatus.Disabled;
        UsedThisCombat = true;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        Status = RelicStatus.Normal;
        UsedThisCombat = false;
        return Task.CompletedTask;
    }
}

