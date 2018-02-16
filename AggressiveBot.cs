using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot
{
    class AggressiveBot : InitializationBot
    {
        public static void SendCapsuleCaptures()
        {
            foreach(var capsule in myCapsules)
            {
                var piratesOrdered = myPirates.Where(p => !p.HasCapsule()).OrderBy(p => p.Steps(capsule.InitialLocation)).OrderBy(p => GameExtension.ClosestDistance(p.Location, capsule.InitialLocation, game.GetAllWormholes())/p.MaxSpeed);
                // Check if we have a close pirate to the capsule.
                if(piratesOrdered.Any())
                {
                    // Send the closest pirate to the spawn.
                    var closestPirate = piratesOrdered.First();
                    var bestWormhole = GameExtension.GetBestWormhole(capsule.InitialLocation, closestPirate);
                    if(bestWormhole!=null)
                        AssignDestination(closestPirate, SmartSailing.SmartSail(closestPirate, bestWormhole));
                    else
                        AssignDestination(closestPirate, capsule.InitialLocation);
                    myPirates.Remove(closestPirate);
                }
            }
        }

        public static void PushAsteroidsNearby()
        {

            var usedPirates = new List<Pirate>();
            foreach (var pirate in myPirates)
            {
                // Get the asteroids that are near our pirates.
                var asteroidsOrdered = game.GetLivingAsteroids().OrderBy(asteroid => asteroid.Location.Add(asteroid.Direction).Distance(pirate));//change game to pirate if they delete the game.pushRange in the future we are ready(Mahmoud)
                if (asteroidsOrdered.Any())
                {
                    // There is an asteroid near us. Push it.
                    if (TryPush.TryPushAsteroidTowardsCapsule(pirate, asteroidsOrdered.FirstOrDefault()) || TryPush.TryPushAsteroid(pirate, asteroidsOrdered.FirstOrDefault()))
                    {
                        usedPirates.Add(pirate);
                    }
                }
            }
            myPirates = myPirates.Except(usedPirates).ToList();
        }


        public static void MoveToIntersection()
        {
            var usedPirates = new List<Pirate>();
            var capsuleHolders = myPirates.Where(p => p.HasCapsule()).ToList();
            while(capsuleHolders.Count()>1)
            {
                // Get the best mothership
                var first = capsuleHolders.FirstOrDefault();
                capsuleHolders.Remove(first);
                var mothership = myMotherships.OrderBy(m => m.Distance(first)/((double)m.ValueMultiplier).Sqrt()).FirstOrDefault();
                if(mothership!=null)
                {

                    // Get the best wormhole
                    var bestWormhole = GameExtension.GetBestWormhole(mothership.Location, first);
                    var closestHolder = capsuleHolders.OrderBy(p => p.Distance(mothership)).FirstOrDefault();
                    capsuleHolders.Remove(closestHolder);
                    if(bestWormhole!=null)
                    {
                        // There is a wormhole. Send them with an intersection.
                        GroupPair(first, closestHolder, bestWormhole.Location);
                        myPirates.Remove(first);
                        myPirates.Remove(closestHolder);
                    }
                    else
                    {
                        // Check if each of the pirates can reach.
                        bool firstReach = false, secondReach=false;
                        if(CheckIfCapsuleCanReach(first, mothership) && !CheckIfCapsuleCanReach(closestHolder, mothership))
                        {
                            AssignDestination(first, mothership.Location);
                            AssignDestination(closestHolder, SmartSailing.SmartSail(closestHolder, mothership));                            
                            firstReach = true;
                        }
                        else if(CheckIfCapsuleCanReach(closestHolder, mothership) && !CheckIfCapsuleCanReach(first, mothership))
                        {
                            AssignDestination(closestHolder, mothership.Location);
                            AssignDestination(first, SmartSailing.SmartSail(first, mothership)); 
                            secondReach = true;
                        }
                        else if(CheckIfCapsuleCanReach(closestHolder, mothership) && CheckIfCapsuleCanReach(first, mothership))
                        {
                            AssignDestination(closestHolder, mothership.Location);
                            AssignDestination(first, mothership.Location);
                        }
                        else if(!CheckIfCapsuleCanReach(closestHolder, mothership) && !CheckIfCapsuleCanReach(first, mothership))
                        {
                            GroupPair(first, closestHolder, mothership.Location);
                        }
                        
                        myPirates.Remove(first);
                        myPirates.Remove(closestHolder);
                    }
                }
            }
            if(capsuleHolders.Count() ==1)
            {
                // There's a lonely pirate. Pair him up for the sake of Valentine.
                var lonelyPirate = capsuleHolders.FirstOrDefault();
                var mothership = myMotherships.OrderBy(m => m.Distance(lonelyPirate)/((double)m.ValueMultiplier).Sqrt()).FirstOrDefault();
                if(mothership!=null)
                {
                    var closestPirate = myPirates.OrderBy(p => p.Distance(lonelyPirate)).FirstOrDefault();
                    var bestWormhole = GameExtension.GetBestWormhole(mothership.Location, lonelyPirate);
                    if(closestPirate!=null)
                    {
                        if(bestWormhole!=null)
                        {
                            GroupPair(lonelyPirate, closestPirate, bestWormhole.Location);
                        }
                        else if(CheckIfCapsuleCanReach(lonelyPirate, mothership))
                        {
                            AssignDestination(lonelyPirate, mothership.Location);
                            capsuleHolders.Remove(lonelyPirate);
                            myPirates.Remove(lonelyPirate);
                        }
                        else
                        {
                            GroupPair(lonelyPirate, closestPirate, mothership.Location);
                        }
                        myPirates.Remove(lonelyPirate);
                        myPirates.Remove(closestPirate);
                    }
                    else
                        AssignDestination(lonelyPirate, SmartSailing.SmartSail(lonelyPirate, bestWormhole.Location));
                }
                
            }
        }

        public static void PushAsteroids()
        {
            var usedPirates = new List<Pirate>();
            foreach (var asteroid in game.GetLivingAsteroids())
            {
                // Get the closest pirate that can push. (push turns > steps)
                var asteroidDestination = asteroid.Location.Add(asteroid.Direction);
                var closestAvailablePirate = myPirates.OrderBy(p => p.Distance(asteroidDestination)).Where(p => p.Steps(asteroidDestination) >= p.PushReloadTurns);
                if (closestAvailablePirate.FirstOrDefault() != null)
                {
                    var pirate = closestAvailablePirate.FirstOrDefault();
                    // Check if the pirate can push it already. If not, sail towards the destination where it is in range.
                    if (!TryPush.TryPushAsteroidTowardsCapsule(pirate, asteroid) && !TryPush.TryPushAsteroid(pirate, asteroid))
                    {
                        // Sail towards the asteroid.
                        if (!pirateDestinations.ContainsKey(pirate))
                        {
                            pirateDestinations.Add(closestAvailablePirate.First(), asteroidDestination.Towards(pirate, pirate.PushRange));
                            myPirates = myPirates.Where(p => !p.Equals(pirate)).ToList();
                        }
                    }
                    else
                    {
                        myPirates.Remove(pirate);
                    }
                }
            }
        }

        public static bool CheckIfCapsuleCanReach(Pirate CapsuleHolder, Mothership mothership)//Working on this Function -Mahmoud
        {
            if(mothership == null) return false;
            if (CapsuleHolder.InRange(mothership, mothership.UnloadRange * 3) && GameExtension.NumberOfAvailableEnemyPushers(CapsuleHolder) < CapsuleHolder.NumPushesForCapsuleLoss)
            {
                AssignDestination(CapsuleHolder, mothership.Location);
                myPirates.Remove(CapsuleHolder);
                return true;

            }
            return false;
        }

        public static void GroupPair(Pirate first, Pirate second, Location destination)
        {
            var firstIntersection = GameExtension.Interception(first.Location,destination, second.Location);
            var secondIntersection = GameExtension.Interception(second.Location,destination, first.Location);
            Location bestIntersection = null;
            if(firstIntersection!=null && secondIntersection!=null)
            {
                // Get the closest to the destination.
                if(firstIntersection.Distance(destination)>secondIntersection.Distance(destination))
                {
                    bestIntersection = secondIntersection;
                }
                else
                    bestIntersection = firstIntersection;
            }
            else if(firstIntersection!=null)
            {
                bestIntersection = firstIntersection;
            }
            else if(secondIntersection!=null)
                bestIntersection = secondIntersection;
            else
                bestIntersection = GameExtension.MidPoint(first, second);
            // Move the pirates now.
            var capsuleHolders = game.GetMyLivingPirates().Where(p => p.HasCapsule());
            var mothershipInLocation = myMotherships.Where(mothership => mothership.Location.Equals(destination)).FirstOrDefault();
            if(first.Distance(second)< destination.Distance(first) && !first.InPushRange(second))
            {
                var FirstDestination=bestIntersection;
                if(first.HasCapsule())
                    FirstDestination=SmartSailing.SmartSail(first,bestIntersection);
                var SecondDestination=bestIntersection;
                if(second.HasCapsule())
                    SecondDestination=SmartSailing.SmartSail(second,bestIntersection);
                AssignDestination(first, FirstDestination);
                AssignDestination(second, SecondDestination);
            }
            else
            {
                var FirstDestination=destination;
                if(first.HasCapsule())
                    FirstDestination=SmartSailing.SmartSail(first,destination);
                var SecondDestination=destination;
                if(second.HasCapsule())
                    SecondDestination=SmartSailing.SmartSail(second,destination);
                AssignDestination(first, FirstDestination);
                AssignDestination(second, SecondDestination);
            }
            myPirates.Remove(first);
            myPirates.Remove(second);
        }
        public static void AttackEnemies()
        {
            var usedPirates = new List<Pirate>();
            foreach (var pirate in myPirates)
            {
                var orderedEnemies = enemyPirates.OrderBy(enemy => enemy.Distance(pirate)).OrderByDescending(enemy => enemy.HasCapsule());
                if (orderedEnemies.Any(enemy => enemy.Distance(pirate) / enemy.MaxSpeed >= pirate.PushReloadTurns))
                {
                    var toAttack = orderedEnemies.Where(enemy => enemy.Distance(pirate) / enemy.MaxSpeed >= pirate.PushReloadTurns).First();
                    if (!TryPush.TryPushEnemy(pirate, toAttack))
                    {
                        // Sail towards that pirate.
                        pirateDestinations.Add(pirate, toAttack.Location);
                        usedPirates.Add(pirate);
                    }
                }
                else if (orderedEnemies.Any())
                {
                    // Attack first pirate.
                    if (!TryPush.TryPushEnemy(pirate, orderedEnemies.First()))
                    {
                        // Sail towards that pirate.
                        pirateDestinations.Add(pirate, orderedEnemies.FirstOrDefault().Location);
                        usedPirates.Add(pirate);
                    }
                }
            }
            myPirates = myPirates.Except(usedPirates).ToList();
        }
    }
}