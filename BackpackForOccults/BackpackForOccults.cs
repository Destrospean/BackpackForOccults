using MonoPatcherLib;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects;
using Sims3.SimIFace;
using Sims3.UI.Hud;
using Tuning = Sims3.Gameplay.Destrospean.BackpackForOccults;

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
            public class DefinitionModified : ImmediateInteractionDefinition<Sim, Backpack, UseBackpack>
            {
                Definition mDefinitionBase = new Definition();

                public override string GetInteractionName(Sim actor, Backpack target, InteractionObjectPair interaction)
                {
                    return mDefinitionBase.GetInteractionName(actor, target, interaction);
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
            return (sim.BuffManager.HasElement(BuffNames.Werewolf) && !Tuning.kUsableInWerewolfForm) || (sim.BuffManager.HasAnyElement(BuffNames.Zombie, BuffNames.PermaZombie) && !Tuning.kUsableForZombies);
        }

        [ReplaceMethod(typeof(Backpack), "IsAllowedToUseBackpack")]
        public static bool IsAllowedToUseBackpack(Sim sim)
        {
            switch (sim.OccultManager.CurrentOccultTypes)
            {
                case OccultTypes.Mummy:
                    if (!Tuning.kUsableForMummies)
                    {
                        return false;
                    }
                    break;
                case OccultTypes.Frankenstein:
                    if (!Tuning.kUsableForSimBots)
                    {
                        return false;
                    }
                    break;
                case OccultTypes.Vampire:
                    if (!Tuning.kUsableForVampires)
                    {
                        return false;
                    }
                    break;
                case OccultTypes.ImaginaryFriend:
                    if (!Tuning.kUsableForImaginaryFriends)
                    {
                        return false;
                    }
                    break;
                case OccultTypes.Unicorn:
                    return false;
                case OccultTypes.Genie:
                    if (!Tuning.kUsableForGenies)
                    {
                        return false;
                    }
                    break;
                case OccultTypes.Ghost:
                    if (!Tuning.kUsableForGhosts)
                    {
                        return false;
                    }
                    break;
                case OccultTypes.Fairy:
                    if (!Tuning.kUsableForFairies)
                    {
                        return false;
                    }
                    break;
            }
            return !(HasDisallowedTransformation(sim) || (sim.IsEP11Bot && !Tuning.kUsableForPlumbots) || ((sim.SimDescription.IsGhost || sim.SimDescription.DeathStyle != 0) && !Tuning.kUsableForGhosts) || sim.IsPet);
        }

        static void OnPreLoad()
        {
            Backpack.UseBackpack.Singleton = new UseBackpack.DefinitionModified();
        }
    }
}