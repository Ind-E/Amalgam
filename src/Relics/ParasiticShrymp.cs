using Amalgam.Cards;
using BaseLib.Abstracts;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Amalgam.Relics;

[Pool(typeof(EventRelicPool))]
public class ParasiticShrymp : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromEnchantment<Imbued>().Append(HoverTipFactory.FromCard<Parasite>());

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    private static bool ImbuedAllTypes;

    public override async Task AfterObtained()
    {
        CardSelectorPrefs prefs = new(CardSelectorPrefs.EnchantSelectionPrompt, 1);
        Imbued canonicalImbued = ModelDb.Enchantment<Imbued>();

        ImbuedAllTypes = true;
        foreach (
            CardModel item in await CardSelectCmd.FromDeckForEnchantment(
                Owner,
                canonicalImbued,
                1,
                prefs
            )
        )
        {
            CardCmd.Enchant(canonicalImbued.ToMutable(), item, 1m);
            CardCmd.Preview(item);
        }
        ImbuedAllTypes = false;

        await CardPileCmd.AddCursesToDeck(
            Enumerable.Repeat(ModelDb.Card<Parasite>(), DynamicVars.Cards.IntValue),
            Owner
        );
    }

    [HarmonyPatch(typeof(Imbued), nameof(Imbued.CanEnchantCardType))]
    static class ImbuedAllCardTypesPatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (ImbuedAllTypes)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
