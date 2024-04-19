using Verse;

namespace Servitors40k
{
    public class Servitors40kSettings : ModSettings
    {

        public bool disableRandomBreaks = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref disableRandomBreaks, "disableRandomBreaks", false);
            base.ExposeData();
        }
    }
}