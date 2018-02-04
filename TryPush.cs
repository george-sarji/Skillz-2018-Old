using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot 
{
    class TryPush : InitializationBot
    {
        // var game = InitializationBot.game;
        // var EnemyPirates = game.GetEnemyLivingPirates();
        // var MyPirates = game.GetMyLivingPirates();
        // var Asteroids = game.GetLivingAsteroids();
        // public List<Pirate> PushAsteroid()
        // {
        //     foreach (var Enemy in EnemyPirates)
        //     {
        //         foreach (var Piratre in MyPirates)
        //         {
        //             foreach (var Asteroid in Asteroid)
        //             {
        //                 var EnemyNextLocation = Enemy.Location.Towards(Asteroid.Location+Asteroid.Direction,Pirate.PushDistance);
        //                 if(Pirate.CanPush(Enemy) && EnemyNextLocation.InRange(Asteroid.Location+Asteroid.Direction,Asteroid.Size))
        //                 {
        //                     Pirate.Push(Enemy,EnemyNextLocation);
        //                 }
        //             }
        //         }
        //     }
        // }
        // Test

        public static int NumOfPushesAvailable(Pirate enemy)
        {
            return myPirates.Where(p => p.CanPush(enemy)).Count();
        }

        public static bool TryPushEnemyCapsule(Pirate pirate, Pirate capsuleHolder)
        {
            if(pirate.CanPush(capsuleHolder))
            {
                // Check how much other pirates can push it.
                var numOfPushers = NumOfPushesAvailable(capsuleHolder);
                // Check if we can either make the pirate lose his capsule or get pushed outside the border.
                var pushesToBorder = capsuleHolder.Distance(GetClosestToBorder(capsuleHolder.Location))/pirate.PushDistance;
                if(numOfPushers>=pushesToBorder || numOfPushers>=capsuleHolder.NumPushesForCapsuleLoss)
                {
                    // Push the pirate towards the border!
                    pirate.Push(capsuleHolder,GetClosestToBorder(capsuleHolder.Location));

                    return true;
                }
            }
            return false;
        }

        public static bool TryPushAsteroid(Pirate pirate, Asteroid asteroid)
        {
            if(pirate.CanPush(asteroid))
            {
                var closestEnemy = enemyPirates.OrderBy(enemy => enemy.Distance(pirate)).FirstOrDefault();
                if(closestEnemy!=null)
                {
                    // Push the asteroid towards it.
                    pirate.Push(asteroid, closestEnemy);
                    ("Pirate "+ pirate.ToString() + " pushes asteroid "+ asteroid.ToString() + " towards "+ closestEnemy.ToString()).Print();
                    return true;
                }
            }
            return false;
        }
        
        public static bool TryPushMyCapsuleHolder(Pirate pirate, Pirate capsuleHolder)
        {
            // If an ally pirate is near an ally capsule holder, he will push him towards the best mothership only if he is far enough from the enemy capsule holder to have his push ability
            // restored by the time he gets to the enemy capsule holder.
            // **** I wanted to check if the ally pirate is heading towards an enemy capsule holder, but I was not sure how to do it. 
            var usedPirates = new List<Pirate>();
            if(pirate.CanPush(capsuleHolder))
            {
                if(ClosestEnemyCapsuleHolder(pirate) != null && ClosestEnemyCapsuleHolder().Distance(pirate)/Pirate.MaxSpeed >= Pirate.PushReloadTurns)
                {
                    var bestMothership = myMotherships.OrderBy(mothership => mothership.Distance(capsuleHolder) / mothership.ValueMultiplier).FirstOrDefault();
                    pirate.Push(capsuleHolder, bestMothership);
                    ("Pirate "+ pirate.ToString() + " pushes Ally Capsule Holder "+ capsuleHolder.ToString() + " towards "+ bestMothership.ToString()).Print();
                    return true;
                }
            }
            return false
        }
        public static Pirate ClosestPirateToEnemyCapsuleHolder(Capsule capsule){
            return game.GetMyLivingPirates().OrderBy(pirate => pirate.Distance(capsule)).FirstOrDefault();
        }
        public static Pirate ClosestEnemyCapsuleHolder(Pirate pirate){
            return game.GetEnemyCapsules().Where(capsule => !capsule.Alive).OrderBy(capsule => capsule.Distance(pirate)).GetFirstOrDefault();
        }
         public static bool IsSafeToPushAsteroid(Location pirateLoc,Location asteroidLoc,Location destination, Asteroid asteroid)
        {
            //checks if the asteroid is going to kill the pirate pushing it.
            int Xa = pirateLoc.Col, Ya = pirateLoc.Row,
            Xb = asteroidLoc.Col, Yb = asteroidLoc.Row,
            Xc = destination.Col, Yc = destination.Row;
            double numerator = (Xa + ((Xc-Xb)*Ya/(Yb-Yc)) + (Yc*(Xb-Xc)/(Yb-Yc)) - Xc);
            double denominator = (((Xc-Xb)/(Yb-Yc)/1.0).Power(2) + 1).Sqrt();
            int distance = (int)(numerator/denominator);
            if(asteroid.Size<distance &&  destination.Distance(pirateLoc) > asteroid.Size)
                return true;
            return false;
        }

        // public static bool IsSafeToPushAsteroid(Location pirateLoc,Location asteroidLoc,Location destination, Asteroid asteroid)
        // {
            //checks if the asteroid is going to kill the pirate pushing it.
            // int Xa = pirateLoc.Col, Ya = pirateLoc.Row,
            // Xb = asteroidLoc.Col, Yb = asteroidLoc.Row,
            // Xc = destination.Col, Yc = destination.Row;
            // double numerator = (Xa + ((Xc-Xb)*Ya/(Yb-Yc)) + (Yc*(Xb-Xc)/(Yb-Yc)) - Xc);
            // double denominator = (((Xc-Xb)/(Yb-Yc)/1).Power(2) + 1).Sqrt();
            // int distance = (int)(numerator/denominator);
            // if(asteroid.Size<distance)
            //     return true;
            // return false;
        // }
    }
}