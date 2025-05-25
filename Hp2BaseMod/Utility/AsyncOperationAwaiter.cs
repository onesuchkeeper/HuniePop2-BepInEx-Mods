using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

public readonly struct AsyncOperationAwaiter : INotifyCompletion
{
    private readonly AsyncOperation _asyncOperation;
    public bool IsCompleted => _asyncOperation.isDone;

    public AsyncOperationAwaiter(AsyncOperation asyncOperation) => _asyncOperation = asyncOperation;

    public void OnCompleted(Action continuation) => _asyncOperation.completed += _ => continuation();

    public void GetResult() { }
}

public readonly struct UnityWebRequestAwaiter : INotifyCompletion
{
    private readonly UnityWebRequestAsyncOperation _asyncOperation;

    public bool IsCompleted => _asyncOperation.isDone;

    public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOperation) => _asyncOperation = asyncOperation;

    public void OnCompleted(Action continuation) => _asyncOperation.completed += _ => continuation();

    public UnityWebRequest GetResult() => _asyncOperation.webRequest;
}

public static class ExtensionMethods
{
    public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
    {
        return new UnityWebRequestAwaiter(asyncOp);
    }
}