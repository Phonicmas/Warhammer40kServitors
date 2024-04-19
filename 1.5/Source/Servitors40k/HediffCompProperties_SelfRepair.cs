using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Servitors40k
{
    public class HediffCompProperties_SelfRepair : HediffCompProperties
    {
        public int tickInterval;

        public HediffCompProperties_SelfRepair()
        {
            compClass = typeof(HediffComp_SelfRepair);
        }

    }
}