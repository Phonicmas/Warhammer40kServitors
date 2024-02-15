using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using VFE.Mechanoids.Needs;

namespace Servitors40k
{
    [StaticConstructorOnStartup]
    public class Building_ServitorCreator : Building_Enterable, IThingHolderWithDrawnPawn, IThingHolder
    {
        //Let it be determined by the specializationDef
        private int ticksRemaining = 60000;

        //Let it be determined by the specializationDef
        private int fuelConsumption = 2;

        private int powerCutTicks;

        [Unsaved(false)]
        private CompPowerTrader cachedPowerComp;

        [Unsaved(false)]
        private Texture2D cachedInsertPawnTex;

        [Unsaved(false)]
        private Texture2D cachedChooseSpecializationTex;

        [Unsaved(false)]
        private Sustainer sustainerWorking;

        [Unsaved(false)]
        private Effecter progressBar;

        private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

        private Pawn ContainedPawn => innerContainer.FirstOrDefault() as Pawn;

        private ServitorSpecializationDef chosenSpecialization = Servitors40kDefOf.BEWH_ServitorUtility;

        public bool PowerOn => PowerTraderComp.PowerOn;

        public override bool IsContentsSuspended => false;

        private CompPowerTrader PowerTraderComp
        {
            get
            {
                if (cachedPowerComp == null)
                {
                    cachedPowerComp = this.TryGetComp<CompPowerTrader>();
                }
                return cachedPowerComp;
            }
        }

        public CompRefuelable fuelComp;

        private CompRefuelable Fuel
        {
            get
            {
                if (fuelComp == null)
                {
                    fuelComp = this.TryGetComp<CompRefuelable>();
                }
                return fuelComp;
            }
        }

        private IEnumerable<ServitorSpecializationDef> specializations = null;

        public IEnumerable<ServitorSpecializationDef> Specializations
        {
            get
            {
                if (specializations == null)
                {
                    specializations = DefDatabase<ServitorSpecializationDef>.AllDefs;
                }
                return specializations;
            }
        }

        public Texture2D InsertPawnTex
        {
            get
            {
                if (cachedInsertPawnTex == null)
                {
                    cachedInsertPawnTex = ContentFinder<Texture2D>.Get("UI/Gizmos/InsertPawn");
                }
                return cachedInsertPawnTex;
            }
        }

        public Texture2D ChooseSpecializationTex
        {
            get
            {
                if (cachedChooseSpecializationTex == null)
                {
                    cachedChooseSpecializationTex = ContentFinder<Texture2D>.Get("UI/Gizmos/ChooseSpecialization");
                }
                return cachedChooseSpecializationTex;
            }
        }

        public float HeldPawnDrawPos_Y => DrawPos.y + 3f / 74f;

        public float HeldPawnBodyAngle => base.Rotation.Opposite.AsAngle;

        public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;

        public override Vector3 PawnDrawOffset => IntVec3.West.RotatedBy(base.Rotation).ToVector3();

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            sustainerWorking = null;
            if (progressBar != null)
            {
                progressBar.Cleanup();
                progressBar = null;
            }
            base.DeSpawn(mode);
        }

        public override void Tick()
        {
            base.Tick();
            innerContainer.ThingOwnerTick();
            if (this.IsHashIntervalTick(250))
            {
                PowerTraderComp.PowerOutput = (base.Working ? (0f - base.PowerComp.Props.PowerConsumption) : (0f - base.PowerComp.Props.idlePowerDraw));
            }
            if (base.Working)
            {
                if (ContainedPawn == null)
                {
                    Cancel();
                    return;
                }
                if (PowerTraderComp.PowerOn)
                {
                    TickEffects();
                    if (PowerOn)
                    {
                        ticksRemaining--;
                    }
                    if (ticksRemaining <= 0)
                    {
                        Finish();
                    }
                    return;
                }
                powerCutTicks++;
                if (powerCutTicks >= 60000)
                {
                    Pawn containedPawn = ContainedPawn;
                    if (containedPawn != null)
                    {
                        Messages.Message("ServitorNoPowerEject".Translate(containedPawn.Named("PAWN")), containedPawn, MessageTypeDefOf.NegativeEvent, historical: false);
                    }
                    Cancel();
                }
            }
            else
            {
                if (selectedPawn != null && selectedPawn.Dead)
                {
                    Cancel();
                }
                if (progressBar != null)
                {
                    progressBar.Cleanup();
                    progressBar = null;
                }
            }
        }

