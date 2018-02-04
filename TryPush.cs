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

        public static bool IsSafeToPushAsteroid(Location pirateLoc,Location asteroidLoc,Location destination, Asteroid asteroid)
        {
            int Xa = pirateLoc.Col, Ya = pirateLoc.Row,
            Xb = asteroidLoc.Col, Yb = asteroidLoc.Row,
            Xc = destination.Col, Yc = destination.Row;
            double numerator = (Xa + ((Xc-Xb)*Ya/(Yb-Yc)) + (Yc*(Xb-Xc)/(Yb-Yc)) - Xc);
            double denominator = (((Xc-Xb)/(Yb-Yc)/1.0).Power(2) + 1).Sqrt();
            int distance = (int)(numerator/denominator);
            if(asteroid.Size<distance)
                return true;
            return false;
        }
    }
}