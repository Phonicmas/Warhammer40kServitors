using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Servitors40k
{
    public class Recipe_AddApparel_Servitor : RecipeWorker
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            Building_ServitorUpgrade building = (Building_ServitorUpgrade)billDoer.CurJob.targetA;

            Servitor servitor = (Servitor)building.SelectedPawn;
            Apparel apparel = (Apparel)ThingMaker.MakeThing(recipe.GetModExtension<DefModExtension_ServitorRecipeRequirement>().armor);
            servitor.apparel.Wear(apparel, false, true);
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

            if (!servitor.apparel.WornApparel.FindAll(x => x.def == recipe.GetModExtension<DefModExtension_ServitorRecipeRequirement>().armor).NullOrEmpty())
            {
                return false;
            }

            return true;
        }

    }
}