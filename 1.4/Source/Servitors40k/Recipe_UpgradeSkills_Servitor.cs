using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Servitors40k
{
    public class Recipe_UpgradeSkills_Servitor : RecipeWorker
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            Building_ServitorUpgrade building = (Building_ServitorUpgrade)billDoer.CurJob.targetA;

            Pawn servitor = building.SelectedPawn;

            //Increase servitor skill here
        }

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

            if (recipe.HasModExtension<DefModExtension_ServitorRecipeRequirement>())
            {
                if (!recipe.GetModExtension<DefModExtension_ServitorRecipeRequirement>().mustBeSpecialization.Contains(servitor.specialization))
                {
                    return false;
                }
            }

            if (pawn.health.hediffSet.HasHediff(recipe.addsHediff))
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(recipe.addsHediff);

                if (hediff.Severity == hediff.def.maxSeverity)
                {
                    return false;
                }
            }

            return true;
        }
    }
}