using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Amalgam.Relics;

[Pool(typeof(EventRelicPool))]
public class OminousRing : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(666)];

    private int _actPickedUp;

    [SavedProperty]
    public int ActPickedUp
    {
        get { return _actPickedUp; }
        set
        {
            AssertMutable();
            _actPickedUp = value;
        }
    }

    public override async Task AfterObtained()
    {
        ActPickedUp = Owner.RunState.CurrentActIndex;
        await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner);
        await RunManager.Instance.GenerateMap();
    }

    public override ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
    {
        if (ActPickedUp != actIndex)
        {
            return map;
        }
        foreach (var point in map.GetAllMapPoints())
        {
            if (point.PointType == MapPointType.Shop)
            {
                point.PointType = MapPointType.Unknown;
            }
        }

        return map;
    }
}