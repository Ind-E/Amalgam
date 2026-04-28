using System.Reflection.Emit;
using Amalgam.Enchantments;
using Amalgam.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace Amalgam.Other;

public class ScatterRestSiteOption(Player owner) : RestSiteOption(owner)
{
    public override string OptionId => "AMALGAM-SCATTER";

    public override LocString Description
    {
        get
        {
            LocString description = Description;
            description.Add(
                "EnchantmentName",
                ModelDb.Enchantment<Scattered>().Title.GetFormattedText()
            );
            return description;
        }
    }

    public override async Task<bool> OnSelect()
    {
        IReadOnlyList<CardModel> cards =
        [
            .. Owner.Deck.Cards.Where(c => c.Enchantment is Scattered),
        ];

        RemoveFromDeckUsesGridLayoutPatch.RemoveFromDeckUsesGridContainer = true;
        NRun.Instance?.GlobalUi.GridCardPreviewContainer.ForceMaxColumnsUntilEmpty(3);

        await CardPileCmd.RemoveFromDeck(cards);

        RemoveFromDeckUsesGridLayoutPatch.RemoveFromDeckUsesGridContainer = false;
        Owner.GetRelic<SandPile>()!.UsedUp = true;
        return true;
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.RemoveFromDeck), MethodType.Async)]
    [HarmonyPatch([typeof(IReadOnlyList<CardModel>), typeof(bool)])]
    public static class RemoveFromDeckUsesGridLayoutPatch
    {
        public static bool RemoveFromDeckUsesGridContainer;

        private static Control GetCardPreviewContainer(NGlobalUi globalUi)
        {
            if (RemoveFromDeckUsesGridContainer)
            {
                return globalUi.GridCardPreviewContainer;
            }

            return globalUi.CardPreviewContainer;
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);

            var oldGetter = AccessTools.PropertyGetter(
                typeof(NGlobalUi),
                nameof(NGlobalUi.CardPreviewContainer)
            );

            var helperMethod = AccessTools.Method(
                typeof(RemoveFromDeckUsesGridLayoutPatch),
                nameof(GetCardPreviewContainer)
            );

            codeMatcher
                .MatchEndForward(CodeMatch.Calls(oldGetter))
                .ThrowIfInvalid("Didn't find a match for remove uses grid layout patch")
                .SetInstruction(new CodeInstruction(OpCodes.Call, helperMethod));

            return codeMatcher.Instructions();
        }
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Short);
        return Task.CompletedTask;
    }

    public override Task DoRemotePostSelectVfx()
    {
        NRestSiteCharacter nRestSiteCharacter = NRestSiteRoom.Instance?.Characters.First(c =>
            c.Player == Owner
        )!;
        nRestSiteCharacter?.Shake();
        var nRelicFlashVfx = NRelicFlashVfx.Create(ModelDb.Relic<SandPile>());
        if (nRelicFlashVfx == null)
        {
            return Task.CompletedTask;
        }
        nRestSiteCharacter?.AddChildSafely(nRelicFlashVfx);
        nRelicFlashVfx.Position = Vector2.Zero;
        return Task.CompletedTask;
    }
}
