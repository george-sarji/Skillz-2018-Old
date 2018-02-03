using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot
{
    class InitializationBot : IPirateBot
    {
        public  static PirateGame game;
        public const bool Debug = false;
        protected static List<Pirate> myPirates;

        protected static List<Capsule> myCapsules;
        
        protected static List<Mothership> myMotherships;

        protected static List<Mothership> enemyMotherships;
        protected static List<Pirate> enemyPirates;

        protected static List<Capsule> enemyCapsules;

        protected static Dictionary<Asteroid, bool> asteroids;

        protected static Dictionary<Pirate, Location> pirateDestinations;

        protected static bool defence = false;

        public void DoTurn(PirateGame game)
        {
            Initialize(game);
            AggressiveBot.CaptureCapsules();


            MovePiratesToDestinations();
        }

        private void Initialize(PirateGame game)
        {
            game=game;
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

        private void MovePiratesToDestinations()
        {
            foreach(var map in pirateDestinations)
            {
                var pirate = map.Key;
                var destination = map.Value;
                string message = "Pirate "+ pirate.ToString() + " sails towards "+ destination.ToString();
                message.Print();
                pirate.Sail(destination);
            }
        }
    }
}