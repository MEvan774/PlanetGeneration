using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeHandler : MonoBehaviour
{
	public int mapWidth;
	public int mapHeight;
	public float noiseScale;

	public Renderer textureRenderer;

	public Material mat;
	public Color biomeGrassColor;

	[SerializeField] GameObject[] chunks;

	private void Start()
    {
		//GenerateBiome();

		//textureRenderer.material.mainTexture = GenerateTexture();
    }


	public Texture2D GenerateTexture()
    {
		Texture2D texture = new Texture2D(mapWidth, mapHeight);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
				Color color = CalculateColor(x, y);
				texture.SetPixel(x,y,color);
            }
        }

		texture.Apply();
		return texture;
    }

	Color CalculateColor(int x, int y)
    {
		float xCoord = (float)x / mapWidth * noiseScale;
		float yCoord = (float)y / mapHeight * noiseScale;

		float sample = Mathf.PerlinNoise(xCoord, yCoord);
		return new Color(sample, sample, sample);
    }


    public void GenerateBiome()
    {
		float[,] noiseMap = GenerateNoiseMap(mapWidth, mapHeight, noiseScale);

		
    }




    public void noise(int _textureSize)
    {
        for (int x = 0; x < _textureSize; x++)
        {
            for (int y = 0; y < _textureSize; y++)
            {

            }
        }
    }

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
	{
		float[,] noiseMap = new float[mapWidth, mapHeight];

		if (scale <= 0)
		{
			scale = 0.0001f;
		}

		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				float sampleX = x / scale;
				float sampleY = y / scale;

				float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
				noiseMap[x, y] = perlinValue;
			}
		}
		return noiseMap;
	}

	public void DrawNoiseMap(float[,] noiseMap)
	{
		int width = noiseMap.GetLength(0);
		int height = noiseMap.GetLength(1);

		Texture2D texture = new Texture2D(width, height);

		Color[] colorMap = new Color[width * height];
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
			}
		}
		texture.SetPixels(colorMap);
		texture.Apply();

		textureRenderer.sharedMaterial.mainTexture = texture;
		textureRenderer.transform.localScale = new Vector3(width, 1, height);

	}



}
