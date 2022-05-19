using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Biome")]
public class Biome_ScriptObj : ScriptableObject
{
    [Header("BiomeGrass color")]
    public Color lightGrassColor;
    public Color darkGrassColor;

    [Header("Texture property")]
    public int textureSize = 32;
    public Texture3D noiseTex;

    [Header("Graph property")]
    public Vector2 graphLoc;
    public float maxWarmth;
    public float minWarmth;
    public float maxHumiddity;
    public float minHumiddity;
    public float graphSize;

    [Header("Biomevegatation")]
    public GameObject[] treePrefabs;

    [Header("Noise properties")]
    [Range(1, 20)] public int octaves = 1;
    [Range(1, 10)] public int multiplier = 2;
    [Range(0.0f, 15.0f)] public float amplitude = 0.5f;
    [Range(0.0f, 500.0f)] public float lacunarity = 2.0f;
    [Range(0.0f, 500.0f)] public float persistence = 0.9f;
}
