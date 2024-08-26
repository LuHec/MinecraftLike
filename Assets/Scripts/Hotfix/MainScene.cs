using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

public class MainScene : MonoBehaviour
{
    public static async void Run()
    {
        Debug.Log("Hello, World!");
        
        var package = YooAssets.GetPackage("ResourcePackage");

        using AssetHandle cubeHandle = package.LoadAssetSync<GameObject>("Cube");
        if (cubeHandle.IsDone)
        {
            var obj = Instantiate(cubeHandle.AssetObject);
            (obj as GameObject)?.AddComponent<Round>();
        }

        AssetHandle sphereHandle = package.LoadAssetAsync<GameObject>("Sphere");
        sphereHandle.Completed += OnHandleComplete;
        
        float timer = 0;
        while (timer < 5.0f)
        {
            timer += Time.deltaTime;
            await UniTask.Yield();
        }

        
        await package.LoadSceneSync("Root");
    }


    private static void OnHandleComplete(AssetHandle handle)
    {
        if (handle.IsDone)
        {
            Instantiate(handle.AssetObject);
        }
    }
}