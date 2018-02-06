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
                if ((numOfPushers >= pushesToBorder && enemyCapsulesPushes[capsuleHolder.Capsule]<pushesToBorder) || (numOfPushers >= capsuleHolder.NumPushesForCapsuleLoss && enemyCapsulesPushes[capsuleHolder.Capsule]<capsuleHolder.NumPushesForCapsuleLoss))
                {
                    // Push the pirate towards the border!
                    pirate.Push(capsuleHolder, GetClosestToBorder(capsuleHolder.Location));
                    enemyCapsulesPushes[capsuleHolder.Capsule]++;
                    return true;
                }
            }
            return false;
        }

        public static bool TryPushAsteroid(Pirate pirate, Asteroid asteroid)
        {
            if (pirate.CanPush(asteroid) && !asteroids[asteroid])
            {
                var closestEnemy = enemyPirates.OrderBy(enemy => enemy.Distance(asteroid)).OrderByDescending(enemy => GetGroupingNumber(enemy)).FirstOrDefault();
                var pushDestination = game.GetLivingAsteroids().OrderBy(ast => ast.Distance(asteroid)).Where(ast => ast != asteroid).FirstOrDefault();
                if(closestEnemy!=null && pushDestination!=null)
                {
                    // Check which one is closest to the pirate.
                    if(closestEnemy.Distance(pirate)<pushDestination.Distance(pirate))
                    {
                        // Push the asteroid towards the enemies.
                        pirate.Push(asteroid, closestEnemy);
                        ("Pirate "+pirate.ToString() + " pushes asteroid "+ asteroid.ToString() + " towards "+ closestEnemy.ToString()).Print();
                    }
                    else
                    {
                        pirate.Push(asteroid, pushDestination);
                        ("Pirate "+pirate.ToString() + " pushes asteroid "+ asteroid.ToString() + " towards "+ pushDestination.ToString()).Print();
                    }
                    asteroids[asteroid] = true;
                    return true;
                }
                else if (closestEnemy != null)
                {
                    // astroids.OrderBy(asteroid => asteroid.Distance(destination)).OrderBy(asteroid => asteroid.Distance(closestAsteroid)).Where(asteroid => asteroid != closestAsteroid).FirstOrDefault()
                    // Push the asteroid towards it.
                    
                    pirate.Push(asteroid, closestEnemy);
                    ("Pirate "+pirate.ToString() + " pushes asteroid "+ asteroid.ToString() + " towards "+ closestEnemy.ToString()).Print();
                    asteroids[asteroid] = true;
                    return true;
                }
                else if(pushDestination!=null)
                {
                    pirate.Push(asteroid, pushDestination);
                    ("Pirate "+pirate.ToString() + " pushes asteroid "+ asteroid.ToString() + " towards "+ pushDestination.ToString()).Print();
                    asteroids[asteroid] = true;
                    return true;
                }
            }
            return false;
        }


        public static bool TryPushAsteroidTowardsCapsule(Pirate pirate, Asteroid asteroid)
        {
            if (pirate.CanPush(asteroid) && !asteroids[asteroid])
            {
                // Check if there is a capsule holder.
                var enemyCapsuleHolders =enemyPirates.Where(p=> p.HasCapsule());
                if(enemyCapsuleHolders.Any())
                {
                    enemyCapsuleHolders = enemyCapsuleHolders.OrderBy(p => p.Distance(asteroid));
                    var closestHolder = enemyCapsuleHolders.FirstOrDefault();
                    var closestMothership = enemyMotherships.OrderBy(m => m.Distance(closestHolder)).FirstOrDefault();
                    if(closestMothership!=null)
                    {
                        // Intercept the capsule with the asteroid.
                        var interception = GameExtension.IntersectionPoint(closestHolder.Location, asteroid.Location, closestMothership.Location, closestHolder.MaxSpeed, asteroid.Speed);
                        if(interception!=null)
                        {
                            // Push the asteroid.
                            pirate.Push(asteroid, interception);
                            asteroids[asteroid]=true;
                            ("Pirate "+ pirate.ToString() + " pushes asteroid " + asteroid.ToString() + " to intercept "+ closestHolder.ToString() + " at "+interception).Print();
                            return true;
                        }
                    }
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
            var PiratesWithCapsuleCanPushOthers = new Dictionary<Pirate, List<Pirate>>();
            var UsedPirates = new List<Pirate>();
            foreach (Pirate pirateWithCapsule in myPiratesWithCapsule)
            {
                var heading = myMotherships.OrderBy(mothership => mothership.Distance(pirateWithCapsule)).FirstOrDefault();
                var PiratesWhoCanPush = myPiratesWithCapsule
                .Where(pirate => pirate.CanPush(pirateWithCapsule)
                && pirateWithCapsule != pirate
                && pirate.Distance(heading)-((Mothership)heading).UnloadRange < pirate.PushDistance).ToList();
                if (PiratesWhoCanPush.Count == 0)
                {
                    continue;
                }
                PiratesWithCapsuleCanPushOthers[pirateWithCapsule] = PiratesWhoCanPush;
            }
            foreach (var pirate in PiratesWithCapsuleCanPushOthers.
            OrderBy(item => item.Value.Count)
            .ToDictionary(pair => pair.Key, pair => pair.Value).Keys.ToList())
            {
                var heading = myMotherships.OrderBy(mothership => mothership.Distance(pirate)).FirstOrDefault();
                if (myPirates.Contains(pirate))
                {
                    Pirate OtherPushingPirate = PiratesWithCapsuleCanPushOthers[pirate].First();
                    PushPair(pirate, OtherPushingPirate, heading.Location);
                    myPirates.Remove(pirate);
                    myPirates.Remove(OtherPushingPirate);
                }
            }
        }

        public static void PushPair(Pirate pirate1, Pirate pirate2, Location destination)
        {
            pirate1.Push(pirate2, destination);
            pirate2.Push(pirate1, destination);
        }
    }
}