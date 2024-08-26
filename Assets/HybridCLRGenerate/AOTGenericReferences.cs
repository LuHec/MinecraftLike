using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"UniTask.dll",
		"UnityEngine.CoreModule.dll",
		"YooAsset.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Cysharp.Threading.Tasks.ITaskPoolNode<object>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<object>
	// System.Action<UnityEngine.Vector3>
	// System.Action<int>
	// System.Action<object>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector3>
	// System.Collections.Generic.ArraySortHelper<int>
	// System.Collections.Generic.Comparer<UnityEngine.Vector3>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.ICollection<UnityEngine.Vector3>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.IComparer<UnityEngine.Vector3>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector3>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector3>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IList<UnityEngine.Vector3>
	// System.Collections.Generic.IList<int>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector3>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List<UnityEngine.Vector3>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector3>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector3>
	// System.Collections.ObjectModel.ReadOnlyCollection<int>
	// System.Comparison<UnityEngine.Vector3>
	// System.Comparison<int>
	// System.Predicate<UnityEngine.Vector3>
	// System.Predicate<int>
	// }}

	public void RefMethods()
	{
		// Cysharp.Threading.Tasks.UniTask.Awaiter Cysharp.Threading.Tasks.EnumeratorAsyncExtensions.GetAwaiter<object>(object)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,MainScene.<Run>d__0>(Cysharp.Threading.Tasks.UniTask.Awaiter&,MainScene.<Run>d__0&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,MainScene.<Run>d__0>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,MainScene.<Run>d__0&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<MainScene.<Run>d__0>(MainScene.<Run>d__0&)
		// object UnityEngine.GameObject.AddComponent<object>()
		// YooAsset.AssetHandle YooAsset.ResourcePackage.LoadAssetAsync<object>(string,uint)
		// YooAsset.AssetHandle YooAsset.ResourcePackage.LoadAssetSync<object>(string)
	}
}