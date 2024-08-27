using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HybridCLR;
using Cysharp.Threading.Tasks;
using YooAsset;


//注意这个脚本是Main程序集中的脚本（你可以创建一个Main程序集，在Main程序集的文件夹下创建这个脚本）
public class LoadDllWithYooAsset : MonoBehaviour

{
    // 资源系统运行模式
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    //CDN地址或这里可以写你的hfs创建的本地链接地址

    public string DefaultHostServer = "http://192.168.255.10/Pak/"; //默认的CDN地址

    public string FallbackHostServer = "http://192.168.255.10/Pak/"; //默认链接失败后，会尝试这个地址

    //热更新的dll名称、注意这里是因为使用了yooasset的打包规则所以直接资源原名称读取即可
    public string HotDllName = "hotfix.dll";

    public string HotUpdatePackageName = "DefaultPackage";

    public List<string> BundlesToLoad;

    private async void Start()

    {
        // 1.初始化资源系统
        YooAssets.Initialize();
        
        // 首先热更dll包
        await DownLoadAssetsByYooAssets(HotUpdatePackageName, EDefaultBuildPipeline.RawFileBuildPipeline);
        
        // 然后热更所有资源包
        foreach (var pakName in BundlesToLoad)
        {
            await DownLoadAssetsByYooAssets(pakName);   
        }
        
        // 开始游戏
        await StartGame();
    }

    #region yooasset

    private async UniTask DownLoadAssetsByYooAssets(string pakName, EDefaultBuildPipeline editorDefaultBuildPipeline = EDefaultBuildPipeline.BuiltinBuildPipeline)
    {
        // 创建默认的资源包
        var package = YooAssets.CreatePackage(pakName);
        // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(package);

        InitializationOperation initializationOperation = null;

        switch (PlayMode)

        {
            case EPlayMode.EditorSimulateMode:
                //编辑器模拟模式
                var initParametersEditorSimulateMode = new EditorSimulateModeParameters();

                initParametersEditorSimulateMode.SimulateManifestFilePath =
                    EditorSimulateModeHelper.SimulateBuild(editorDefaultBuildPipeline,
                        pakName);

                initializationOperation = package.InitializeAsync(initParametersEditorSimulateMode);

                break;

            case EPlayMode.OfflinePlayMode:
                //单机模式
                var initParametersOfflinePlayMode = new OfflinePlayModeParameters();

                initializationOperation = package.InitializeAsync(initParametersOfflinePlayMode);

                break;

            case EPlayMode.HostPlayMode:
                //联机运行模式，获取版本更新
                var initParametersHostPlayMode = new HostPlayModeParameters();

                initParametersHostPlayMode.BuildinQueryServices = new GameQueryServices();
                // 服务器地址为根目录+包名
                initParametersHostPlayMode.RemoteServices = new RemoteServices(DefaultHostServer + pakName, FallbackHostServer + pakName);
                // 资源解密类，暂时用不到
                // initParametersHostPlayMode.DecryptionServices = new FileStreamDecryption();

                initializationOperation = package.InitializeAsync(initParametersHostPlayMode);

                break;
        }

        // 等待资源初始化完成
        await initializationOperation;

        // 如果初始化失败弹出提示界面
        if (initializationOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning($"{initializationOperation.Error}");
            return;
        }
        else
        {
            var version = initializationOperation.PackageVersion;
            Debug.Log($"Init resource package version : {version}");
        }

        //2.获取资源版本

        var operationGetRemoteVersion = package.UpdatePackageVersionAsync();
        await operationGetRemoteVersion.ToUniTask();


        if (operationGetRemoteVersion.Status != EOperationStatus.Succeed)
        {
            //ShowText("获取远程资源版本信息失败", DownloadEnum.false_download).Forget();

            Debug.LogError($"{operationGetRemoteVersion.Error}, 获取远程资源版本信息失败");

            return;
        }

        string packageVersion = operationGetRemoteVersion.PackageVersion;


        //3.更新补丁清单

        var operationUpdateManifest = package.UpdatePackageManifestAsync(packageVersion);
        await operationUpdateManifest.ToUniTask();


        if (operationUpdateManifest.Status != EOperationStatus.Succeed)

        {
            // ShowText("更新资源版本清单失败", DownloadEnum.false_download).Forget();

            //更新失败

            Debug.LogError($"{operationUpdateManifest.Error}, 更新资源版本清单失败");

            return;
        }


        //4.以上流程走通后开始进行下载补丁包信息
        await Download(pakName);
    }

    private async UniTask Download(string pakName)

    {
        int downloadingMaxNum = 10;
        int failedTryAgain = 3; //失败尝试次数
        int timeout = 60; //超时时间
        var package = YooAssets.GetPackage(pakName);
        var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);

        //没有需要下载的资源
        if (downloader.TotalDownloadCount == 0)

        {
            Debug.Log("没有资源更新，直接进入游戏加载环节");

            //await ShowText("没有资源更新，直接进入游戏加载环节", DownloadEnum.no_download_show_text);
            // StartGame().Forget(); //进入游戏加载环节

            return;
        }

