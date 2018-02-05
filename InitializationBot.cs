using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot
{
    class InitializationBot : IPirateBot
    {
        public  static PirateGame game;
        public const bool Debug = true;
        protected static List<Pirate> myPirates;

        protected static List<Capsule> myCapsules;
        
        protected static List<Mothership> myMotherships;

        protected static List<Mothership> enemyMotherships;
        protected static List<Pirate> enemyPirates;

        protected static List<Capsule> enemyCapsules;

        protected static Dictionary<Asteroid, bool> asteroids;

        protected static Dictionary<Pirate, Location> pirateDestinations;

        protected static Dictionary<MapObject , int> GeneralPriority;

        protected static int MinPriorirty = 0;
        protected static int MaxPriority = 10;

        protected static bool defence = false;

        public void DoTurn(PirateGame game)
        {
            Initialize(game);
            AggressiveBot.PushAsteroidsNearby();
            AggressiveBot.MoveCapsuleHolders();
            AggressiveBot.GoHelpAllyWithCapsule();
            AggressiveBot.CaptureCapsules();
            DefensiveBot.PerformBunker();
            AggressiveBot.PushAsteroids();
            AggressiveBot.AttackEnemies();
            MovePiratesToDestinations();
            Priorities.GenerateGeneralPriority();
            PrintDictionary(pirateDestinations);
        }

        private void Initialize(PirateGame pirateGame)
        {
            game=pirateGame;
            myPirates = game.GetMyLivingPirates().ToList();
            myCapsules = game.GetMyCapsules().ToList();
            myMotherships = game.GetMyMotherships().ToList();
            enemyMotherships = game.GetEnemyMotherships().ToList();
            enemyPirates = game.GetEnemyLivingPirates().ToList();
            enemyCapsules = game.GetEnemyCapsules().ToList();
            pirateDestinations = new Dictionary<Pirate, Location>();
            asteroids = new Dictionary<Asteroid, bool>();
            foreach(var asteroid in game.GetLivingAsteroids())
            {
                asteroids.Add(asteroid, false);
            }
        }
        private void PrintDictionary(Dictionary<Pirate,Location> dictionary)
        {
            string str="{";
            foreach(var key in dictionary.Keys)
            {
                str+=key.Id+":"+"("+dictionary[key].Col+","+dictionary[key].Row+")"+",";
            }
            game.Debug(str+"}");
        }
        private void MovePiratesToDestinations()
        {
            foreach(var map in pirateDestinations)
            {
                var pirate = map.Key;
                var destination = map.Value;
                string message = "";
                pirate.Sail(destination);
                message = "Pirate "+ pirate.ToString() + " sails towards "+destination.ToString();
                message.Print();
            }
        }

        protected static Location Closest(Location location, params Location[] locations)
        {
            return locations.OrderBy(l => l.Distance(location)).First();
        }

        protected static Location GetClosestToBorder(Location location)
        {
            var up = new Location(0, location.Col);
            var down = new Location(game.Rows - 1, location.Col);
            var left = new Location(location.Row, 0);
            var right = new Location(location.Row, game.Cols - 1);

            return Closest(location, up, down, left, right);
        }
    }
}