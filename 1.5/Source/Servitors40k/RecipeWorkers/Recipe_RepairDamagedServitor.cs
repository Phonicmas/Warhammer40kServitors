using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Servitors40k
{
    public class Recipe_RepairDamagedServitor : RecipeWorker
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (!(thing is Building_ServitorUpgrade building))
            {
                return false;
            }

            if (building.SelectedPawn == null)
            {
                return false;
            }

            Pawn pawn = building.SelectedPawn;

            if (!(pawn is Servitor servitor))
            {
                return false;
            }

            List<Hediff> hediffs = servitor.health.hediffSet.hediffs.FindAll(x => x is Hediff_Injury);
            if (hediffs.NullOrEmpty())
            {
                return false;
            }

            return true;
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            Building_ServitorUpgrade building = (Building_ServitorUpgrade)billDoer.CurJob.targetA;

            Servitor servitor = (Servitor)building.SelectedPawn;

            List<Hediff> hediffs = servitor.health.hediffSet.hediffs.FindAll(x => x is Hediff_Injury);
            foreach (Hediff hediff in hediffs)
            {
                HealthUtility.Cure(hediff);
            }
        }
    }
}