        //需要下载的文件总数和总大小
        int totalDownloadCount = downloader.TotalDownloadCount;
        long totalDownloadBytes = downloader.TotalDownloadBytes;

        Debug.Log($"文件总数:{totalDownloadCount}:::总大小:{totalDownloadBytes}");

        await UniTask.DelayFrame(1);
        // TODO:这里为什么要用Forget？按理说应该等待全部下载完毕后才执行后续
        //进行下载
        GetDownload(pakName).Forget();
        //显示更新提示UI界面 有需要自己可以制作UI面板
        // ShowText($"文件总数:{totalDownloadCount},总大小:{totalDownloadBytes}KB", DownloadEnum.goto_download).Forget();
    }


    //从这里判断是否热更新资源完成从而进入的游戏加载环节
    private async UniTask GetDownload(string pakName)

    {
        int downloadingMaxNum = 10;

        int failedTryAgain = 3;

        int timeout = 60;

        var package = YooAssets.GetPackage(pakName);

        var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);
        //注册回调方法，下载的各个时段回调函数
        downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
        downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction; // 下载中每次更新的回调函数
        downloader.OnDownloadOverCallback = OnDownloadOverFunction;
        downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;

        //开启下载

        downloader.BeginDownload();

        await downloader.ToUniTask();

        //检测下载结果
        if (downloader.Status == EOperationStatus.Succeed)
        {
            //下载成功,加载AOT泛型dll数据
            // await StartGame();
            Debug.Log("更新完成!");
        }
        else
        {
            //下载失败
            Debug.LogError("更新失败！");
            //TODO:
        }
    }


    #region yooasset下载回调函数

    /// <summary>
    /// 下载数据大小
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sizeBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnStartDownloadFileFunction(string fileName, long sizeBytes)

    {
        Debug.Log(string.Format("开始下载：文件名：{0}, 文件大小：{1}", fileName, sizeBytes));
    }

    /// <summary>
    /// 下载完成与否
    /// </summary>
    /// <param name="isSucceed"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadOverFunction(bool isSucceed)

    {
        Debug.Log("下载" + (isSucceed ? "成功" : "失败"));
    }


    /// <summary>
    /// 更新中
    /// </summary>
    /// <param name="totalDownloadCount"></param>
    /// <param name="currentDownloadCount"></param>
    /// <param name="totalDownloadBytes"></param>
    /// <param name="currentDownloadBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount,
        long totalDownloadBytes, long currentDownloadBytes)

    {
        Debug.Log(string.Format("文件总数：{0}, 已下载文件数：{1}, 下载总大小：{2}, 已下载大小：{3}", totalDownloadCount, currentDownloadCount,
            totalDownloadBytes, currentDownloadBytes));
    }

    /// <summary>
    /// 下载出错
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="error"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadErrorFunction(string fileName, string error)

    {
        Debug.LogError(string.Format("下载出错：文件名：{0}, 错误信息：{1}", fileName, error));
    }

    #endregion


    // 内置文件查询服务类
    //
    // private class QueryStreamingAssetsFileServices : IQueryServices
    //
    // {
    //     public bool QueryStreamingAssets(string fileName)
    //
    //     {
    //         // 注意：使用了BetterStreamingAssets插件，使用前需要初始化该插件！
    //
    //         string buildinFolderName = YooAssets.GetStreamingAssetBuildinFolderName();
    //
    //         return StreamingAssetsHelper.FileExists($"{buildinFolderName}/{fileName}");
    //     }
    // }

    #endregion


    /// <summary>
    /// 加载热更新资源，记得要把dll打进AB包里
    /// </summary>
    private async UniTask StartGame()
    {
        // 1.获取最新包
        // 2.用最新包加载dll完成热更
        //进入热更新脚本逻辑，Yooasset下载核对完成后加载后面package
        var package = YooAssets.GetPackage(HotUpdatePackageName);
        //加载原生打包的dll文件
        RawFileHandle handleFiles = package.LoadRawFileAsync(HotDllName);
        await handleFiles.ToUniTask();

#if !UNITY_EDITOR
        //非编辑器下要加载yooAsset中的dll文件
        byte[] dllBytes = handleFiles.GetRawFileData();
        //加载dll，完成dll热更新
        System.Reflection.Assembly.Load(dllBytes);
#endif
        // Editor下无需加载，直接查找获得HotUpdate程序集
        Assembly hotUpdateAss =
            System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == HotDllName.Split('.')[0]);
        
        // //完成dll加载后，即可进行加载热更新预制体，通过实例化挂载了热更新脚本的预制体直接进入到热更新层的逻辑
        // AssetHandle handle = package.LoadAssetAsync<GameObject>("HotFix_Import");
        // await handle.ToUniTask();
        // GameObject go = handle.InstantiateSync();
        // Debug.Log($"HotFix Prefab Loaded, name is {go.name}");
        //

        // 也可以反射直接调取加载函数
        Type type = hotUpdateAss.GetType("MainScene");
        type.GetMethod("Run").Invoke(null, null);
    }

    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }

        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }

        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }
}