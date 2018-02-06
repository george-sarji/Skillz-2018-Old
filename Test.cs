using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot
{
    class Test : IPirateBot
    {
        //InitializationBot
        public  static PirateGame game;
        public const bool Debug = false;
        protected static List<Pirate> myPirates;
        protected static List<Pirate> myPiratesWithCapsule;
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
            AvoidAsteroids();
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
            myPiratesWithCapsule = game.GetMyLivingPirates().Where(pirate => pirate.HasCapsule()).ToList();
            pirateDestinations = new Dictionary<Pirate, Location>();
            asteroids = new Dictionary<Asteroid, bool>();
            foreach(var asteroid in game.GetLivingAsteroids())
            {
                asteroids.Add(asteroid, false);
            }
        }
        public static void AvoidAsteroids()
        {
            var pirate = myPirates.FirstOrDefault();
            if(pirate==null)
                return;
            Location destination;
            if(pirate.HasCapsule())
                destination = myMotherships.OrderBy(ship => ship.Distance(pirate)).FirstOrDefault().GetLocation();
            else 
                destination = myCapsules.OrderBy(capsule => capsule.InitialLocation.Distance(pirate)).FirstOrDefault().GetLocation();
            var astroids = game.GetLivingAsteroids().OrderBy(asteroid => asteroid.Distance(destination)).OrderBy(asteroid => asteroid.Distance(pirate));
            var closestAsteroid = astroids.FirstOrDefault();
            if(closestAsteroid != null && pirate.CanPush(closestAsteroid))
                pirate.Push(closestAsteroid,astroids.OrderBy(asteroid => asteroid.Distance(destination)).OrderBy(asteroid => asteroid.Distance(closestAsteroid)).Where(asteroid => asteroid != closestAsteroid).FirstOrDefault());
            // else if(closestAsteroid != null && closestAsteroid.Distance(pirate)<=pirate.MaxSpeed+closestAsteroid.Speed+closestAsteroid.Size)
            // {
            //     var secondClosestAsteroid = astroids.Where(asteroid => asteroid != closestAsteroid).FirstOrDefault();
            //     var firstNextLocation = new Location(closestAsteroid.GetLocation().Row+closestAsteroid.Direction.Row, closestAsteroid.GetLocation().Col+closestAsteroid.Direction.Col);
            //     var secondNextLocation = new Location(secondClosestAsteroid.GetLocation().Row+secondClosestAsteroid.Direction.Row, secondClosestAsteroid.GetLocation().Col+secondClosestAsteroid.Direction.Col);
            //     var midPoint = new Location((int)((firstNextLocation.Row+secondNextLocation.Row)/2),(int)((firstNextLocation.Col+secondNextLocation.Col)/2));
            //     pirate.Sail(midPoint);
            // }
            else
                pirate.Sail(destination);

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