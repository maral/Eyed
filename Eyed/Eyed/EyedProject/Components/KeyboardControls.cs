using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Input;

namespace EyedProject.Components
{
    public enum PlayerControls {
        UP, LEFT, RIGHT, SHOOT
    }

    public delegate ButtonState GetButtonStateDelegate(KeyboardState keyboardState);

    class KeyboardControls
    {
        public static Dictionary<PlayerControls, GetButtonStateDelegate> GetPlayerControls(int index)
        {
            Dictionary<PlayerControls, GetButtonStateDelegate> dict = new Dictionary<PlayerControls, GetButtonStateDelegate>();

            switch (index)
            {
                case 0:
                    dict[PlayerControls.LEFT]   = new GetButtonStateDelegate((KeyboardState s) => s.Left);
                    dict[PlayerControls.RIGHT]  = new GetButtonStateDelegate((KeyboardState s) => s.Right);
                    dict[PlayerControls.UP]     = new GetButtonStateDelegate((KeyboardState s) => s.Up);
                    dict[PlayerControls.SHOOT]  = new GetButtonStateDelegate((KeyboardState s) => s.RightControl);
                    break;
                case 1:
                    dict[PlayerControls.LEFT]   = new GetButtonStateDelegate((KeyboardState s) => s.A);
                    dict[PlayerControls.RIGHT]  = new GetButtonStateDelegate((KeyboardState s) => s.D);
                    dict[PlayerControls.UP]     = new GetButtonStateDelegate((KeyboardState s) => s.W);
                    dict[PlayerControls.SHOOT]  = new GetButtonStateDelegate((KeyboardState s) => s.Tab);
                    break;
                case 2:
                    dict[PlayerControls.LEFT]   = new GetButtonStateDelegate((KeyboardState s) => s.J);
                    dict[PlayerControls.RIGHT]  = new GetButtonStateDelegate((KeyboardState s) => s.L);
                    dict[PlayerControls.UP]     = new GetButtonStateDelegate((KeyboardState s) => s.I);
                    dict[PlayerControls.SHOOT]  = new GetButtonStateDelegate((KeyboardState s) => s.Space);
                    break;
            }
            return dict;
        }
    }
}
