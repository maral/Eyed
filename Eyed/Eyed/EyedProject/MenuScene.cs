using WaveEngine.Common.Graphics;
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
            var camera2D = new FixedCamera2D("Camera2D") { BackgroundColor = Color.Black };
            EntityManager.Add(camera2D);

            int offset = 100;
            var title = new Entity("Title")
                               .AddComponent(new Sprite("Content/Texture/title.wpk"))
                               .AddComponent(new SpriteRenderer(DefaultLayers.Alpha))
                               .AddComponent(new Transform2D()
                               {
                                   Y = WaveServices.Platform.ScreenHeight / 2 - offset,
                                   X = WaveServices.Platform.ScreenWidth / 2 - 150
                               });
            EntityManager.Add(title);
        }

        protected override void Start()
        {
            base.Start();

            // This method is called after the CreateScene and Initialize methods and before the first Update.
        }
    }
}
