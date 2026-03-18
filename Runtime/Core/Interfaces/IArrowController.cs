using IndoorNavigation.Core.Models;

namespace IndoorNavigation.Core.Interfaces {
    public interface IArrowController {
        void RenderArrows(NavigationPath path, int startCornerIndex);
        void Clear();
    }
}