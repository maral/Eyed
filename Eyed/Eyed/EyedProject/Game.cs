#region Using Statements
using System;
using WaveEngine.Common;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
#endregion

namespace EyedProject
{
    public class Game : WaveEngine.Framework.Game
    {
        #region CONSTANTS
        public static int FIELD_SIZE = 80;
        public static int WIDTH = 1280;
        public static int HEIGHT = 960;
        public static int PLAYER_COUNT = 2;
        public static float BODY_MASS = 1f;
        public static float BODY_FRICTION = 0.02f;
        public static float BODY_RESTITUTION = 0.1f;
        public static float EYE_RESTITUTION = 0.5f;
        public static int EYE_ANIMATION_COUNT = 12;

        public static string CLASS_PROPERTY = "class";
        public static string WALL_CLASS = "wall";
        public static string TAG_EYE = "eye";
        public static string FIRST_LAYER = "layer1";

        public static string ANIMATION_TURNING = "turning";
        public static string ANIMATION_LEFT = "left";
        public static string ANIMATION_RIGHT = "right";
        public static string ANIMATION_HOT = "hot";

        public static string PLAYER_SMALL_WALKING = "small_walking";
        public static string PLAYER_GROWING = "growing";
        public static string PLAYER_BIG_WALKING = "big_walking";
        public static string PLAYER_SHRINKING = "shrinking";
        #endregion


        public override void Initialize(IApplication application)
        {
            base.Initialize(application);

            // ViewportManager is used to automatically adapt resolution to fit screen size
            ViewportManager vm = WaveServices.ViewportManager;
            vm.Activate(WIDTH, HEIGHT, ViewportManager.StretchMode.Uniform);

            ScreenContext screenContext = new ScreenContext(new MenuScene());
            WaveServices.ScreenContextManager.To(screenContext);
        }
    }
}
