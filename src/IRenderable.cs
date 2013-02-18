using ComputerGraphicsCoursework.Shaders;

namespace ComputerGraphicsCoursework
{
    interface IRenderable<T>
        where T : ShaderProgram
    {
        void Render(T shader);
    }
}
