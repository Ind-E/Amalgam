using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Amalgam.Relics;

[Pool(typeof(EventRelicPool))]
public class JunkDrawer : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    private const string _relicsKey = "Relics";

    public override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(_relicsKey, 2)];

    public override bool TryModifyCardRewardAlternatives(
        Player player,
        CardReward cardReward,
        List<CardRewardAlternative> alternatives
    )
    {
        if (Owner != player)
        {
            return false;
        }
        alternatives.Add(
            new CardRewardAlternative(
                PaelsWing.sacrificeAlternativeKey,
                OnSacrificeSynchronized,
                PostAlternateCardRewardAction.DismissScreenAndRemoveReward
            )
        );
        return true;
    }

    private async Task OnSacrificeSynchronized()
    {
        int num = Owner.Relics.IndexOf(this);
        if (num < 0)
        {
            throw new InvalidOperationException();
        }
        var rewardSync = RunManager.Instance.RewardSynchronizer;
        PaelsWingSacrificeMessage message = new()
        {
            relicIndex = (uint)num,
            Location = rewardSync._messageBuffer.CurrentLocation,
        };
        rewardSync._gameService.SendMessage(message);
        await OnSacrifice();
    }

    public async Task OnSacrifice()
    {
        Flash();
        for (int i = 0; i < DynamicVars[_relicsKey].IntValue; i++)
        {
            var randomRelic = RelicFactory.PullNextRelicFromFront(Owner).ToMutable();
            ScrapRelic.IsScrap.Set(randomRelic, true);
            await RelicCmd.Obtain(randomRelic, Owner);
        }
    }

    public override async Task AfterCombatEnd(CombatRoom _)
    {
        if (
            Owner.Relics.FirstOrDefault(r =>
                r is not null && ScrapRelic.IsScrap.Get(r) && !r.IsMelted
            )
            is RelicModel relicModel
        )
        {
            Flash();
            relicModel.IsWax = true; // so melt doesn't throw an error.
            await RelicCmd.Melt(relicModel);
            await Cmd.CustomScaledWait(0.5f, 0.75f);
        }
    }

    [HarmonyPatch(typeof(RelicModel))]
    static class ScrapRelic
    {
        public static readonly SavedSpireField<RelicModel, bool> IsScrap = new(
            () => false,
            "AMALGAM-IsScrap"
        );

        [HarmonyPatch(nameof(RelicModel.HoverTip), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool PrefixHoverTip(RelicModel __instance, ref HoverTip __result)
        {
            if (__instance.IsMelted && IsScrap.Get(__instance))
            {
                LocString brokenRelicPrefix = new(
                    "relics",
                    "AMALGAM-JUNK_DRAWER.brokenRelicPrefix"
                );
                brokenRelicPrefix.Add("description", __instance.DynamicDescription);

                HoverTip result = new(__instance.Title, brokenRelicPrefix);
                result.SetCanonicalModel(__instance.CanonicalInstance);
                __result = result;

                return false;
            }
            return true;
        }

        public static LocString ScrapRelicPrefix =>
            new("relics", "AMALGAM-JUNK_DRAWER.scrapRelicPrefix");

        [HarmonyPatch(nameof(RelicModel.Title), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool PrefixTitle(RelicModel __instance, ref LocString __result)
        {
            if (IsScrap.Get(__instance))
            {
                LocString title = new("relics", __instance.Id.Entry + ".title");
                LocString scrapRelicPrefix = ScrapRelicPrefix;
                scrapRelicPrefix.Add("Title", title);
                __result = scrapRelicPrefix;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(RelicModel.UpdateTexture))]
        [HarmonyPrefix]
        private static void PrefixUpdateTexture(RelicModel __instance, TextureRect texture)
        {
            if (!IsScrap.Get(__instance) || texture.GetParent().Name != "Relic")
                return;

            Vector2 originalGlobalPos = texture.GlobalPosition;

            Control control = new();

            ShaderMaterial material = new()
            {
                Shader = GD.Load<Shader>("res://Amalgam/shaders/scrap_relic.gdshader"),
            };

            material.SetShaderParameter("seed", texture.GetHashCode() % 1000.0 / 100.0);
            CanvasGroup canvasGroup = new() { Material = material };

            texture.AddSibling(canvasGroup);
            texture.GetParent().RemoveChild(texture);
            canvasGroup.AddChild(control);
            control.AddChild(texture);

            texture.GlobalPosition = originalGlobalPos;
        }
    }
}
