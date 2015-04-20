#region Using Statements
using System;
using System.Diagnostics;
using EyedProject.Components;
using WaveEngine.Common;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Cameras;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Components.Animation;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics2D;
using WaveEngine.Framework.Resources;
using WaveEngine.Framework.Services;
using WaveEngine.ImageEffects;
using WaveEngine.TiledMap;
using System.Collections.Generic;
#endregion

namespace EyedProject
{
    public class GameScene : Scene
    {
        private TiledMap tiledMap;
        private SoundManager soundManager;
        private List<RectangleF> debugRects;

        public FishEyeLens FishLens;
        protected override void CreateScene()
        {
            debugRects = new List<RectangleF>();
            CreateCamera();
            CreateTiledMap();
            CreateSoundManager();
            if (WaveServices.ScreenContextManager.CurrentContext.Name == "ThreePlayers")
            {
                Game.PLAYER_COUNT = 3;
            }
        }

        protected override void Start()
        {
            base.Start();

            AddSceneColliders();

            CreatePlayers();

            AddSceneBehavior(new GameplaySceneBehavior(), SceneBehavior.Order.PostUpdate);
        }


        private void CreateCamera()
        {
            var camera2D = new FixedCamera2D("Camera2D") { BackgroundColor = Color.LightBlue };
            if (WaveServices.Platform.PlatformType == PlatformType.Windows ||
                WaveServices.Platform.PlatformType == PlatformType.Linux ||
                WaveServices.Platform.PlatformType == PlatformType.MacOS)
            {
                //camera2D.Entity.AddComponent(this.FishLens = ImageEffects.FishEye());
                //camera2D.Entity.AddComponent(new ChromaticAberrationLens() { AberrationStrength = 5.5f });
                //camera2D.Entity.AddComponent(new RadialBlurLens() { Center = new Vector2(0.5f, 0.75f), BlurWidth = 0.02f, Nsamples = 5 });
                camera2D.Entity.AddComponent(ImageEffects.Vignette());
                camera2D.Entity.AddComponent(new FilmGrainLens() { GrainIntensityMin = 0.075f, GrainIntensityMax = 0.15f });
            }
            EntityManager.Add(camera2D);
        }

        private void CreateTiledMap()
        {
            this.tiledMap = new TiledMap("Content/test.tmx");
            var map = new Entity("map")
                .AddComponent(new Transform2D())
                .AddComponent(this.tiledMap);

            this.EntityManager.Add(map);
        }

        private void CreateSoundManager()
        {
            Entity sound = new Entity("soundManager")
                .AddComponent(this.soundManager = new SoundManager());

            this.EntityManager.Add(sound);
        }

        private Entity CreateEye(int index)
        {
            var eye = new Entity("eye" + index)
                { 
                    Tag = Game.TAG_EYE
                }
                .AddComponent(new Transform2D()
                {
                    Origin = Vector2.Center
                })
                .AddComponent(new Sprite("Content/Animations/eye.wpk"))
                .AddComponent(Animation2D.Create<TexturePackerGenericXml>("Content/Animations/eye.xml")
                    .Add(Game.ANIMATION_RIGHT, new SpriteSheetAnimationSequence()
                    {
                        First = 1 + index * Game.EYE_ANIMATION_COUNT,
                        Length = 1
                    })
                    .Add(Game.ANIMATION_TURNING, new SpriteSheetAnimationSequence()
                    {
                        First = 1 + index * Game.EYE_ANIMATION_COUNT,
                        Length = 11,
                        FramesPerSecond = 50
                    })
                    .Add(Game.ANIMATION_LEFT, new SpriteSheetAnimationSequence()
                    {
                        First = 11 + index * Game.EYE_ANIMATION_COUNT,
                        Length = 1
                    })
                    .Add(Game.ANIMATION_HOT, new SpriteSheetAnimationSequence()
                    {
                        First = 12 + index * Game.EYE_ANIMATION_COUNT,
                        Length = 1
                    })
                )
                .AddComponent(new AnimatedSpriteRenderer())
                .AddComponent(new CircleCollider())
                .AddComponent(new RigidBody2D()
                {
                    Restitution = Game.EYE_RESTITUTION
                })
                .AddComponent(new JointMap2D())
                .AddComponent(new EyeController());
            EntityManager.Add(eye);

            return eye;
        }

