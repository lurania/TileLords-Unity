using DataModel.Client;
using DataModel.Common;
using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/// <summary>
/// Main class to handle anything concerning the map (creating , updating etc.)
/// </summary>
public class MapHandler : MonoBehaviour
{

    public GameObject emptyCube;
    public GameObject otherPlayersObj;
    public GameObject playerObj;
    public GameObject miniTileHolder;
    public PlayerU playerU;

    List<GameObject> otherPlayers = new List<GameObject>();

    public LoadingScreen loadingScreen;
    public ClientConnection clientConnection;


    public Dictionary<PlusCode, MiniTileUnity> plusCodeMiniTileDict;


    public GPSUnity gps;


    List<MiniTile> miniTileList;
    bool firstExec = true;
    int counter = 0;

    List<GPS> list = DataModelFunctions.GPSNodesWithOffsets(new GPS(46.636833, -31.861802), 0.000350, 0.000150, 60);



    public bool useFakeGPS = false;
    public bool useRealGPS = true;

    GPS fakeGPS = new GPS(46.636833, -31.861802);
    PlusCode currentLocation;
    bool movementCompleted = true;
    bool nonNullUpdate = false;

    //used by loading screen
    public bool stopFakeUpdate = false;
    public bool loading = true;
    bool cubesInitialized = false;
    bool loadingSprites = true;



    Dictionary<string, List<Matrix4x4>> gpuInstancingData = new Dictionary<string, List<Matrix4x4>>();
    Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();
    Dictionary<string, Material> cubeMaterialDict = new Dictionary<string, Material>();
    public Dictionary<string, Sprite> iconSpriteDict = new Dictionary<string, Sprite>();
    public List<GameObject> cubeList = new List<GameObject>();

    int updateCounter = 0;

    public GameObject oneResource;
    public GameObject multiResource;

    public static bool disableRendering = false;

    BatchContentMessage message;

    //loads assets and subscribes to the map updates of the DLL
    public void LoadAndSubscribe()
    {

        Debug.Log("Loading and Subscribing!");
        clientConnection.ClientInstance.SendMessage((FromGps(new GPS(0, 0))));
        counter = 0;
        list = DataModelFunctions.GPSNodesWithOffsets(new GPS(46.636833, -31.861802), 0.000350, 0.000150, 60);
        plusCodeMiniTileDict = new Dictionary<PlusCode, MiniTileUnity>();

        miniTileList = new List<MiniTile>();



        //******************************LOADING**********************************
        //load all assets into a dictionary


        LoadFromResourceFolder();

        StartCoroutine(LoadSprites());

        //************************************************************************




        //if gps is enabled, send real gps position to the server
        if (useRealGPS)
        {
            UnityFunc.GPSCreated(gps).Subscribe(v =>
            {
                clientConnection.ClientInstance.SendMessage(new UserGpsMessage() { Lat = v.Lat, Lon = v.Lon });
            });


        }


        StartCoroutine(WaitForLoading());


        //subscribe to map updates from Client DLL

        var contentUpdate = clientConnection.ClientInstance.InboundTraffic.Where(v => v is BatchContentMessage).Select(v => v as BatchContentMessage);
        clientConnection.ClientInstance.ClientMapStream.CombineLatest(contentUpdate.DistinctUntilChanged(), (map, content) => new { map, content })
                                                       .ObserveOnMainThread()
                                                       .Subscribe(update =>
                                                       {

                                                           message = update.content;

                                                           RebuildMap(update.map);

                                                           if (cubesInitialized)
                                                           {
                                                               RebuildContentOnMap(update.content);
                                                           }
                                                       });



    }


