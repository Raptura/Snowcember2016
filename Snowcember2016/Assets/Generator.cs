using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public CameraScript cam;

    // Use this for initialization
    void Start()
    {
        if (GameManager.instance.mapType == GameManager.MapType.Small)
            GameManager.instance.SetupSmallMap(cam);
        else if (GameManager.instance.mapType == GameManager.MapType.Medium)
            GameManager.instance.SetupMediumMap(cam);
        else if (GameManager.instance.mapType == GameManager.MapType.Large)
            GameManager.instance.SetupLargeMap(cam);
    }

}
