using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Input;
using WaveEngine.Common.Math;
using WaveEngine.Components.Animation;
using WaveEngine.Framework;
using WaveEngine.Framework.Diagnostic;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics2D;
using WaveEngine.Framework.Services;
using WaveEngine.Physics;

namespace EyedProject.Components
{
    public enum EyeAnimationState
    {
        None,
        Left,
        TurningLeft,
        Right,
        TurningRight
    }

    public enum PlayerAnimationState
    {
        SmallWalking,
        Growing,
        BigWalking,
        Shrinking
    }

    public class PlayerController : Behavior
    {
        private const float SideImpulse = 30f;
        private const float JumpImpulse = 10f;
        private const float MaxSideFloorVelocity = 7f;
        private const float SideShootVelocity = 6f;
        private const float VerticalShootVelocity = 2f;
        private const float MaxSideAirVelocity = MaxSideFloorVelocity * 0.7f;
        private const int JumpDelay = 70; // ms after the last touch of floor when player can still jump
        private const int ShotProtection = 300; // ms after shot to protect player from his own projectile
        private const int SpawnProtection = 2000;
        private const int AnimationTransition = 700;
        public static Vector2 RightShootVelocity = new Vector2(SideShootVelocity, -VerticalShootVelocity);
        public static Vector2 LeftShootVelocity = new Vector2(-SideShootVelocity, -VerticalShootVelocity);
        public static Vector2 EyePosition = new Vector2(0, -50f);

        private const int LeftFrame = 1;
        private const int RightFrame = 11;

        private Input input;
        private bool isJump;
        private bool isMoving;
        private int jumpRemaining = 0;
        private int protectionRemaining = 0;
        private int animationRemaining = 0;
        private EyeAnimationState animState;
        private PlayerAnimationState playerAnimState;
        private Vector2 initPosition;
        private int index = 0;
        private HashSet<RigidBody2D> collidingBodies;
        private Dictionary<PlayerControls, GetButtonStateDelegate> controls;

        private SoundManager soundManager;
        private ViewportManager vm;
        private Entity eye;


        private bool OnFloor
        {
            get { return this.collidingBodies.Count > 0; }
        }

        private bool WasOnFloor
        {
            get { return OnFloor || jumpRemaining > 0; }
        }

        private bool IsProtected
        {
            get { return protectionRemaining > 0; }
        }

        [RequiredComponent]
        public RigidBody2D RigidBody;

        [RequiredComponent]
        public Transform2D Transform2D;

        [RequiredComponent(false)]
        public Collider2D Collider;

        [RequiredComponent]
        public Animation2D Animation;

        public PlayerController(int i)
        {
            index = i;
        }

        protected override void Initialize()
        {
            base.Initialize();

            soundManager = EntityManager.Find("soundManager").FindComponent<SoundManager>();
            vm = WaveServices.ViewportManager;
            collidingBodies = new HashSet<RigidBody2D>();


            input = WaveServices.Input;
            controls = KeyboardControls.GetPlayerControls(index);
            initPosition = Transform2D.Position;

            RigidBody.OnPhysic2DCollision += OnPlayerCollision;
            RigidBody.OnPhysic2DSeparation += OnPlayerSeparation;
            animState = EyeAnimationState.None;
            playerAnimState = PlayerAnimationState.SmallWalking;
            Animation.CurrentAnimation = Game.PLAYER_SMALL_WALKING;
            Animation.Play(true);
            isMoving = false;
        }

        public void Reset()
        {
            RigidBody.ResetPosition(this.initPosition);
            RigidBody.Rotation = 0;
            if (HasEye())
                ResetEye();
            collidingBodies.Clear();
        }

        protected override void Update(TimeSpan gameTime)
        {
            HandleKeys(gameTime);
            
            if (jumpRemaining > 0)
            {
                jumpRemaining -= gameTime.Milliseconds;
            }

            if (protectionRemaining > 0)
            {
                protectionRemaining -= gameTime.Milliseconds;
            }

            if (animationRemaining > 0)
            {
                animationRemaining -= gameTime.Milliseconds;
            }
            else if (animationRemaining <= 0)
            {
                if (playerAnimState == PlayerAnimationState.Growing)
                {
                    playerAnimState = PlayerAnimationState.BigWalking;
                    Animation.CurrentAnimation = Game.PLAYER_BIG_WALKING;
                    Animation.Play(true);
                }
                else if (playerAnimState == PlayerAnimationState.Shrinking)
                {
                    playerAnimState = PlayerAnimationState.SmallWalking;
                    Animation.CurrentAnimation = Game.PLAYER_SMALL_WALKING;
                    Animation.Play(true);
                }
            }
        }

