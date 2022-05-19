using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeHandler : MonoBehaviour
{
	[Header("vegatation Settings")]
	[SerializeField] private LayerMask groundLayer;

	[HideInInspector] public float[] graphSizes;
	[HideInInspector] public float[] graphLocationsX;
	[HideInInspector] public float[] graphLocationsY;

	[HideInInspector] public float[] minWarmth;
	[HideInInspector] public float[] maxWarmth;

	[HideInInspector] public float[] minHumiddity;
	[HideInInspector] public float[] maxHumiddity;

	[HideInInspector] public Color[] lightColors;
	[HideInInspector] public Color[] darkColors;

	[HideInInspector] public GameObject[] treePrefabs;

	/*
	[Header("Texture Settings")]
	[SerializeField] private int _textureSize = 32;

	[Header("Noise Settings")]
	[SerializeField] [Range(1, 20)] private int _octaves = 1;
	[SerializeField] [Range(1, 10)] private int _multiplier = 2;
	[SerializeField] [Range(0.0f, 15.0f)] private float _amplitude = 0.5f;
	[SerializeField] [Range(0.0f, 500.0f)] private float _lacunarity = 2.0f;
	[SerializeField] [Range(0.0f, 500.0f)] private float _persistence = 0.9f;
	*/

	[Header("NoiseSettings")]
	[SerializeField] private TextureFormat _format = TextureFormat.RHalf;

	public int mapWidth;
	public int mapHeight;
	public int mapSize;
	public float noiseScale;

	public Renderer textureRenderer;

	public Material mat;
	public Color biomeGrassColor;

	public Biome_ScriptObj[] biomes;

	//--BiomeHeathmap--
	[SerializeField] private Texture2D biomeMap;
	private Vector2 biomeGraph;//X = Rain, Y = Warmth

    public Color BiomeColor(int x, int y)
    {


		Color pixelColor = biomeMap.GetPixel(x,y);
		return pixelColor;
    }

	public void BiomeManager(Material _mat)
    {
		lightColors = new Color[10];
		darkColors = new Color[10];
		graphLocationsX = new float[10];
		graphLocationsY = new float[10];

		minWarmth = new float[10];
		maxWarmth = new float[10];
		minHumiddity = new float[10];
		maxHumiddity = new float[10];

		graphSizes = new float[10];

		treePrefabs = new GameObject[10];

		_mat.SetInt("_biomeAmount", biomes.Length);

		for (int i = 0; i < biomes.Length; i++)
        {
			lightColors[i] = biomes[i].lightGrassColor;
			darkColors[i] = biomes[i].darkGrassColor;

			minWarmth[i] = biomes[i].minWarmth;
			maxWarmth[i] = biomes[i].maxWarmth;
			minHumiddity[i] = biomes[i].minHumiddity;
			maxHumiddity[i] = biomes[i].maxHumiddity;

			graphLocationsX[i] = biomes[i].graphLoc.x;
			graphLocationsY[i] = biomes[i].graphLoc.y;

			/*
			if (biomes[i].treePrefabs[i] != null)
            {
                for (int t = 0; t < biomes[i].treePrefabs.Length; t++)
                {

                }
            }
				*/

		}

		_mat.SetFloatArray("_minWarmth", minWarmth);
		_mat.SetFloatArray("_maxWarmth", maxWarmth);
		_mat.SetFloatArray("_minHumiddity", minHumiddity);
		_mat.SetFloatArray("_maxHumiddity", maxHumiddity);

		_mat.SetFloatArray("_graphLocY", graphLocationsY);
		
		_mat.SetColorArray("_biomeGrassLight", lightColors);
		_mat.SetColorArray("_biomeGrassDark", darkColors);
		_mat.SetFloatArray("_biomeGraphSize", graphSizes);
	}

	public void SetVegatation(int _genSize)
    {
		//if (biomes[0].treePrefabs[0] != null)
			//return;


			int planetEdge = _genSize + 5;
		int GenArea = _genSize * 2;
		float treePercentage = 0.01f;

		int rayDist = _genSize;




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
						 Instantiate(biomes[0].treePrefabs[0], hit.point, Quaternion.LookRotation(Vector3.forward , hit.normal));

					}
					//else
					//Debug.DrawRay(new Vector3(x, planetEdge, z), Vector3.down * 500, Color.green, 100);


				}


			}
        }

	}

	public void GetBiomeFromNoise(float _noise, Biome_ScriptObj[] _biomes)
    {
        for (int i = 0; i < biomes.Length; i++)
        {
			//warmth
			if(_noise >= _biomes[i].minWarmth && _noise <= _biomes[i].maxWarmth)
            {
				//return _biomes[i];
            }
        }

		//return _biomes[0];
    }

	float scale = 10;
	float offSetX = 0;
	float offSetY = 0;

	public void GenBiome()
    {
        for (int i = 0; i < biomes.Length; i++)
        {
			biomes[i].noiseTex = GenerateNoise(biomes[i].textureSize, biomes[i].octaves, biomes[i].multiplier, biomes[i].amplitude, biomes[i].lacunarity, biomes[i].persistence);
		}
    }

	public Texture3D GenerateBiomeNoise(int _textureSize, int _octavesMax, int _multiplierMax, float _amplitudeMax, float _lacunarityMax, float _persistenceMax)
	{
		int _octaves = Random.Range(1, _octavesMax);
		int _multiplier = Random.Range(1, _multiplierMax);
		float _amplitude = Random.Range(0, _amplitudeMax);
		float _lacunarity = Random.Range(0, _lacunarityMax);
		float _persistence = Random.Range(0, _persistenceMax);


		SimplexNoiseGenerator noise = new SimplexNoiseGenerator();
		Color[] colorArray = new Color[_textureSize * _textureSize * _textureSize];
		Texture3D texture = new Texture3D(_textureSize, _textureSize, _textureSize, _format, false);
		for (int x = 0; x < _textureSize; x++)
		{
			for (int y = 0; y < _textureSize; y++)
			{
				for (int z = 0; z < _textureSize; z++)
				{
					float value = noise.coherentNoise(x, y, z, _octaves, _multiplier, _amplitude, _lacunarity, _persistence);
					float warmthNoise = noise.coherentNoise(x, y, z);

					float[] noiseArray = new float[]
					{
						warmthNoise
					};

					Color c = new Color(value, 0.0f, 0.0f, 1.0f);
					colorArray[x + (y * _textureSize) + (z * _textureSize * _textureSize)] = c;
				}
			}
		}

		texture.SetPixels(colorArray);
		texture.Apply();
		return texture;
	}


	public Texture3D GenerateNoise(int _textureSize, int _octavesMax, int _multiplierMax, float _amplitudeMax, float _lacunarityMax, float _persistenceMax)
	{
		int _octaves = Random.Range(1, _octavesMax);
		int _multiplier = Random.Range(1, _multiplierMax);
		float _amplitude = Random.Range(0, _amplitudeMax);
		float _lacunarity = Random.Range(0, _lacunarityMax);
		float _persistence = Random.Range(0, _persistenceMax);


		SimplexNoiseGenerator noise = new SimplexNoiseGenerator();
		Color[] colorArray = new Color[_textureSize * _textureSize * _textureSize];
		Texture3D texture = new Texture3D(_textureSize, _textureSize, _textureSize, _format, false);
		for (int x = 0; x < _textureSize; x++)
		{
			for (int y = 0; y < _textureSize; y++)
			{
				for (int z = 0; z < _textureSize; z++)
				{
					float value = noise.coherentNoise(x, y, z, _octaves, _multiplier, _amplitude, _lacunarity, _persistence);

					Color c = new Color(value, 0.0f, 0.0f, 1.0f);
					colorArray[x + (y * _textureSize) + (z * _textureSize * _textureSize)] = c;
				}
			}
		}

		texture.SetPixels(colorArray);
		texture.Apply();
		return texture;
	}

	/*
	public static float Perlin3D(float x, float y, float z)
    {
		float ab = Mathf.PerlinNoise(x,y);
		float bc = Mathf.PerlinNoise(y,z);
		float ac = Mathf.PerlinNoise(x,z);

		float ba = Mathf.PerlinNoise(y,x);
		float cb = Mathf.PerlinNoise(z,y);
		float ca = Mathf.PerlinNoise(z,x);

		float abc = ab + bc + ac + ba + cb + ca;
		return abc / 6f;
    }
	*/
}
