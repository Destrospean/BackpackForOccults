using MonoPatcherLib;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects;
using Sims3.SimIFace;
using Sims3.UI.Hud;
using static Sims3.Gameplay.Destrospean.BackpackForOccults;

namespace Destrospean
{
    [Plugin]
    public class BackpackForOccults
    {
        [Tunable]
        protected static bool kInstantiator;

        static BackpackForOccults()
        {
            kInstantiator = false;
            LoadSaveManager.ObjectGroupsPreLoad += OnPreLoad;
        }

        public class UseBackpack : Backpack.UseBackpack
        {
            [DoesntRequireTuning]
            public class DefinitionModified : ImmediateInteractionDefinition<Sim, Backpack, UseBackpack>
            {
                Definition mDefinitionBase = new Definition();

                public override string GetInteractionName(Sim actor, Backpack target, InteractionObjectPair iop)
                {
                    return mDefinitionBase.GetInteractionName(actor, target, iop);
                }

                public override bool Test(Sim actor, Backpack target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (actor.SimDescription.ChildOrBelow)
                    {
                        return false;
                    }
                    if (HasDisallowedTransformation(actor))
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Backpack.LocalizeString(actor.IsFemale, "TransformationBuffCantWearBackpack"));
                        return false;
                    }
                    if (!IsAllowedToUseBackpack(actor))
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Backpack.LocalizeString(actor.IsFemale, "OccultCantWearBackpack"));
                        return false;
                    }
                    return true;
                }
            }

            public override bool RunFromInventory()
            {
                if (Target.UsingBackpack)
                {
                    if (Actor.Posture is Backpack.HoldingBackpackPosture)
                    {
                        if (!Actor.InteractionQueue.HasInteractionOfTypeAndTarget(Backpack.StopUsingBackpack.Singleton, Target))
                        {
                            InteractionInstance startUsingBackpack = Backpack.StopUsingBackpack.Singleton.CreateInstance(Target, Actor, new InteractionPriority(InteractionPriorityLevel.UserDirected), false, true);
                            startUsingBackpack.Hidden = true;
                            Actor.InteractionQueue.PushAsContinuation(startUsingBackpack, true);
                        }
                    }
                    else
                    {
                        Target.UsingBackpack = false;
                    }
                }
                else if (Actor.Posture is Backpack.HoldingBackpackPosture || Actor.Posture == Actor.Standing)
                {
                    if (!Actor.InteractionQueue.HasInteractionOfTypeAndTarget(Backpack.StartUsingBackpack.Singleton, Target))
                    {
                        InteractionInstance stopUsingBackpack = Backpack.StartUsingBackpack.Singleton.CreateInstance(Target, Actor, new InteractionPriority(InteractionPriorityLevel.UserDirected), false, true);
                        stopUsingBackpack.Hidden = true;
                        Actor.InteractionQueue.PushAsContinuation(stopUsingBackpack, true);
                    }
                }
                else
                {
                    Target.UsingBackpack = true;
                }
                return true;
            }
        }

        public static bool HasDisallowedTransformation(Sim sim)
        {
            return (sim.BuffManager.HasElement(BuffNames.Werewolf) && !kUsableInWerewolfForm) || (sim.BuffManager.HasAnyElement(BuffNames.Zombie, BuffNames.PermaZombie) && !kUsableForZombies);
        }

        [ReplaceMethod(typeof(Backpack), nameof(Backpack.IsAllowedToUseBackpack))]
        public static bool IsAllowedToUseBackpack(Sim sim)
        {
            switch (sim.OccultManager.CurrentOccultTypes)
            {
                case OccultTypes.Mummy:
                    if (!kUsableForMummies)
                    {
                        return false;
                    }
                    break;
                case OccultTypes.Frankenstein:
                    if (!kUsableForSimbots)
                    {
                        return false;
                    }
                    break;
                case OccultTypes.Vampire:
                    if (!kUsableForVampires)
                    {
                        return false;
                    }
                    break;
                case OccultTypes.ImaginaryFriend:
                    if (!kUsableForImaginaryFriends)
                    {
                        return false;
                    }
                    break;
                case OccultTypes.Unicorn:
                    return false;
                case OccultTypes.Genie:
                    if (!kUsableForGenies)
                    {
                        return false;
                    }
                    break;
                case OccultTypes.Ghost:
                    if (!kUsableForGhosts)
                    {
                        return false;
                    }
                    break;
                case OccultTypes.Fairy:
                    if (!kUsableForFairies)
                    {
                        return false;
                    }
                    break;
            }
            return !(HasDisallowedTransformation(sim) || (sim.IsEP11Bot && !kUsableForPlumbots) || ((sim.SimDescription.IsGhost || sim.SimDescription.DeathStyle != 0) && !kUsableForGhosts) || sim.IsPet);
        }

        static void OnPreLoad()
        {
            Backpack.UseBackpack.Singleton = new UseBackpack.DefinitionModified();
        }
    }
}