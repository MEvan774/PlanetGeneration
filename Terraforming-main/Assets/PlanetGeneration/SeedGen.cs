using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedGen : MonoBehaviour
{
    public string stringSeed = "default";
    public int seed = 0;
    public bool useStringSeed;
    public bool randomizedSeed;

    private void Awake()
    {
        if (useStringSeed)
            seed = stringSeed.GetHashCode();

        if (randomizedSeed)
            seed = Random.Range(0, 99999);

        Random.InitState(seed);
    }
}
