using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Servitors40k
{
    public class JobDriver_EnterBuildingServitor : JobDriver
    {
        public const int EnterDelay = 60;

        private Building_ServitorUpgrade Building => (Building_ServitorUpgrade)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !Building.CanAcceptPawn(pawn));
            yield return Toils_General.Do(delegate
            {
                Building.SelectedPawn = pawn;
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_General.WaitWith(TargetIndex.A, 60, useProgressBar: true);
            yield return Toils_General.Do(delegate
            {
                Building.TryAcceptPawn(pawn);
            });
        }
    }
}