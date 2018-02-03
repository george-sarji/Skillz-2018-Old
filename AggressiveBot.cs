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
                    var bestMothership = myMotherships.OrderBy(mothership => mothership.Distance(capsuleHolder) / mothership.ValueMultiplier).FirstOrDefault();
                    if(bestMothership!=null)
                    {
                        // Sail towards the city.
                        pirateDestinations.Add(capsuleHolder, capsule.InitialLocation);
                        usedPirates.Add(pirateSailer);
                    }
                    myPirates = myPirates.Except(usedPirates).ToList();
                }
                else if(myPirates.Any())
                {
                    // Send the closest pirate to capture the capsule.
                    var sailingPirate = myPirates.First();
                    pirateDestinations.Add(sailingPirate, capsule.InitialLocation);
                    myPirates = myPirates.Where(pirate => !pirate.Equals(sailingPirate)).ToList();
                }
            }
        }


    }

}