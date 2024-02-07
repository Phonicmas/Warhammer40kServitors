using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Servitors40k
{
    public class Recipe_InstallImplantWithLevels_ServitorUpgrade : Core40k.Recipe_InstallImplantWithLevels
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            Building_ServitorUpgrade building = (Building_ServitorUpgrade)billDoer.CurJob.targetA;

            Pawn servitor = building.SelectedPawn;

            (BodyPartRecord, bool) t = UpdateRecipeSettings(servitor);

            if (t.Item2)
            {
                servitor.health.hediffSet.hediffs.Find(x => x.def == recipe.addsHediff && x.Part == t.Item1).Severity += 1;
            }
            else
            {
                servitor.health.AddHediff(recipe.addsHediff, t.Item1);
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

            (BodyPartRecord, bool) t = UpdateRecipeSettings(servitor);

            if (t.Item1 == null)
            {
                return false;
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
    
        private (BodyPartRecord, bool) UpdateRecipeSettings(Pawn servitor)
        {
            IEnumerable<BodyPartRecord> parts = GetParts(servitor, recipe);

            BodyPartRecord chosenPart = null;

            bool upgradeExisting = false;

            float currentLowest = 99;

            foreach (BodyPartRecord partRecord in parts)
            {
                if (servitor.health.hediffSet.HasHediff(recipe.addsHediff, partRecord))
                {
                    Hediff hediff = servitor.health.hediffSet.hediffs.Find(x => x.def == recipe.addsHediff && x.Part == partRecord);
                    if (hediff.Severity == hediff.def.maxSeverity)
                    {
                        continue;
                    }
                    if (hediff.Severity <= currentLowest)
                    {
                        currentLowest = hediff.Severity;
                        chosenPart = partRecord;
                        upgradeExisting = true;
                    }
                }
                else
                {
                    currentLowest = -1;
                    chosenPart = partRecord;
                    upgradeExisting = false;
                }
            }
            return (chosenPart, upgradeExisting);
        }
    }
}