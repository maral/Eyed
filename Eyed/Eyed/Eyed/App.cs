using System;
using System.IO;
using System.Reflection;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Input;
using WaveEngine.Common.Math;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;

namespace Eyed
{
    public class App : WaveEngine.Adapter.Application
    {
        EyedProject.Game game;
        SpriteBatch spriteBatch;

        public App()
        {
            this.Width = 800;
            this.Height = 600;
            this.FullScreen = false;
            this.WindowTitle = "Eyed";
        }

        public override void Initialize()
        {
            this.game = new EyedProject.Game();
            this.game.Initialize(this);

            this.spriteBatch = new SpriteBatch(WaveServices.GraphicsDevice);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (this.game != null && !this.game.HasExited)
            {
                if (WaveServices.Input.KeyboardState.F10 == ButtonState.Pressed)
                {
                    this.FullScreen = !this.FullScreen;
                }

                if (WaveServices.Input.KeyboardState.Escape == ButtonState.Pressed)
                {
                    WaveServices.Platform.Exit();
                }
                else
                {
                    this.game.UpdateFrame(elapsedTime);
                }
            }
        }

        public override void Draw(TimeSpan elapsedTime)
        {
            if (this.game != null && !this.game.HasExited)
            {
                this.game.DrawFrame(elapsedTime);
            }
        }

        /// <summary>
        /// Called when [activated].
        /// </summary>
        public override void OnActivated()
        {
            base.OnActivated();
            if (this.game != null)
            {
                game.OnActivated();
            }
        }

        /// <summary>
        /// Called when [deactivate].
        /// </summary>
        public override void OnDeactivate()
        {
            base.OnDeactivate();
            if (this.game != null)
            {
                game.OnDeactivated();
            }
        }
    }
}

