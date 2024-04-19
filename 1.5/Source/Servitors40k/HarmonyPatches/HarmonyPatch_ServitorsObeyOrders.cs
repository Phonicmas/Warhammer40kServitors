using HarmonyLib;
using RimWorld;
using Verse;

namespace Servitors40k
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "CanTakeOrder")]
    public static class ServitorsObeyOrders
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (!__result && pawn.drafter != null && pawn is Servitor && pawn.Faction != null && pawn.Faction.IsPlayer)
            {
                __result = true;
            }
        }
    }
}