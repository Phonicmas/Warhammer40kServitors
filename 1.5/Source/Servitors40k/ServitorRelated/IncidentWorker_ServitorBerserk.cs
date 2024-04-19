using RimWorld;
using System.Collections.Generic;
using Verse;
using System;

namespace Servitors40k
{
    public class IncidentWorker_ServitorBerserk : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            List<Pawn> servitors = parms.target.GameConditionManager.ownerMap.mapPawns.AllPawns.FindAll(x => x is Servitor);
            if (servitors.NullOrEmpty())
            {
                return false;
            }
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            //Find random amount of servitor between 1 and max and make them go berserk
            List<Pawn> servitors = parms.target.GameConditionManager.ownerMap.mapPawns.AllPawns.FindAll(x => x is Servitor);
            Random rand = new Random();
            int amount = rand.Next(1, servitors.Count);
            IEnumerable<Pawn> randomServitors = servitors.TakeRandom(amount);
            foreach (Pawn servitor in randomServitors)
            {
                servitor.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
            }
            return true;
        }
    }
}