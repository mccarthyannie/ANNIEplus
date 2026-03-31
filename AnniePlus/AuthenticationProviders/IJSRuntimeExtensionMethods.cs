using Microsoft.JSInterop;

namespace AnniePlus.AuthenticationProviders
{
    public static class IJSRuntimeExtensionMethods
    {
        public static ValueTask<object> SetLocalStorage(this IJSRuntime js, string key, string content) 
        { 
            return js.InvokeAsync<object>("localStorage.setItem", key, content);
        }

        public static ValueTask<string> GetLocalStorage(this IJSRuntime js, string key) 
        { 
            return js.InvokeAsync<string>("localStorage.getItem", key);
        }

        public static ValueTask<object> RemoveLocalStorage(this IJSRuntime js, string key) 
        { 
            return js.InvokeAsync<object>("localStorage.removeItem", key);
        }
    }
}
