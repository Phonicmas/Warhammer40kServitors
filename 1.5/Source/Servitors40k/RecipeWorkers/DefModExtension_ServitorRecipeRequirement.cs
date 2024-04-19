using System.Collections.Generic;
using Verse;

namespace Servitors40k
{
    public class DefModExtension_ServitorRecipeRequirement : DefModExtension
    {
        public List<ServitorSpecializationDef> mustBeSpecialization;

        public ThingDef weapon;

        public ThingDef armor;
    }
}