    /// <summary>
    /// Function to add a sprite icon above a miniTile with content
    /// </summary>
    /// <param name="icon">The sprite that needs to be changed.</param>
    /// <param name="contentIndex">The index of this content in the contentList (contentList is attached to MiniTileU and represents all content that is on the miniTile).</param>
    /// <param name="miniU">The MiniTileUnity script attached to the miniTile gameObject.</param>
    public void SetIcon(Image icon, int contentIndex, MiniTileUnity miniU)
    {


        if (miniU.contentList[contentIndex].Type == ContentType.PLAYER)
        {

            icon.sprite = iconSpriteDict["PlayerIcon"];
        }
        else if (miniU.contentList[contentIndex].Type == ContentType.QUESTLEVEL1)
        {

            icon.sprite = iconSpriteDict["QuestIcon"];
        }
        else if (miniU.contentList[contentIndex].Type == ContentType.RESOURCE)
        {

            icon.sprite = iconSpriteDict[((miniU.contentList[contentIndex].ResourceType + "").Substring(0, 1)) + (miniU.contentList[contentIndex].ResourceType + "").ToLower().Substring(1) + "Icon"];
        }



    }




    /// <summary>
    /// Rebuilds the entire map with the data received from the Client DLL (the seed generator generates this data, therefore the server is not needed here)
    /// </summary>
    /// <param name="map">The map data from the DLL.</param>

    public void RebuildMap(UnityMapMessage map)
    {

        PlayerU.currentPlusCodeLocation = map.ClientLocation;
        nonNullUpdate = true;
        miniTileList = map.VisibleMap;


        //create cubes on first execution
        if (firstExec)
        {
            CreateCubes(map.VisibleMap);
            firstExec = false;
        }
        //update cubes appearance
        else
        {

            //updates the current player location on unity's side
            if (currentLocation.Code != map.ClientLocation.Code)
            {

                StartCoroutine(playerU.WalkCycle());
                currentLocation = map.ClientLocation;


            }
            RecreateMap(map.VisibleMap);

        }

    }




    /// <summary>
    /// Creates all the icons representing the content on the map, using the data received from the server
    /// </summary>
    /// <param name="contentMessage">The content data from the server.</param>
    public void RebuildContentOnMap(BatchContentMessage contentMessage)
    {

        //clear the list containing the content on each miniTileUnity
        foreach (var plusMiniTilePair in plusCodeMiniTileDict)
        {
            MiniTileUnity miniU = plusMiniTilePair.Value;
            miniU.contentList.Clear();

        }

        //add all content to the tile it belongs to
        var list = contentMessage.ContentList;
        foreach (var content in list)
        {
            if (plusCodeMiniTileDict.TryGetValue(new PlusCode(content.Location, 10), out MiniTileUnity miniTileU))
            {


                miniTileU.contentList.Add(content);

            }
        }

        foreach (var plusMiniTilePair in plusCodeMiniTileDict)
        {
            MiniTileUnity miniU = plusMiniTilePair.Value;

            //respawn all existing icons

            miniU.icon1.sprite = iconSpriteDict["X"];
            miniU.icon2.sprite = iconSpriteDict["X"];
            miniU.icon3.sprite = iconSpriteDict["X"];
            miniU.singleIcon.sprite = iconSpriteDict["X"];
            miniU.oneResource.SetActive(false);
            miniU.multiResource.SetActive(false);


            if (miniU.contentList.Count > 0)
            {
                if (miniU.contentList.Count > 1)
                {
                    if (miniU.contentList.Count <= 3)
                    {
                        miniU.multiResource.SetActive(true);
                        if (miniU.icon1.sprite.name == "X") //see which icon clone still has a free slot (marked with a sprite called "X")
                        {

                            SetIcon(miniU.icon1, 0, miniU);


                        }
                        if (miniU.icon2.sprite.name == "X" && miniU.contentList.Count >= 2)
                        {

                            SetIcon(miniU.icon2, 1, miniU);

                        }
                        if (miniU.icon3.sprite.name == "X" && miniU.contentList.Count >= 3)
                        {

                            SetIcon(miniU.icon3, 2, miniU);

                        }

                    }
                    else //more than 3 resources -> show a special icon for this
                    {

                        miniU.oneResource.SetActive(true);
                        miniU.singleIcon.sprite = iconSpriteDict["MultiContentIcon"];

                    }

                }
                else //only 1 content on tile
                {

                    miniU.oneResource.SetActive(true);
                    SetIcon(miniU.singleIcon, 0, miniU);
                }


            }



        }
    }



