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

        public void TryPushMyCapsule()
        { // Get all my pirates with capsule
            foreach (Pirate myPirateWithCapsule in myPiratesWithCapsule)
            {
                PushAlliesToEnemy(myPirateWithCapsule);
                int count = game.GetMyLivingPirates().Where(pirate => pirate.CanPush(myPirateWithCapsule)).Count();  // Number of my living pirate who can push enemy pirates
                int numberOfPushesNeeded = myPirateWithCapsule.NumPushesForCapsuleLoss;
                int numberOfEnemiesAroundMyCapsule = game.GetEnemyLivingPirates().Where(enemy => enemy.CanPush(myPirateWithCapsule)).Count();
                foreach (Pirate mypirate in game.GetMyLivingPirates().Where(pirate => pirate.CanPush(myPirateWithCapsule)))  // We push until we drop it
                {
                    if (numberOfPushesNeeded - numberOfEnemiesAroundMyCapsule == 1 || !myPirates.Contains(mypirate))
                        break;
                    mypirate.Push(
                        myPirateWithCapsule,
                        myMotherships.OrderBy(mothership => mothership.Distance(myPirateWithCapsule))
                            .FirstOrDefault());
                    myPirates.Remove(mypirate);
                    numberOfPushesNeeded--;
                }
            }
        }

        // public void TryPushEnemyCapsule()
        // {
        //     foreach (Pirate enemyWithCapsule in EnemyPiratesWithCapsule)
        //     {
        //         PushAlliesToEnemy(enemyWithCapsule);
        //         int count = game.GetMyLivingPirates().Where(pirate => pirate.CanPush(enemyWithCapsule) && !FinishedTurn[pirate]).Count();  // Number of my living pirate who can push enemy pirates
        //         if (count >= enemyWithCapsule.NumPushesForCapsuleLoss)  // If we can drop the capsule
        //         {
        //             foreach (Pirate mypirate in game.GetMyLivingPirates().Where(pirate => pirate.CanPush(enemyWithCapsule) && !FinishedTurn[pirate]))  // We push until we drop it
        //             {
        //                 if (!enemyWithCapsule.HasCapsule())  // I think all the operations happen simultaneously at the end of the turn, so this will never be the case.
        //                     break;
        //                 mypirate.Push(enemyWithCapsule, enemyWithCapsule.InitialLocation);
        //                 FinishedTurn[mypirate] = true;
        //             }
        //         }
        //     }
        // }
        public bool PushAlliesToEnemy(Pirate target)
        {
            int count = game.GetMyLivingPirates().Where(pirate => pirate.CanPush(target)).Except(myPiratesWithCapsule).Count();
            if (!(count >= target.NumPushesForCapsuleLoss))
            {
                var MyPiratesNotInRange = game.GetMyLivingPirates().Where(pirate => !pirate.CanPush(target)).Except(myPiratesWithCapsule.ToList());
                Dictionary<Pirate, int> PiratesPush = new Dictionary<Pirate, int>();
                foreach (Pirate pirate in MyPiratesNotInRange)
                {
                    int PiratesCanPush = MyPiratesNotInRange.Where(mypirate => pirate != mypirate && pirate.CanPush(mypirate)).Count();
                    int PushesNeeded = pirate.Distance(target) / game.PushDistance;
                    if (PiratesCanPush >= PushesNeeded)
                    {
                        PiratesPush[pirate] = pirate.Distance(target) / game.PushDistance;
                    }
                }
                var PushingPirate = PiratesPush.OrderBy(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value).FirstOrDefault();
                int PushesLeft = PushingPirate.Value;
                if (PushingPirate.Key != null)
                {
                    foreach (Pirate pirate in MyPiratesNotInRange)
                    {
                        if (pirate.CanPush(PushingPirate.Key) && pirate != PushingPirate.Key && PushesLeft > 0)
                        {
                            pirate.Push(PushingPirate.Key, target);
                            myPirates.Remove(pirate);
                            PushesLeft--;
                        }
                    }
                    return true;
                }
                return false;
            }
            return true;

        }

        public static Location IntersectionPoint(Location enemyLoc, Location myLoc, Location destLoc, int S1, int S2)
        {
            int Xa = enemyLoc.Col, Ya = enemyLoc.Row,
            Xb = destLoc.Col, Yb = destLoc.Row,
            Xc = myLoc.Col, Yc = myLoc.Row;
            double a = ((Xb-Xa).Power(2)+(Yb-Ya).Power(2))/S2.Power(2)-(((Xb-Xa).Power(2)+(Yb-Ya).Power(2))/S1.Power(2));
            double b = 2*((Xb-Xa)*(Xa-Xc)+(Yb-Ya)*(Ya-Yc))/S2.Power(2);
            double c = ((Xa-Xc).Power(2)+(Ya-Yc).Power(2))/S2.Power(2);
            double T1 = -b+(b.Power(2)-4*a*c).Sqrt();
            double T2 = -b-(b.Power(2)-4*a*c).Sqrt();
            if(T1<=1&&T1>=0)
                return new Location((int)(Ya+T1*(Yb-Ya)),(int)(Xa+T1*(Xb-Xa)));
            else if(T2<=1&&T2>=0)
                return new Location((int)(Ya+T2*(Yb-Ya)),(int)(Xa+T2*(Xb-Xa)));
            return null;
        }
    }
}