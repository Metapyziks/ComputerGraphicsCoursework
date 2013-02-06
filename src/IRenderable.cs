using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphicsCoursework
{
    interface IRenderable<T>
        where T : ShaderProgram
    {
        void Render(T shader);
    }
}