        private void HandleKeys(TimeSpan gameTime)
        {
            if (this.input.KeyboardState.IsConnected)
            {
                var keyState = input.KeyboardState;

                if (controls[PlayerControls.RIGHT](keyState) == ButtonState.Pressed)
                {
                    MoveRight(gameTime);
                }
                else if (controls[PlayerControls.LEFT](keyState) == ButtonState.Pressed)
                {
                    MoveLeft(gameTime);
                } 
                else
                {
                    Break(gameTime);
                }

                if (controls[PlayerControls.UP](keyState) == ButtonState.Pressed)
                {
                    Jump();
                    isJump = true;
                }
                else
                {
                    isJump = false;
                }

                if (controls[PlayerControls.SHOOT](keyState) == ButtonState.Pressed)
                {
                    Shoot();
                }
            }
        }
        private void Shoot()
        {
            if (HasEye())
            {
                bool isLeft = IsFacingLeft();
                RigidBody2D eyeBody = DetachEye().FindComponent<RigidBody2D>();
                eyeBody.LinearVelocity *= 3;
                eyeBody.LinearVelocity += isLeft ? LeftShootVelocity : RightShootVelocity;
                protectionRemaining = ShotProtection;
                
                playerAnimState = PlayerAnimationState.Growing;
                Animation.CurrentAnimation = Game.PLAYER_GROWING;
                Animation.Play();
                animationRemaining = AnimationTransition;
            }
        }

        public void Die()
        {
            // notify scene controller
            protectionRemaining = SpawnProtection;
            Reset();
        }

        #region MOVEMENT
        private bool IsFacingLeft()
        {
            return animState == EyeAnimationState.Left || animState == EyeAnimationState.TurningLeft;
        }

        private void Jump()
        {
            if (!isJump && WasOnFloor)
            {
                RigidBody.LinearVelocity = new Vector2(RigidBody.LinearVelocity.X, RigidBody.LinearVelocity.Y - JumpImpulse);
                //this.soundManager.PlaySound(SoundType.Jump);
            }
        }

        private void LastTouch()
        {
            if (!OnFloor)
                jumpRemaining = JumpDelay;
        }

        private void MoveLeft(TimeSpan gameTime)
        {
            Move(gameTime, -1);
            TurnLeft();
        }

        private void MoveRight(TimeSpan gameTime)
        {
            Move(gameTime, 1);
            TurnRight();
        }

        private void Move(TimeSpan gameTime, int direction)
        {
            if (!isMoving)
            {
                isMoving = true;
                if (playerAnimState == PlayerAnimationState.BigWalking || playerAnimState == PlayerAnimationState.SmallWalking)
                    Animation.Play(true);
            }
            RigidBody.LinearVelocity = new Vector2(RigidBody.LinearVelocity.X + (direction * SideImpulse * gameTime.Milliseconds / 1000f),
                RigidBody.LinearVelocity.Y);
            float maxVel = OnFloor ? MaxSideFloorVelocity : MaxSideAirVelocity;
            if (Math.Abs(RigidBody.LinearVelocity.X) > maxVel)
                RigidBody.LinearVelocity = new Vector2(direction * maxVel, RigidBody.LinearVelocity.Y);
        }

        private void Break(TimeSpan gameTime)
        {
            if (false && isMoving) // remove?
            {
                isMoving = false;
                if (playerAnimState == PlayerAnimationState.BigWalking || playerAnimState == PlayerAnimationState.SmallWalking)
                    Animation.Stop();
            }

            if (RigidBody.LinearVelocity.X == 0)
                return;
            int direction = RigidBody.LinearVelocity.X > 0 ? 1 : -1;
            RigidBody.LinearVelocity = new Vector2(RigidBody.LinearVelocity.X - (direction * SideImpulse / 2 * gameTime.Milliseconds / 1000f),
                RigidBody.LinearVelocity.Y);
            int dir2 = RigidBody.LinearVelocity.X > 0 ? 1 : -1;
            if (direction != dir2) // if we crossed zero, keep zero
                RigidBody.LinearVelocity = Vector2.UnitY * RigidBody.LinearVelocity.Y;
        }