    /// <summary>
    /// Function to load all resources currently in the Material and Prefabs folder under Resource folder
    /// </summary>

    public void LoadFromResourceFolder()
    {
        int lengthOfAllWorldObjects = System.Enum.GetNames(typeof(WorldObjectType)).Length;
        int lengthOfAllMiniTileTypes = System.Enum.GetNames(typeof(MiniTileType)).Length;

        for (int i = 0; i < lengthOfAllWorldObjects; i++)
        {
            if (!prefabDict.ContainsKey((WorldObjectType)i + ""))
            {
                prefabDict.Add((WorldObjectType)i + "", Resources.Load<GameObject>("Prefabs/" + (WorldObjectType)i));
            }
            else
            {
                break;
            }
        }

        for (int i = 0; i < lengthOfAllMiniTileTypes; i++)
        {
            if (!cubeMaterialDict.ContainsKey((MiniTileType)i + ""))
            {
                cubeMaterialDict.Add((MiniTileType)i + "", Resources.Load<Material>("Material/" + (MiniTileType)i));
            }
            else
            {
                break;
            }
        }
    }

    //Addressable to load Icons
    public IEnumerator LoadSprites()
    {
        Debug.Log("StartingToLoadSprites");
        Addressables.LoadAssetsAsync<Sprite>("Icons", v => { }).Completed += OnLoadDone;
        yield return null;

    }

    private void OnLoadDone(AsyncOperationHandle<IList<Sprite>> obj)
    {

        foreach (var item in obj.Result)
        {
            if (!iconSpriteDict.ContainsKey(item.name))
            {
                Debug.Log("Adding: " + item.name);
                iconSpriteDict.Add(item.name, item);
            }
            else
            {

                break;
            }
        }

        loadingSprites = false;
        Debug.Log("Sprites loaded");


    }


    //calling the gpuInstancing every frame
    private void Update()
    {
        if (!disableRendering)
        {
            if (gpuInstancingData != null)
            {
                foreach (var prefabData in gpuInstancingData)
                {
                    //dont render anything on tiles with no tilecontent
                    if (prefabData.Key != "Empty")
                    {
                        //render the previously collected meshes, materials etc.
                        Graphics.DrawMeshInstanced(prefabDict[prefabData.Key].GetComponent<MeshFilter>().sharedMesh, 0, prefabDict[prefabData.Key].GetComponent<MeshRenderer>().sharedMaterial, prefabData.Value.ToArray(), prefabData.Value.ToArray().Length, null);
                    }
                }
            }
        }

    }

    //move the player smoothly onto the next tile, recreate the map and teleport the player back into the middle
    /*   private IEnumerator MovePlayerObject(float time, Vector3 tilePos)
       {
           Vector3 startingPos = playerObj.transform.position;



           if (!movementCompleted)
           {
               yield break;
           }

           movementCompleted = false;
           float elapsedTime = 0;
           while (elapsedTime < time)
           {


               playerObj.transform.position = Vector3.Lerp(startingPos, new Vector3(tilePos.x, tilePos.y + 1f, tilePos.z), (elapsedTime / time));
               elapsedTime += Time.deltaTime;
               yield return null;
           }
           movementCompleted = true;
           //playerObj.transform.position = new Vector3(tilePos.x, tilePos.y + 1f, tilePos.z);
           RecreateMap(miniTileList);
           if (!loadingSprites)
           {
               RebuildContentOnMap(message);
           }
           playerObj.transform.position = new Vector3(10, 1, 10);

       } */


