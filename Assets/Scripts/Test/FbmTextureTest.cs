using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class FbmTextureTest : MonoBehaviour
{
    private const int MAX_JOB_COUNT = 5;

    public enum TestTarget
    {
        Noise1D,
        Noise2D,
        Noise3D,
        Biome
    }

    public enum Zoom
    {
        X1 = 1, // LOD3 采样分辨率1:8 着色1:1
        X2 = 2, // LOD2 采样分辨率1:8 着色1:4
        X4 = 4, // LOD2 采样分辨率1:4 着色1:4
        X8 = 8, // LOD1 采样分辨率1:4 着色1:8   
        X16 = 16, // LOD1 采样分辨率1:1 着色1:16
        // X32 = 32,
        // X64 = 64,   // 原生1:1 LOD0
    }

    [SerializeField] private ComputeShader computeShader;

    [SerializeField] private int seed = 114514;

    [SerializeField] private bool toggle = false;

    [SerializeField] TestTarget _target;

    // 决定了噪声的分形次数
    [SerializeField, Range(1, 5)] int _fractalLevel = 1;

    [SerializeField] int size = 64;
    [SerializeField] int blockSize = 256;
    [SerializeField] private Zoom zoom = Zoom.X1;
    private int WorldScale => (int)zoom;
    Texture2D texture;
    private RenderTexture mRenderTexture;
    FastNoiseLite noise = new FastNoiseLite();
    private WorldBiomeSource mWorldBiomeSource;
    private UniTask[] tasks;

    private CancellationTokenSource cts = new();

    struct DrawTask
    {
        public Vector3Int leftDown;
        public Vector3Int rightUp;
        public int zoom;
        public bool finished;
    }

    private Queue<DrawTask> mDrawTaskQueue = new();


    void Start()
    {
        texture = new Texture2D(size, size);
        texture.wrapMode = TextureWrapMode.Clamp;
        int kernelIdx = computeShader.FindKernel("CSMain");
        
        mRenderTexture = new RenderTexture(size, size, 16);
        mRenderTexture.enableRandomWrite = true;
        mRenderTexture.Create();
        
        GetComponent<Renderer>().material.mainTexture = mRenderTexture;
        computeShader.SetTexture(kernelIdx, "Result", mRenderTexture);
        
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        // UpdateTextureBiomeCorrectly(cts.Token);
        mCoroutine = StartCoroutine(UpdateTextureCoroutine());
    }

    [ContextMenu("Refresh")]
    void Refresh()
    {
        // UpdateTextureBiomeCorrectly(cts.Token);
        if (mCoroutine != null) StopCoroutine(mCoroutine);
        mCoroutine = StartCoroutine(UpdateTextureCoroutine());
    }

    private Coroutine mCoroutine;
    private bool mFinished = false;
    private Queue<Vector3Int> mDrawQueue = new();
    private int[,] mDir;

    IEnumerator UpdateTextureCoroutine()
    {
        mDrawQueue.Clear();
        mDir = new int[,] { { 0, blockSize }, { blockSize, 0 }, { -blockSize, 0 }, { 0, -blockSize } };
        WorldBiomeSource worldBiomeSource = new WorldBiomeSource(seed);
        bool[,] drawMap = new bool[size, size];
        // Vector4[,] tempColors = new Vector4[size, size];
        Vector4[] tempColors = new Vector4[size * size];
        // pixelCenter:coordinate center,coordinate=>(t - size >> 1) * scale
        Vector3Int pixelCenter = new Vector3Int(size >> 1, 0, size >> 1);
        drawMap[pixelCenter.x, pixelCenter.z] = true;
        mDrawQueue.Enqueue(pixelCenter);
        ComputeBuffer computeBuffer = new ComputeBuffer(size * size, 4 * 4);
        int waitingJob = 0;

        while (!mFinished)
        {
            // 满批处理数量条件统一绘制
            if (waitingJob >= MAX_JOB_COUNT || mDrawQueue.Count == 0)
            {
                if (waitingJob > 0)
                {
                    waitingJob = 0;
                    // call cs to draw
                    int kernelIdx = computeShader.FindKernel("CSMain");
                    computeBuffer.SetData(tempColors);
                    computeShader.SetFloat("size", size);
                    computeShader.SetBuffer(kernelIdx, "dataBuffer", computeBuffer);
                    computeShader.Dispatch(kernelIdx, size / 16, size / 16, 1);
                    // for (int i = 0; i < size; i++)
                    // {
                    //     for (int j = 0; j < size; j++)
                    //     {
                    //         texture.SetPixel(i,j,tempColors[i * size + j]);        
                    //     }
                    // }
                    // texture.Apply();
                }
                else
                {
                    mFinished = true;
                }
            }
            // 否则先取出任务完成
            else
            {
                Vector3Int pixelCoordinate = mDrawQueue.Peek();
                mDrawQueue.Dequeue();

                waitingJob++;

                // sample
                for (int x = pixelCoordinate.x; x < pixelCoordinate.x + blockSize; x++)
                {
                    for (int y = pixelCoordinate.z; y < pixelCoordinate.z + blockSize; y++)
                    {
                        int sampleX = (x - (size >> 1)) * WorldScale;
                        int sampleY = (y - (size >> 1)) * WorldScale;

                        int value = worldBiomeSource.GetBiome(sampleX, 0, sampleY);
                        Color color = Biomes.GetBiome(value).Color;
                        // texture.SetPixel(x, y, color);
                        tempColors[x * size + y] = color;
                    }
                }

                // dfs
                for (int i = 0; i < 4; i++)
                {
                    int x = pixelCoordinate.x + mDir[i, 0], y = pixelCoordinate.z + mDir[i, 1];
                    if (x < size && y < size && x >= 0 && y >= 0 && drawMap[x, y] == false)
                    {
                        drawMap[x, y] = true;
                        mDrawQueue.Enqueue(new(pixelCoordinate.x + mDir[i, 0], 0, pixelCoordinate.z + mDir[i, 1]));
                    }
                }
            }

            yield return 0;
        }
        
        computeBuffer.Release();
        computeBuffer.Dispose();
    }


    void UpdateTexture(System.Func<float, float, float, float> generator)
    {
        var scale = 1.0f / size;
        var time = Time.time;

        // 这里可能要改一下，这个库的采样次数是在内部决定的，导致所有采样都用的同样的偏移量
        System.Random prng = new System.Random(seed);
        float offsetX = prng.Next(-10000, 10000);
        float offsetY = prng.Next(-10000, 10000);

        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                float sampleX = x * scale * frequency + offsetX;
                float sampleY = y * scale * frequency + offsetY;
                float value = generator.Invoke(sampleX, sampleY, time) * amplitude;
                texture.SetPixel(x, y, Color.white * (value / 1.4f + 0.5f));
            }
        }

        texture.Apply();
    }

    async void UpdateTextureBiome(CancellationToken token)
    {
        int blockCount = size / blockSize;
        tasks = new UniTask[blockCount * blockCount];
        Color[,] tempColors = new Color[size, size];

        for (int i = 0; i < blockCount; i++)
        {
            for (int j = 0; j < blockCount; j++)
            {
                int tX = i;
                int tY = j;
                tasks[i * blockCount + j] = UniTask.RunOnThreadPool(async () =>
                {
                    await Sample(new WorldBiomeSource(seed), tX * blockSize, tY * blockSize, blockSize, tempColors,
                        token);
                    // Sample(new WorldBiomeSource(seed), tX * blockSize, tY * blockSize, blockSize, tempColors);
                });
            }
        }

        // 等待所有任务完成
        await UniTask.WhenAll(tasks);

        // 在主线程中更新纹理
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (token.IsCancellationRequested) return;
                texture.SetPixel(x, y, tempColors[x, y]);
            }
        }

        texture.Apply();
    }

    async UniTask Sample(WorldBiomeSource worldBiomeSource, int xS, int yS, int blockSize,
        Color[,] tempColors, CancellationToken token)
    {
        for (var y = yS; y < yS + blockSize; y += WorldScale)
        {
            for (var x = xS; x < xS + blockSize; x += WorldScale)
            {
                if (token.IsCancellationRequested) return;
                int sampleX = x / WorldScale;
                int sampleY = y / WorldScale;

                int value = worldBiomeSource.GetBiome(sampleX, 0, sampleY);
                Color color = Biomes.GetBiome(value).Color;

                for (int py = y; py < Mathf.Clamp(y + WorldScale, 0, yS + blockSize); py++)
                {
                    for (int px = x; px < Mathf.Clamp(x + WorldScale, 0, xS + blockSize); px++)
                    {
                        tempColors[px, py] = color;
                    }
                }
            }
        }

        await UniTask.Yield();
    }

    private async void UpdateTextureBiomeCorrectly(CancellationToken token)
    {
        int blockCount = size / blockSize;
        tasks = new UniTask[blockCount * blockCount];
        Color[,] tempColors = new Color[size, size];

        for (int i = 0; i < blockCount; i++)
        {
            for (int j = 0; j < blockCount; j++)
            {
                int tX = i;
                int tY = j;
                tasks[i * blockCount + j] = UniTask.RunOnThreadPool(async () =>
                {
                    await SampleCorrectly(new WorldBiomeSource(seed), tX * blockSize, tY * blockSize, blockSize,
                        tempColors,
                        token);

                    for (int y = 0; y < (tY + 1) * blockSize; y++)
                    {
                        for (int x = tX * blockSize; x < (tX + 1) * blockSize; x++)
                        {
                            if (token.IsCancellationRequested) return;
                            texture.SetPixel(x, y, tempColors[x, y]);
                        }
                    }

                    texture.Apply();
                });
            }
        }

        // // 等待所有任务完成
        // await UniTask.WhenAll(tasks);
        //
        // // 在主线程中更新纹理
        // for (int y = 0; y < size; y++)
        // {
        //     for (int x = 0; x < size; x++)
        //     {
        //         if (token.IsCancellationRequested) return;
        //         texture.SetPixel(x, y, tempColors[x, y]);
        //     }
        // }

        // texture.Apply();
    }

    async UniTask SampleCorrectly(WorldBiomeSource worldBiomeSource, int xS, int yS, int blockSize,
        Color[,] tempColors, CancellationToken token)
    {
        for (var y = yS; y < yS + blockSize; y++)
        {
            for (var x = xS; x < xS + blockSize; x++)
            {
                if (token.IsCancellationRequested) return;
                int sampleX = x * WorldScale;
                int sampleY = y * WorldScale;

                int value = worldBiomeSource.GetBiome(sampleX, 0, sampleY);
                Color color = Biomes.GetBiome(value).Color;
                tempColors[x, y] = color;
            }
        }

        await UniTask.Yield();
    }

    void Update()
    {
        if (_target == TestTarget.Noise1D)
            UpdateTexture((x, y, t) => Perlin.Fbm(x + t, _fractalLevel));
        else if (_target == TestTarget.Noise2D)
        {
            // UpdateTexture((x, y, t) => Perlin.Fbm(x + t, y, _fractalLevel));
            UpdateTexture((x, y, t) => Perlin.Fbm(x, y, _fractalLevel));
        }
        else if (_target == TestTarget.Noise3D)
            UpdateTexture((x, y, t) => Perlin.Fbm(x, y, t, _fractalLevel));
        else
        {
            // UpdateTextureBiome(cts.Token);
            // UpdateTextureBiome(new WorldBiomeSource(seed));
            // mWorldBiomeSource = new WorldBiomeSource(seed);
            // UpdateTextureBiome((x, y, t) => mWorldBiomeSource.Sample(x,0,y));
        }
    }

    private void OnDestroy()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}