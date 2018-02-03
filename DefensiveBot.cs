using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot 
{
    class DefensiveBot : InitializationBot
    {
        public void PerformBunker()
        {
            var pirates = myPirates;

            // Get the amount of pirates needed for the bunker
            foreach(var mothership in enemyMotherships)
            {
                // Get the closest point to the border from the mothership
                var closestToBorder = GetClosestToBorder(mothership.Location);
                // Get how much pushes it needs to get to the border + 1
                var amountOfPushes = mothership.Distance(closestToBorder)/game.PushDistance;
                amountOfPushes++;
                // Check if that amount is bigger than the required for the capsule loss.
                if(amountOfPushes>=game.NumPushesForCapsuleLoss)
                {
                    
                }
            }
        } 
    }
}