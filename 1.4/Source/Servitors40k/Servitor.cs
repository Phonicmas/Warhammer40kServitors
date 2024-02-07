using RimWorld.Planet;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using VFE.Mechanoids;

namespace Servitors40k
{
    public class Servitor : Pawn
    {
        public ServitorSpecializationDef specialization = null;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            mindState.lastJobTag = JobTag.Idle;
            guest = new Pawn_GuestTracker(this);

            drafter = new Pawn_DraftController(this);

            if (Faction == Faction.OfPlayer && Name == null)
            {
                Name = PawnBioAndNameGenerator.GeneratePawnName(this, NameStyle.Numeric);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref specialization, "chosenSpecialization");
        }
    }
}