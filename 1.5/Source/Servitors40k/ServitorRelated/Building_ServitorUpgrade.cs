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
    public class Building_ServitorUpgrade : Building_WorkTable, ISuspendableThingHolder, IThingHolderWithDrawnPawn, IThingHolder
    {
        [Unsaved(false)]
        private Texture2D cachedInsertPawnTex;

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

        private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

        public ThingOwner innerContainer;

        private Pawn ContainedPawn => innerContainer.FirstOrDefault() as Pawn;

        private CompPowerTrader powerComp;

        public CompPowerTrader PowerCompTrader
        {
            get
            {
                if (powerComp == null)
                {
                    powerComp = GetComp<CompPowerTrader>();
                }
                return powerComp;
            }
        }

        public bool PowerOn
        {
            get
            {
                return PowerCompTrader.PowerOn;
            }
        }

        public virtual bool IsContentsSuspended => false;

        protected Pawn selectedPawn;

        public bool AnyAcceptablePawns
        {
            get
            {
                if (!base.Spawned)
                {
                    return false;
                }
                return base.Map.mapPawns.AllPawnsSpawned.Any((Pawn x) => CanAcceptPawn(x));
            }
        }

        public Pawn SelectedPawn
        {
            get
            {
                return selectedPawn;
            }
            set
            {
                selectedPawn = value;
            }
        }

        public float HeldPawnDrawPos_Y => DrawPos.y + 3f / 74f;

        public float HeldPawnBodyAngle => base.Rotation.Opposite.AsAngle;

        public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;

        public virtual Vector3 PawnDrawOffset => IntVec3.West.RotatedBy(base.Rotation).ToVector3();

        public override void Tick()
        {
            base.Tick();
            innerContainer.ThingOwnerTick();
            if (ContainedPawn == null)
            {
                ExitPawn();
                return;
            }
            if (PowerOn)
            {
                return;
            }
            else
            {
                if (selectedPawn != null && selectedPawn.Dead)
                {
                    ExitPawn();
                }
            }
        }

        public virtual AcceptanceReport CanAcceptPawn(Pawn pawn)
        {
            if (!(pawn is Servitor))
            {
                return false;
            }
            if (selectedPawn != null && selectedPawn != pawn)
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

        public void ExitPawn()
        {
            selectedPawn = null;
            if (ContainedPawn == null)
            {
                return;
            }
            if (innerContainer != null)
            {
                Servitor servitor = (Servitor)innerContainer.First();
                servitor.beingServiced = false;
                innerContainer.TryDropAll(def.hasInteractionCell ? InteractionCell : base.Position, base.Map, ThingPlaceMode.Near);
            }   
        }

        private void ExitPawnCancel()
        {
            ExitPawn();
            if (BillStack != null && BillStack.Count > 0)
            {
                if (billStack.FirstShouldDoNow != null)
                {
                    BillStack.FirstShouldDoNow.suspended = true;
                }
                BillStack.Clear();
            }
        }

        public virtual void TryAcceptPawn(Pawn pawn)
        {
            if ((bool)CanAcceptPawn(pawn))
            {
                selectedPawn = pawn;
                bool num = pawn.DeSpawnOrDeselect();
                innerContainer.TryAddOrTransfer(pawn);
                Servitor servitor = (Servitor)innerContainer.First();
                servitor.beingServiced = true;
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
            if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
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
            else if (SelectedPawn == selPawn)
            {
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("EnterBuilding".Translate(this), delegate
                {
                    selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Servitors40kDefOf.BEWH_EnterBuildingServitor, this), JobTag.Misc);
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
            foreach (Thing item in (IEnumerable<Thing>)innerContainer)
            {
                Gizmo gizmo;
                if ((gizmo = SelectContainedItemGizmo(this, item)) != null)
                {
                    yield return gizmo;
                }
            }
            if (selectedPawn != null)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "EjectServitor".Translate();
                command_Action.defaultDesc = "EjectServitorDesc".Translate();
                command_Action.icon = CancelIcon;
                command_Action.action = delegate
                {
                    ExitPawnCancel();
                };
                command_Action.activateSound = SoundDefOf.Designate_Cancel;
                yield return command_Action;
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
                };
                yield return command_Action3;
                yield break;
            }
            Command_Action command_Action4 = new Command_Action();
            command_Action4.defaultLabel = "InsertServitor".Translate() + "...";
            command_Action4.defaultDesc = "InsertServitorUpgradeDesc".Translate();
            command_Action4.icon = InsertPawnTex;
            command_Action4.action = delegate
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (Pawn item in base.Map.mapPawns.AllPawnsSpawned)
                {
                    Pawn pawn = item;
                    AcceptanceReport acceptanceReport = CanAcceptPawn(pawn);
                    string text = (pawn.LabelShortCap);
                    if (!acceptanceReport.Accepted)
                    {
                        if (!acceptanceReport.Reason.NullOrEmpty())
                        {
                            list.Add(new FloatMenuOption(text + ": " + acceptanceReport.Reason, null, pawn, Color.white));
                        }
                    }
                    else
                    {
                        if (pawn.Downed)
                        {
                            Pawn carrier = Map.mapPawns.FreeColonistsSpawned.Find(x => x.workSettings.WorkIsActive(WorkTypeDefOf.Crafting));
                            if (carrier == null)
                            {
                                carrier = Map.mapPawns.FreeColonistsSpawned.RandomElement();
                            }
                            list.Add(new FloatMenuOption(text, delegate
                            {
                                carrier.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Servitors40kDefOf.BEWH_CarryToBuildingServitor, this, pawn), JobTag.Misc);
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
        }

        public override void Draw()
        {
            base.Draw();
            if (selectedPawn != null && innerContainer.Contains(selectedPawn))
            {
                selectedPawn.Drawer.renderer.RenderPawnAt(DrawPos, null, neverAimWeapon: true);
            }
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (selectedPawn != null && innerContainer.Count == 0)
            {
                if (!text.NullOrEmpty())
                {
                    text += "\n";
                }
                text += "WaitingForPawn".Translate(selectedPawn.Named("PAWN")).Resolve();
            }
            else if (ContainedPawn != null)
            {
                if (!text.NullOrEmpty())
                {
                    text += "\n";
                }
                text += "UpgradingServitor".Translate(selectedPawn.Named("PAWN")).Resolve();
            }
            
             
            return text;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_References.Look(ref selectedPawn, "selectedPawn");
        }

        protected virtual void SelectPawn(Pawn pawn)
        {
            selectedPawn = pawn;
            if (!pawn.Downed)
            {
                pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Servitors40kDefOf.BEWH_EnterBuildingServitor, this), JobTag.Misc);
            }
        }

        public Building_ServitorUpgrade()
        {
            innerContainer = new ThingOwner<Thing>(this);
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            if (selectedPawn != null && selectedPawn.Map == Map)
            {
                GenDraw.DrawLineBetween(this.TrueCenter(), selectedPawn.TrueCenter());
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
            base.DeSpawn(mode);
        }
    }
}