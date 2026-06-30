using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Amalgam.AmalgamCode.Cards;

[Pool(typeof(CurseCardPool))]
public class Parasite() : AmalgamCard(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    public override bool CanBeGeneratedByModifiers => false;
    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(3)];

    public override async Task BeforeCardRemoved(CardModel card)
    {
        if (card != this)
        {
            return;
        }
        await CreatureCmd.LoseMaxHp(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            DynamicVars.MaxHp.BaseValue,
            isFromCard: false
        );
    }
}
