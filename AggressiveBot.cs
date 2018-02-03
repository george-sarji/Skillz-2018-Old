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
            foreach(var capsule in myCapsules)
            {
                var usedPirates = new List<Pirate>();
                // Sort the pirates per the distance
                myPirates = myPirates.OrderBy(pirate => pirate.Distance(capsule.InitialLocation)).ToList();
                // Check if the capsule was taken.
                if(capsule.Holder!=null && myPirates.Any())
                {
                    // Send the pirate to the capsule spawn
                    var pirateSailer = myPirates.FirstOrDefault(pirate => !pirate.HasCapsule());
                    if(pirateSailer!=null)
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
                else if(myPirates.Any())
                {
                    // Send the closest pirate to capture the capsule.
                    var sailingPirate = myPirates.First();
                    if(pirateDestinations.ContainsKey(sailingPirate))
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
            foreach(Capsule capsule in game.GetMyCapsules())
            {
                if(pirateDestinations[capsule.Holder]==capsule.Holder.Location)
                {
                    var pirate=myPirates.OrderBy(available => available.Distance(capsule.Holder)).FirstOrDefault();
                    pirateDestinations[pirate]=SmartSailing.SmartSail(pirate,capsule.Holder);
                }
            }
        }

        public static void PushAsteroidsNearby()
        {
            var usedPirates = new List<Pirate>();
            foreach(var pirate in myPirates)
            {
                // Get the asteroids that are near our pirates.
                var asteroidsOrdered = game.GetLivingAsteroids().Where(asteroid => asteroid.Location.Add(asteroid.Direction).InRange(pirate, pirate.PushRange*2));//change game to pirate if they delete the game.pushRange in the future we are ready(Mahmoud)
                if(asteroidsOrdered.Any())
                {
                    // There is an asteroid near us. Push it.
                    if(TryPush.TryPushAsteroid(pirate, asteroidsOrdered.FirstOrDefault()))
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
            foreach(var capsuleHolder in myPirates.Where(p => p.HasCapsule()))  
            {
                var bestMothership = myMotherships.OrderBy(mothership => mothership.Distance(capsuleHolder) / mothership.ValueMultiplier).FirstOrDefault();
                if(bestMothership!=null)
                {
                    // Sail towards the city.
                    // pirateDestinations.Add(capsuleHolder, SmartSailing.SmartSail(capsuleHolder,bestMothership));
                    if(pirateDestinations.ContainsKey(capsuleHolder))
                        pirateDestinations[capsuleHolder] = bestMothership.Location;
                    else
                        pirateDestinations.Add(capsuleHolder, bestMothership.Location);
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

        public static void PushAsteroids()
        {
            var usedPirates = new List<Pirate>();
            foreach(var asteroid in game.GetLivingAsteroids())
            {
                // Get the closest pirate that can push. (push turns > steps)
                var asteroidDestination = asteroid.Location.Add(asteroid.Direction);
                var closestAvailablePirate = myPirates.OrderBy(p => p.Distance(asteroidDestination)).Where(p => p.Steps(asteroidDestination)>=p.PushReloadTurns);
                if(closestAvailablePirate.FirstOrDefault()!=null)
                {
                    var pirate = closestAvailablePirate.FirstOrDefault();
                    // Check if the pirate can push it already. If not, sail towards the destination where it is in range.
                    if(!TryPush.TryPushAsteroid(pirate,  asteroid))
                    {
                        // Sail towards the asteroid.
                        if(!pirateDestinations.ContainsKey(pirate))
                        {
                            pirateDestinations.Add(closestAvailablePirate.First(), asteroidDestination.Towards(pirate, pirate.PushRange));
                            myPirates = myPirates.Where(p => !p.Equals(pirate)).ToList();
                        }
                    }
                }
            }
        }
    }
}