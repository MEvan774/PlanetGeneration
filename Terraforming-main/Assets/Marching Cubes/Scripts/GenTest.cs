using System.Collections.Generic;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;

public class GenTest : MonoBehaviour
{
	SeedGen seedGen;
	BiomeHandler biomeHandler;
	TreeHandler treehandler;

	[Header("Init Settings")]
	public int numChunks = 4;

	public int numPointsPerAxis = 10;
	public float boundsSize = 10;
	public float isoLevel = 0f;
	public bool useFlatShading;

	public float noiseScale;
	public float noiseHeightMultiplier;
	public bool blurMap;
	public int blurRadius = 3;

	[Header("References")]
	public ComputeShader meshCompute;
	public ComputeShader densityCompute;
	public ComputeShader blurCompute;
	public ComputeShader editCompute;
	public ComputeShader treeCompute;
	public Material material;


	// Private
	ComputeBuffer triangleBuffer;
	ComputeBuffer triCountBuffer;
	[HideInInspector] public RenderTexture rawDensityTexture;
	[HideInInspector] public RenderTexture processedDensityTexture;
	[HideInInspector] public Chunk[] chunks;

	VertexData[] vertexDataArray;

	int totalVerts;

	// Stopwatches
	System.Diagnostics.Stopwatch timer_fetchVertexData;
	System.Diagnostics.Stopwatch timer_processVertexData;
	RenderTexture originalMap;
	RenderTexture biomeMap;
	RenderTexture newRender;



	Texture3D biomeNoiseTex;

	Texture3D rainfallNoiseTex;
	Texture3D tempNoiseTex;

	void Start()
	{
		seedGen = GetComponent<SeedGen>();
		biomeHandler = GetComponent<BiomeHandler>();
		treehandler = GetComponent<TreeHandler>();

		biomeHandler.GenBiome();
		//material.SetInt("_biomeSize", biomeHandler.biomes.Length);

		/*for (int i = 0; i < biomeHandler.biomes.Length; i++)
        {
			material.SetTexture("_biomeNoiseTex", biomeHandler.biomes[i].noiseTex);
			material.SetColor("_biomeGrassLight", biomeHandler.biomes[i].lightGrassColor);
			material.SetColor("_biomeGrassDark", biomeHandler.biomes[i].darkGrassColor);
		}
		*/
		

		biomeNoiseTex = biomeHandler.GenerateNoise(32,1,2,0.5f,2,0.9f);
		rainfallNoiseTex = biomeHandler.GenerateNoise(32,1,2,0.5f,2,0.9f);
		tempNoiseTex = biomeHandler.GenerateNoise(32,1,2,0.5f,2,0.9f);

		biomeHandler.BiomeManager(material);

		InitTextures();
		CreateBuffers();

		CreateChunks();

		var sw = System.Diagnostics.Stopwatch.StartNew();
		GenerateAllChunks();
		Debug.Log("Generation Time: " + sw.ElapsedMilliseconds + " ms");

		ComputeHelper.CreateRenderTexture3D(ref originalMap, processedDensityTexture);
		ComputeHelper.CopyRenderTexture3D(processedDensityTexture, originalMap);


		treehandler.rawDensityTexture = rawDensityTexture;

		treehandler.SetTree(100, new Vector3(0, 700, 0), rawDensityTexture);

		//ComputeHelper.CreateRenderTexture3D(ref originalMap, biomeMap);

		//meshCompute.SetTexture(0, "_biomeNoiseTex", biomeHandler.GenerateTexture());
		//		meshCompute.SetTexture(0, "_biomeNoiseTex", biomeHandler.GenerateNoise(32, 1, 2, 0.5f, 2, 0.9f));

		//biomeHandler = GetComponent<BiomeHandler>();
		//--

		//--

	}

