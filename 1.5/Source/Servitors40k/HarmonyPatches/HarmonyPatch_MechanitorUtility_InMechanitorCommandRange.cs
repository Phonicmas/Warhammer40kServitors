using HarmonyLib;
using Verse;

namespace Servitors40k
{
    [HarmonyPatch(typeof(MechanitorUtility), "InMechanitorCommandRange")]
    public static class MechanitorUtility_InMechanitorCommandRange
    {
        public static void Postfix(Pawn mech, ref bool __result)
        {
            if (mech is Servitor)
            {
                __result = true;
            }
        }
    }
}