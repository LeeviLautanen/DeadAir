using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Utility
{
    public static async Task<List<T>> LoadAllByLabel<T>(string label) where T : ScriptableObject
    {
        var handle = Addressables.LoadAssetsAsync<T>(label, null);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
            return null;

        List<T> list = new(handle.Result);
        return list;
    }
}
