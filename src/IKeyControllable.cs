using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Input;

namespace ComputerGraphicsCoursework
{
    interface IKeyControllable
    {
        void KeyDown(Key key);
        void KeyUp(Key key);

        void UpdateKeys(KeyboardDevice keyboard);
    }
}
