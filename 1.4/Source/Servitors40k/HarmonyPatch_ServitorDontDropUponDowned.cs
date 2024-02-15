using HarmonyLib;
using Verse;

namespace Servitors40k
{
    [HarmonyPatch(typeof(Pawn), "DropAndForbidEverything")]
    public static class HarmonyPatch_ServitorDontDropUponDowned
    {
        public static bool Prefix(Pawn __instance)
        {
            if (__instance is Servitor)
            {
                return false;
            }
            return true;
        }
    }
}