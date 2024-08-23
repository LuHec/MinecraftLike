using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TaskQueue<T>
{
    private GameObject mOwner;
    private LinkedList<T> mQueue;
    

    private CancellationTokenSource mCancellationTokenSource;

    public void Clear()
    {
        mCancellationTokenSource.Cancel();
        
        mCancellationTokenSource = new CancellationTokenSource();
        
    }

    // 添加任务时不能打断上一个任务
    // 协程：依然是单线程，任务量大还是会阻塞
    public async void Work(UniTask task, CancellationToken cancellationToken)
    {
        
        await UniTask.Yield();
    }
}