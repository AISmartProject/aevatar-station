﻿namespace Aevatar.WebHook.Deploy;

public class DefaultHostDeployManager : IHostDeployManager
{
    public async Task<string> CreateNewWebHookAsync(string appId, string version, string imageName)
    {
        return string.Empty;
    }

    public async Task DestroyWebHookAsync(string appId, string version)
    {
        return;
    }

    public async Task RestartWebHookAsync(string appId, string version)
    {
        return;
    }

    public async Task<string> CreateHostAsync(string appId, string version, List<string> corsUrls)
    {
        return string.Empty;
    }

    public async Task DestroyHostAsync(string appId, string version)
    {
        return;
    }

    public async Task RestartHostAsync(string appId, string version)
    {
        return;
    }
}