        private void CreatePlayers()
        {
            var playerObjects = this.tiledMap.ObjectLayers["players"].Objects;
            for (int i = 0; i < Game.PLAYER_COUNT; i++)
            {
                var eye = CreateEye(i);

                var player = new Entity("player" + i)
                    .AddComponent(new Transform2D()
                    {
                        Position = new Vector2(playerObjects[i].X + Game.FIELD_SIZE / 2, playerObjects[i].Y),
                        Origin = new Vector2(0.5f, 1)
                    })
                    .AddComponent(new Sprite("Content/Animations/body.wpk"))
                .AddComponent(Animation2D.Create<TexturePackerGenericXml>("Content/Animations/body.xml")
                    .Add(Game.PLAYER_SMALL_WALKING, new SpriteSheetAnimationSequence()
                    {
                        First = 1, // + i * Game.EYE_ANIMATION_COUNT,
                        Length = 10,
                        FramesPerSecond = 30
                    })
                    .Add(Game.PLAYER_GROWING, new SpriteSheetAnimationSequence()
                    {
                        First = 11, // + index * Game.EYE_ANIMATION_COUNT,
                        Length = 8,
                        FramesPerSecond = 30
                    })
                    .Add(Game.PLAYER_BIG_WALKING, new SpriteSheetAnimationSequence()
                    {
                        First = 19,  //+ index * Game.EYE_ANIMATION_COUNT,
                        Length = 12,
                        FramesPerSecond = 30
                    })
                    .Add(Game.PLAYER_SHRINKING, new SpriteSheetAnimationSequence()
                    {
                        First = 31, // + index * Game.EYE_ANIMATION_COUNT,
                        Length = 8,
                        FramesPerSecond = 30
                    })
                )
                .AddComponent(new AnimatedSpriteRenderer())
                    .AddComponent(new RectangleCollider())
                    .AddComponent(new RigidBody2D()
                    {
                        FixedRotation = true,
                        Mass = Game.BODY_MASS,
                        Friction = Game.BODY_FRICTION,
                        Restitution = Game.BODY_RESTITUTION
                    })
                    .AddComponent(new PlayerController(i));

                EntityManager.Add(player);
                player.FindComponent<PlayerController>().AttachEye(eye);
            }
        }