	void InitTextures()
	{

		// Explanation of texture size:
		// Each pixel maps to one point.
		// Each chunk has "numPointsPerAxis" points along each axis
		// The last points of each chunk overlap in space with the first points of the next chunk
		// Therefore we need one fewer pixel than points for each added chunk
		int size = numChunks * (numPointsPerAxis - 1) + 1;
		Create3DTexture(ref rawDensityTexture, size, "Raw Density Texture");
		Create3DTexture(ref processedDensityTexture, size, "Processed Density Texture");

		//--
		densityCompute.SetTexture(0, "BaseTexture", GenerateBase(size));


		//--

		if (!blurMap)
		{
			processedDensityTexture = rawDensityTexture;
		}

		// Set textures on compute shaders
		densityCompute.SetTexture(0, "DensityTexture", rawDensityTexture);
		editCompute.SetTexture(0, "EditTexture", rawDensityTexture);
		treeCompute.SetTexture(0, "TreeMapTexture", rawDensityTexture);
		blurCompute.SetTexture(0, "Source", rawDensityTexture);
		blurCompute.SetTexture(0, "Result", processedDensityTexture);
		//meshCompute.SetTexture(0, "DensityTexture", (blurCompute) ? processedDensityTexture : rawDensityTexture);
		meshCompute.SetTexture(0, "DensityTexture", rawDensityTexture);
	}

	void GenerateAllChunks()
	{
		// Create timers:
		timer_fetchVertexData = new System.Diagnostics.Stopwatch();
		timer_processVertexData = new System.Diagnostics.Stopwatch();

		totalVerts = 0;
		ComputeDensity();


		for (int i = 0; i < chunks.Length; i++)
		{
			GenerateChunk(chunks[i]);
		}
		Debug.Log("Total verts " + totalVerts);


		//Invoke("SetVegatation", 0.5f);


		// Print timers:
		Debug.Log("Fetch vertex data: " + timer_fetchVertexData.ElapsedMilliseconds + " ms");
		Debug.Log("Process vertex data: " + timer_processVertexData.ElapsedMilliseconds + " ms");
		Debug.Log("Sum: " + (timer_fetchVertexData.ElapsedMilliseconds + timer_processVertexData.ElapsedMilliseconds));


	}

	void SetVegatation()
    {
		//--Gen Vegatation--
		biomeHandler.SetVegatation(rawDensityTexture.width);
	}

	void ComputeDensity()
	{
		// Get points (each point is a vector4: xyz = position, w = density)
		int textureSize = rawDensityTexture.width;

		noiseScale = Random.Range(0.1f, 0.95f);
		//noiseHeightMultiplier = Random.Range(0.0001f, 0.5f);

		densityCompute.SetInt("textureSize", textureSize);

		densityCompute.SetFloat("planetSize", boundsSize);
		densityCompute.SetFloat("noiseHeightMultiplier", noiseHeightMultiplier);
		densityCompute.SetFloat("noiseScale", noiseScale);

		ComputeHelper.Dispatch(densityCompute, textureSize, textureSize, textureSize);

		ProcessDensityMap();
	}

	void ProcessDensityMap()
	{
		if (blurMap)
		{
			int size = rawDensityTexture.width;
			blurCompute.SetInts("brushCentre", 0, 0, 0);
			blurCompute.SetInt("blurRadius", blurRadius);
			blurCompute.SetInt("textureSize", rawDensityTexture.width);
			ComputeHelper.Dispatch(blurCompute, size, size, size);
		}
	}

	public void GenerateChunk(Chunk chunk)
	{


		// Marching cubes
		int numVoxelsPerAxis = numPointsPerAxis - 1;
		int marchKernel = 0;


		meshCompute.SetInt("textureSize", processedDensityTexture.width);
		meshCompute.SetInt("numPointsPerAxis", numPointsPerAxis);
		meshCompute.SetFloat("isoLevel", isoLevel);
		meshCompute.SetFloat("planetSize", boundsSize);
		triangleBuffer.SetCounterValue(0);
		meshCompute.SetBuffer(marchKernel, "triangles", triangleBuffer);

		Vector3 chunkCoord = (Vector3)chunk.id * (numPointsPerAxis - 1);
		meshCompute.SetVector("chunkCoord", chunkCoord);

		ComputeHelper.Dispatch(meshCompute, numVoxelsPerAxis, numVoxelsPerAxis, numVoxelsPerAxis, marchKernel);

		// Create mesh
		int[] vertexCountData = new int[1];
		triCountBuffer.SetData(vertexCountData);
		ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);

		timer_fetchVertexData.Start();
		triCountBuffer.GetData(vertexCountData);

		int numVertices = vertexCountData[0] * 3;

		// Fetch vertex data from GPU

		triangleBuffer.GetData(vertexDataArray, 0, 0, numVertices);

		timer_fetchVertexData.Stop();

