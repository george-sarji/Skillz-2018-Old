using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot 
{
    class DefensiveBot : InitializationBot
    {
        public static void PerformBunker()
        {
            // Get the amount of pirates needed for the bunker
            foreach(var mothership in enemyMotherships)
            {
                var usedPirates = new List<Pirate>();
                // Get the closest point to the border from the mothership
                var closestToBorder = GetClosestToBorder(mothership.Location);
                // Get how much pushes it needs to get to the border + 1
                var amountOfPushes = mothership.Distance(closestToBorder)/(game.PushDistance+1);
                amountOfPushes++;
                // Check if that amount is bigger than the required for the capsule loss.
                if(amountOfPushes>=game.NumPushesForCapsuleLoss)
                {
                    // Send only the amount of pushes for capsule loss towards the capsule.
                    myPirates = myPirates.OrderBy(pirate => pirate.Distance(mothership)).ToList();
                    var useablePirates = myPirates.Where(pirate => pirate.Steps(mothership)>=pirate.PushReloadTurns).ToList();
                    if(useablePirates.Count()>=game.NumPushesForCapsuleLoss)
                    {
                        foreach(var closestCapsule in enemyCapsules.Where(cap => cap.Holder!=null).Where(cap => GameExtension.GetBestMothershipThroughWormholes(cap.Holder).Equals(mothership)).OrderBy(cap => cap.Holder.Steps(mothership)))
                        {
                            // Take the first number of pirates and send them.
                            foreach(var pirate in useablePirates.Take(game.NumPushesForCapsuleLoss))
                            {
                                // var closestCapsule = enemyCapsules.OrderBy(cap => cap.Distance(mothership)).FirstOrDefault();
                                // Send the pirates towards the unload range * 0.5
                                var destination = mothership.Location.Towards(closestCapsule,  (int)(mothership.UnloadRange*0.8));
                                if(closestCapsule.Holder!=null && TryPush.TryPushEnemyCapsule(pirate, closestCapsule.Holder))
                                {
                                    usedPirates.Add(pirate);
                                    continue;
                                }
                                // Add to the destination.
                                else if(!pirateDestinations.ContainsKey(pirate))
                                {
                                    pirateDestinations.Add(pirate, destination);
                                    usedPirates.Add(pirate);
                                }
                            }
                        }
                    }
                }
                else
                {
                    myPirates = myPirates.OrderBy(pirate => pirate.Distance(mothership)).ToList();
                    var useablePirates = myPirates.Where(pirate => pirate.Steps(mothership)>=pirate.PushReloadTurns).ToList();
                    if(useablePirates.Count()>=amountOfPushes)
                    {
                        foreach(var pirate in useablePirates.Take(amountOfPushes))
                        {
                            var closestCapsule = enemyCapsules.OrderBy(cap => cap.Distance(mothership)).FirstOrDefault();
                            if(closestCapsule!=null)
                            {
                                // Send the pirates towards the unload range * 0.5
                                var destination = mothership.Location.Towards(closestCapsule,  (int)(mothership.UnloadRange*0.8));
                                if(closestCapsule.Holder!=null && TryPush.TryPushEnemyCapsule(pirate, closestCapsule.Holder))
                                {
                                    usedPirates.Add(pirate);
                                    continue;
                                }
                                // Add to the destination.
                                else if(!pirateDestinations.ContainsKey(pirate))
                                {
                                    pirateDestinations.Add(pirate, destination);
                                    usedPirates.Add(pirate);
                                }
                                
                            }
                        }
                    }
                }
                myPirates = myPirates.Except(usedPirates).ToList();
            }
        } 


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

        public static void BuildBunkerTest()
        {
            var capsuleMothership = new Dictionary<Capsule, Mothership>();
            foreach(var capsule in game.GetEnemyCapsules().Where(cap => cap.Holder!=null))
            {
                // Print the best mothership
                ("Best mothership: "+GameExtension.GetBestMothershipThroughWormholes(capsule.Holder).ToString()).Print();
                // Get the closest mothership.
                var closestMothership = GameExtension.GetBestMothershipThroughWormholes(capsule.Holder);
                if(closestMothership!=null)
                {
                    capsuleMothership.Add(capsule, closestMothership);
                    var rangeNeeded = capsuleMothership.Count(map => map.Value.Equals(closestMothership)).Power(2)*game.PushDistance;
                    var requiredPirates=capsule.Holder.NumPushesForCapsuleLoss+1;
                    var useablePirates = myPirates.OrderBy(p => p.Distance(closestMothership)).Where(p=> p.Steps(closestMothership)>p.PushReloadTurns);
                    var closestWormhole = GameExtension.GetBestWormhole(closestMothership.Location, capsule.Holder);
                    if(useablePirates.Count()>=requiredPirates)
                    {
                        var usedPirates = new List<Pirate>();
                        foreach(var pirate in useablePirates.Take(requiredPirates))
                        {
                            if(closestWormhole!=null && closestWormhole.Partner.InRange(closestMothership, closestWormhole.WormholeRange*2))
                            {
                                closestWormhole = closestWormhole.Partner;
                                if(!TryPush.TryPushWormhole(pirate, closestWormhole))
                                    AssignDestination(pirate, closestWormhole.Location.Towards(pirate, closestWormhole.WormholeRange));
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
    }
}