using Amalgam.Other;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Amalgam.Relics;

[Pool(typeof(EventRelicPool))]
public class WornCleaver : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [StaticTip("AmalgamPare")];

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
        LocString title = HoverTipFactory.L10NStatic(text + ".title");
        LocString description = HoverTipFactory.L10NStatic(text + ".description");
        foreach (DynamicVar dynamicVar in vars)
        {
            title.Add(dynamicVar);
            description.Add(dynamicVar);
        }
        return new HoverTip(title, description);
    }
}
