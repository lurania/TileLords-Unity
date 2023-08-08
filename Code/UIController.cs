using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    //[SerializeField] private SomeUIElement uiElement;
    public static event Action<GameObject> MiniTileHasBeenSelected;

    private void Awake()
    {

        // ClickController.SelectedGameObjectChanged += HandleSelection;
        // ClickController.SelectedButtonChanged += HandleSelection;
        ClickController.GameObjectAndButtonChanged += HandleSelection;
        ClickController.MiniTileHeldDown += HandleMiniTileSelection;



    }

    private void OnDestroy()
    {

        ClickController.GameObjectAndButtonChanged -= HandleSelection;
    }
    private void HandleMiniTileSelection(GameObject obj)
    {
       
        MiniTileHasBeenSelected?.Invoke(obj);
        

    }

    private void HandleSelection(GameObject obj, GameObject uiObj)
    {
        Debug.Log("Handle Selection");
       /* if (obj != null)
        {
            if (obj.tag == "MiniTile")
            {
                MiniTileHasBeenSelected?.Invoke(obj);
            }
        } */
        // uiElement.Bind(obj);



    }
}
