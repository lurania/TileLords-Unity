using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickController : MonoBehaviour
{
    public static event Action<GameObject> SelectedGameObjectChanged;
    public static event Action<GameObject> SelectedButtonChanged;
    public static event Action<GameObject, GameObject> GameObjectAndButtonChanged;
    public static event Action<GameObject> MiniTileHeldDown;

    public static GameObject SelectedGameObject { get; private set; }
    public static GameObject SelectedButton { get; private set; }

    float touchTime = 0;
    bool miniTileSelected = false;

    // Update is called once per frame
    void Update()
    {
        //detect finger touch on screen and if it is stationary
        if ((Input.touchCount > 0) && Input.GetTouch(0).phase == TouchPhase.Stationary) 
        {

            touchTime += Time.deltaTime;

            //held gameObject for x seconds
            if (touchTime >= 0.35f)
            {

             
                touchTime = 0;
                Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit raycastHit;

                //check what gameObject is being hit
                if (Physics.Raycast(raycast, out raycastHit))
                {
                    
                    Debug.Log("selected Object");
                    GameObject gobject = raycastHit.collider.gameObject;

                    SelectedGameObject = gobject;

                    if (SelectedGameObject.tag == "MiniTile")
                    {
                        miniTileSelected = true;
                        MiniTileHeldDown?.Invoke(SelectedGameObject); //let a handler handle the miniTile selection
                    }
                }
            }
        }

        if ((Input.touchCount > 0) && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
          
            touchTime = 0;
        }
        //detect finger touch on screen and if it is tapped
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))

        {

            //detects interactions with UI objects
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                Debug.Log("selected UI");
                GameObject button = EventSystem.current.currentSelectedGameObject;
                if (button != null)
                {
                    if (button.tag == "MiniTileMenu")
                    {
                        //do nothing if miniTileMenu is selected (is handled by unity eventsystem with buttons)
                    }
                    else
                    {
                        MiniTileHeldDown?.Invoke(null); //if anything but the miniTileMenu is touched the selected miniTile needs to be unselected


                        //any other UI
                        SelectedButton = button;

                        GameObjectAndButtonChanged?.Invoke(SelectedGameObject, SelectedButton); 
                    }
                }
                else
                {
                    //unselect everything
                    MiniTileHeldDown?.Invoke(null);
                    Debug.Log("unselected UI");
                    SelectedButton = null;

                    GameObjectAndButtonChanged?.Invoke(SelectedGameObject, SelectedButton);
                }

                return;
            }


            //detect gameObjects 
            Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit raycastHit;



            if (Physics.Raycast(raycast, out raycastHit))
            {
                MiniTileHeldDown?.Invoke(null);
                Debug.Log("selected Object");
                GameObject gobject = raycastHit.collider.gameObject;

                SelectedGameObject = gobject;

                if (SelectedGameObject.tag == "MiniTile")
                {
                    //do nothing since you should not be able to tap miniTiles, only hold
                }
                else
                {
                    //any other gameObjects
                    GameObjectAndButtonChanged?.Invoke(SelectedGameObject, SelectedButton);
                }

            }
            else
            {
                //unselect everything
                MiniTileHeldDown?.Invoke(null);
                SelectedGameObject = null;
                Debug.Log("unselected Object");
                GameObjectAndButtonChanged?.Invoke(SelectedGameObject, SelectedButton);
            }

        }


    }
}
