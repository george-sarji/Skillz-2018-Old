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
                        // Take the first number of pirates and send them.
                        foreach(var pirate in useablePirates.Take(game.NumPushesForCapsuleLoss))
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
            foreach(var capsule in game.GetEnemyCapsules().Where(cap => cap.Holder!=null))
            {
                // Get the closest mothership.
                var closestMothership = enemyMotherships.OrderBy(mothership => mothership.Distance(capsule)).FirstOrDefault();
                if(closestMothership!=null)
                {
                    capsuleMothership.Add(capsule, closestMothership);
                    var rangeNeeded = capsuleMothership.Values.Where(val => val.Equals(closestMothership)).Count();
                    // Get the amount of pushes towards the border.
                    var closestToBorder = GetClosestToBorder(closestMothership.Location);
                    var amountOfPushes = closestMothership.Distance(closestToBorder)/(game.PushDistance+1);
                    var pushesTillLoss = capsule.Holder.NumPushesForCapsuleLoss;
                    int requiredPirates;
                    if(amountOfPushes>=pushesTillLoss)
                        requiredPirates = pushesTillLoss;
                    else
                        requiredPirates = amountOfPushes;
                    // Get the pirates that we can use.
                    requiredPirates=pushesTillLoss+1;
                    var useablePirates = myPirates.OrderBy(p => p.Distance(closestMothership)).OrderBy(p => p.Distance(capsule)).Where(p=> p.Steps(closestMothership)>p.PushReloadTurns);
                    if(useablePirates.Count()>=requiredPirates)
                    {
                        var closestWormhole = game.GetAllWormholes().Where(wormhole => wormhole.InRange(closestMothership, closestMothership.UnloadRange*2)).OrderBy(wormhole => wormhole.Partner.Distance(capsule.Holder)).FirstOrDefault();
                        var usedPirates = new List<Pirate>();
                        ("Wormhole: "+closestWormhole).Print();
                        foreach(var pirate in useablePirates.Take(requiredPirates))
                        {
                            var bestWormhole = GameExtension.GetBestWormhole(closestMothership.Location, capsule.Holder);
                            if(closestWormhole!=null)
                            {
                                var closestToCapsule = closestWormhole.Partner;
                                ("Closest wormhole to capsule: "+closestToCapsule).Print();
                                ("Closest wormhole to mothership: "+closestWormhole).Print();
                                if(!TryPush.TryPushWormhole(pirate, closestWormhole))
                                {
                                    AssignDestination(pirate, closestWormhole.Location.Towards(pirate, closestWormhole.WormholeRange));
                                }
                                usedPirates.Add(pirate);
                                continue;
                            }
                            else if(!TryPush.TryPushEnemyCapsule(pirate, capsule.Holder))
                                AssignDestination(pirate, closestMothership.Location.Towards(capsule, pirate.PushRange*(int)((double)rangeNeeded).Power(2)));
                            usedPirates.Add(pirate);
                        }
                        myPirates = myPirates.Except(usedPirates).ToList();
                    }
                }
            }
        }
    }
}