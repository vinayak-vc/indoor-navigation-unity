using IndoorNavigation.Core.Models;

namespace IndoorNavigation.Core.Interfaces
{
    public interface IPathRenderer
    {
        void RenderPath(NavigationPath path, int startCornerIndex);
        void Clear();
    }
}
