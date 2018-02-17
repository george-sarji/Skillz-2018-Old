using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Bot
{
    partial class SSJS12Bot : IPirateBot
    {
        public void SendCapsuleCaptures()
        {
            foreach (var capsule in game.GetMyCapsules())
            {
                var piratesOrdered = myPirates.Where(p => !p.HasCapsule()).OrderBy(p => p.Steps(capsule.InitialLocation)).OrderBy(p => ClosestDistance(p.Location, capsule.InitialLocation, game.GetAllWormholes()) / p.MaxSpeed);
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

        public void PushAsteroidsNearby()
        {

            // var usedPirates = new List<Pirate>();
            // foreach (var pirate in myPirates)
            // {
            //     // Get the asteroids that are near our pirates.
            //     var asteroidsOrdered = game.GetLivingAsteroids().OrderBy(asteroid => asteroid.Location.Add(asteroid.Direction).Distance(pirate));//change game to pirate if they delete the game.pushRange in the future we are ready(Mahmoud)
            //     if (asteroidsOrdered.Any())
            //     {
            //         // There is an asteroid near us. Push it.
            //         if (TryPushAsteroidTowardsCapsule(pirate, asteroidsOrdered.FirstOrDefault()) || TryPushAsteroid(pirate, asteroidsOrdered.FirstOrDefault()))
            //         {
            //             usedPirates.Add(pirate);
            //         }
            //     }
            // }
            // myPirates = myPirates.Except(usedPirates).ToList();
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
                        // else if(CheckIfCapsuleCanReach(lonelyPirate, mothership))
                        // {
                        //     AssignDestination(lonelyPirate, mothership.Location);
                        //     capsuleHolders.Remove(lonelyPirate);
                        //     myPirates.Remove(lonelyPirate);
                        // }
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
            // var usedPirates = new List<Pirate>();
            // foreach (var asteroid in game.GetLivingAsteroids())
            // {
            //     // Get the closest pirate that can push. (push turns > steps)
            //     var asteroidDestination = asteroid.Location.Add(asteroid.Direction);
            //     var closestAvailablePirate = myPirates.OrderBy(p => p.Steps(asteroidDestination)).Where(p => p.Steps(asteroidDestination) >= p.PushReloadTurns);
            //     if (closestAvailablePirate.FirstOrDefault() != null)
            //     {
            //         var pirate = closestAvailablePirate.FirstOrDefault();
            //         // Check if the pirate can push it already. If not, sail towards the destination where it is in range.
            //         if (!TryPushAsteroidTowardsCapsule(pirate, asteroid) && !TryPushAsteroid(pirate, asteroid))
            //         {
            //             // Sail towards the asteroid.
            //             if (!pirateDestinations.ContainsKey(pirate))
            //             {
            //                 pirateDestinations.Add(closestAvailablePirate.First(), asteroidDestination.Towards(pirate, pirate.PushRange));
            //                 myPirates = myPirates.Where(p => !p.Equals(pirate)).ToList();
            //             }
            //         }
            //         else
            //         {
            //             myPirates.Remove(pirate);
            //         }
            //     }
            // }

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
            var usedPirates = new List<Pirate>();
            // foreach (var pirate in myPirates)
            // {
            //     var orderedEnemies = game.GetEnemyLivingPirates().OrderBy(enemy => enemy.Distance(pirate)).OrderByDescending(enemy => enemy.HasCapsule());
            //     if (orderedEnemies.Any(enemy => enemy.Distance(pirate) / enemy.MaxSpeed >= pirate.PushReloadTurns))
            //     {
            //         var toAttack = orderedEnemies.Where(enemy => enemy.Distance(pirate) / enemy.MaxSpeed >= pirate.PushReloadTurns).First();
            //         if (!TryPushEnemy(pirate, toAttack))
            //         {
            //             // Sail towards that pirate.
            //             pirateDestinations.Add(pirate, toAttack.Location);
            //             usedPirates.Add(pirate);
            //         }
            //     }
            //     else if (orderedEnemies.Any())
            //     {
            //         // Attack first pirate.
            //         if (!TryPushEnemy(pirate, orderedEnemies.First()))
            //         {
            //             // Sail towards that pirate.
            //             pirateDestinations.Add(pirate, orderedEnemies.FirstOrDefault().Location);
            //             usedPirates.Add(pirate);
            //         }
            //     }
            // }
            // myPirates = myPirates.Except(usedPirates).ToList();

            // foreach(var pirate in myPirates)
            // {
            //     var closestEnemy = game.GetEnemyLivingPirates().Where(enemy => pirate.Steps(enemy)>=pirate.PushReloadTurns).FirstOrDefault();
            //     if(closestEnemy!=null)
            //     {
            //         if(!TryPushEnemy(pirate, closestEnemy))
            //             AssignDestination(pirate, closestEnemy.Location);
            //         usedPirates.Add(pirate);
            //     }
            // }
            // myPirates = myPirates.Except(usedPirates).ToList();

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
    }
}