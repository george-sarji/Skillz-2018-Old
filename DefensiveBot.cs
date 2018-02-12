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
            foreach(var capsule in game.GetEnemyCapsules().Where(cap => cap.Holder!=null))
            {
                // Get the closest mothership.
                var closestMothership = enemyMotherships.OrderBy(mothership => mothership.Distance(capsule)).FirstOrDefault();
                if(closestMothership!=null)
                {
                    // Get the amount of pushes towards the border.
                    var closestToBorder = GetClosestToBorder(closestMothership.Location);
                    var amountOfPushes = closestMothership.Distance(closestToBorder)/(game.PushDistance+1);
                    var pushesTillLoss = capsule.Holder.NumPushesForCapsuleLoss;
                    int requiredPirates;
                    if(amountOfPushes>=pushesTillLoss)
                        requiredPirates = pushesTillLoss;
                    else
                        requiredPirates = amountOfPushes;
                    requiredPirates+=2;
                    // Get the pirates that we can use.
                    var useablePirates = myPirates.OrderBy(p => p.Distance(closestMothership)).Where(p=> p.Steps(closestMothership)>p.PushReloadTurns);
                    if(useablePirates.Count()>=requiredPirates)
                    {
                        var usedPirates = new List<Pirate>();
                        foreach(var pirate in useablePirates.Take(requiredPirates))
                        {
                            if(TryPush.TryPushEnemyCapsule(pirate, capsule.Holder))
                                usedPirates.Add(pirate);
                            else
                            {
                                AssignDestination(pirate, closestMothership.Location.Towards(capsule, closestMothership.UnloadRange));
                                usedPirates.Add(pirate);
                            }
                        }
                        myPirates = myPirates.Except(usedPirates).ToList();
                    }
                }
            }
        }
    }
}