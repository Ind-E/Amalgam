using Amalgam.AmalgamCode.Extensions;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Amalgam.AmalgamCode.Relics;

[Pool(typeof(EventRelicPool))]
public abstract class AmalgamRelic : CustomRelicModel
{
    // Amalgam/images/relics
    public override string PackedIconPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();
    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();
    protected override string BigIconPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}
