using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Amalgam.Relics;

[Pool(typeof(EventRelicPool))]
public class TinyHatchet : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
}
