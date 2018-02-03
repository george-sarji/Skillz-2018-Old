using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot
{
    class InitializationBot : IPirateBot
    {
        public  PirateGame game;
        protected const bool Debug = false;
        protected static List<Pirate> myPirates;

        protected static List<Capsule> myCapsules;
        
        protected static List<Mothership> myMotherships;

        protected static List<Mothership> enemyMotherships;
        protected static List<Pirate> enemyPirates;

        protected static List<Capsule> enemyCapsules;

        protected static Dictionary<Asteroid, bool> asteroids;

        protected static bool defence = false;

        public void DoTurn(PirateGame game)
        {
            Initialize(game);
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
            asteroids = new Dictionary<Asteroid, bool>();
            foreach(var asteroid in game.GetLivingAsteroids())
            {
                asteroids.Add(asteroid, false);
            }
        }
    }
}