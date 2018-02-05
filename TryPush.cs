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
            if(pirate.CanPush(asteroid) && !asteroids[asteroid])
            {
                var closestEnemy = enemyPirates.OrderBy(enemy => enemy.Distance(pirate)).OrderBy(enemy => GetGroupingNumber(enemy)).FirstOrDefault();
                if(closestEnemy!=null)
                {
                    // Push the asteroid towards it.
                    pirate.Push(asteroid, closestEnemy);
                    asteroids[asteroid]=true;
                    ("Pirate "+ pirate.ToString() + " pushes asteroid "+ asteroid.ToString() + " towards "+ closestEnemy.ToString()).Print();
                    return true;
                }
            }
            return false;
        }
        

        public static bool TryPushEnemy(Pirate pirate, Pirate enemy)
        {
            if(pirate.CanPush(enemy))
            {
                pirate.Push(enemy, GetClosestToBorder(enemy.Location));
                ("Pirate "+pirate.ToString() + " pushes enemy "+ enemy.ToString() + " towards "+ GetClosestToBorder(enemy.Location)).Print();
                return true;
            }
            return false;
        }


        public static int GetGroupingNumber(Pirate pirate )
        {
            return game.GetEnemyLivingPirates().Where(p => p.InRange(pirate, game.PushRange*2)).Count();
        }
    }
}