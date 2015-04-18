#region Using Statements
using System;
using WaveEngine.Common;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Cameras;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Components.Animation;

using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Resources;
using WaveEngine.Framework.Services;
using WaveEngine.Materials;
#endregion

namespace EyedProject
{
    public class GameScene : Scene
    {
        protected override void CreateScene()
        {
            var camera2D = new FixedCamera2D("Camera2D") { BackgroundColor = Color.Black };
            EntityManager.Add(camera2D);

            var eye = new Entity("eye")
                .AddComponent(new Transform2D()
                {
                    X = WaveServices.Platform.ScreenWidth / 2,
                    Y = WaveServices.Platform.ScreenHeight - 100,
                    Origin = new Vector2(0.5f, 0.5f)
                })
                .AddComponent(new Sprite("Content/texture.wpk"))
                .AddComponent(Animation2D.Create<TexturePackerGenericXml>("Content/texture.xml")
                    .Add("Idle", new SpriteSheetAnimationSequence()
                    {
                        First = 1,
                        Length = 1,
                        FramesPerSecond = 1
                    })
                    .Add("Running", new SpriteSheetAnimationSequence()
                    {
                        First = 2,
                        Length = 1,
                        FramesPerSecond = 1
                    }))
                .AddComponent(new AnimatedSpriteRenderer());
            EntityManager.Add(eye);
        }

        protected override void Start()
        {
            base.Start();

            // This method is called after the CreateScene and Initialize methods and before the first Update.
        }
    }
}
