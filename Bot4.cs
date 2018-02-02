using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot 
{
    class Bot4 : IPirateBot
    {

        private PirateGame game;
        private const bool Debug = false;
        private List<Pirate> myPirates;

        private List<Capsule> myCapsules;
        
        private List<Mothership> myMotherships;

        private List<Mothership> enemyMotherships;
        private List<Pirate> enemyPirates;

        private List<Capsule> enemyCapsules;

        private Dictionary<Asteroid, bool> asteroids;

        private bool defence = false;
        public void DoTurn(PirateGame game)
        {
            Initialize(game);
            MovePirates();
        }

        private void Initialize(PirateGame game)
        {
            this.game=game;
            this.myPirates = game.GetMyLivingPirates().ToList();
            this.myCapsules = game.GetMyCapsules().ToList();
            this.myMotherships = game.GetMyMotherships().ToList();
            this.enemyMotherships = game.GetEnemyMotherships().ToList();
            this.enemyPirates = game.GetEnemyLivingPirates().ToList();
            this.enemyCapsules = game.GetEnemyCapsules().ToList();
            this.asteroids = new Dictionary<Asteroid, bool>();
            foreach(var asteroid in game.GetLivingAsteroids())
            {
                asteroids.Add(asteroid, false);
            }
        }

        private void MovePirates()
        {
            if(!game.GetMyMotherships().Any() || !game.GetMyCapsules().Any())
            {
                // Handle game defensively.
                Defence();
            }
            else
            {
                CaptureGameplay();
            }
        }


        private void Defence()
        {
            foreach(var pirate in game.GetMyLivingPirates())
            {
                // Get the closest enemy capsule.
                var closestCapsule = game.GetEnemyCapsules().OrderBy(capsule => capsule.Distance(pirate)).FirstOrDefault();
                if(closestCapsule!=null)
                {
                    // Get the person who holds the capsule.
                    var capsuleHolder = game.GetEnemyLivingPirates().Where(enemy => enemy.HasCapsule() && enemy.Capsule.Equals(closestCapsule)).FirstOrDefault();
                    var closestMothership = game.GetEnemyMotherships().OrderBy(mothership => mothership.Distance(closestCapsule)).FirstOrDefault();
                    if(capsuleHolder!=null)
                    {
                        // There is a capsule holder. Attempt push.
                        if(!DefensivePush(pirate, capsuleHolder))
                        {
                            // Go towards the capsule's location between the closest mothership to the capsule as well.
                            if(closestMothership!=null)
                            {
                                // Go inbetween.
                                pirate.Sail(closestMothership.Location.Towards(capsuleHolder, (int)(game.MothershipUnloadRange*0.5)));
                                Print("Pirate "+ pirate.ToString() + " sails towards "+ closestMothership.Location.Towards(capsuleHolder, (int)(game.MothershipUnloadRange*0.5)));
                            }
                            else
                            {
                                // Leave the capsule holder. They dont have a mothership. Go do something else.
                            }
                        }
                    }
                    else
                    {
                        // No capsule holder. Regardless, go between.
                        pirate.Sail(closestMothership.Location.Towards(closestCapsule, (int)(game.MothershipUnloadRange*0.5)));
                        Print("Pirate "+ pirate.ToString() + " sails towards "+ closestMothership.Location.Towards(closestCapsule, (int)(game.MothershipUnloadRange*0.5)));
                    }
                }
            }
        }


        private void CaptureGameplay()
        {
            // This gameplay means the bot will play as normal, go grab the capsule and kill the enemy's capsule holder.
            var usedPirates = new List<Pirate>();
            foreach(var pirate in myPirates)
            {
                // Get the asteroids that are near our pirates.
                var asteroidsOrdered = game.GetLivingAsteroids().Where(asteroid => asteroid.Location.Add(asteroid.Direction).InRange(pirate, game.PushRange*2));
                if(asteroidsOrdered.Any())
                {
                    // There is an asteroid near us. Push it.
                    if(DefensivePush(pirate, asteroidsOrdered.FirstOrDefault()))
                    {
                        usedPirates.Add(pirate);
                    }
                }
                else if(game.GetLivingAsteroids().Where(asteroid => asteroid.InRange(pirate, game.PushRange)).Any())
                {
                    asteroidsOrdered = game.GetLivingAsteroids().Where(asteroid => asteroid.InRange(pirate, game.PushRange));
                    if(DefensivePush(pirate, asteroidsOrdered.First()))
                    {
                        usedPirates.Add(pirate);
                    }
                }
            }
            myPirates = myPirates.Except(usedPirates).ToList();

            // Grab our capsules.
            foreach(var capsule in myCapsules)
            {
                if(capsule.Holder==null)
                {
                    // Order my pirates according to the distance and go towards the capsule.
                    var closestPirate = myPirates.OrderBy(pirate => pirate.Distance(capsule)).Where(pirate => !pirate.HasCapsule()).FirstOrDefault();
                    if(closestPirate!=null)
                    {
                        // Send that pirate towards the capsule.
                        closestPirate.Sail(capsule);
                        myPirates = myPirates.Where(pirate => !pirate.Equals(closestPirate)).ToList();
                    }
                }
                if(capsule.Holder!=null)
                {
                    // Send the capsule holder towards the closest mothership.
                    var holder = capsule.Holder;
                    var closestMothership = myMotherships.OrderBy(mothership => holder.Distance(mothership)).FirstOrDefault();
                    if(closestMothership!=null && myPirates.Contains(holder))
                    {
                        // Sail towards the mothership.
                        holder.Sail(closestMothership);
                        Print("Capsule holder "+holder.ToString() + " sails towards mothership "+ closestMothership.ToString());
                        myPirates = myPirates.Where(pirate => !pirate.Equals(holder)).ToList();
                    }
                    if(myPirates.Count()>myCapsules.Count())
                    {
                        var closestPirateToSpawn = myPirates.OrderBy(pirate => pirate.Distance(capsule.InitialLocation)).Where(pirate => !pirate.HasCapsule()).FirstOrDefault();
                        if(closestPirateToSpawn!=null)
                        {
                            // Send the pirate to the spawn to wait for the capsule
                            closestPirateToSpawn.Sail(capsule.InitialLocation);
                            Print("Pirate "+ closestPirateToSpawn.ToString() + " sails towards capsule spawn at "+ capsule.InitialLocation);
                            myPirates = myPirates.Where(pirate => !pirate.Equals(closestPirateToSpawn)).ToList();
                        }
                    }
                }
            }

            // We start by intercepting enemy capsules.
            foreach(var mothership in enemyMotherships)
            {
                // Sort the capsules per their distance to this mothership
                var closestEnemyCapsules = enemyCapsules.OrderBy(cap => cap.Distance(mothership)).FirstOrDefault();
                if(closestEnemyCapsules!=null && enemyPirates.Any(enemy => enemy.HasCapsule() && enemy.Capsule.Equals(closestEnemyCapsules)))
                {
                    var capsuleHolder = enemyPirates.First(enemy => enemy.HasCapsule() && enemy.Capsule.Equals(closestEnemyCapsules));
                    // Interecept that capsule.
                    var orderedPirates = myPirates.OrderBy(pirate => pirate.Distance(closestEnemyCapsules))
                        .Where(pirate => pirate.Distance(mothership)<closestEnemyCapsules.Distance(mothership));
                    if(orderedPirates.Count() >= game.NumPushesForCapsuleLoss)
                    {
                        // Take the first pirates and send them to the interception.
                        var dest = mothership.Location.Towards(closestEnemyCapsules, (int)(game.MothershipUnloadRange*0.5));
                        // Send the pirates.
                        usedPirates = new List<Pirate>();
                        foreach(var pirate in orderedPirates.Take(game.NumPushesForCapsuleLoss))
                        {
                            if(pirate.Location.Equals(dest))
                            {
                                // Keep trying to push
                                DefensivePush(pirate, capsuleHolder);
                            }
                            else
                            {
                                pirate.Sail(dest);
                                Print("Pirate "+ pirate.ToString() + " sails towards interception " + dest);
                            }
                            usedPirates.Add(pirate);
                        }
                        myPirates = myPirates.Except(usedPirates).ToList();
                    }
                }
            }
            
            // Make the rest of the pirates attack their closest enemy that they can attack.
            foreach(var pirate in myPirates)
            {
                var orderedEnemies = enemyPirates.OrderBy(enemy => enemy.Distance(pirate)).OrderBy(enemy => enemy.HasCapsule());
                if(orderedEnemies.Any(enemy => enemy.Distance(pirate)/enemy.MaxSpeed>pirate.PushReloadTurns))
                {
                    var toAttack = orderedEnemies.Where(enemy => enemy.Distance(pirate)/enemy.MaxSpeed>pirate.PushReloadTurns).First();
                    if(!DefensivePush(pirate, toAttack))
                    {
                        // Sail towards that pirate.
                        pirate.Sail(toAttack);
                        Print("Pirate "+ pirate.ToString() + " sails towards "+ toAttack.ToString());
                    }
                }
                else if(orderedEnemies.Any())
                {
                    // Attack first pirate.
                    if(!DefensivePush(pirate, orderedEnemies.First()))
                    {
                        // Sail towards that pirate.
                        pirate.Sail(orderedEnemies.First());
                        Print("Pirate "+ pirate.ToString() + " sails towards "+ orderedEnemies.First().ToString());
                    }
                }
            }
        }


        private bool DefensivePush(Pirate pirate, SpaceObject obj)
        {
            // If pirate, push towards the border.
            if(obj is Pirate)
            {
                // This pirate has a capsule. Push him to the edge, all his friends are dead.
                var enemy = (Pirate)obj;
                if(pirate.CanPush(enemy))
                {
                    pirate.Push(enemy, GetClosestToBorder(enemy.Location));
                    Print("Pirate "+ pirate.ToString() + " has pushed enemy "+ enemy.ToString() + " towards "+GetClosestToBorder(enemy.Location));
                    return true;
                }
                return false;
            }
            if(obj is Asteroid)
            {
                var asteroid = (Asteroid)obj;
                if(asteroids[asteroid])
                    return true;
                // This object is an asteroid. Push it to the nearest enemy.
                if(pirate.CanPush(asteroid))
                {
                    var closestEnemy = enemyPirates.OrderBy(enemy => enemy.Distance(pirate)).FirstOrDefault();
                    if(closestEnemy!=null)
                    {
                        // Push the asteroid towards it.
                        pirate.Push(asteroid, closestEnemy);
                        Print("Pirate "+ pirate.ToString() + " pushes asteroid "+ asteroid.ToString() + " towards "+ closestEnemy.ToString());
                        return true;
                    }
                }
                else if(!pirate.HasCapsule())
                {
                    // Go towards the asteroid's destination where it is in range of a push.
                    var dest = asteroid.Location.Add(asteroid.Direction).Towards(pirate, game.PushRange);
                    pirate.Sail(dest);
                    Print("Pirate "+ pirate.ToString() + " sails towards asteroid "+ asteroid.ToString() + " in his direction to push.");
                    return true;
                }
            }
            return false;
        }


        private void Print(string s)
        {
            if(Debug)
                game.Debug(s);
        }

        private Location Closest(Location location, params Location[] locations)
        {
            return locations.OrderBy(l => l.Distance(location)).First();
        }

        private Location GetClosestToBorder(Location location)
        {
            var up = new Location(0, location.Col);
            var down = new Location(game.Rows - 1, location.Col);
            var left = new Location(location.Row, 0);
            var right = new Location(location.Row, game.Cols - 1);

            return Closest(location, up, down, left, right);
        }



    }

}