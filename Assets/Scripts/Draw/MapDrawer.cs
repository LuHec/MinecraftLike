using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public struct DrawJob
{
    // 纹理坐标起点
    public Vector3 index;
    public int zoom;
}

public class MapDrawer
{
    // redraw
    public Action onZooming;
    // only add job 
    public Action onMove;
    
    // 采样时划分的Block大小
    public const int BLOCK_SIZE = 128;
    public const int DEFAULT_SCALE = 15;
    public int Zoom => DEFAULT_SCALE + mZoom;
    
    private readonly int mWidth;
    private readonly int mHeight;
    private readonly int m2Width;
    private readonly int m2Height;
    private int mZoom = 1;
    private WorldBiomeSource mWorldBiomeSource;
    private ComputeShader mComputeShader;
    private RenderTexture mTargetTexture;

    private Vector3Int mCenterPoint = Vector3Int.zero;

    private Queue<DrawJob> mRenderQueue;  


    public MapDrawer(int width, int height, WorldBiomeSource worldBiomeSource, ComputeShader computeShader,
        RenderTexture targetTexture)
    {
        // 保证半长为整数
        mWidth = (width & 1) == 0 ? width : width + 1;
        mHeight = (height & 1) == 0 ? height : height + 1;
        m2Width = (int)(mWidth * .5f);
        m2Height = (int)(mHeight * .5f);
        mWorldBiomeSource = worldBiomeSource;
        mComputeShader = computeShader;
        mTargetTexture = targetTexture;
    }
    
    public async void Draw()
    {
        // 每次移动，都会根据中心点偏移添加/删除任务
        // 每次缩放，都会删除所有任务
        
        await UniTask.Yield();
    }
    
    public void SetZoom(int zoomScale)
    {
        mZoom = zoomScale;
    }

    public void SetCenter(Vector3Int center)
    {
        mCenterPoint = center;
    }

    public void ZoomCoordinate(ref Vector3Int inPosition)
    {
        inPosition /= Zoom;
    }

    // 以新的center为中心，重新绘制整个地图
    private void ReDraw(Vector3 center)
    {
        // TODO:测试1:1采样率下绘制
        // 实际显示和采样没有半毛钱关系，纹理全都交给材质来解决；采样只需要根据zoom mip来决定步长
    }

    private async UniTask SetJob(Vector3Int downLeft, Vector3Int upRight)
    {
        ZoomCoordinate(ref downLeft);
        ZoomCoordinate(ref upRight);
        for (int x = downLeft.x; x <= upRight.x; x++)
        {
            for (int z = downLeft.z; z <= upRight.z; z++)
            {
                mWorldBiomeSource.Sample(x, 0, z);
            }
        }

        await UniTask.Yield();
    }
    

}