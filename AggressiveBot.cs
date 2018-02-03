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
            // Go over the capsules
            foreach(var capsule in myCapsules)
            {
                // Sort the pirates per the distance
                myPirates = myPirates.OrderBy(pirate => pirate.Distance(capsule.InitialLocation)).ToList();
                // Check if the capsule was taken.
                if(capsule.Holder!=null && myPirates.Any())
                {
                    // Send the pirate to the capsule spawn
                    var pirateSailer = myPirates.First();
                    pirateSailer.Sail(capsule.InitialLocation);
                    string message = "Pirate "+ pirateSailer.ToString() + " sails towards "+ capsule.InitialLocation;
                    message.Print();
                    myPirates = myPirates.Where(pirate => !pirate.Equals(pirateSailer)).ToList();
                    // Get the pirate that has the capsule
                    var capsuleHolder = capsule.Holder;
                    // Send him to the closest city orderd by distance and value
                    var bestMothership = myMotherships.OrderBy(mothership => mothership.Distance(capsuleHolder)).OrderBy(mothership => mothership.ValueMultiplier).FirstOrDefault();
                    if(bestMothership!=null)
                    {
                        // Sail towards the city.
                        capsuleHolder.Sail(bestMothership);
                        message = "Pirate " + capsuleHolder.ToString() + " sails towards " + bestMothership.ToString();
                        message.Print(); 
                         myPirates = myPirates.Where(pirate => !pirate.Equals(capsuleHolder)).ToList();
                    }
                }
                else if(myPirates.Any())
                {
                    // Send the closest pirate to capture the capsule.
                    var sailingPirate = myPirates.First();
                    sailingPirate.Sail(capsule.InitialLocation);
                    string message = "Pirate "+ sailingPirate.ToString() + " sails towards "+ capsule.InitialLocation;
                    message.Print();
                     myPirates = myPirates.Where(pirate => !pirate.Equals(sailingPirate)).ToList();
                }
            }
        }
    }

}