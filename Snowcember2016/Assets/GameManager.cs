using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            //If 
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }
            if (_instance == null)
            {
                GameObject gm = new GameObject("Instance");
                DontDestroyOnLoad(gm);
                _instance = gm.AddComponent<GameManager>();
                _instance.loadAllAssets();

            }

            return _instance;
        }
    }

    public enum SceneBuild
    {
        GamecheeOpening = 0,
        MainMenu = 1,
        PlayMode = 2
    }

    private GameObject[] enemyUnits, friendlyUnits;
    private ControlScript[] aiScripts;
    private ControlScript playerScript;
    private Sprite[] floorTiles, wallTiles;
    public TextAsset howToPlay, credits;

    public enum MapType
    {
        Small,
        Medium,
        Large
    }
    public MapType mapType;

    public void loadAllAssets()
    {
        howToPlay = Resources.Load<TextAsset>("How To Play");
        credits = Resources.Load<TextAsset>("Credits");
        enemyUnits = Resources.LoadAll<GameObject>("Units/Enemy/");
        friendlyUnits = Resources.LoadAll<GameObject>("Units/Friendly/");
        aiScripts = Resources.LoadAll<ControlScript>("Control Scripts/");
        playerScript = Resources.Load<ControlScript>("Player Control");
        floorTiles = Resources.LoadAll<Sprite>("Floor Tiles/");
        wallTiles = Resources.LoadAll<Sprite>("Wall Tiles/");
    }

    public void SetupSmallMap(CameraScript cam)
    {
        GameObject room = new GameObject("Room Gen");
        RoomGen gen = room.AddComponent<RoomGen>();
        gen.cam = cam;
        gen.columns = 20;
        gen.rows = 20;

        //width
        gen.w_min = 10;
        gen.w_max = 10;
        //height
        gen.h_min = 10;
        gen.h_max = 10;
        //enemy
        gen.e_min = 4;
        gen.e_max = 4;

        gen.friendlyCount = 4;
        gen.isAuto = false;
        gen.hasFlatTop = false;
        gen.cellSize = 1.0f;
        gen.enemyScripts = aiScripts;
        gen.enemyUnits = enemyUnits;
        gen.friendlyUnits = friendlyUnits;
        gen.floorTiles = floorTiles;
        gen.wallTiles = wallTiles;
        gen.playerScript = playerScript;

    }

    public void SetupMediumMap(CameraScript cam)
    {
        GameObject room = new GameObject("Room Gen");
        RoomGen gen = room.AddComponent<RoomGen>();
        gen.cam = cam;
        gen.columns = 20;
        gen.rows = 20;

        //width
        gen.w_min = 15;
        gen.w_max = 15;
        //height
        gen.h_min = 15;
        gen.h_max = 15;
        //enemy
        gen.e_min = 6;
        gen.e_max = 6;

        gen.friendlyCount = 6;
        gen.isAuto = false;
        gen.hasFlatTop = false;
        gen.cellSize = 1.0f;
        gen.enemyScripts = aiScripts;
        gen.enemyUnits = enemyUnits;
        gen.friendlyUnits = friendlyUnits;
        gen.floorTiles = floorTiles;
        gen.wallTiles = wallTiles;
        gen.playerScript = playerScript;
    }

    public void SetupLargeMap(CameraScript cam)
    {
        GameObject room = new GameObject("Room Gen");
        RoomGen gen = room.AddComponent<RoomGen>();
        gen.cam = cam;
        gen.columns = 30;
        gen.rows = 30;

        //width
        gen.w_min = 25;
        gen.w_max = 25;
        //height
        gen.h_min = 25;
        gen.h_max = 25;
        //enemy
        gen.e_min = 10;
        gen.e_max = 10;

        gen.friendlyCount = 10;
        gen.isAuto = false;
        gen.hasFlatTop = false;
        gen.cellSize = 1.0f;
        gen.enemyScripts = aiScripts;
        gen.enemyUnits = enemyUnits;
        gen.friendlyUnits = friendlyUnits;
        gen.floorTiles = floorTiles;
        gen.wallTiles = wallTiles;
        gen.playerScript = playerScript;
    }

    public void changeMapType(MapType type)
    {
        mapType = type;
    }
}
