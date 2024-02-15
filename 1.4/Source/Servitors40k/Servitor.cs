using RimWorld.Planet;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using VFE.Mechanoids;
using System;

namespace Servitors40k
{
    public class Servitor : Pawn
    {
        public ServitorSpecializationDef specialization = null;

        public int breakChance = 2;

        public bool broken = false;

        public bool beingServiced = false;

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

        public override void Tick()
        {
            base.Tick();
            if (!beingServiced && !broken && this.IsHashIntervalTick(30000))
            {
                Log.Message("Break Chance: " + breakChance);
                Random rand = new Random();
                breakChance += 2;
                if (rand.Next(0,100) < breakChance)
                {
                    Log.Message("Broke Down");
                    broken = true;
                    health.AddHediff(Servitors40kDefOf.BEWH_ServitorBroken);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref specialization, "chosenSpecialization");
            Scribe_Values.Look(ref breakChance, "breakChance", 2);
            Scribe_Values.Look(ref broken, "broken", false);
            Scribe_Values.Look(ref beingServiced, "beingServiced", false);
        }
    }
}