        private void TickEffects()
        {
            if (sustainerWorking == null || sustainerWorking.Ended)
            {
                sustainerWorking = SoundDefOf.GeneExtractor_Working.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            }
            else
            {
                sustainerWorking.Maintain();
            }
            if (progressBar == null)
            {
                progressBar = EffecterDefOf.ProgressBarAlwaysVisible.Spawn();
            }
            progressBar.EffectTick(new TargetInfo(base.Position + IntVec3.North.RotatedBy(base.Rotation), base.Map), TargetInfo.Invalid);
            MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBar.children[0]).mote;
            if (mote != null)
            {
                mote.progress = 1f - Mathf.Clamp01((float)ticksRemaining / 30000f);
                mote.offsetZ = ((base.Rotation == Rot4.North) ? 0.5f : (-0.5f));
            }
        }

        public override AcceptanceReport CanAcceptPawn(Pawn pawn)
        {
            if (!pawn.IsColonist && !pawn.IsSlaveOfColony && !pawn.IsPrisonerOfColony)
            {
                return false;
            }
            if (selectedPawn != null && selectedPawn != pawn)
            {
                return false;
            }
            if (!pawn.RaceProps.Humanlike || pawn.IsQuestLodger())
            {
                return false;
            }
            if (!PowerOn)
            {
                return "NoPower".Translate().CapitalizeFirst();
            }
            if (innerContainer.Count > 0)
            {
                return "Occupied".Translate();
            }

            return true;
        }

        private void Cancel()
        {
            startTick = -1;
            selectedPawn = null;
            sustainerWorking = null;
            powerCutTicks = 0;

            //Kill pawn by chance?
            innerContainer.TryDropAll(def.hasInteractionCell ? InteractionCell : base.Position, base.Map, ThingPlaceMode.Near);
        }

        private void Finish()
        {
            startTick = -1;
            selectedPawn = null;
            sustainerWorking = null;
            powerCutTicks = 0;
            if (ContainedPawn == null)
            {
                return;
            }

            Servitor servitor = (Servitor)PawnGenerator.GeneratePawn(new PawnGenerationRequest(chosenSpecialization.pawnKind, Faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: false));
            IntVec3 intVec = (def.hasInteractionCell ? InteractionCell : base.Position);
            servitor.Position = intVec;
            servitor.Rotation = Rot4.South;
            servitor.SpawnSetup(Map, false);
            servitor.specialization = chosenSpecialization;
            if (servitor.story == null)
            {
                servitor.story = new Pawn_StoryTracker(servitor);
                if (innerContainer.FirstOrDefault() is Pawn pawn)
                {
                    if (pawn.gender == Gender.Male)
                    {
                        servitor.story.bodyType = BodyTypeDefOf.Male;
                    }
                    else if (pawn.gender == Gender.Female)
                    {
                        servitor.story.bodyType = BodyTypeDefOf.Female;
                    }
                    else
                    {
                        servitor.story.bodyType = BodyTypeDefOf.Hulk;
                    }
                }
                
            }
            if (chosenSpecialization.givesTrait != null)
            {
                if (servitor.story.traits == null)
                {
                    servitor.story.traits = new TraitSet(servitor);
                }
                Trait trait = new Trait(chosenSpecialization.givesTrait);
                if (trait != null)
                {
                    servitor.story.traits.GainTrait(trait);
                }
            }
            innerContainer.FirstOrDefault().Destroy();
            if (servitor.skills == null)
            {
                servitor.skills = new Pawn_SkillTracker(servitor);
            }          
            if (servitor.workSettings == null)
            {
                servitor.workSettings = new Pawn_WorkSettings(servitor);
            }              
            if (chosenSpecialization.startingEquipment != null)
            {
                ThingWithComps newEq = (ThingWithComps)ThingMaker.MakeThing(chosenSpecialization.startingEquipment);
                servitor.equipment.AddEquipment(newEq);
            }
            if (servitor.relations == null)
            {
                servitor.relations = new Pawn_RelationsTracker(servitor);
            }
            DefMap<WorkTypeDef, int> defMap = new DefMap<WorkTypeDef, int>();
            defMap.SetAll(0);
            typeof(Pawn_WorkSettings).GetField("priorities", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(servitor.workSettings, defMap);
            if (servitor.def.race.mechEnabledWorkTypes != null)
            {
                foreach (WorkTypeDef mechEnabledWorkType in servitor.def.race.mechEnabledWorkTypes)
                {
                    foreach (SkillDef skill in mechEnabledWorkType.relevantSkills)
                    {
                        SkillRecord skillRecord = servitor.skills.skills.Find((SkillRecord rec) => rec.def == skill);
                        skillRecord.levelInt = servitor.RaceProps.mechFixedSkillLevel;
                    }
                    servitor.workSettings.SetPriority(mechEnabledWorkType, 1);                    
                }
            }
            if (!chosenSpecialization.skillLevels.NullOrEmpty())
            {
                foreach (KeyValuePair<SkillDef, int> skills in chosenSpecialization.skillLevels)
                {
                    SkillRecord skillRecord = servitor.skills.skills.Find((SkillRecord rec) => rec.def == skills.Key);
                    skillRecord.levelInt = skills.Value;
                }
            }
        }

        public override void TryAcceptPawn(Pawn pawn)
        {
            if ((bool)CanAcceptPawn(pawn))
            {
                selectedPawn = pawn;
                IntVec3 pos = pawn.Position;
                bool num = pawn.DeSpawnOrDeselect();
                if (innerContainer.TryAddOrTransfer(pawn))
                {
                    startTick = Find.TickManager.TicksGame;
                    Fuel.ConsumeFuel(fuelConsumption);
                    ticksRemaining = 2500;
                    pawn.equipment.DropAllEquipment(pos);
                    pawn.apparel.DropAll(pos);
                }
                if (num)
                {
                    Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
                }
            }
        }

        //Method for when right clicking with a pawn
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
            {
                yield return floatMenuOption;
            }
            if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly) && !selPawn.IsPrisonerOfColony)
            {
                yield return new FloatMenuOption("CannotEnterBuilding".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }
            AcceptanceReport acceptanceReport = CanAcceptPawn(selPawn);
            if (acceptanceReport.Accepted)
            {
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("EnterBuilding".Translate(this), delegate
                {
                    SelectPawn(selPawn);
                }), selPawn, this);
            }
            else if (base.SelectedPawn == selPawn)
            {
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("EnterBuilding".Translate(this), delegate
                {
                    selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.EnterBuilding, this), JobTag.Misc);
                }), selPawn, this);
            }
            else if (!acceptanceReport.Reason.NullOrEmpty())
            {
                yield return new FloatMenuOption("CannotEnterBuilding".Translate(this) + ": " + acceptanceReport.Reason.CapitalizeFirst(), null);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (base.Working)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "CommandCancelProcedure".Translate();
                command_Action.defaultDesc = "CommandCancelProcedureDesc".Translate();
                command_Action.icon = CancelIcon;
                command_Action.action = delegate
                {
                    Cancel();
                };
                command_Action.activateSound = SoundDefOf.Designate_Cancel;
                yield return command_Action;
                if (DebugSettings.ShowDevGizmos)
                {
                    Command_Action command_Action2 = new Command_Action();
                    command_Action2.defaultLabel = "DevFinishProcedure".Translate();
                    command_Action2.action = delegate
                    {
                        Finish();
                    };
                    yield return command_Action2;
                }
                yield break;
            }
            if (selectedPawn != null)
            {
                Command_Action command_Action3 = new Command_Action();
                command_Action3.defaultLabel = "CommandCancelLoad".Translate();
                command_Action3.defaultDesc = "CommandCancelLoadDesc".Translate();
                command_Action3.icon = CancelIcon;
                command_Action3.activateSound = SoundDefOf.Designate_Cancel;
                command_Action3.action = delegate
                {
                    innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
                    if (selectedPawn.CurJobDef == JobDefOf.EnterBuilding)
                    {
                        selectedPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                    selectedPawn = null;
                    startTick = -1;
                    sustainerWorking = null;
                };
                yield return command_Action3;
                yield break;
            }
            Command_Action command_Action4 = new Command_Action();
            command_Action4.defaultLabel = "InsertPerson".Translate() + "...";
            command_Action4.defaultDesc = "InsertPersonServitorCreationDesc".Translate();
            command_Action4.icon = InsertPawnTex;
            command_Action4.action = delegate
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (Pawn item in base.Map.mapPawns.AllPawnsSpawned)
                {
                    Pawn pawn = item;
                    AcceptanceReport acceptanceReport = CanAcceptPawn(pawn);
                    string text;
                    if (pawn.IsPrisonerOfColony)
                    {
                        text = "Prisoner".Translate();
                    }
                    else if (pawn.IsSlaveOfColony)
                    {
                        text = "Slave".Translate();
                    }
                    else
                    {
                        text = "Colonist".Translate();
                    }
                    text += (" " + pawn.LabelShortCap);
                    if (!acceptanceReport.Accepted)
                    {
                        if (!acceptanceReport.Reason.NullOrEmpty())
                        {
                            list.Add(new FloatMenuOption(text + ": " + acceptanceReport.Reason, null, pawn, Color.white));
                        }
                    }
                    else
                    {
                        if (pawn.IsPrisonerOfColony)
                        {
                            Pawn carrier = Map.mapPawns.FreeColonistsSpawned.Find(x => x.workSettings.WorkIsActive(WorkTypeDefOf.Warden));
                            if (carrier == null)
                            {
                                carrier = Map.mapPawns.FreeColonistsSpawned.RandomElement();
                            }
                            list.Add(new FloatMenuOption(text, delegate
                            {
                                carrier.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.CarryToBuilding, this, pawn), JobTag.Misc);
                                if (carrier.jobs.curJob.count <= 0)
                                {
                                    carrier.jobs.curJob.count = 1;
                                }
                            }, pawn, Color.white));
                        }
                        else
                        {
                            list.Add(new FloatMenuOption(text, delegate
                            {
                                SelectPawn(pawn);
                            }, pawn, Color.white));
                        }
                        
                    }
                }
                if (!list.Any())
                {
                    list.Add(new FloatMenuOption("NoValidPawns".Translate(), null));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            };
            if (!PowerOn)
            {
                command_Action4.Disable("NoPower".Translate().CapitalizeFirst());
            }
            yield return command_Action4;
            Command_Action command_Action5 = new Command_Action();
            command_Action5.defaultLabel = "SelectSpecialization".Translate() + "...";
            command_Action5.defaultDesc = "SelectSpecializationDesc".Translate();
            command_Action5.icon = ChooseSpecializationTex;
            command_Action5.action = delegate
            {
                //Make a float menu for slection specialization just like when selecting a pawn to insert
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                string text;
                foreach (ServitorSpecializationDef servitorSpec in Specializations)
                {
                    if (servitorSpec == chosenSpecialization)
                    {
                        continue;
                    }
                    text = servitorSpec.label;
                    list.Add(new FloatMenuOption(text, delegate
                    {
                        chosenSpecialization = servitorSpec;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            };
            yield return command_Action5;
        }

        public override void Draw()
        {
            base.Draw();
            if (base.Working && selectedPawn != null && innerContainer.Contains(selectedPawn))
            {
                selectedPawn.Drawer.renderer.RenderPawnAt(DrawPos, null, neverAimWeapon: true);
            }
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            text += "\nChosen specialization: ";
            if (chosenSpecialization != null)
            {
                text += "" + chosenSpecialization.label;
            }
            else
            {
                text += "None";
            }
            if (selectedPawn != null && innerContainer.Count == 0)
            {
                if (!text.NullOrEmpty())
                {
                    text += "\n";
                }
                text += "WaitingForPawn".Translate(selectedPawn.Named("PAWN")).Resolve();
            }
            else if (base.Working && ContainedPawn != null)
            {
                if (!text.NullOrEmpty())
                {
                    text += "\n";
                }
                text += "CreatingServitorFrom".Translate(selectedPawn.Named("PAWN")).Resolve();
            }
            
             
            return text;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
            Scribe_Values.Look(ref powerCutTicks, "powerCutTicks", 0);
            Scribe_Defs.Look(ref chosenSpecialization, "chosenSpecialization");
        }

        protected override void SelectPawn(Pawn pawn)
        {
            selectedPawn = pawn;
            if (!pawn.IsPrisonerOfColony && !pawn.Downed)
            {
                pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.EnterBuilding, this), JobTag.Misc);
            }
        }
    }
}