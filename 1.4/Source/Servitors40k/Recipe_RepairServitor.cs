using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Servitors40k
{
    public class Recipe_RepairServitor : RecipeWorker
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

            if (!servitor.health.hediffSet.HasHediff(recipe.removesHediff))
            {
                return false;
            }

            return true;
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            Building_ServitorUpgrade building = (Building_ServitorUpgrade)billDoer.CurJob.targetA;

            Servitor servitor = (Servitor)building.SelectedPawn;

            Hediff hediff = servitor.health.hediffSet.hediffs.Find((Hediff x) => x.def == recipe.removesHediff);
            if (hediff != null)
            {
                servitor.health.RemoveHediff(hediff);
            }
            servitor.broken = false;
            servitor.breakChance = 2;
        }
    }
}