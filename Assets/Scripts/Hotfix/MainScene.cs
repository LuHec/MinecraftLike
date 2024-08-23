using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class MainScene : MonoBehaviour
{
    public static void Run()
    {
        Debug.Log("Hello, World!");

        var package = YooAssets.GetPackage("DefaultPackage");

        using AssetHandle cubeHandle = package.LoadAssetSync<GameObject>("Cube");
        if (cubeHandle.IsDone)
        {
            var obj = Instantiate(cubeHandle.AssetObject);
            (obj as GameObject)?.AddComponent<Round>();
        }

        AssetHandle sphereHandle = package.LoadAssetAsync<GameObject>("Sphere");
        sphereHandle.Completed += OnHandleComplete;
    }


    private static void OnHandleComplete(AssetHandle handle)
    {
        if (handle.IsDone)
        {
            Instantiate(handle.AssetObject);
        }
    }
}