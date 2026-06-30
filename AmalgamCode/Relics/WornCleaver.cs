using Amalgam.AmalgamCode.RestSite;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Amalgam.AmalgamCode.Relics;

public class WornCleaver : AmalgamRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [StaticTip("AmalgamPare")];

    public override bool TryModifyRestSiteOptions(
        Player player,
        ICollection<RestSiteOption> options
    )
    {
        if (player != Owner)
        {
            return false;
        }
        options.Add(new PareRestSiteOption(player));
        return true;
    }

    private static HoverTip StaticTip(string tip, params DynamicVar[] vars)
    {
        string text = StringHelper.Slugify(tip);
        LocString title = new("static_hover_tips", text + ".title");
        LocString description = new("static_hover_tips", text + ".description");
        foreach (DynamicVar dynamicVar in vars)
        {
            title.Add(dynamicVar);
            description.Add(dynamicVar);
        }
        return new HoverTip(title, description);
    }
}
