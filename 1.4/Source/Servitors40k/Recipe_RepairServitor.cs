using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Servitors40k
{
    public class Recipe_RepairServitor : Recipe_RemoveHediff
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            if (!(thing is Building_ServitorUpgrade building))
            {
                return false;
            }

            if (building.SelectedPawn == null)
            {
                return false;
            }

            Pawn pawn = building.SelectedPawn;

            if (!(pawn is Servitor servitor))
            {
                return false;
            }

            if (!servitor.health.hediffSet.HasHediff(recipe.removesHediff))
            {
                return false;
            }

            return true;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
                if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(billDoer))
                {
                    string text = (recipe.successfullyRemovedHediffMessage.NullOrEmpty() ? ((string)"MessageSuccessfullyRemovedHediff".Translate(billDoer.LabelShort, pawn.LabelShort, recipe.removesHediff.label.Named("HEDIFF"), billDoer.Named("SURGEON"), pawn.Named("PATIENT"))) : ((string)recipe.successfullyRemovedHediffMessage.Formatted(billDoer.LabelShort, pawn.LabelShort)));
                    Messages.Message(text, pawn, MessageTypeDefOf.PositiveEvent);
                }
            }
            Hediff hediff = pawn.health.hediffSet.hediffs.Find((Hediff x) => x.def == recipe.removesHediff);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
            if (pawn is Servitor servitor)
            {
                servitor.broken = false;
                servitor.breakChance = 2;
            }
        }
    }
}