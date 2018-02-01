using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace DutchmanBotReviewed
{
    class Bot3 : IPirateBot
    {
        // Very impressive results! And great job on documenting what happens with each bot.
        // ------------------------------------------
        // Game statistics
        // ------------------------------------------
        // Bronze:
        // SpaceGarbage - Win 8-0 - 126 turns
        // WinBunkerBot - Win 8-1 - 130 turns
        // NotATutorial - Win 8-0 - 115 turns
        // KillerTutorial - Win 8-0 - 120 turns
        // BunkerBot - Win 8-0 - 211 turns
        // ------------------------------------------
        // Silver:
        // Pushatron - Win 8-0 - 180 turns
        // TheAssassin - Win 8-0 - 242 turns
        // PushingDude - Win 8-1 - 117 turns
        // GitPush - Win 8-3 - 218 turns - 4 enemy pirates camp our mothership in a straight line. Try an alternative route. Enemy pirates push their capsule into the mothership past our interceptors.
        // PushUps - Win 8-4 - 158 turns - 3 enemy pirates camp infront of our mothership. Try an alternative route. Enemy pirates push our interceptors away and into the border.
        // ------------------------------------------
        // Gold:
        // GenghisBot - Win 8-3 - 255 turns - Enemies camp our capsule holder and shoot it everytime it takes it into the border.
        // Joe - Win 8-0 - 339 turns
        // DoubleTrouble - Win 8-0 - 229 turns
        // GeorgeWPush - Win 8-0 - 360 turns
        // Pushpushon - Win 8-5 - 560 turns - 2 enemy pirates camp our mothership. Try an alternative route or push our capsule into the mothership. Enemy capsule avoids our pirates.
        // ------------------------------------------
        // Plan for the next bot:
        // 1)   a) Take an alternative route
        //      b) Push our capsules into the mothership
        // ------------------------------------------


        // ------------------------------------------
        // Environment variables
        // ------------------------------------------

        private PirateGame game;
        private const bool Debug = true;
        private List<Pirate> myPirates;

        private Dictionary<Pirate, Location> loggedLocations;
        private Dictionary<int, Location> loggedIDLocations;

        // ------------------------------------------
        // Initiating functions
        // ------------------------------------------
        public void DoTurn(PirateGame game)
        {
            if(game.Turn==1 || loggedLocations==null)
            {
                loggedLocations = new Dictionary<Pirate, Location>();
                loggedIDLocations = new Dictionary<int, Location>();
            }
            
            Initialize(game);
            PrintLogs();
            PrintIDLogs();
            IsThereABunker();
            MovePirates();
            LogLocations();
            LogLocationsID();
        }

        public void Initialize(PirateGame game)
        {
            this.game = game;
            this.myPirates = game.GetMyLivingPirates().ToList();  // This gets overridden in MovePirates().
        }


        // ------------------------------------------
        // Pirate functions
        // ------------------------------------------

        private void MovePirates()
        {
            // This function will move our pirates.
            // The closest pirate to the capsule will go towards it. The second pirate will cover the spawn, the rest will attack the enemy pirates.
            // Addition 1: A number of pirates (NumPushesForCapsuleLoss) will camp the enemy mothership inbetween the capsule and the mothership
            myPirates = myPirates.OrderBy(pirate => pirate.Distance(game.GetMyCapsule().InitialLocation)).ToList();
            // if(myPirates.Any(pirate => pirate.HasCapsule()))
            // {
            //     // There is a pirate with a capsule. Make him go to the mothership
            //     var capsuleHolder = myPirates.First(pirate => pirate.HasCapsule());
            //     capsuleHolder.Sail(game.GetMyMothership());
            //     Print("Pirate "+ capsuleHolder + " sails to "+ game.GetMyMothership());
            //     myPirates.Remove(capsuleHolder);
            // }
            myPirates = ManageCapsulePirates();
            if (myPirates.Any())
            {
                var currentPirate = myPirates.First();
                currentPirate.Sail(GetCapsulePickupLocation());
                Print("Pirate "+ currentPirate + " sails to capsule at " + GetCapsulePickupLocation());
                myPirates.Remove(currentPirate);
            }
            myPirates = InterceptCapsule();
            // Pirates will remain unused below, only if there are no enemies. So you can simplify things a bit:
            if (game.GetEnemyLivingPirates().Any())
            {
                foreach (var pirate in myPirates)
                {
                    // Sort the enemies per their distance then their capsule possession.
                    var enemies = game.GetEnemyLivingPirates()
                        .OrderBy(enemy => enemy.Distance(pirate))
                        .OrderByDescending(enemy => enemy.HasCapsule());

                    if (!TryPush(pirate, enemies.First()))
                    {
                        // Sail to the pirate
                        pirate.Sail(enemies.First());
                        Print("Pirate "+ pirate + " sails to "+ enemies.First());
                    }
                }
            }
            else
            {
                foreach (var left in myPirates)
                {
                    Print("All enemy pirates are dead. Unused pirate: " + left);
                }
            }
        }

        // Returns the remaining pirates.
        private List<Pirate> ManageCapsulePirates()
        {
            // Use camelCase for variable names (enemies, myMothership, capsulePirates, capsuleHolder, closeEnemy, etc.)
            var Enemies = game.GetEnemyLivingPirates();
            var MyMothership = game.GetMyMothership();

            // Take the closest 2 pirates to the capsule.
            var CapsulePirates = myPirates.OrderBy(pirate => pirate.Distance(game.GetMyCapsule())).Take(2);
            // Seperate them to holder and pusher.
            var CapsuleHolder = CapsulePirates.Where(pirate => pirate.HasCapsule()).FirstOrDefault();

            // Too many if's and else's in this function...
            // Seems like if (capsuleHolder != null), then you're always sending him to the Mothership,
            // so CapsuleHolder.Sail(MyMothership); can be outside some if/else combos.
            // See the changes I made - I think it simplifies the code but doesn't change what it does.

            // if there's no holder, send both to pick it up.
            if (CapsuleHolder == null)
            {
                foreach (Pirate pirate in CapsulePirates)
                    // Impressive - saving time.
                    // But you seem to have this in 3 different places: game.GetMyCapsule().InitialLocation.Towards(mothership, game.CapsulePickupRange-1)
                    // So why not put it into a util function GetCapsulePickupLocation().
                    pirate.Sail(GetCapsulePickupLocation());
            }
            else  // else, send them to the mothership.
            {
                CapsuleHolder.Sail(MyMothership);

                var CapsulePusher = CapsulePirates.Where(pirate => pirate != CapsuleHolder).FirstOrDefault();
                if (CapsulePusher != null)
                {
                    // Get the closest enemy.
                    var CloseEnemy = Enemies.OrderBy(enemy => enemy.Distance(CapsuleHolder)).FirstOrDefault();
                    // if the holder can reach the Mothership with one push, push him.
                    // or if there's a threat from a close enemy, push the holder.
                    if (CapsulePusher.CanPush(CapsuleHolder) &&
                        (CapsuleHolder.Distance(MyMothership) < MyMothership.UnloadRange + CapsulePusher.PushDistance + CapsuleHolder.MaxSpeed ||
                        CloseEnemy != null && CapsuleHolder.InRange(CloseEnemy, CloseEnemy.PushRange + CloseEnemy.MaxSpeed)))
                    {
                        CapsulePusher.Push(CapsuleHolder, MyMothership);
                    }
                    else  // if it's safe, sail normally to the MotherShip.
                    {
                        CapsulePusher.Sail(CapsuleHolder);
                    }
                }
            }
            // Remove the used pirates from the pirates array.
            return myPirates.Except(CapsulePirates).ToList();
        }


        private List<Pirate> InterceptCapsule()
        {
            // The destination is from the mothership towards the capsule
            var destination = game.GetEnemyMothership().Location.Towards(game.GetEnemyCapsule().InitialLocation, game.PushRange * 2);
            // Sort the pirates by their distance to the destination
            var pushers = myPirates.OrderBy(pirate => pirate.Distance(destination)).Take(game.NumPushesForCapsuleLoss);
            foreach (var pirate in pushers)
            {
                // Check if the pirate is in the destination
                if (pirate.Location.Equals(destination))
                {
                    var capsuleHolder = game.GetEnemyCapsule().Holder;
                    if (capsuleHolder != null)
                    {
                        // Keep trying to push
                        // Don't TryPush() inside the call to Print().
                        bool pushResult = TryPush(pirate, capsuleHolder);
                        Print("Pirate " + pirate + " is at the interception and tried to push " + capsuleHolder + ":" + pushResult);
                        Print("Pirate "+ pirate + " reload turns: "+ pirate.PushReloadTurns);
                    }
                    else
                    {
                        // Don't do anything, just wait.
                        Print("Pirate " + pirate + " has arrived to the interception point " + destination + " and is awaiting the capsule to be taken.");
                    }
                }
                else
                {
                    // Sail towards the destination
                    pirate.Sail(destination);
                    Print("Pirate "+ pirate + " sails to " + destination + " to intercept "+ game.GetEnemyCapsule());
                }
            }
            return myPirates.Except(pushers).ToList();
        }

        private bool TryPush(Pirate pirate, Pirate enemy)
        {
            if (pirate.CanPush(enemy))
            {
                if (enemy.HasCapsule())
                {
                    // Push the capsule to the initial location
                    pirate.Push(enemy, game.GetEnemyCapsule().InitialLocation);
                    Print("Pirate "+ pirate + " pushes "+ enemy + " (capsule holder) to "+ game.GetEnemyCapsule().InitialLocation);
                }
                else
                {
                    // Push the enemy to the border
                    pirate.Push(enemy, GetOutsideBorder(enemy.Location));
                    Print("Pirate "+ pirate + " pushes "+ enemy + " towards "+ GetOutsideBorder(enemy.Location));
                }
                return true;
            }
            return false;
        }


        // ------------------------------------------
        // Assisting functions
        // ------------------------------------------

        private void Print(string s)
        {
            if(Debug)
                game.Debug(s);
        }

        private Location GetCapsulePickupLocation() {
            return game.GetMyCapsule().InitialLocation.Towards(game.GetMyMothership(), game.CapsulePickupRange-1);
        }

        private Location GetClosestToBorder(Location location)
        {
            var up = new Location(0, location.Col);
            var down = new Location(game.Rows - 1, location.Col);
            var left = new Location(location.Row, 0);
            var right = new Location(location.Row, game.Cols - 1);

            return Closest(location, up, down, left, right);
        }

        // What does this actually do? GetClosestToBorder() is unused.
        private Location GetOutsideBorder(Location location)
        {
            return new Location(location.Row * 2, location.Col * 2);
        }

        private Location Closest(Location location, params Location[] locations)
        {
            return locations.OrderBy(l => l.Distance(location)).First();
        }

        private Location MidPoint(MapObject a, MapObject b)
        {
            return new Location((a.GetLocation().Row + b.GetLocation().Row) / 2,
                                (a.GetLocation().Col + b.GetLocation().Col) / 2);
        }


        private void PushInTheBestWay(Pirate p1, Pirate p2)
        {
            if(p1.CanPush(p2))
            {
                if(p2.HasCapsule())
                {
                    p1.Push(p2, game.GetEnemyMothership().Location.Towards(p2, game.PushRange*2));
                }
                else
                {
                    p1.Push(p2, GetOutsideBorder(p2.Location));
                }
            }
        }

        private void LogLocations()
        {
            foreach(var enemy in game.GetEnemyLivingPirates())
            {
                if(loggedLocations.ContainsKey(enemy))
                {
                    loggedLocations[enemy] = enemy.Location;
                }
                else
                {
                    loggedLocations.Add(enemy, enemy.Location);
                }
            }
        }

        private void LogLocationsID()
        {
            foreach(var enemy in game.GetEnemyLivingPirates())
            {
                if(loggedIDLocations.ContainsKey(enemy.UniqueId))
                {
                    loggedIDLocations[enemy.UniqueId] = enemy.Location;
                }
                else
                {
                    loggedIDLocations.Add(enemy.UniqueId, enemy.Location);
                }
            }
        }

        private void PrintIDLogs()
        {
            foreach(var map in loggedIDLocations)
            {
                Print("Log for ID "+ map.Key + " is @ "+ map.Value);
                var enemyID = game.GetAllEnemyPirates().Where(pirate => pirate.UniqueId == map.Key).FirstOrDefault();
                if(enemyID==null)
                    Print("Enemy is null.");
                else
                    Print("Current location for pirate: "+ enemyID.Location);
            }
        }

        private void PrintLogs()
        {
            foreach(var map in loggedLocations)
            {
                Print("Log for "+ map.Key.toString()+" is @ "+ map.Value+". Current location: "+map.Key.Location);
            }
        }

        private bool IsThereABunker()
        {
            var bunker = false;
            foreach(var map in loggedLocations)
            {
                var pirate = map.Key;
                var location = map.Value;
                if(pirate.InRange(game.GetMyMothership(), game.PushRange*2) && location.Equals(pirate.Location))
                {
                    Print("Possible bunker by "+ pirate.toString());
                    bunker = true;
                }
            }
            return bunker;
        }

        // Unused.
        // private List<Pirate> WhoCanPush(List<Pirate> pirates, Pirate enemy)
        // {
        //     return pirates.Where(pirate => pirate.CanPush(enemy)).ToList();
        // }
    }
}