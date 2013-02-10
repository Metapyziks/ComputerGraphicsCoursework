using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphicsCoursework
{
    interface IUpdateable
    {
        void Update(double time, Water water);
    }
}
