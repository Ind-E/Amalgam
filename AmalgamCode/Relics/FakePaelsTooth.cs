using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Amalgam.AmalgamCode.Relics;

[Pool(typeof(EventRelicPool))]
public class FakePaelsTooth : AmalgamRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    private const int Cards = 5;

    private List<SerializableCard> _serializableCards = [];

    private const string _cardTitlesKey = "CardTitles";
    private const string _sharpKey = "SharpAmount";
    private const string _nimbleKey = "NimbleAmount";
    private const string _swiftKey = "SwiftAmount";

    public override bool ShowCounter => IsMutable && _serializableCards.Count > 0;

    public override int DisplayAmount => IsMutable ? _serializableCards.Count : 0;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new CardsVar(Cards),
            new StringVar(_cardTitlesKey),
            new DynamicVar(_sharpKey, 3),
            new DynamicVar(_nimbleKey, 3),
            new DynamicVar(_swiftKey, 2),
        ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[]
        {
            HoverTipFactory.FromEnchantment<Sharp>(DynamicVars[_sharpKey].IntValue),
            HoverTipFactory.FromEnchantment<Nimble>(DynamicVars[_nimbleKey].IntValue),
            HoverTipFactory.FromEnchantment<Swift>(DynamicVars[_swiftKey].IntValue),
        }.SelectMany(tips => tips);

    [SavedProperty]
    public List<SerializableCard> SerializableCards
    {
        get { return _serializableCards; }
        private set
        {
            AssertMutable();
            _serializableCards.Clear();
            _serializableCards.AddRange(value);
            UpdateCardList();
        }
    }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        _serializableCards = [];
    }

    public override async Task AfterObtained()
    {
        IEnumerable<CardModel> enumerable = (
            await CardSelectCmd.FromDeckForRemoval(
                player: Owner,
                prefs: new CardSelectorPrefs(
                    CardSelectorPrefs.RemoveSelectionPrompt,
                    DynamicVars.Cards.IntValue
                )
            )
        ).OrderBy(c => c.Id.Entry, StringComparer.Ordinal);
        foreach (CardModel item in enumerable)
        {
            CardModel cardModel = (CardModel)item.MutableClone();
            SerializableCards.Add(cardModel.ToSerializable());
            await CardPileCmd.RemoveFromDeck(item);
        }
        UpdateCardList();
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (!Owner.Creature.IsDead && SerializableCards.Count > 0)
        {
            Flash();
            await Cmd.CustomScaledWait(0.1f, 1f);
            SerializableCard serializableCard = Owner.PlayerRng.Rewards.NextItem(
                SerializableCards
            )!;
            CardModel cardModel = CardModel.FromSerializable(serializableCard);
            if (!Owner.RunState.ContainsCard(cardModel))
            {
                Owner.RunState.AddCard(cardModel, Owner);
            }
            var sharpEnchant = ModelDb.Enchantment<Sharp>();
            var nimbleEnchant = ModelDb.Enchantment<Nimble>();
            var swiftEnchat = ModelDb.Enchantment<Swift>();
            if (sharpEnchant.CanEnchant(cardModel))
            {
                CardCmd.Enchant(
                    sharpEnchant.ToMutable(),
                    cardModel,
                    DynamicVars[_sharpKey].IntValue
                );
            }
            else if (nimbleEnchant.CanEnchant(cardModel))
            {
                CardCmd.Enchant(
                    nimbleEnchant.ToMutable(),
                    cardModel,
                    DynamicVars[_nimbleKey].IntValue
                );
            }
            else
            {
                CardCmd.Enchant(
                    swiftEnchat.ToMutable(),
                    cardModel,
                    DynamicVars[_swiftKey].IntValue
                );
            }

            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(cardModel, PileType.Deck));
            Status = (SerializableCards.Count == 0) ? RelicStatus.Disabled : RelicStatus.Normal;
            SerializableCards.Remove(serializableCard);
            UpdateCardList();
        }
    }

    private void UpdateCardList()
    {
        Status = (SerializableCards.Count == 0) ? RelicStatus.Disabled : RelicStatus.Normal;
        StringVar stringVar = (StringVar)DynamicVars[_cardTitlesKey];
        if (SerializableCards.Count == 0)
        {
            stringVar.StringValue = string.Empty;
        }
        else
        {
            stringVar.StringValue = string.Join(
                '\n',
                SerializableCards.Select(c => "- " + SaveUtil.CardOrDeprecated(c.Id!).Title)
            );
        }
        InvokeDisplayAmountChanged();
    }

    public static bool CanSpawn(IReadOnlyList<CardModel> cards) =>
        cards.Count(c => c.IsRemovable) >= Cards;
}
