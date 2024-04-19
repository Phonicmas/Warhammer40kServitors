using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Servitors40k
{
    public class Recipe_SwitchEquipment_Servitor : RecipeWorker
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            Building_ServitorUpgrade building = (Building_ServitorUpgrade)billDoer.CurJob.targetA;

            Servitor servitor = (Servitor)building.SelectedPawn;

            if (servitor.equipment.HasAnything())
            {
                servitor.equipment.DestroyEquipment(servitor.equipment.Primary);
            }

            Thing thing = GenSpawn.Spawn(recipe.GetModExtension<DefModExtension_ServitorRecipeRequirement>().weapon, building.Position, building.Map);
            servitor.equipment.AddEquipment((ThingWithComps)thing.SplitOff(thing.stackCount));
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

            if (servitor.equipment.Primary.def == recipe.GetModExtension<DefModExtension_ServitorRecipeRequirement>().weapon)
            {
                return false;
            }

            return true;
        }

    }
}