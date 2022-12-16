using Microsoft.AspNetCore.Components;

namespace RankOverlay.Web.BlazorWasm.Client.Components;

public interface IRequireNavigationManager
{
    void SetNavigateionManager(NavigationManager navigationManager);
}
