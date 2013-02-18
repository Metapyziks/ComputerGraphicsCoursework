using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphicsCoursework
{
    public class CubeMapTexture : Texture
    {
        public Bitmap Top { get; private set; }
        public Bitmap Left { get; private set; }
    
        protected override void Load()
        {
 	        throw new NotImplementedException();
        }
    }
}
