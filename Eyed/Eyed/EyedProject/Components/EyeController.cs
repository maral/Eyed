using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Components.Animation;
using WaveEngine.Framework;
using WaveEngine.Framework.Physics2D;

namespace EyedProject.Components
{
    public enum EyeState
    {
        Attached, Hot, Cool
    }

    class EyeController : Behavior
    {
        private const float CoolSpeedSquared = 0.09f;

        private PlayerController player;

        [RequiredComponent]
        public RigidBody2D RigidBody;

        [RequiredComponent]
        public Animation2D Animation;

        public EyeState State { get; set; }
        public bool IsAttached { get { return State == EyeState.Attached; } }
        public bool IsHot { get { return State == EyeState.Hot; } }
        public bool IsCool { get { return State == EyeState.Cool; } }

        protected override void Initialize()
        {
            base.Initialize();
            State = EyeState.Cool;

            RigidBody.OnPhysic2DCollision += OnEyeCollision;
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (IsHot)
            {
                if (RigidBody.LinearVelocity.LengthSquared() < CoolSpeedSquared)
                {
                    State = EyeState.Cool;
                    Animation.CurrentAnimation = Game.ANIMATION_RIGHT;
                    Animation.Play();
                }
            }
        }


        public void Attach(PlayerController player)
        {
            State = EyeState.Attached;
            this.player = player;
        }



        public void Detach()
        {
            State = EyeState.Hot;
            Animation.CurrentAnimation = Game.ANIMATION_HOT;
            Animation.Play();
            this.player = null;
        }

        private void OnEyeCollision(object sender, Physic2DCollisionEventArgs args)
        {
            RigidBody2D other = args.Body2DA == RigidBody ? args.Body2DB : args.Body2DA;
            if (other.Owner.Tag == Game.TAG_EYE)
            {
                EyeController eye = other.Owner.FindComponent<EyeController>();
                if (eye.IsHot && IsAttached)
                {
                    player.Die();
                }
            }
        }
    }
}
