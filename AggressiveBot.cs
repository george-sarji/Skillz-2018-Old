using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Bot
{
    partial class SSJS12Bot : IPirateBot
    {
        public void SendCapsuleCaptures()
        {
            if (myPirates.Any())
            {
                foreach (var capsule in game.GetMyCapsules().OrderBy(
                        capsule => capsule.Distance(
                            myPirates.OrderBy(p => ClosestDistance(
                                p.Location, capsule.Location,
                                game.GetAllWormholes().Where(wormhole => wormhole.TurnsToReactivate < p.Steps(capsule.InitialLocation))
                            )).FirstOrDefault()
                        )
                    ))
                {
                    var piratesOrdered = myPirates.Where(p => !p.HasCapsule()).OrderBy(p => p.Steps(capsule.InitialLocation)).OrderBy(p => ClosestDistance(p.Location, capsule.InitialLocation, game.GetAllWormholes()
                        .Where(wormhole => wormhole.TurnsToReactivate < p.Steps(capsule.InitialLocation) / 4)) / p.MaxSpeed);
                    // Check if we have a close pirate to the capsule.
                    if (piratesOrdered.Any())
                    {
                        // Send the closest pirate to the spawn.
                        var closestPirate = piratesOrdered.First();
                        var bestWormhole = GetBestWormhole(capsule.InitialLocation, closestPirate);
                        if (bestWormhole != null)
                            AssignDestination(closestPirate, SmartSail(closestPirate, bestWormhole));
                        else
                            AssignDestination(closestPirate, capsule.InitialLocation);
                        myPirates.Remove(closestPirate);
                    }
                }
            }
        }

        public void PushAsteroidsNearby()
        {
            foreach (var asteroid in game.GetLivingAsteroids())
            {
                var closestPirate = myPirates.OrderBy(p => p.Steps(asteroid)).FirstOrDefault();
                if (closestPirate != null && (TryPushAsteroidTowardsCapsule(closestPirate, asteroid) ||
                        TryPushAsteroid(closestPirate, asteroid)))
                    myPirates.Remove(closestPirate);

            }
        }

        public void MoveToIntersection()
        {
            var usedPirates = new List<Pirate>();
            var capsuleHolders = myPirates.Where(p => p.HasCapsule()).ToList();
            while (capsuleHolders.Count() > 1)
            {
                // Get the best mothership
                var first = capsuleHolders.FirstOrDefault();
                capsuleHolders.Remove(first);
                myPirates.Remove(first);
                var mothership = game.GetMyMotherships().OrderBy(m => m.Distance(first) / ((double) m.ValueMultiplier).Sqrt()).FirstOrDefault();
                if (mothership != null)
                {

                    // Get the best wormhole
                    var bestWormhole = GetBestWormhole(mothership.Location, first);
                    Pirate closestHolder = null;
                    closestHolder = myPirates.OrderBy(p => p.Steps(first)).FirstOrDefault();
                    // if (piratePairs.ContainsKey(first))
                    //     closestHolder = piratePairs[first];
                    // else if (piratePairs.Values.Contains(first))
                    //     closestHolder = piratePairs.Where(p => p.Value != null && p.Value.Equals(first)).FirstOrDefault().Key;
                    // else if (capsuleHolders.Any())
                    //     closestHolder = capsuleHolders.OrderBy(p => p.Distance(mothership)).FirstOrDefault();
                    // else if (myPirates.Any())
                    //     closestHolder = myPirates.OrderBy(p => p.Steps(first)).FirstOrDefault();
                    // var closestHolder = capsuleHolders.OrderBy(p => p.Distance(mothership)).FirstOrDefault();
                    capsuleHolders.Remove(closestHolder);
                    if (bestWormhole != null)
                    {
                        // There is a wormhole. Send them with an intersection.
                        MakePair(first, closestHolder, bestWormhole.Location);
                        myPirates.Remove(first);
                        myPirates.Remove(closestHolder);
                    }
                    else if (closestHolder != null)
                    {
                        if (!CheckIfCapsuleCanReach(first, mothership) && CheckIfCapsuleCanReach(closestHolder, mothership))
                        {
                            AssignDestination(first, SmartSail(first, mothership.Location));
                        }
                        else if (CheckIfCapsuleCanReach(first, mothership) && !CheckIfCapsuleCanReach(closestHolder, mothership))
                            AssignDestination(closestHolder, SmartSail(closestHolder, mothership.Location));
                        else
                            MakePair(first, closestHolder, mothership.Location);

                        myPirates.Remove(first);
                        myPirates.Remove(closestHolder);
                    }
                    else
                        AssignDestination(first, SmartSail(first, mothership.Location));
                }
            }
            if (capsuleHolders.Count() == 1)
            {
                // There's a lonely pirate. Pair him up for the sake of Valentine.
                var lonelyPirate = capsuleHolders.FirstOrDefault();
                var mothership = game.GetMyMotherships().OrderBy(m => m.Distance(lonelyPirate) / ((double) m.ValueMultiplier).Sqrt()).FirstOrDefault();
                if (mothership != null)
                {
                    var closestPirate = myPirates.OrderBy(p => p.Distance(lonelyPirate)).FirstOrDefault();
                    var bestWormhole = GetBestWormhole(mothership.Location, lonelyPirate);
                    if (closestPirate != null)
                    {
                        if (bestWormhole != null)
                        {
                            MakePair(lonelyPirate, closestPirate, bestWormhole.Location);
                        }
                        else if (!CheckIfCapsuleCanReach(lonelyPirate, mothership))
                        {
                            MakePair(lonelyPirate, closestPirate, mothership.Location);
                        }
                        myPirates.Remove(lonelyPirate);
                        myPirates.Remove(closestPirate);
                    }
                    else
                        AssignDestination(lonelyPirate, SmartSail(lonelyPirate, mothership.Location));
                    myPirates.Remove(lonelyPirate);
                }

            }
        }

        public void PushAsteroids()
        {
            foreach (var asteroid in game.GetLivingAsteroids())
            {
                var asteroidDestination = asteroid.Location.Add(asteroid.Direction);
                var closestPirate = myPirates.OrderBy(p => p.Steps(asteroidDestination)).Where(p => p.Steps(asteroidDestination) >= p.PushReloadTurns).FirstOrDefault();
                if (closestPirate != null)
                {
                    if (!TryPushAsteroidTowardsCapsule(closestPirate, asteroid) && !TryPushAsteroid(closestPirate, asteroid))
                    {
                        if (!pirateDestinations.ContainsKey(closestPirate))
                        {
                            AssignDestination(closestPirate, asteroidDestination.Towards(closestPirate, closestPirate.PushRange));
                            myPirates.Remove(closestPirate);
                            continue;
                        }
                    }
                    myPirates.Remove(closestPirate);
                }
            }
        }

        public bool CheckIfCapsuleCanReach(Pirate CapsuleHolder, Mothership mothership) //Working on this Function -Mahmoud
        {
            if (mothership == null) return false;
            if (CapsuleHolder.InRange(mothership, mothership.UnloadRange * 3) && NumberOfAvailableEnemyPushers(CapsuleHolder) < CapsuleHolder.NumPushesForCapsuleLoss)
            {
                AssignDestination(CapsuleHolder, mothership.Location);
                myPirates.Remove(CapsuleHolder);
                return true;
            }
            return false;
        }

        public void MakePair(Pirate first, Pirate second, Location destination)
        {
            if (second == null)
            {
                AssignDestination(first, SmartSail(first, destination));
                return;
            }
            var intersections = new List<Location>();
            intersections.Add(Interception(first.Location, destination, second.Location));
            intersections.Add(Interception(second.Location, destination, first.Location));
            var speeds = new List<int>();
            var slowestSpeed = Min(first.MaxSpeed, second.MaxSpeed);
            // intersections.Add(MidPoint(first, second));
            var bestIntersection = intersections.Where(location => location != null).OrderBy(location => location.Distance(destination)).FirstOrDefault();
            Location finalDest = null;
            if (first.InPushRange(second))
            {
                finalDest = destination;
            }
            else
            {
                finalDest = bestIntersection;
            }
            if (first.HasCapsule())
                AssignDestination(first, SmartSail(first, finalDest));
            else
                AssignDestination(first, first.Location.Towards(finalDest, slowestSpeed));
            if (second.HasCapsule())
                AssignDestination(second, SmartSail(second, finalDest));
            else
                AssignDestination(second, second.Location.Towards(finalDest, slowestSpeed));
        }

        public void AttackEnemies()
        {
            foreach (var enemy in game.GetEnemyLivingPirates().Where(p => p.HasCapsule()))
            {
                // Get the closest pirate that can push.
                var closestPirate = myPirates.Where(p => p.Steps(Interception(enemy.Location, GetBestMothershipThroughWormholes(enemy).Location, p.Location)) > p.PushReloadTurns)
                    .OrderBy(p => p.Steps(enemy)).FirstOrDefault();
                if (closestPirate != null)
                {
                    if (!TryPushInterceptedEnemyCapsule(closestPirate, enemy))
                        AssignDestination(closestPirate, Interception(enemy.Location, GetBestMothershipThroughWormholes(enemy).Location, closestPirate.Location));
                    myPirates.Remove(closestPirate);
                }
            }
        }

        public void PairMyPirates()
        {
            foreach (var pirate in myPirates)
            {
                if (!piratePairs.ContainsKey(pirate) && !piratePairs.ContainsValue(pirate))
                {
                    // This pirate is not used.
                    // Attempt to pair the pirate with the closest pirate that is in the same state and not used.
                    var closestPirate = myPirates.Where(p => !p.Equals(pirate) && p.IsSameState(pirate) &&
                            !piratePairs.ContainsKey(p) && !piratePairs.ContainsValue(p))
                        .OrderBy(p => p.Steps(pirate)).FirstOrDefault();
                    if (closestPirate != null)
                    {
                        // Pair the pirates.
                        piratePairs[pirate] = closestPirate;
                        ("Paired " + pirate.ToString() + " with " + closestPirate).Print();
                    }
                    else
                    {
                        // There is no good closest pirate. Solo pair.
                        piratePairs[pirate] = null;
                        (pirate + " is a solo pair.").Print();
                    }
                }
            }
        }
    }
}