using AnniePlus.Services;
using Microsoft.AspNetCore.Components;

namespace AnniePlus.Components.Pages.Auth
{
    public partial class Logout
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private ILoginService LoginService { get; set; } = default!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            await LoginService.LogoutAsync();
            NavigationManager.NavigateTo("/");
        }
    }
}