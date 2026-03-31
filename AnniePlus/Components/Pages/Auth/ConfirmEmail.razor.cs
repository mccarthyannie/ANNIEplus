using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Gantt;

namespace AnniePlus.Components.Pages.Auth
{
    public partial class ConfirmEmail
    {
        [Inject] private NavigationManager Nav { get; set; } = null!;
        [Inject] private HttpClient HttpClient { get; set; } = null!;

        [Parameter, SupplyParameterFromQuery] public string UserId { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string Token { get; set; } = string.Empty;

        private string? message;

        protected async Task ConfirmAccountAsync()
        {
            var response = await HttpClient.GetAsync($"/api/auth/ConfirmEmail/?userId={UserId}&token={Token}");

            if (!response.IsSuccessStatusCode)
            {
                message = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Email Confirmation Error:\n" + message);
                Nav.NavigateTo("/");
                return;
            }

            Nav.NavigateTo("/login");
        }
    }
}