using DataModel.Common;
using DataModel.Common.GameModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniTileSelectionHandler : MonoBehaviour
{
    // Start is called before the first frame update

    public static event Action<GameObject> TileWithResource;

    public static event Action<GameObject> TileWithEnemy;

    public static event Action<GameObject> TileWithBuildable;

    public static event Action<GameObject> TileWithTradeable;

    public static event Action<GameObject> TileWithSearchable;

    [SerializeField] private SomeUIElement uiElement;
    public static GameObject SelectedMiniTile { get; private set; }

    private void Awake()
    {

        
        UIController.MiniTileHasBeenSelected += HandleMiniTileSelection;
        TileWithResource += HandleTileWithResource;



    }

    private void OnDestroy()
    {

        UIController.MiniTileHasBeenSelected -= HandleMiniTileSelection;
        TileWithResource -= HandleTileWithResource;
    }



    private void HandleMiniTileSelection(GameObject miniTileObject)
    {
        //disable menu when no gameObject is selected
        if(miniTileObject == null)
        {
            uiElement.DisableAll();
        }
        else { 

        Debug.Log("HandleMiniTileSelection");

        //determine what content is on the miniTile
        MiniTileUnity miniTileU = miniTileObject.GetComponent<MiniTileUnity>();
        MiniTile miniTile = miniTileU.getMiniTile();

            //example code
            foreach (var content in miniTile.Content)
            {
                if (content is WorldObject)
                {
                    WorldObject contentW = (WorldObject)content;
                    if (contentW.Type == WorldObjectType.Empty)
                    {
                        TileWithResource?.Invoke(null); //do nothing if the content is empty
                    }
                    else
                    {
                        TileWithResource?.Invoke(miniTileObject);
                    }
                }

            }


           /* //if minitile has tilecontent
            //if tilecontent is resource
            TileWithResource?.Invoke(miniTileObject);
            //if tilecontent is enemy
            TileWithEnemy?.Invoke(miniTileObject);

            //if tilecontent has no building and is able to hold a building
            TileWithBuildable?.Invoke(miniTileObject);

            //if tilecontent has a tradeable town / tradeable outpost / caravan on this tile
            TileWithTradeable?.Invoke(miniTileObject);

            //if tile can be searched
            TileWithSearchable?.Invoke(miniTileObject); */
        }

    }


    private void HandleTileWithResource(GameObject miniTileWithResource)
    {
        
       // uiElement.Bind(miniTileWithResource);
    }
}
