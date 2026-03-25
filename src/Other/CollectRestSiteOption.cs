using Amalgam.Enchantments;
using Amalgam.Relics;
using Godot;
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

public class CollectRestSiteOption : RestSiteOption
{
    public override string OptionId => "AMALGAM-COLLECT";

    public override LocString Description
    {
        get
        {
            LocString description = base.Description;
            description.Add(
                "EnchantmentName",
                ModelDb.Enchantment<Scattered>().Title.GetFormattedText()
            );
            return description;
        }
    }

    public CollectRestSiteOption(Player owner)
        : base(owner) { }

    public override async Task<bool> OnSelect()
    {
        IEnumerable<CardModel> enumerable = base
            .Owner.Deck.Cards.Where((CardModel c) => c.Enchantment is Scattered)
            .ToList();

        NRun.Instance?.GlobalUi.GridCardPreviewContainer.ForceMaxColumnsUntilEmpty(3);
        foreach (CardModel item in enumerable)
        {
            CardCmd.Upgrade(item, CardPreviewStyle.GridLayout);
        }
        Owner.GetRelic<SandPile>().UsedUp = true;
        return true;
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default(CancellationToken))
    {
        NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Normal);
        return Task.CompletedTask;
    }

    public override Task DoRemotePostSelectVfx()
    {
        NRestSiteCharacter nRestSiteCharacter = NRestSiteRoom.Instance?.Characters.First(
            (NRestSiteCharacter c) => c.Player == base.Owner
        );
        nRestSiteCharacter?.Shake();
        NRelicFlashVfx nRelicFlashVfx = NRelicFlashVfx.Create(ModelDb.Relic<SandPile>());
        if (nRelicFlashVfx == null)
        {
            return Task.CompletedTask;
        }
        nRestSiteCharacter?.AddChildSafely(nRelicFlashVfx);
        nRelicFlashVfx.Position = Vector2.Zero;
        return Task.CompletedTask;
    }
}
