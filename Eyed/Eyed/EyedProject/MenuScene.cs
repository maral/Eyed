using System;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Cameras;
using WaveEngine.Components.Gestures;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Components.UI;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics2D;
using WaveEngine.Framework.Services;

namespace EyedProject
{
    public class MenuScene : Scene
    {
        protected override void CreateScene()
        {
            var camera2D = new FixedCamera2D("Camera2D") { BackgroundColor = Color.White };
            EntityManager.Add(camera2D);

            int offset = 50;
            var title = new Entity("Title")
                .AddComponent(new Sprite("Content/Textures/home.wpk"))
                .AddComponent(new SpriteRenderer(DefaultLayers.Alpha))
                .AddComponent(new Transform2D()
                {
                    X = 0,
                    Y = 0
                });
            EntityManager.Add(title);

            var twoPlayers = new Entity("2Players")
                    .AddComponent(new Transform2D()
                    {
                        Rectangle = new RectangleF(80, 400, 500, 170)
                    })
                    .AddComponent(new RectangleCollider())
                    .AddComponent(new TouchGestures());

            twoPlayers.FindComponent<TouchGestures>().TouchPressed += new EventHandler<GestureEventArgs>(TwoPlayers_TouchPressed);
            EntityManager.Add(twoPlayers);

            var threePlayers = new Entity("3Players")
                    .AddComponent(new Transform2D()
                    {
                        Rectangle = new RectangleF(730, 400, 520, 170)
                    })
                    .AddComponent(new RectangleCollider())
                    .AddComponent(new TouchGestures());

            threePlayers.FindComponent<TouchGestures>().TouchPressed += new EventHandler<GestureEventArgs>(ThreePlayers_TouchPressed);

            EntityManager.Add(threePlayers);
        }

        private void ThreePlayers_TouchPressed(object sender, GestureEventArgs e)
        {
            ScreenContext screenContext = new ScreenContext(new GameScene())
            {
                Name = "ThreePlayers",
            };
            WaveServices.ScreenContextManager.To(screenContext);
        }

        private void TwoPlayers_TouchPressed(object sender, GestureEventArgs e)
        {
            ScreenContext screenContext = new ScreenContext(new GameScene())
            {
                Name = "TwoPlayers",
            };
            WaveServices.ScreenContextManager.To(screenContext);
        }

        protected override void Start()
        {
            base.Start();

            // This method is called after the CreateScene and Initialize methods and before the first Update.
        }
    }
}
