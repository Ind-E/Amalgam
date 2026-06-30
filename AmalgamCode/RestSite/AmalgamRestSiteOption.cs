using Amalgam.AmalgamCode.Extensions;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Amalgam.AmalgamCode.RestSite;

public abstract class AmalgamRestSiteOption(Player owner) : CustomRestSiteOption(owner)
{
    public override string CustomIconPath => $"ui/rest_site/{Id}.png".ImagePath();

    protected virtual string? Id => null;

    public override string OptionId => Id!.ToUpperInvariant();
}
