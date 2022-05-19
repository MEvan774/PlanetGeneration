using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorGen : MonoBehaviour
{
    [SerializeField] ColorSettings settings;
    Texture2D texture;
    const int textureResolution = 50;

    public void UpdateSettings(ColorSettings settings)
    {
        this.settings = settings;
        if(texture == null || texture.height != settings.biomeColorSettings.biomes.Length)
        {
            texture = new Texture2D(textureResolution, settings.biomeColorSettings.biomes.Length);
        }
    }

    public void UpdateElevation(MinMax elevationMinMax)
    {
        settings.planetMat.SetVector("_elevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max));
    }

    public float BiomePercentFromPoint(Vector3 pointOnUnitSphere)
    {
        float heightPercent = (pointOnUnitSphere.y + 1) / 2;
        float BiomeIndex = 0;
        int numBiomes = settings.biomeColorSettings.biomes.Length;

        for (int i = 0; i < numBiomes; i++)
        {
            if (settings.biomeColorSettings.biomes[i].startHeight < heightPercent)
                BiomeIndex = i;
            else
                break;
        }

        return BiomeIndex / Mathf.Max(1, numBiomes - 1);
    }

    public void UpdateColors()
    {
        Color[] colors = new Color[textureResolution];
        int colorIndex = 0;
        foreach (var biome in settings.biomeColorSettings.biomes)
        {
            for (int i = 0; i < textureResolution; i++)
            {
                Color gradientCol = biome.gradient.Evaluate(i / (textureResolution - 1f));
                Color tintCol = biome.tint;
                colors[colorIndex] = gradientCol * (1 - biome.tintPercent) + tintCol * biome.tintPercent;
                colorIndex++;
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        settings.planetMat.SetTexture("_texture", texture);
    }
}
