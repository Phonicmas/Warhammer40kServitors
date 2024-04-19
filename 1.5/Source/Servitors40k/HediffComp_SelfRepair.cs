using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Servitors40k
{
    public class HediffComp_SelfRepair : HediffComp
    {
        public HediffCompProperties_SelfRepair Props => (HediffCompProperties_SelfRepair)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.IsHashIntervalTick((int)(Props.tickInterval * parent.Severity)))
            {
                HealthUtility.FixWorstHealthCondition(Pawn);
            }
        }
    }
}