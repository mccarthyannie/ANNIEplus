namespace AnniePlus.Services
{
    public interface ILoginService
    {
        Task LoginAsync(string token);

        Task LogoutAsync();

    }
}
