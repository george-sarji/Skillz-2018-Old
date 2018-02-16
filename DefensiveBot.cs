using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot 
{
    class DefensiveBot : InitializationBot
    {
        public static void BuildDefensiveBunker()
        {
            var capsuleMothership = new Dictionary<Capsule, Mothership>();
            foreach(var closestMothership in enemyMotherships)
            {
                foreach(var capsule in enemyCapsules.Where(cap => cap.Holder!=null).Where(cap => GameExtension.GetBestMothershipThroughWormholes(cap.Holder).Equals(closestMothership)).OrderBy(cap => cap.Holder.Steps(closestMothership)))
                {
                    // Get the amount of pushes towards the border.
                    var closestToBorder = GetClosestToBorder(closestMothership.Location);
                    var requiredPirates=capsule.Holder.NumPushesForCapsuleLoss+1;
                    var closestWormholeToCapsule = game.GetAllWormholes().Where(wormhole => wormhole.InRange(capsule, wormhole.WormholeRange*2)).OrderBy(wormhole => wormhole.Distance(capsule)).FirstOrDefault();
                    int rangeNeeded=0;
                    var closestWormholeToMothership = game.GetAllWormholes().Where(wormhole => wormhole.InRange(closestMothership, wormhole.WormholeRange*2)).OrderBy(wormhole => wormhole.Distance(closestMothership)).FirstOrDefault();
                    Mothership closestMothershipThroughWormhole = null;
                    if(closestWormholeToCapsule!=null)
                    {
                        closestMothershipThroughWormhole = enemyMotherships.OrderBy(mothership => GameExtension.DistanceThroughWormhole(capsule.Location, mothership.Location, closestWormholeToCapsule, game.GetAllWormholes())).FirstOrDefault();
                        if(closestMothershipThroughWormhole!=null)
                        {
                            capsuleMothership.Add(capsule, closestMothership);
                            rangeNeeded = capsuleMothership.Count(map => map.Value.Equals(closestMothership)).Power(2)*game.PushRange;
                        }
                    }
                    else
                    {
                        capsuleMothership.Add(capsule, closestMothership);
                        rangeNeeded = capsuleMothership.Count(map => map.Value.Equals(closestMothership)).Power(2)*game.PushRange;
                    }
                    
                    var useablePirates = myPirates.OrderBy(p => p.Distance(closestMothership)).Where(p=> p.Steps(closestMothership)>p.PushReloadTurns);
                    if(useablePirates.Count()>=requiredPirates)
                    {
                        var usedPirates = new List<Pirate>();
                        foreach(var pirate in useablePirates.Take(requiredPirates))
                        {
                            if(closestMothershipThroughWormhole!=null)
                            {
                                // Attempt pushing the closest wormhole.
                                var closestWormhole = game.GetAllWormholes().Where(wormhole => wormhole.InRange(closestMothershipThroughWormhole, wormhole.WormholeRange*2)).FirstOrDefault();
                                if(closestWormhole!=null)
                                {
                                    if(!TryPush.TryPushWormhole(pirate, closestWormhole))
                                        AssignDestination(pirate, closestWormhole.Location.Towards(closestMothershipThroughWormhole, closestWormhole.WormholeRange));
                                    usedPirates.Add(pirate);
                                    continue;
                                }
                                else
                                {
                                    if(!TryPush.TryPushEnemyCapsule(pirate, capsule.Holder))
                                        AssignDestination(pirate, closestMothership.Location.Towards(capsule, rangeNeeded));
                                }
                            }
                            else if(!TryPush.TryPushEnemyCapsule(pirate, capsule.Holder))
                                AssignDestination(pirate, closestMothership.Location.Towards(capsule, rangeNeeded));                            
                            usedPirates.Add(pirate);
                        }
                        myPirates = myPirates.Except(usedPirates).ToList();
                    }
                }
            }
        }

        public static void BuildBunkerForDefence()
        {
            foreach(var mothership in enemyMotherships)
            {
                foreach(var capsule in enemyCapsules.Where(cap => cap.Holder!=null).Where(cap => GameExtension.GetBestMothershipThroughWormholes(cap.Holder).Equals(mothership)).OrderBy(cap => cap.Holder.Steps(mothership)))
                {
                    ("Entered bunker defence").Print();
                    var useablePirates = myPirates.Where(pirate => pirate.Steps(mothership)>=pirate.PushReloadTurns).ToList();
                    var closestToBorder = GetClosestToBorder(mothership.Location);
                    // Get how much pushes it needs to get to the border + 1
                    var amountOfPushes = mothership.Distance(closestToBorder)/(game.PushDistance+1);
                    var pushesTillBorder = mothership.Distance(GetClosestToBorder(mothership.Location))/(game.PushDistance+1);
                    if(pushesTillBorder==0)
                        pushesTillBorder++;
                    int requiredPirates = 0;
                    if(capsule.Holder.NumPushesForCapsuleLoss<=pushesTillBorder)
                        requiredPirates = capsule.Holder.NumPushesForCapsuleLoss;
                    else
                        requiredPirates = pushesTillBorder;
                    if(useablePirates.Count()>=requiredPirates)
                    {
                        var usedPirates = new List<Pirate>();
                        foreach(var pirate in useablePirates.OrderBy(p => p.Steps(capsule)).Take(requiredPirates))
                        {
                            var destination = mothership.Location.Towards(capsule,  (int)(mothership.UnloadRange*0.5));
                            if(!TryPush.TryPushEnemyCapsuleDefensively(pirate, capsule.Holder))
                            {
                                AssignDestination(pirate, destination);
                            }
                            // Add to the destination.
                            usedPirates.Add(pirate);
                        }
                        myPirates = myPirates.Except(usedPirates).ToList();
                    }
                }
            }
        }
    }
}