    //fake movement for non GPS testing
    public void MoveRight()
    {
        fakeGPS.Lon = fakeGPS.Lon + 0.00013;
        fakeGPS.Lon = System.Math.Round(fakeGPS.Lon, 5);
        clientConnection.ClientInstance.SendMessage(new UserGpsMessage() { Lat = fakeGPS.Lat, Lon = fakeGPS.Lon });
    }
    public void MoveLeft()
    {
        fakeGPS.Lon = fakeGPS.Lon - 0.00013;
        fakeGPS.Lon = System.Math.Round(fakeGPS.Lon, 5);
        clientConnection.ClientInstance.SendMessage(new UserGpsMessage() { Lat = fakeGPS.Lat, Lon = fakeGPS.Lon });


    }
    public void MoveDown()
    {
        fakeGPS.Lat = fakeGPS.Lat - 0.00013;
        fakeGPS.Lat = System.Math.Round(fakeGPS.Lat, 5);

        clientConnection.ClientInstance.SendMessage(new UserGpsMessage() { Lat = fakeGPS.Lat, Lon = fakeGPS.Lon }); ;


    }
    public void MoveUp()
    {
        fakeGPS.Lat = fakeGPS.Lat + 0.00013;
        fakeGPS.Lat = System.Math.Round(fakeGPS.Lat, 5);
        clientConnection.ClientInstance.SendMessage(new UserGpsMessage() { Lat = fakeGPS.Lat, Lon = fakeGPS.Lon });
    }

    //keep sending a (fake) gps position while waiting for the map update (from DLL)
    IEnumerator WaitForLoading()
    {
        while (!stopFakeUpdate)
        {

            while (!nonNullUpdate)
            {
                if (useFakeGPS)
                {
                    Debug.Log("gpsfake" + FromGps(list[counter]).Lat + " " + FromGps(list[counter]).Lon);
                    clientConnection.ClientInstance.SendMessage(FromGps(list[counter]));

                    yield return new WaitForSeconds(2);
                    clientConnection.ClientInstance.SendMessage(FromGps(list[counter + 1]));
                }
            }
            //the map has been created properly, loading screen can be stopped
            if (nonNullUpdate && !loadingSprites)
            {

                Debug.Log("CubeSpawer Done!");
                stopFakeUpdate = true;
                loading = false;
                loadingScreen.CubeSpawnerToggleReady();

            }
            yield return new WaitForSeconds(2);
        }
    }

    UserGpsMessage FromGps(GPS gps)
    {
        return new UserGpsMessage() { Lat = gps.Lat, Lon = gps.Lon };
    }




    /// <summary>
    /// On start create the miniTile cube objects and attach a miniTileUnity object to them. Those objects will not be destroyed anymore!
    /// </summary>
    /// <param name="miniTileList">The map data from the DLL converted to a list.</param>
    public void CreateCubes(List<MiniTile> miniTileList)
    {

        if (miniTileList != null)
        {
            int size = (int)Mathf.Sqrt(miniTileList.Count);
            for (int z = size - 1; z >= 0; z--)
            {
                for (int x = 0; x < size; x++)
                {

                    //create the miniTile gameObjects

                    GameObject myCube = Instantiate(emptyCube, new Vector3(x, 0, z), Quaternion.Euler(0, 90, 0), miniTileHolder.transform);
                    //myCube.transform.localPosition = new Vector3(x, 0, z);

                    //attach MiniTileUnity script and assign the gameObject to it
                    MiniTileUnity miniTile = myCube.AddComponent<MiniTileUnity>();
                    miniTile.setMiniTile(myCube);
                    cubeList.Add(myCube);

                    GameObject multiRes = Instantiate(multiResource);
                    GameObject oneRes = Instantiate(oneResource);

                    oneRes.transform.position = new Vector3(myCube.transform.position.x, myCube.transform.position.y + 2, myCube.transform.position.z);
                    oneRes.transform.SetParent(myCube.transform);
                    GameObject buttonOne = oneRes.transform.Find("BubbleButton").gameObject;
                    Image iconOne = buttonOne.transform.Find("Icon").gameObject.GetComponent<Image>();
                    miniTile.singleIcon = iconOne;


                    multiRes.transform.position = new Vector3(myCube.transform.position.x, myCube.transform.position.y + 2, myCube.transform.position.z);
                    multiRes.transform.SetParent(myCube.transform);
                    GameObject button = multiRes.transform.Find("BubbleButton").gameObject;
                    Image icon1 = button.transform.Find("Icon1").gameObject.GetComponent<Image>();
                    Image icon2 = button.transform.Find("Icon2").gameObject.GetComponent<Image>();
                    Image icon3 = button.transform.Find("Icon3").gameObject.GetComponent<Image>();

                    miniTile.multiResource = multiRes;
                    miniTile.oneResource = oneRes;
                    miniTile.icon1 = icon1;
                    miniTile.icon2 = icon2;
                    miniTile.icon3 = icon3;
                    miniTile.icon1 = icon1;

                    cubesInitialized = true;
                }
            }
            RecreateMap(miniTileList);
        }

    }