		//CreateMesh(vertices);
		timer_processVertexData.Start();
		chunk.CreateMesh(vertexDataArray, numVertices, useFlatShading);
		timer_processVertexData.Stop();
	}


    void Update()
	{

		material.SetTexture("_biomeNoiseTex", biomeNoiseTex);
		material.SetTexture("_warmthNoiseTex", tempNoiseTex);
		material.SetTexture("_rainfallNoiseTex", rainfallNoiseTex);

		// TODO: move somewhere more sensible
		material.SetTexture("DensityTex", originalMap);
		//material.SetFloat("oceanRadius", FindObjectOfType<Water>().radius);
		material.SetFloat("planetBoundsSize", boundsSize);

		/*
		if (Input.GetKeyDown(KeyCode.G))
		{
			Debug.Log("Generate");
			GenerateAllChunks();
		}
		*/
	}


	void CreateBuffers()
	{
		int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
		int numVoxelsPerAxis = numPointsPerAxis - 1;
		int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
		int maxTriangleCount = numVoxels * 5;
		int maxVertexCount = maxTriangleCount * 3;

		triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
		triangleBuffer = new ComputeBuffer(maxVertexCount, ComputeHelper.GetStride<VertexData>(), ComputeBufferType.Append);
		vertexDataArray = new VertexData[maxVertexCount];
	}

	void ReleaseBuffers()
	{
		ComputeHelper.Release(triangleBuffer, triCountBuffer);
	}

	void OnDestroy()
	{
		ReleaseBuffers();
		foreach (Chunk chunk in chunks)
		{
			chunk.Release();
		}
	}


	void CreateChunks()
	{
		chunks = new Chunk[numChunks * numChunks * numChunks];
		float chunkSize = (boundsSize) / numChunks;
		int i = 0;

		for (int y = 0; y < numChunks; y++)
		{
			for (int x = 0; x < numChunks; x++)
			{
				for (int z = 0; z < numChunks; z++)
				{
					Vector3Int coord = new Vector3Int(x, y, z);
					float posX = (-(numChunks - 1f) / 2 + x) * chunkSize;
					float posY = (-(numChunks - 1f) / 2 + y) * chunkSize;
					float posZ = (-(numChunks - 1f) / 2 + z) * chunkSize;
					Vector3 centre = new Vector3(posX, posY, posZ);

					GameObject meshHolder = new GameObject($"Chunk ({x}, {y}, {z})");
					meshHolder.transform.parent = transform;
					meshHolder.layer = gameObject.layer;

					Chunk chunk = new Chunk(coord, centre, chunkSize, numPointsPerAxis, meshHolder);
					chunk.SetMaterial(material);
					chunks[i] = chunk;
					i++;
				}
			}
		}
	}


	public void Terraform(Vector3 point, float weight, float radius)
	{

		int editTextureSize = rawDensityTexture.width;
		float editPixelWorldSize = boundsSize / editTextureSize;
		int editRadius = Mathf.CeilToInt(radius / editPixelWorldSize);
		//Debug.Log(editPixelWorldSize + "  " + editRadius);

		float tx = Mathf.Clamp01((point.x + boundsSize / 2) / boundsSize);
		float ty = Mathf.Clamp01((point.y + boundsSize / 2) / boundsSize);
		float tz = Mathf.Clamp01((point.z + boundsSize / 2) / boundsSize);

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

		if (blurMap)
		{
			blurCompute.SetInt("textureSize", rawDensityTexture.width);
			blurCompute.SetInts("brushCentre", editX - blurRadius - editRadius, editY - blurRadius - editRadius, editZ - blurRadius - editRadius);
			blurCompute.SetInt("blurRadius", blurRadius);
			blurCompute.SetInt("brushRadius", editRadius);
			int k = (editRadius + blurRadius) * 2;
			ComputeHelper.Dispatch(blurCompute, k, k, k);
		}

		//ComputeHelper.CopyRenderTexture3D(originalMap, processedDensityTexture);

		float worldRadius = (editRadius + 1 + ((blurMap) ? blurRadius : 0)) * editPixelWorldSize;
		for (int i = 0; i < chunks.Length; i++)
		{
			Chunk chunk = chunks[i];
			if (MathUtility.SphereIntersectsBox(point, worldRadius, chunk.centre, Vector3.one * chunk.size))
			{

				chunk.terra = true;
				GenerateChunk(chunk);

			}
		}
	}



	public void PlaceTree(Vector3 point, float weight, float radius)
	{
		Vector3 Placepoint = new Vector3(point.x,point.y + 10, point.z);

		int editTextureSize = rawDensityTexture.width;
		float editPixelWorldSize = boundsSize / editTextureSize;
		int editRadius = Mathf.CeilToInt(radius / editPixelWorldSize);
		//Debug.Log(editPixelWorldSize + "  " + editRadius);

		float tx = Mathf.Clamp01((point.x + boundsSize / 2) / boundsSize);
		float ty = Mathf.Clamp01((point.y + boundsSize / 2) / boundsSize);
		float tz = Mathf.Clamp01((point.z + boundsSize / 2) / boundsSize);

		int editX = Mathf.RoundToInt(tx * (editTextureSize - 1));
		int editY = Mathf.RoundToInt(ty * (editTextureSize - 1));
		int editZ = Mathf.RoundToInt(tz * (editTextureSize - 1));

		treeCompute.SetFloat("weight", weight);
		treeCompute.SetFloat("deltaTime", Time.deltaTime);
		treeCompute.SetInts("brushCentre", editX, editY, editZ);
		treeCompute.SetInt("brushRadius", editRadius);

		treeCompute.SetInt("size", editTextureSize);
		ComputeHelper.Dispatch(treeCompute, editTextureSize, editTextureSize, editTextureSize);

		//ProcessDensityMap();
		int size = rawDensityTexture.width;

		if (blurMap)
		{
			blurCompute.SetInt("textureSize", rawDensityTexture.width);
			blurCompute.SetInts("brushCentre", editX - blurRadius - editRadius, editY - blurRadius - editRadius, editZ - blurRadius - editRadius);
			blurCompute.SetInt("blurRadius", blurRadius);
			blurCompute.SetInt("brushRadius", editRadius);
			int k = (editRadius + blurRadius) * 2;
			ComputeHelper.Dispatch(blurCompute, k, k, k);
		}

		//ComputeHelper.CopyRenderTexture3D(originalMap, processedDensityTexture);

		float worldRadius = (editRadius + 1 + ((blurMap) ? blurRadius : 0)) * editPixelWorldSize;
		for (int i = 0; i < chunks.Length; i++)
		{
			Chunk chunk = chunks[i];
			if (MathUtility.SphereIntersectsBox(point, worldRadius, chunk.centre, Vector3.one * chunk.size))
			{

				chunk.terra = true;
				GenerateChunk(chunk);

			}
		}
	}

	void Create3DTexture(ref RenderTexture texture, int size, string name)
	{
		//
		var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat;
		if (texture == null || !texture.IsCreated() || texture.width != size || texture.height != size || texture.volumeDepth != size || texture.graphicsFormat != format)
		{
			//Debug.Log ("Create tex: update noise: " + updateNoise);
			if (texture != null)
			{
				texture.Release();
			}
			const int numBitsInDepthBuffer = 0;
			texture = new RenderTexture(size, size, numBitsInDepthBuffer);
			texture.graphicsFormat = format;
			texture.volumeDepth = size;
			texture.enableRandomWrite = true;
			texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;


			texture.Create();
		}
		texture.wrapMode = TextureWrapMode.Repeat;
		texture.filterMode = FilterMode.Bilinear;
		texture.name = name;
	}

	public Texture3D GenerateBase(int _textureSize)
	{
		TextureFormat _format = TextureFormat.RHalf;



		SimplexNoiseGenerator noise = new SimplexNoiseGenerator();
		Color[] colorArray = new Color[_textureSize * _textureSize * _textureSize];
		Texture3D texture = new Texture3D(_textureSize, _textureSize, _textureSize, _format, false);
		for (int x = 0 + 20; x < _textureSize - 20; x++)
		{
			for (int y = 0 + 20; y < _textureSize - 20; y++)
			{
				for (int z = 0 + 20; z < _textureSize - 20; z++)
				{
					//float value = noise.coherentNoise(x, y, z, _octaves, _multiplier, _amplitude, _lacunarity, _persistence);
					float value = 1;
					Color c = new Color(value, 0.0f, 0.0f, 1.0f);
					colorArray[x + (y * _textureSize) + (z * _textureSize * _textureSize)] = c;
				}
			}
		}


		texture.SetPixels(colorArray);
		texture.Apply();
		return texture;
	}
}