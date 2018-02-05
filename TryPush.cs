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
            if (pirate.CanPush(capsuleHolder))
            {
                // Check how much other pirates can push it.
                var numOfPushers = NumOfPushesAvailable(capsuleHolder);
                // Check if we can either make the pirate lose his capsule or get pushed outside the border.
                var pushesToBorder = capsuleHolder.Distance(GetClosestToBorder(capsuleHolder.Location)) / pirate.PushDistance;
                if (numOfPushers >= pushesToBorder || numOfPushers >= capsuleHolder.NumPushesForCapsuleLoss)
                {
                    // Push the pirate towards the border!
                    pirate.Push(capsuleHolder, GetClosestToBorder(capsuleHolder.Location));
                    return true;
                }
            }
            return false;
        }

        public static bool TryPushAsteroid(Pirate pirate, Asteroid asteroid)
        {
            if (pirate.CanPush(asteroid) && !asteroids[asteroid])
            {
                var closestEnemy = enemyPirates.OrderBy(enemy => enemy.Distance(pirate)).OrderBy(enemy => GetGroupingNumber(enemy)).FirstOrDefault();
                if (closestEnemy != null)
                {
                    // Push the asteroid towards it.
                    pirate.Push(asteroid, closestEnemy);
                    asteroids[asteroid] = true;
                    ("Pirate " + pirate.ToString() + " pushes asteroid " + asteroid.ToString() + " towards " + closestEnemy.ToString()).Print();
                    return true;
                }
            }
            return false;
        }


        public static bool TryPushEnemy(Pirate pirate, Pirate enemy)
        {
            if (pirate.CanPush(enemy))
            {
                pirate.Push(enemy, GetClosestToBorder(enemy.Location));
                ("Pirate " + pirate.ToString() + " pushes enemy " + enemy.ToString() + " towards " + GetClosestToBorder(enemy.Location)).Print();
                return true;
            }
            return false;
        }


        public static int GetGroupingNumber(Pirate pirate)
        {
            return game.GetEnemyLivingPirates().Where(p => p.InRange(pirate, game.PushRange * 2)).Count();
        }

        public static void TryPushMyCapsule()
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
        public static bool PushAlliesToEnemy(Pirate target)
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
        public static void PushEachOther()
        {
            bool pirate1Reaches = false, pirate2Reaches = false;
            foreach (Pirate pirateWithCapsule in myPiratesWithCapsule)
            {
                var heading = myMotherships.OrderBy(mothership => mothership.Distance(pirateWithCapsule)).FirstOrDefault();
                pirate1Reaches = false;
                pirate2Reaches = false;
                var PiratesWhoCanPush = myPiratesWithCapsule
                .Where(pirate => pirate.CanPush(pirate)
                && pirateWithCapsule != pirate
                && pirate.Distance(heading)>pirate.PushDistance).ToList();
                while (PiratesWhoCanPush.Count > 1)
                {
                    var first = PiratesWhoCanPush.First();
                    PiratesWhoCanPush.Remove(first);
                    var second = PiratesWhoCanPush.First();
                    PushPair(first,second,heading.Location);
                }
            }
        }

        public static void PushPair(Pirate pirate1,Pirate pirate2,Location destination)
        {  
            pirate1.Push(pirate2,destination);
            pirate2.Push(pirate1,destination);
        }
    }
}