using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniTileToggle : MonoBehaviour
{
    // Start is called before the first frame update

    
    public static GameObject miniTileHolder;


    void Start()
    {
        miniTileHolder = this.gameObject;
    }


    public static void setMiniTilesActive(bool active)
    {
        if (active)
        {
            MapHandler.disableRendering = false;
            miniTileHolder.SetActive(true);
        }
        else
        {
            MapHandler.disableRendering = true;
            miniTileHolder.SetActive(false);
        }
    }
   
}
