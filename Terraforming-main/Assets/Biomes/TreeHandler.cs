using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeHandler : MonoBehaviour
{
	public event System.Action onTerrainModified;
	[SerializeField] private LayerMask groundLayer;

	GenTest genTest;

	public ComputeShader blurCompute;
	public ComputeShader editCompute;
	[HideInInspector] public RenderTexture rawDensityTexture;


	public ComputeShader treeCompute;
	public int blurRadius = 30;
	public float treeSize = 1000;

	private void Start()
    {
		genTest = GetComponent<GenTest>();


		//SetTree(100, new Vector3(0,200,0));
    }

    public void SetTree(int _genSize, Vector3 treePos, RenderTexture _rawDensity)
	{
		//if (biomes[0].treePrefabs[0] != null)
		//return;
		rawDensityTexture = _rawDensity;
		int textureSize = rawDensityTexture.width;

		int planetEdge = _genSize + 5;
		int GenArea = _genSize * 2;
		float treePercentage = 0.01f;

		int rayDist = _genSize;

		//Vector3 treePos;
		/*
		treeCompute.SetInt("textureSize", textureSize);

		treeCompute.SetFloat("treeSize", treeSize);
		treeCompute.SetVector("treePos", treePos);

		treeCompute.SetTexture(0, "DensityTexture", rawDensityTexture);
		//treeCompute.SetFloat("noiseHeightMultiplier", noiseHeightMultiplier);
		//treeCompute.SetFloat("noiseScale", noiseScale);

		ComputeHelper.Dispatch(treeCompute, textureSize, textureSize, textureSize);

		*/


		/*
		for (int x = 0; x < GenArea; x++)
		{
			for (int z = 0; z < GenArea; z++)
			{
				float digits = Random.Range(0, 101);


				if (digits <= treePercentage)
				{
					//Vector3 rayLoc = new Vector3(x, 0, z);
					RaycastHit hit;

					if (Physics.Raycast(new Vector3(x, planetEdge, z), Vector3.down, out hit, rayDist, groundLayer))
					{
						Debug.Log(hit);
						Debug.DrawRay(new Vector3(x, planetEdge, z), Vector3.down * hit.distance, Color.red, 100);
						genTest.Terraform(hit.point, 1, 1);
						//Instantiate(biomes[0].treePrefabs[0], hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal));

					}
					//else
					//Debug.DrawRay(new Vector3(x, planetEdge, z), Vector3.down * 500, Color.green, 100);


				}


			}
		}
		*/
		Invoke("StartGen",1);
	}

	bool set = false;

	void StartGen()
    {
		set = true;
		//onTerrainModified?.Invoke();
		//Terraform(new Vector3(0, 150, 0), -500, 50);
		genTest.PlaceTree(new Vector3(0, 150, 0), -10, 20);

	}




	Vector3 hitPoint;
	float hitRadius;

	void OnDrawGizmos()
	{

			//Gizmos.color = Color.green;
			//Gizmos.DrawSphere(hitPoint, hitRadius);

	}

	public void Terraform(Vector3 point, float weight, float radius)
	{
		hitPoint = point;
		hitRadius = radius;

		int editTextureSize = rawDensityTexture.width;
		float editPixelWorldSize = genTest.boundsSize / editTextureSize;
		int editRadius = Mathf.CeilToInt(radius / editPixelWorldSize);
		//Debug.Log(editPixelWorldSize + "  " + editRadius);

		float tx = Mathf.Clamp01((point.x + genTest.boundsSize / 2) / genTest.boundsSize);
		float ty = Mathf.Clamp01((point.y + genTest.boundsSize / 2) / genTest.boundsSize);
		float tz = Mathf.Clamp01((point.z + genTest.boundsSize / 2) / genTest.boundsSize);

		int editX = Mathf.RoundToInt(tx * (editTextureSize - 1));
		int editY = Mathf.RoundToInt(ty * (editTextureSize - 1));
		int editZ = Mathf.RoundToInt(tz * (editTextureSize - 1));

		editCompute.SetFloat("weight", weight);
		editCompute.SetFloat("deltaTime", Time.deltaTime);
		editCompute.SetInts("brushCentre", editX, editY, editZ);
		editCompute.SetInt("brushRadius", editRadius);

		editCompute.SetInt("size", editTextureSize);
		ComputeHelper.Dispatch(editCompute, editTextureSize, editTextureSize, editTextureSize);

		//ProcessDensityMap();
		int size = rawDensityTexture.width;

		if (genTest.blurMap)
		{
			//treeCompute.SetInt("textureSize", textureSize);

			//treeCompute.SetFloat("treeSize", treeSize);
			//treeCompute.SetVector("treePos", treePos);

			treeCompute.SetTexture(0, "DensityTexture", rawDensityTexture);
			//treeCompute.SetFloat("noiseHeightMultiplier", noiseHeightMultiplier);
			//treeCompute.SetFloat("noiseScale", noiseScale);

			//ComputeHelper.Dispatch(treeCompute, k, k, k);


			blurCompute.SetInt("textureSize", rawDensityTexture.width);
			blurCompute.SetInts("brushCentre", editX - blurRadius - editRadius, editY - blurRadius - editRadius, editZ - blurRadius - editRadius);
			blurCompute.SetInt("blurRadius", blurRadius);
			blurCompute.SetInt("brushRadius", editRadius);
			int k = (editRadius + blurRadius) * 2;
			ComputeHelper.Dispatch(blurCompute, k, k, k);
		}
		else
		Debug.LogWarning(genTest.blurMap);
		//ComputeHelper.CopyRenderTexture3D(originalMap, processedDensityTexture);

		float worldRadius = (editRadius + 1 + ((genTest.blurMap) ? blurRadius : 0)) * editPixelWorldSize;
		for (int i = 0; i < genTest.chunks.Length; i++)
		{
			Chunk chunk = genTest.chunks[i];

			if (MathUtility.SphereIntersectsBox(point, worldRadius, chunk.centre, Vector3.one * chunk.size))
			{

				chunk.terra = true;
				genTest.GenerateChunk(chunk);

			}
			//else Debug.LogError(genTest.chunks[i] + "Nope");

		}
	}
}
