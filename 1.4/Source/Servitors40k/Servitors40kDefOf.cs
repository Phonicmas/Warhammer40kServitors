using RimWorld;
using Verse;

namespace Servitors40k
{
    [DefOf]
    public static class Servitors40kDefOf
    {
        public static ServitorSpecializationDef BEWH_ServitorUtility;

        public static JobDef BEWH_EnterBuildingServitor;

        static Servitors40kDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(Servitors40kDefOf));
        }
    }
}