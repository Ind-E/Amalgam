using Amalgam.AmalgamCode.Enchantmnets;
using Amalgam.AmalgamCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace Amalgam.AmalgamCode.RestSite;

public class CollectRestSiteOption(Player owner) : AmalgamRestSiteOption(owner)
{
    protected override string Id => "collect";

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

    public override async Task<bool> OnSelect()
    {
        IEnumerable<CardModel> enumerable =
        [
            .. Owner.Deck.Cards.Where(c => c.Enchantment is Scattered),
        ];

        NRun.Instance?.GlobalUi.GridCardPreviewContainer.ForceMaxColumnsUntilEmpty(3);
        foreach (CardModel item in enumerable)
        {
            CardCmd.Upgrade(item, CardPreviewStyle.GridLayout);
        }
        if (Owner.GetRelic<SandPile>() is SandPile sandPile)
        {
            sandPile.UsedUp = true;
        }
        return true;
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Normal);
        return Task.CompletedTask;
    }

    public override Task DoRemotePostSelectVfx()
    {
        if (
            NRestSiteRoom.Instance?.Characters.FirstOrDefault(c => c.Player == Owner)
            is not NRestSiteCharacter restSiteCharacter
        )
        {
            return Task.CompletedTask;
        }

        restSiteCharacter.Shake();

        if (NRelicFlashVfx.Create(ModelDb.Relic<SandPile>()) is not NRelicFlashVfx relicFlashVfx)
            return Task.CompletedTask;

        relicFlashVfx.Position = Vector2.Zero;
        restSiteCharacter.AddChildSafely(relicFlashVfx);
        return Task.CompletedTask;
    }
}