        /// <summary>
        /// Add Collision entities from TMX tile layer.
        /// 
        /// Connect horizontally as many tiles together as one object as possible.
        /// When separate walls, the player gets stuck :(
        /// </summary>
        private void AddSceneColliders()
        {
            var collisionLayer = this.tiledMap.TileLayers[Game.FIRST_LAYER];

            int y = 0;
            int x = 0;
            int currentWallHeight = 0;
            int currentWallLength = 0;
            int prevWallHeight = 0;
            int prevWallLength = 0;
            var temp = new List<int>();

            foreach (var obj in collisionLayer.Tiles)
            {
                if (obj.X == temp.Count)
                    temp.Add(0);
                if (y != obj.Y)
                {
                    if (prevWallLength > 0)
                    {
                        AddWall(x - prevWallLength + 1, y - prevWallHeight, prevWallLength, prevWallHeight);
                        prevWallHeight = prevWallLength = 0;
                    }
                    currentWallHeight = currentWallLength = 0;
                }
                else
                {
                    if (prevWallLength > 0)
                    {
                        if (temp[obj.X] != 0)
                            prevWallLength++;
                        else // add previous wall
                        {
                            AddWall(obj.X - prevWallLength, obj.Y - prevWallHeight, prevWallLength, prevWallHeight);
                            prevWallHeight = prevWallLength = 0;
                        }
                    }
                }

                // this is wall
                if (obj.TilesetTile != null &&
                    obj.TilesetTile.Properties.ContainsKey(Game.CLASS_PROPERTY) &&
                    obj.TilesetTile.Properties[Game.CLASS_PROPERTY] == Game.WALL_CLASS)
                {
                    if (currentWallLength > 0) // wall continues
                    {
                        if (temp[obj.X] == 0 && currentWallHeight > 1) // connected top wall finished
                        {
                            AddWall(obj.X - currentWallLength, obj.Y - currentWallHeight + 1, currentWallLength, currentWallHeight - 1);
                            for (int i = 1; i <= currentWallLength; i++)
                                temp[obj.X - i] = 1;
                            currentWallHeight = 1;
                        }
                        if (temp[obj.X] != 0 && currentWallHeight == 1 && prevWallLength == 0)
                        {
                            prevWallLength = 1;
                            prevWallHeight = temp[obj.X];
                        }
                        currentWallLength++;
                    }
                    else // wall starting here
                    {
                        currentWallHeight = 1 + (prevWallLength > 0 ? 0 : temp[obj.X]);
                        currentWallLength = 1;
                    }
                }
                else // no wall here
                {
                    if ((temp[obj.X] != 0 &&
                        (currentWallHeight > 1 || obj.X == 0)) ||
                            temp[obj.X] != 0 && prevWallLength == 0) // wall connected to previous
                    {
                        prevWallLength = currentWallHeight > 1 ? currentWallLength + 1 : 1;
                        prevWallHeight = temp[obj.X];
                        for (int i = 1; i <= currentWallLength; i++)
                            temp[obj.X - i] = 1;
                    }    
                    currentWallLength = currentWallHeight = 0;
                }

                y = obj.Y;
                x = obj.X;
                temp[obj.X] = currentWallHeight;
            }

            if (prevWallLength > 0) // add the last wall of the row before the last one
                AddWall(x - prevWallLength + 1, y - prevWallHeight, prevWallLength, prevWallHeight);

            currentWallLength = currentWallHeight = 0;
            for (int i = 0; i < temp.Count; i++)
            {
                if (temp[i] != 0)
                {
                    if (currentWallLength == 0)
                        currentWallHeight = temp[i];
                    currentWallLength++;
                }

                if (temp[i] == 0 || i == temp.Count - 1)
                {
                    if (currentWallLength > 0)
                    {
                        AddWall(i - currentWallLength + (i == temp.Count - 1 ? 1 : 0), y, currentWallLength, currentWallHeight);
                        currentWallHeight = currentWallLength = 0;
                    }
                }
            }

            // create walls around the whole map
            AddWall(-1, -1, x + 3, 1);
            AddWall(-1, 0, 1, y + 1);
            AddWall(x + 1, 0, 1, y + 1);
            AddWall(-1, y + 1, x + 3, 1);

            //Entity debug = new Entity("debugRects")
            //    .AddComponent(new DebugRects(debugRects));
            //EntityManager.Add(debug);
        }

        private void AddWall(int x, int y, int width, int height)
        {
            float drawCoef = Game.FIELD_SIZE / 2;
            debugRects.Add(new RectangleF(x * drawCoef, y * drawCoef, width * drawCoef, height * drawCoef));
            var colliderEntity = new Entity(String.Format("{0}_{1}_{2}", Game.WALL_CLASS, x, y))
                .AddComponent(new Transform2D()
                {
                    Origin = Vector2.Zero,
                    LocalPosition = new Vector2(x * Game.FIELD_SIZE, y * Game.FIELD_SIZE),
                    Rectangle = new RectangleF(0, 0, width * Game.FIELD_SIZE, height * Game.FIELD_SIZE),
                })
                .AddComponent(new RectangleCollider())
                .AddComponent(new RigidBody2D()
                {
                    PhysicBodyType = PhysicBodyType.Static
                });
            colliderEntity.Tag = Game.WALL_CLASS;


            this.EntityManager.Add(colliderEntity);
        }
    }
}
