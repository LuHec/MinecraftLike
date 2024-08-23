using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using YooAsset;
using Object = UnityEngine.Object;

public class LoadDll : MonoBehaviour
{
    IEnumerator Start()
    {
        // 首先初始化YooAsset
        YooAssets.Initialize();

        // 然后创建已有包
        YooAssets.CreatePackage("DefaultPackage");

        var package = YooAssets.GetPackage("DefaultPackage");
        YooAssets.SetDefaultPackage(package);
#if !UNITY_EDITOR
        yield return InitializeYooAsset(package);
#else
        yield return EditorInitializeYooAsset(package);
#endif


        // Editor环境下，HotUpdate.dll.bytes已经被自动加载，不需要加载，重复加载反而会出问题。
#if !UNITY_EDITOR
        Assembly hotUpdateAss =
 Assembly.Load(File.ReadAllBytes($"{Application.streamingAssetsPath}/HotUpdate.dll.bytes"));
#else
        // Editor下无需加载，直接查找获得HotUpdate程序集
        Assembly hotUpdateAss =
            System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
#endif

        Type type = hotUpdateAss.GetType("MainScene");
        type.GetMethod("Run").Invoke(null, null);

        GameObject obj = new GameObject();
        
        float timer = 0;
        while (timer < 5.0f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        
        package.LoadSceneSync("Root");
    }

    private IEnumerator InitializeYooAsset(ResourcePackage package)
    {
        var initParameters = new OfflinePlayModeParameters();
        yield return package.InitializeAsync(initParameters);
    }

    private IEnumerator EditorInitializeYooAsset(ResourcePackage package)
    {
        var initParameters = new EditorSimulateModeParameters();
        initParameters.SimulateManifestFilePath =
            EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline, "DefaultPackage");
        yield return package.InitializeAsync(initParameters);
    }
}