    //handle the map update from the DLL by collecting all objects needed for the GPUInstancing, handling other player objects, and setting cube materials
    public void RecreateMap(List<MiniTile> miniTileList)
    {
        //destroy all other player objects 
        foreach (var gameObject in otherPlayers)
        {
            GameObject.Destroy(gameObject);
        }

        //make sure there is a list to access
        if (miniTileList != null)
        {
            //release the old information from the dictionaries 
            plusCodeMiniTileDict.Clear();
            gpuInstancingData.Clear();

            int listCounter = 0;

            foreach (MiniTile miniT in miniTileList)
            {
                //get the GameObject from the list, change the name, get the attached MiniTileUnity script and change miniTile-relating values in the script
                GameObject myCube = cubeList[listCounter];

                myCube.name = miniT.MiniTileId.Code;
                MiniTileUnity miniTileU = myCube.GetComponent<MiniTileUnity>();
                miniTileU.setPlusCode(miniT.MiniTileId);
                miniTileU.setMiniTileMaterial(cubeMaterialDict[miniT.TileType.ToString()]);
                miniTileU.setMiniTile(miniT);

                plusCodeMiniTileDict.Add(miniT.MiniTileId, miniTileU);


                WorldObject worldObject = null;

                //check that the miniTile has a worldObject 
                if (miniT.Content != null)
                {
                    if (miniT.Content.Count > 0)
                    {
                        foreach (var content in miniT.Content)
                        {
                            if (content is WorldObject)
                            {
                                //cast content to worldobject

                                worldObject = (WorldObject)content;

                            }


                        }
                    }
                }


                //set rotation and scale with a seed that consists from the hashcode of the miniTile pluscode
                System.Random r = new System.Random((miniT.MiniTileId.Code.GetHashCode()));
                int tmp = r.Next(20, 30);
                float f = ((float)tmp) / 100f;
                int i = r.Next(0, 5);



                if (worldObject != null && miniT.MiniTileId.Code != currentLocation.Code)
                {
                    //did another miniTile already have the same tileContent
                    if (gpuInstancingData.ContainsKey(worldObject.Type.ToString()))
                    {
                        //create gpu instancing data...
                        gpuInstancingData[worldObject.Type.ToString()].Add(Matrix4x4.TRS(new Vector3(myCube.transform.position.x, myCube.transform.position.y + 0.5f,
                            myCube.transform.position.z), Quaternion.Euler(-90, 90 * i, 0), new Vector3(f, f, f)));
                    }

                    //otherwise add it to the dictionary
                    else
                    {
                        gpuInstancingData.Add(worldObject.Type.ToString(), new List<Matrix4x4>());
                        gpuInstancingData[worldObject.Type.ToString()].Add(Matrix4x4.TRS(new Vector3(myCube.transform.position.x, myCube.transform.position.y + 0.5f,
                            myCube.transform.position.z), Quaternion.Euler(-90, 90 * i, 0), new Vector3(f, f, f)));

                    }

                }
                listCounter++;

            }



        }
    }






}