        private void TurnLeft()
        {
            if (HasEye() && animState != EyeAnimationState.Left)
            {
                Animation2D eyeAnimation = eye.FindComponent<Animation2D>();
                Transform2D eyeTransform = eye.FindComponent<Transform2D>();
                eyeAnimation.CurrentAnimation = Game.ANIMATION_TURNING;
                eyeTransform.Effect = SpriteEffects.None;
                eyeAnimation.Play();
                eyeTransform.X -= 10;
                animState = EyeAnimationState.Left;
            }
        }

        private void TurnRight()
        {
            if (HasEye() && animState != EyeAnimationState.Right)
            {
                Animation2D eyeAnimation = eye.FindComponent<Animation2D>();
                Transform2D eyeTransform = eye.FindComponent<Transform2D>();
                eyeTransform.X += 10;
                eyeAnimation.CurrentAnimation = Game.ANIMATION_TURNING;
                eyeTransform.Effect = SpriteEffects.FlipHorizontally;
                eyeAnimation.Play();
                animState = EyeAnimationState.Right;
            }
        }
        #endregion


        #region EYE HANDLING
        private bool HasEye()
        {
            return eye != null;
        }

        public Entity DetachEye()
        {
            JointMap2D jointMap = eye.FindComponent<JointMap2D>();
            jointMap.ClearJoints();
            Entity lostEye = eye;
            animState = EyeAnimationState.None;
            eye = null;
            lostEye.FindComponent<EyeController>().Detach();
            return lostEye;
        }

        public void AttachEye(Entity newEye)
        {
            eye = newEye;
            eye.FindComponent<EyeController>().Attach(this);
            Transform2D trans = eye.FindComponent<Transform2D>();
            trans.Position = this.Transform2D.Position + EyePosition;
            RigidBody2D body = eye.FindComponent<RigidBody2D>();
            body.ResetPosition(trans.Position);
            body.Rotation = 0;
            body.LinearVelocity = RigidBody.LinearVelocity;
            eye.FindComponent<JointMap2D>()
                .AddJoint("playerEyeDistJoint" + index, new DistanceJoint2D(Owner, Vector2.Zero, new Vector2(0, -55f))
                {
                    Lenght = 5f
                })
                .AddJoint("playerEyeAngleJoint" + index, new AngleJoint2D(Owner));
        }

        private void ResetEye()
        {
            RigidBody2D body = eye.FindComponent<RigidBody2D>();
            body.ResetPosition(initPosition + EyePosition);
        }

        #endregion

        private void OnPlayerCollision(object sender, Physic2DCollisionEventArgs args)
        {
            RigidBody2D other = args.Body2DA == RigidBody ? args.Body2DB : args.Body2DA;
            if (other.Owner.Tag == Game.TAG_EYE)
            {
                EyeController eye = other.Owner.FindComponent<EyeController>();
                if (eye.IsHot && !IsProtected)
                {
                    Die();
                }
                else if (eye.IsCool && !HasEye())
                {
                    AttachEye(other.Owner);
                    Animation.CurrentAnimation = Game.PLAYER_SHRINKING;
                    Animation.Play();
                    animationRemaining = AnimationTransition;
                    playerAnimState = PlayerAnimationState.Shrinking;
                }
            }
            else if (args.PointA.HasValue && args.PointB.HasValue &&
                args.PointA.Value.Y == args.PointB.Value.Y &&
                args.PointA.Value.X != args.PointB.Value.X)
            {
                ;//this.soundManager.PlaySound(SoundType.Contact);
                collidingBodies.Add(other);
            }
        }

        private void OnPlayerSeparation(object sender, Physic2DSeparationEventArgs args)
        {
            RigidBody2D other = args.Body2DA == RigidBody ? args.Body2DB : args.Body2DA;
            if (collidingBodies.Contains(other))
            {
                collidingBodies.Remove(other);
            }

            LastTouch();
        }
    }
}