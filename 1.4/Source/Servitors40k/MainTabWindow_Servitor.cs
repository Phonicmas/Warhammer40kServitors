using RimWorld.Planet;
using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;

namespace Servitors40k
{
    public class MainTabWindow_Servitors : MainTabWindow_PawnTable
    {
        [DefOf]
        public static class PawnTableDefOf
        {
            public static PawnTableDef BEWH_ServitorPawnTable;
        }

        protected override PawnTableDef PawnTableDef => PawnTableDefOf.BEWH_ServitorPawnTable;

        protected override IEnumerable<Pawn> Pawns => from p in Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer)
                                                      where p is Servitor
                                                      select p;

        public override void PostOpen()
        {
            base.PostOpen();
            Find.World.renderer.wantedMode = WorldRenderMode.None;
        }
    }
}