using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Servitors40k
{
    public class ServitorSpecializationDef : Def
    {
        public PawnKindDef pawnKind;

        public Dictionary<SkillDef, int> skillLevels;

        public ThingDef startingEquipment;

        public bool draftable = false;

        public TraitDef givesTrait = null;
    }
}