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
                var mothership = game.GetMyMotherships().OrderBy(m => m.Distance(first) / ((double) m.ValueMultiplier).Sqrt()).FirstOrDefault();
                if (mothership != null)
                {

                    // Get the best wormhole
                    var bestWormhole = GetBestWormhole(mothership.Location, first);
                    var closestHolder = capsuleHolders.OrderBy(p => p.Distance(mothership)).FirstOrDefault();
                    capsuleHolders.Remove(closestHolder);
                    if (bestWormhole != null)
                    {
                        // There is a wormhole. Send them with an intersection.
                        GroupPair(first, closestHolder, bestWormhole.Location);
                        myPirates.Remove(first);
                        myPirates.Remove(closestHolder);
                    }
                    else
                    {
                        // Check if each of the pirates can reach.
                        if (CheckIfCapsuleCanReach(first, mothership) && !CheckIfCapsuleCanReach(closestHolder, mothership))
                        {
                            AssignDestination(first, mothership.Location);
                            AssignDestination(closestHolder, SmartSail(closestHolder, mothership));
                        }
                        else if (CheckIfCapsuleCanReach(closestHolder, mothership) && !CheckIfCapsuleCanReach(first, mothership))
                        {
                            AssignDestination(closestHolder, mothership.Location);
                            AssignDestination(first, SmartSail(first, mothership));
                        }
                        else if (CheckIfCapsuleCanReach(closestHolder, mothership) && CheckIfCapsuleCanReach(first, mothership))
                        {
                            AssignDestination(closestHolder, mothership.Location);
                            AssignDestination(first, mothership.Location);
                        }
                        else if (!CheckIfCapsuleCanReach(closestHolder, mothership) && !CheckIfCapsuleCanReach(first, mothership))
                        {
                            GroupPair(first, closestHolder, mothership.Location);
                        }

                        myPirates.Remove(first);
                        myPirates.Remove(closestHolder);
                    }
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
                            GroupPair(lonelyPirate, closestPirate, bestWormhole.Location);
                        }
                        else if (!CheckIfCapsuleCanReach(lonelyPirate, mothership))
                        {
                            GroupPair(lonelyPirate, closestPirate, mothership.Location);
                        }
                        myPirates.Remove(lonelyPirate);
                        myPirates.Remove(closestPirate);
                    }
                    else
                        AssignDestination(lonelyPirate, SmartSail(lonelyPirate, bestWormhole.Location));
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

        public void GroupPair(Pirate first, Pirate second, Location destination)
        {
            var firstIntersection = Interception(first.Location, destination, second.Location);
            var secondIntersection = Interception(second.Location, destination, first.Location);
            Location bestIntersection = null;
            if (firstIntersection != null && secondIntersection != null)
            {
                // Get the closest to the destination.
                if (firstIntersection.Distance(destination) > secondIntersection.Distance(destination))
                {
                    bestIntersection = secondIntersection;
                }
                else
                    bestIntersection = firstIntersection;
            }
            else if (firstIntersection != null)
            {
                bestIntersection = firstIntersection;
            }
            else if (secondIntersection != null)
                bestIntersection = secondIntersection;
            else
                bestIntersection = MidPoint(first, second);
            // Move the pirates now.
            var capsuleHolders = game.GetMyLivingPirates().Where(p => p.HasCapsule());
            var mothershipInLocation = game.GetMyMotherships().Where(mothership => mothership.Location.Equals(destination)).FirstOrDefault();
            if (first.Distance(second) < destination.Distance(first) && !first.InPushRange(second))
            {
                var FirstDestination = bestIntersection;
                if (first.HasCapsule())
                    FirstDestination = SmartSail(first, bestIntersection);
                var SecondDestination = bestIntersection;
                if (second.HasCapsule())
                    SecondDestination = SmartSail(second, bestIntersection);
                AssignDestination(first, FirstDestination);
                AssignDestination(second, SecondDestination);
            }
            else
            {
                var FirstDestination = destination;
                if (first.HasCapsule())
                    FirstDestination = SmartSail(first, destination);
                var SecondDestination = destination;
                if (second.HasCapsule())
                    SecondDestination = SmartSail(second, destination);
                AssignDestination(first, FirstDestination);
                AssignDestination(second, SecondDestination);
            }
            myPirates.Remove(first);
            myPirates.Remove(second);
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
            foreach (var pirate in game.GetMyLivingPirates())
            {
                if (!piratePairs.ContainsKey(pirate) && !piratePairs.ContainsValue(pirate))
                {
                    // This pirate is not used.
                    // Attempt to pair the pirate with the closest pirate that is in the same state and not used.
                    var closestPirate = game.GetMyLivingPirates().Where(p => !p.Equals(pirate) && p.IsSameState(pirate) &&
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