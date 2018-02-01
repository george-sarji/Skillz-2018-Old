using System.Collections.Generic;
using System.Text;
using System.Linq;
using Pirates;

namespace Bot 
{
    class Bot4 : IPirateBot
    {

        private PirateGame game;
        private const bool Debug = true;
        private List<Pirate> myPirates;

        private List<Capsule> myCapsules;
        
        private List<Mothership> myMotherships;

        private List<Mothership> enemyMotherships;
        private List<Pirate> enemyPirates;

        private List<Capsule> enemyCapsules;

        private bool defence = false;
        public void DoTurn(PirateGame game)
        {
            Initialize(game);
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
        }

        private void MovePirates()
        {

        }
    }

}