using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EyedProject.Components;
using WaveEngine.Common.Math;
using WaveEngine.Framework;
using WaveEngine.Framework.Diagnostic;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics2D;
using WaveEngine.Framework.Services;

namespace EyedProject
{
    public class GameplaySceneBehavior : SceneBehavior
    {
        private List<Entity> players;
        private List<Vector2> initPlayerPositions;
        private SoundManager soundManager;
        private GameScene gameScene;


        private List<PlayerController> playerControllers;

        bool isInitialized = false;

        protected override void ResolveDependencies()
        {

        }

        private void Initialize()
        {
            gameScene = (GameScene) Scene;
            soundManager = Scene.EntityManager.Find("soundManager").FindComponent<SoundManager>();
            InitPlayers();
        }

        private void InitPlayers()
        {
            players = new List<Entity>();
            playerControllers = new List<PlayerController>();
            initPlayerPositions = new List<Vector2>();

            for (int i = 0; i < Game.PLAYER_COUNT; i++)
            {
                players.Add(Scene.EntityManager.Find("player" + i));
                playerControllers.Add(players[i].FindComponent<PlayerController>());
                initPlayerPositions.Add(players[i].FindComponent<Transform2D>().Position);
            }
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (!this.isInitialized)
            {
                this.Initialize();
                this.isInitialized = true;
            }
            else
            {
                //gameScene.FishLens.StrengthX += 0.01f;
            }
        }

        private void Win()
        {
            this.soundManager.PlaySound(SoundType.Victory);
            this.ResetGame();
        }

        private void Defeat()
        {
            this.soundManager.PlaySound(SoundType.Crash);
            this.ResetGame();
        }

        private void ResetGame()
        {
            foreach (var controller in playerControllers)
                controller.Reset();
        }
    }
}
