using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot
{
    class AggressiveBot : InitializationBot
    {
        public static void CaptureCapsules()
        {
            myCapsules = myCapsules.OrderBy(capsule => capsule.Value).ToList();
            // Go over the capsules
            foreach (var capsule in myCapsules)
            {
                var usedPirates = new List<Pirate>();
                // Sort the pirates per the distance
                myPirates = myPirates.OrderBy(pirate => pirate.Distance(capsule.InitialLocation)).ToList();
                // Check if the capsule was taken.
                if (capsule.Holder != null && myPirates.Any())
                {
                    // Send the pirate to the capsule spawn
                    var pirateSailer = myPirates.FirstOrDefault(pirate => !pirate.HasCapsule());
                    if (pirateSailer != null)
                    {
                        pirateDestinations.Add(pirateSailer, capsule.InitialLocation);
                        usedPirates.Add(pirateSailer);
                    }
                    // Get the pirate that has the capsule
                    var capsuleHolder = capsule.Holder;
                    // Send him to the closest city orderd by distance and value
                    // var bestMothership = myMotherships.OrderBy(mothership => mothership.Distance(capsuleHolder) / mothership.ValueMultiplier).FirstOrDefault();
                    // if(bestMothership!=null)
                    // {
                    //     // Sail towards the city.
                    //     // pirateDestinations.Add(capsuleHolder, SmartSailing.SmartSail(capsuleHolder,bestMothership));
                    //     if(pirateDestinations.ContainsKey(capsuleHolder))
                    //         pirateDestinations[capsuleHolder] = bestMothership.Location;
                    //     else
                    //         pirateDestinations.Add(capsuleHolder, bestMothership.Location);
                    //     usedPirates.Add(capsuleHolder);
                    //     // if(pirateDestinations[capsuleHolder]==capsuleHolder.Location)
                    //     // {
                    //     //     var closestPirate=usedPirates.OrderBy(pirate => pirate.Distance(capsuleHolder)).FirstOrDefault();
                    //     //     pirateDestinations[closestPirate]=capsuleHolder.Location;
                    //     //     usedPirates.Add(closestPirate);
                    //     // }
                    // }
                    myPirates = myPirates.Except(usedPirates).ToList();
                }
                else if (myPirates.Any())
                {
                    // Send the closest pirate to capture the capsule.
                    var sailingPirate = myPirates.First();
                    if (pirateDestinations.ContainsKey(sailingPirate))
                    {
                        pirateDestinations[sailingPirate] = capsule.InitialLocation;
                    }
                    else
                        pirateDestinations.Add(sailingPirate, capsule.InitialLocation);
                    myPirates = myPirates.Where(pirate => !pirate.Equals(sailingPirate)).ToList();
                }
            }
        }
        public static void GoHelpAllyWithCapsule()
        {
            foreach (Pirate pirate in game.GetMyLivingPirates().Where(pirate => pirate.HasCapsule()))
            {
                if (pirateDestinations.ContainsKey(pirate) && pirateDestinations[pirate] == pirate.Location)
                {
                    var Closesetpirate = myPirates.OrderBy(available => available.Distance(pirate)).FirstOrDefault();
                    if (Closesetpirate == null)
                        break;
                    pirateDestinations[pirate] = SmartSailing.SmartSail(Closesetpirate, pirate);
                    myPirates.Remove(pirate);
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


        public static void MoveCapsuleHolders()
        {
            var usedPirates = new List<Pirate>();
            foreach (var capsuleHolder in myPirates.Where(p => p.HasCapsule()))
            {
                var bestMothership = myMotherships.OrderBy(mothership => mothership.Distance(capsuleHolder) / mothership.ValueMultiplier).FirstOrDefault();
                if (bestMothership != null)
                {
                    // Sail towards the city.
                    // pirateDestinations.Add(capsuleHolder, SmartSailing.SmartSail(capsuleHolder,bestMothership));
                    game.Debug("Reached");
                    if (pirateDestinations.ContainsKey(capsuleHolder))
                        pirateDestinations[capsuleHolder] = SmartSailing.SmartSail(capsuleHolder, bestMothership.Location);
                    else
                        pirateDestinations.Add(capsuleHolder, SmartSailing.SmartSail(capsuleHolder, bestMothership.Location));
                    usedPirates.Add(capsuleHolder);
                    // if(pirateDestinations[capsuleHolder]==capsuleHolder.Location)
                    // {
                    //     var closestPirate=usedPirates.OrderBy(pirate => pirate.Distance(capsuleHolder)).FirstOrDefault();
                    //     pirateDestinations[closestPirate]=capsuleHolder.Location;
                    //     usedPirates.Add(closestPirate);
                    // }
                }
            }
            myPirates = myPirates.Except(usedPirates).ToList();
        }


        public static void MoveCapsuleHoldersToIntersection()
        {
            var usedPirates = new List<Pirate>();
            // Get the capsule holders
            var capsuleHolders = myPirates.Where(pirate => pirate.HasCapsule()).ToList();
            // We have the best mothership. Make sure it's not null.
            while (capsuleHolders.Count() > 1)
            {
                // Take the first pair
                var firstHolder = capsuleHolders.First();
                var secondHolder = capsuleHolders.ElementAt(1);
                // Get the best mothership for the first holder.
                var bestMothership = myMotherships.OrderBy(mothership => mothership.Distance(firstHolder) / mothership.ValueMultiplier).FirstOrDefault();
                if (bestMothership != null)
                {
                    bool first = false, second = false;
                    if (firstHolder.InRange(bestMothership, bestMothership.UnloadRange * 3) && GameExtension.NumberOfAvailableEnemyPushers(firstHolder) < firstHolder.NumPushesForCapsuleLoss)
                    {
                        if (pirateDestinations.ContainsKey(firstHolder))
                            pirateDestinations[firstHolder] = bestMothership.Location;
                        else
                            pirateDestinations.Add(firstHolder, bestMothership.Location);
                        first = true;

                    }
                    if (secondHolder.InRange(bestMothership, bestMothership.UnloadRange * 3) && GameExtension.NumberOfAvailableEnemyPushers(secondHolder) < firstHolder.NumPushesForCapsuleLoss)
                    {
                        if (pirateDestinations.ContainsKey(secondHolder))
                            pirateDestinations[secondHolder] = bestMothership.Location;
                        else
                            pirateDestinations.Add(secondHolder, bestMothership.Location);
                        second = true;

                    }
                    if (!first && !second)
                    {
                        // Get the intersection point
                        var intersectionPoint = GameExtension.Interception(firstHolder.Location, bestMothership.Location, secondHolder.Location);
                        var secondIntersection = GameExtension.Interception(secondHolder.Location, bestMothership.Location, firstHolder.Location);
                        Location bestIntersection = null;
                        if (intersectionPoint != null && secondIntersection != null)
                        {
                            // Get the closest.
                            if (bestMothership.Distance(intersectionPoint) > bestMothership.Distance(secondIntersection))
                            {
                                bestIntersection = secondIntersection;
                            }
                            else
                                bestIntersection = intersectionPoint;
                        }
                        else if (intersectionPoint != null)
                            bestIntersection = intersectionPoint;
                        else
                            bestIntersection = secondIntersection;
                        if (bestIntersection != null)
                        {
                            // Send the pirates to the intersection if they're not in each other's push range.
                            if (!firstHolder.Location.Equals(secondHolder.Location))
                            {
                                if (pirateDestinations.ContainsKey(firstHolder))
                                    pirateDestinations[firstHolder] = bestIntersection;
                                else
                                    pirateDestinations.Add(firstHolder, bestIntersection);
                                if (pirateDestinations.ContainsKey(secondHolder))
                                    pirateDestinations[secondHolder] = bestIntersection;
                                else
                                    pirateDestinations.Add(secondHolder, bestIntersection);
                            }
                            else
                            {
                                // Send them towards the mothership
                                if (pirateDestinations.ContainsKey(firstHolder))

                                    pirateDestinations[firstHolder] = SmartSailing.SmartSail(firstHolder, bestMothership);
                                else
                                    pirateDestinations.Add(firstHolder, SmartSailing.SmartSail(firstHolder, bestMothership));
                                if (pirateDestinations.ContainsKey(secondHolder))
                                    pirateDestinations[secondHolder] = SmartSailing.SmartSail(secondHolder, bestMothership);
                                else
                                    pirateDestinations.Add(secondHolder, SmartSailing.SmartSail(secondHolder, bestMothership));
                            }
                        }
                    }
                    else if (second)
                    {
                        if (pirateDestinations.ContainsKey(secondHolder))
                            pirateDestinations[secondHolder] = bestMothership.Location;
                        else
                            pirateDestinations.Add(secondHolder, bestMothership.Location);
                    }
                    else if (first)
                    {
                        if (pirateDestinations.ContainsKey(firstHolder))
                            pirateDestinations[firstHolder] = bestMothership.Location;
                        else
                            pirateDestinations.Add(firstHolder, bestMothership.Location);
                    }
                    capsuleHolders.Remove(firstHolder);
                    capsuleHolders.Remove(secondHolder);
                    myPirates.Remove(firstHolder);
                    myPirates.Remove(secondHolder);
                }
            }
            if (capsuleHolders.Count() == 1)
            {
                // Send this pirate to his own best mothership.
                var leftOutPirate = capsuleHolders.First();
                var bestMothership = myMotherships.OrderBy(mothership => mothership.Distance(leftOutPirate) / mothership.ValueMultiplier).FirstOrDefault();
                if (pirateDestinations.ContainsKey(leftOutPirate))
                    pirateDestinations[leftOutPirate] = SmartSailing.SmartSail(leftOutPirate, bestMothership.Location);
                else
                    pirateDestinations.Add(leftOutPirate, SmartSailing.SmartSail(leftOutPirate, bestMothership.Location));
                Pirate CapsuleAssister = myPirates.OrderBy(pirate => pirate.Distance(leftOutPirate)).FirstOrDefault();
                if (CapsuleAssister != null)
                {
                    if (bestMothership != null)
                    {
                        bool first = false, second = false;
                        if (leftOutPirate.InRange(bestMothership, bestMothership.UnloadRange * 3) && GameExtension.NumberOfAvailableEnemyPushers(leftOutPirate) < leftOutPirate.NumPushesForCapsuleLoss)
                        {
                            if (pirateDestinations.ContainsKey(leftOutPirate))
                                pirateDestinations[leftOutPirate] = bestMothership.Location;
                            else
                                pirateDestinations.Add(leftOutPirate, bestMothership.Location);
                            first = true;

                        }
                        if (CapsuleAssister.InRange(bestMothership, bestMothership.UnloadRange * 3) && GameExtension.NumberOfAvailableEnemyPushers(CapsuleAssister) < leftOutPirate.NumPushesForCapsuleLoss)
                        {
                            if (pirateDestinations.ContainsKey(CapsuleAssister))
                                pirateDestinations[CapsuleAssister] = bestMothership.Location;
                            else
                                pirateDestinations.Add(CapsuleAssister, bestMothership.Location);
                            second = true;

                        }
                        if (!first && !second)
                        {
                            // Get the intersection point
                            var intersectionPoint = GameExtension.Interception(leftOutPirate.Location, bestMothership.Location, CapsuleAssister.Location);
                            var secondIntersection = GameExtension.Interception(CapsuleAssister.Location, bestMothership.Location, leftOutPirate.Location);
                            Location bestIntersection = null;
                            if (intersectionPoint != null && secondIntersection != null)
                            {
                                // Get the closest.
                                if (bestMothership.Distance(intersectionPoint) > bestMothership.Distance(secondIntersection))
                                {
                                    bestIntersection = secondIntersection;
                                }
                                else
                                    bestIntersection = intersectionPoint;
                            }
                            else if (intersectionPoint != null)
                                bestIntersection = intersectionPoint;
                            else
                                bestIntersection = secondIntersection;
                            if (bestIntersection != null)
                            {
                                // Send the pirates to the intersection if they're not in each other's push range.
                                if (!leftOutPirate.Location.Equals(CapsuleAssister.Location))
                                {
                                    if (pirateDestinations.ContainsKey(leftOutPirate))
                                        pirateDestinations[leftOutPirate] = bestIntersection;
                                    else
                                        pirateDestinations.Add(leftOutPirate, bestIntersection);
                                    if (pirateDestinations.ContainsKey(CapsuleAssister))
                                        pirateDestinations[CapsuleAssister] = bestIntersection;
                                    else
                                        pirateDestinations.Add(CapsuleAssister, bestIntersection);
                                }
                                else
                                {
                                    // Send them towards the mothership
                                    if (pirateDestinations.ContainsKey(leftOutPirate))

                                        pirateDestinations[leftOutPirate] = SmartSailing.SmartSail(leftOutPirate, bestMothership);
                                    else
                                        pirateDestinations.Add(leftOutPirate, SmartSailing.SmartSail(leftOutPirate, bestMothership));
                                    if (pirateDestinations.ContainsKey(CapsuleAssister))
                                        pirateDestinations[CapsuleAssister] = SmartSailing.SmartSail(CapsuleAssister, bestMothership);
                                    else
                                        pirateDestinations.Add(CapsuleAssister, SmartSailing.SmartSail(CapsuleAssister, bestMothership));
                                }
                            }
                        }
                        else if (second)
                        {
                            if (pirateDestinations.ContainsKey(CapsuleAssister))
                                pirateDestinations[CapsuleAssister] = bestMothership.Location;
                            else
                                pirateDestinations.Add(CapsuleAssister, bestMothership.Location);
                        }
                        else if (first)
                        {
                            if (pirateDestinations.ContainsKey(leftOutPirate))
                                pirateDestinations[leftOutPirate] = bestMothership.Location;
                            else
                                pirateDestinations.Add(leftOutPirate, bestMothership.Location);
                        }
                        capsuleHolders.Remove(leftOutPirate);
                        capsuleHolders.Remove(CapsuleAssister);
                        myPirates.Remove(leftOutPirate);
                        myPirates.Remove(CapsuleAssister);
                    }
                }
                // Get the closest pirate to this one
                capsuleHolders.Remove(leftOutPirate);
                myPirates.Remove(leftOutPirate);
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

        public static bool CheckIfCapsuleCanReach(Pirate CapsuleHolder, Mothership mothership)
        {
            if (CapsuleHolder.InRange(mothership, mothership.UnloadRange * 3) && GameExtension.NumberOfAvailableEnemyPushers(CapsuleHolder) < CapsuleHolder.NumPushesForCapsuleLoss)
            {
                if (pirateDestinations.ContainsKey(CapsuleHolder))
                    pirateDestinations[CapsuleHolder] = mothership.Location;
                else
                    pirateDestinations.Add(CapsuleHolder, mothership.Location);
                return true;

            }
            return false;
        }

        public static void GroupPair(Pirate FirstPirate, Pirate SecondPirate, Mothership heading)
        {
            // Get the intersection point
            var intersectionPoint = GameExtension.Interception(FirstPirate.Location, heading.Location, SecondPirate.Location);
            var secondIntersection = GameExtension.Interception(SecondPirate.Location, heading.Location, FirstPirate.Location);
            Location bestIntersection = null;
            if (intersectionPoint != null && secondIntersection != null)
            {
                // Get the closest.
                if (heading.Distance(intersectionPoint) > heading.Distance(secondIntersection))
                {
                    bestIntersection = secondIntersection;
                }
                else
                    bestIntersection = intersectionPoint;
            }
            else if (intersectionPoint != null)
                bestIntersection = intersectionPoint;
            else
                bestIntersection = secondIntersection;
            if (bestIntersection != null)
            {
                // Send the pirates to the intersection if they're not in each other's push range.
                if (!FirstPirate.Location.Equals(SecondPirate.Location))
                {
                    if (pirateDestinations.ContainsKey(FirstPirate))
                        pirateDestinations[FirstPirate] = bestIntersection;
                    else
                        pirateDestinations.Add(FirstPirate, bestIntersection);
                    if (pirateDestinations.ContainsKey(SecondPirate))
                        pirateDestinations[SecondPirate] = bestIntersection;
                    else
                        pirateDestinations.Add(SecondPirate, bestIntersection);
                }
                else
                {
                    // Send them towards the mothership
                    if (pirateDestinations.ContainsKey(FirstPirate))

                        pirateDestinations[FirstPirate] = SmartSailing.SmartSail(FirstPirate, heading);
                    else
                        pirateDestinations.Add(FirstPirate, SmartSailing.SmartSail(FirstPirate, heading));
                    if (pirateDestinations.ContainsKey(SecondPirate))
                        pirateDestinations[SecondPirate] = SmartSailing.SmartSail(SecondPirate, heading);
                    else
                        pirateDestinations.Add(SecondPirate, SmartSailing.SmartSail(SecondPirate, heading));
                }
            }
            else if (second)
            {
                if (pirateDestinations.ContainsKey(SecondPirate))
                    pirateDestinations[SecondPirate] = heading.Location;
                else
                    pirateDestinations.Add(SecondPirate, heading.Location);
            }
            else if (first)
            {
                if (pirateDestinations.ContainsKey(FirstPirate))
                    pirateDestinations[FirstPirate] = heading.Location;
                else
                    pirateDestinations.Add(FirstPirate, heading.Location);
                myPirates.Remove(FirstPirate);
                myPirates.Remove(SecondPirate);
                myPirates.Remove(FirstPirate);
                myPirates.Remove(SecondPirate);
            }
        }
        public static void AttackEnemies()
        {
            var usedPirates = new List<Pirate>();
            foreach (var pirate in myPirates)
            {
                var orderedEnemies = enemyPirates.OrderBy(enemy => enemy.Distance(pirate)).OrderByDescending(enemy => enemy.HasCapsule());
                if (orderedEnemies.Any(enemy => enemy.Distance(pirate) / enemy.MaxSpeed > pirate.PushReloadTurns))
                {
                    var toAttack = orderedEnemies.Where(enemy => enemy.Distance(pirate) / enemy.MaxSpeed > pirate.PushReloadTurns).First();
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