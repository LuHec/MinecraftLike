using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{

	public ComputeShader TextureShader;

	public RenderTexture _rTexture;

	

	private void Start() {
		Debug.Log("111");
		int kernel = TextureShader.FindKernel("CSMain");

		if (_rTexture == null) { 
			_rTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			_rTexture.enableRandomWrite = true;
			_rTexture.Create();
		}

		TextureShader.SetTexture(kernel, "Result", _rTexture);

		int workgroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
		int workgroupsY = Mathf.CeilToInt(Screen.height / 8.0f);

		TextureShader.Dispatch(kernel, workgroupsX, workgroupsY, 1);

		GetComponent<Renderer>().material.mainTexture = _rTexture;
	}
}
