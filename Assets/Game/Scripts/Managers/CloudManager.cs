using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

public class CloudManager : MonoBehaviour
{
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    internal static async Task SendToCloud(string key, object value)
    {
        var data = new Dictionary<string, object> { { key, value } };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    internal static async Task<T> LoadFromCloud<T>(string key)
    {
        var query = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

        return query.TryGetValue(key, out var value) ? Deserialize<T>(value.Value.GetAsString()) : default;
    }

    private static T Deserialize<T>(string input)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)input;

        return JsonConvert.DeserializeObject<T>(input);
    }
}
