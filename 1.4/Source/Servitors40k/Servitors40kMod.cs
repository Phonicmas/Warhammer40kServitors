using HarmonyLib;
using UnityEngine;
using Verse;


namespace Servitors40k
{
    public class Servitors40kMod : Mod
    {
        public static Harmony harmony;

        public Servitors40kMod(ModContentPack content) : base(content)
        {
            harmony = new Harmony("Servitors40k.Mod");
            harmony.PatchAll();
        }
    }
}