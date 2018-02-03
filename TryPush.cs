using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot 
{
    class TryPush : InitializationBot
    {
        var game = InitializationBot.game;
        var EnemyPirates = game.GetEnemyLivingPirates();
        var MyPirates = game.GetMyLivingPirates();
        var Asteroids = game.GetLivingAsteroids();
        public List<Pirate> PushAsteroid()
        {
            foreach (var Enemy in EnemyPirates)
            {
                foreach (var Piratre in MyPirates)
                {
                    foreach (var Asteroid in Asteroid)
                    {
                        var EnemyNextLocation = Enemy.Location.Towards(Asteroid.Location+Asteroid.Direction,Pirate.PushDistance);
                        if(Pirate.CanPush(Enemy) && EnemyNextLocation.InRange(Asteroid.Location+Asteroid.Direction,Asteroid.Size))
                        {
                            Pirate.Push(Enemy,EnemyNextLocation);
                        }
                    }
                }
            }
        }
    }
}