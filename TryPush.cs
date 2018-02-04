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
                    ("Pirate "+ pirate.ToString() + " pushes Enemy Capsule Holder "+ enemy.ToString() + " towards border").Print();
                    usedPirates.Add(pirate);
                    return true;
                }
            }
            return false;
        }
        public static bool TryPushEnemyPirate(Pirate pirate, Pirate enemy)
        {
            var numOfPushers = NumOfPushesAvailable(enemy);
            var pushesToBorder = (enemy.Distance(GetClosestToBorder(enemy.Location))-pirate.MaxSpeed)/pirate.PushRange;
            //First, it tries to push the enemy out of the border
            if(numOfPushers > pushesToBorder)
            {
                pirate.Push(enemy, GetClosestToBorder(enemy.Location));
                ("Pirate "+ pirate.ToString() + " pushes Enemy Pirate "+ enemy.ToString() + " towards border").Print();
                usedPirates.Add(pirate);
                return true;
            }
            //Second, it tries to push the enemy away from his best capsule if a capsule is alive
            if(game.GetEnemyCapsules().Where(capsule => capsule.IsAlive()).Any())
            {
                var bestCapsule = game.GetEnemyCapsules().Where(capsule => capsule.IsAlive()).OrderBy(capsule => capsule.Distance(enemy)).FirstOrDefault();
                if(bestMothership != null)
                {
                    var negativeDistance = pirate.PushRange * -1;
                    pirate.Push(enemy, enemy.Location.Towards(bestMothership, negativeDistance));
                    ("Pirate "+ pirate.ToString() + " pushes Enemy Pirate "+ enemy.ToString() + " away from mothership").Print();
                    usedPirates.Add(pirate);
                    return true;
                }
            }
            //Third, it tries to push the enemy to an asteroid if it's in range of one
            var closestAsteroid = game.GetLivingAsteroids().Where(asteroid => asteroid.Distance(enemy) <= (pirate.PushRange - enemy.MaxSpeed)).FirstOrDefault();
            if(closestAsteroid != null)
            {
                pirate.Push(enemy, closestAsteroid);
                ("Pirate "+ pirate.ToString() + " pushes Enemy Pirate "+ enemy.ToString() + " towards closest asteroid").Print();
                usedPirates.Add(pirate);
                return true;
            }
            //Fourth, it checks if the enemy is heading to a capsule of ours, if so it pushes it in the other direction
            //Fifth, it ignores it
            return false;
        }
        public static bool TryPushAsteroid(Pirate pirate, Asteroid asteroid)
        {
            //Need to decide whether to send to closest enemy capsule if all else fails, or not
            var usedPirates = new List<Pirate>();
            //First priority is defending our mothership from a bunker by the enemy
            foreach(mothership is myMotherships)
            {
                if(IsBunkered(mothership))
                {
                    pirate.Push(asteroid,mothership);
                    usedPirates.Add(pirate);
                    ("Pirate "+ pirate.ToString() + " pushes Asteroid "+ asteroid.ToString() + " towards "+ mothership.ToString()).Print();
                    return true;
                }
            }
            //Second priority is killing the enemy capsule holder, we need an interception method for varying speed objects.
            //Third priority is sending it towards the enemy mothership, if we are not surrounding it.
            foreach(mothership in enemyMotherships)
            {
                if(!IsBunkeredEnemy(mothership))
                {
                    pirate.Push(asteroid, mothership);
                    usedPirates.Add(pirate);
                    ("Pirate "+ pirate.ToString() + " pushes Asteroid "+ asteroid.ToString() + " towards "+ mothership.ToString()).Print();
                    return true;
                }
            }
            return false;
        } 
        public static bool IsBunkered(Mothership mothership)
        {
            //Checks if a mothership is surrounded by enemies
            return game.GetEnemyLivingPirates().Where(pirate => pirate.Distance(mothership) <= pirate.PushRange).Count() >= game.NumPushesForCapsuleLoss();
        }
        public static bool IsBunkeredEnemy(Mothership mothership)
        {
            //Checks if we are surrounding the enemy mothership with more than 1 pirate
            return game.GetMyLivingPirates().Where(pirate => pirate.Distance(mothership) <= pirate.PushRange).Count() >= 2;
        }

        // public static bool TryPushAsteroid(Pirate pirate, Asteroid asteroid)
        // {
        //     if(pirate.CanPush(asteroid))
        //     {
        //         var closestEnemy = enemyPirates.OrderBy(enemy => enemy.Distance(pirate)).FirstOrDefault();
        //         if(closestEnemy!=null)
        //         {
        //             // Push the asteroid towards it.
        //             pirate.Push(asteroid, closestEnemy);
        //             ("Pirate "+ pirate.ToString() + " pushes asteroid "+ asteroid.ToString() + " towards "+ closestEnemy.ToString()).Print();
        //             return true;
        //         }
        //     }
        //     return false;
        // }
        
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