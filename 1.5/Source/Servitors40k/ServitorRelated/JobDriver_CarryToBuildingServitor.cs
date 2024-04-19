using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Servitors40k
{
    public class JobDriver_CarryToBuildingServitor : JobDriver
    {
        private Pawn Takee => (Pawn)job.GetTarget(TargetIndex.B).Thing;

        private Building_ServitorUpgrade Building => (Building_ServitorUpgrade)job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => !Building.CanAcceptPawn(Takee));
            yield return Toils_General.Do(delegate
            {
                Building.SelectedPawn = Takee;
            });
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_General.WaitWith(TargetIndex.A, 60, useProgressBar: true);
            yield return Toils_General.Do(delegate
            {
                Building.TryAcceptPawn(Takee);
            });
        }
    }
}