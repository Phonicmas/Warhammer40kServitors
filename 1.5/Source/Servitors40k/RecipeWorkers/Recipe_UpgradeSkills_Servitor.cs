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

            Servitor servitor = (Servitor)building.SelectedPawn;

            ServitorSpecializationDef chosenSpecialization = servitor.specialization;

            if (servitor.def.race.mechEnabledWorkTypes != null)
            {
                foreach (WorkTypeDef mechEnabledWorkType in servitor.def.race.mechEnabledWorkTypes)
                {
                    foreach (SkillDef skill in mechEnabledWorkType.relevantSkills)
                    {
                        SkillRecord skillRecord = servitor.skills.skills.Find((SkillRecord rec) => rec.def == skill);
                        skillRecord.levelInt += 5;
                    }
                    servitor.workSettings.SetPriority(mechEnabledWorkType, 1);
                }
            }
            if (chosenSpecialization != null)
            {
                if (!chosenSpecialization.skillLevels.NullOrEmpty())
                {
                    foreach (KeyValuePair<SkillDef, int> skills in chosenSpecialization.skillLevels)
                    {
                        SkillRecord skillRecord = servitor.skills.skills.Find((SkillRecord rec) => rec.def == skills.Key);
                        skillRecord.levelInt += 5;
                    }
                }
            }

            if (recipe.addsHediff != null)
            {
                if (servitor.health.hediffSet.HasHediff(recipe.addsHediff))
                {
                    servitor.health.hediffSet.GetFirstHediffOfDef(recipe.addsHediff).Severity += 1;
                }
                else
                {
                    servitor.health.AddHediff(recipe.addsHediff, GetParts(servitor, recipe).FirstOrDefault()); ;
                }
            }
            
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

        private IEnumerable<BodyPartRecord> GetParts(Pawn pawn, RecipeDef recipe)
        {
            return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, delegate (BodyPartRecord record)
            {
                if (!pawn.health.hediffSet.GetNotMissingParts().Contains(record))
                {
                    return false;
                }
                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record))
                {
                    return false;
                }
                return true;
            });
        }
    }
}