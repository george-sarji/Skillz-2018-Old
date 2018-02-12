using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot
{
    class InitializationBot : IPirateBot
    {

        // ----------------------------------------
        // Bot stats:
        // ----------------------------------------
        // GeorgePushSenior - Win 8-0 - 88 turns
        // LargeRocks - Win 8-5 - 223 turns
        // OneManArmy - Win 1-0 - 400 turns
        // Meteorite - Win 8-3  - 219 turns
        // Steroids - Win 8-0 - 467 turns
        // Lepton - Win 8-4 - 556 turns
        // Voyager1 - Win 8-3 - 376 turns
        // YouShallNotPass - Win 8-7 - 366 turns
        // Pullpullon - Win 8-4 - 209 turns
        // ---------------------------------------- 
        public static PirateGame game;
        public const bool Debug = true;
        protected static List<Pirate> myPirates;
        protected static List<Pirate> myPiratesWithCapsule;
        protected static List<Capsule> myCapsules;

        protected static List<Mothership> myMotherships;

        protected static List<Mothership> enemyMotherships;
        protected static List<Pirate> enemyPirates;

        protected static List<Capsule> enemyCapsules;

        protected static Dictionary<Asteroid, bool> asteroids;

        protected static Dictionary<Pirate, Location> pirateDestinations;

        protected static Dictionary<MapObject, int> GeneralPriority;

        protected static Dictionary<Capsule, int> enemyCapsulesPushes;

        protected static List<Wormhole> activeWormholes;
        protected static Dictionary<Pirate, bool> FinishedTurn;

        protected static int MinPriorirty = 0;
        protected static int MaxPriority = 10;

        protected static bool defence = false;

        public void DoTurn(PirateGame game)
        {
            Initialize(game);
            if (defence)
            {
                DefensiveBot.PerformBunker();
            }
            else
            {
                TryPush.PushEachOther();
                AggressiveBot.PushAsteroidsNearby();
                AggressiveBot.MoveCapsuleHoldersToIntersection();
                DefensiveBot.BuildBunker();
                // AggressiveBot.GoHelpAllyWithCapsule();
                AggressiveBot.CaptureCapsules();
                AggressiveBot.PushAsteroids();
                AggressiveBot.AttackEnemies();
                MovePiratesToDestinations();
                // Priorities.GenerateGeneralPriority();
                PrintDictionary(pirateDestinations);
            }
        }

        private void Initialize(PirateGame pirateGame)
        {
            game = pirateGame;
            FinsihedTurn = new Dictionary<Pirate, bool>();
            myPirates = game.GetMyLivingPirates().ToList();
            myCapsules = game.GetMyCapsules().ToList();
            myMotherships = game.GetMyMotherships().ToList();
            enemyMotherships = game.GetEnemyMotherships().ToList();
            enemyPirates = game.GetEnemyLivingPirates().ToList();
            enemyCapsules = game.GetEnemyCapsules().ToList();
            myPiratesWithCapsule = game.GetMyLivingPirates().Where(pirate => pirate.HasCapsule()).ToList();
            enemyCapsulesPushes = game.GetEnemyCapsules().ToDictionary(key => key, value => 0);
            activeWormholes = game.GetActiveWormholes().ToList();
            pirateDestinations = new Dictionary<Pirate, Location>();
            asteroids = new Dictionary<Asteroid, bool>();
            foreach (Pirate pirate in myPirates)
            {
                FinishedTurn.Add(pirate, false);
            }
            foreach (var asteroid in game.GetLivingAsteroids())
            {
                asteroids.Add(asteroid, false);
            }
            defence = game.GetMyMotherships().Count() == 0 || game.GetMyCapsules().Count() == 0;
        }
        private void PrintDictionary(Dictionary<Pirate, Location> dictionary)
        {
            string str = "{";
            foreach (var key in dictionary.Keys)
            {
                str += key.Id + ":" + "(" + dictionary[key].Col + "," + dictionary[key].Row + ")" + ",";
            }
            (str + "}").Print();
        }
        private void MovePiratesToDestinations()
        {
            foreach (var map in pirateDestinations)
            {
                var pirate = map.Key;
                var destination = map.Value;
                if (!FinishedTurn[pirate])
                {
                    string message = "";
                    pirate.Sail(destination);
                    message = "Pirate " + pirate.ToString() + " sails towards " + destination.ToString();
                    message.Print();
                }
            }
        }

        protected static Location Closest(Location location, params Location[] locations)
        {
            return locations.OrderBy(l => l.Distance(location)).First();
        }

        protected static Location GetClosestToBorder(Location location)
        {
            var up = new Location(-5, location.Col);
            var down = new Location(game.Rows + 5, location.Col);
            var left = new Location(location.Row, -5);
            var right = new Location(location.Row, game.Cols + 5);

            return Closest(location, up, down, left, right);
        }

        protected static void AssignDestination(Pirate pirate, Location destination)
        {
            if (pirateDestinations.ContainsKey(pirate))
                pirateDestinations[pirate] = destination;
            else
                pirateDestinations.Add(pirate, destination);
        }
    }
}