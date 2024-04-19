using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;


namespace Servitors40k
{
    public class Servitors40kMod : Mod
    {
        public static Harmony harmony;

        readonly Servitors40kSettings settings;

        public Servitors40kMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Servitors40kSettings>();
            harmony = new Harmony("Servitors40k.Mod");
            harmony.PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled("disableRandomBreaks".Translate(), ref settings.disableRandomBreaks);

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "ModSettingsNameServitors".Translate();
        }

    }
}