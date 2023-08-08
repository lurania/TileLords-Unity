using DataModel.Common;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SomeUIElement : MonoBehaviour
{


    [SerializeField] private GameObject infoTextRoot;
    [SerializeField] private GameObject buttonRoot;

    [SerializeField] private TMP_Text infoText;

    [SerializeField] private GameObject holder1;
    [SerializeField] private GameObject holder2;
    [SerializeField] private GameObject holder3;
    [SerializeField] private GameObject holder4;
    [SerializeField] private GameObject holder5;

    [SerializeField] private Button resource;
    [SerializeField] private Button build;
    [SerializeField] private Button search;
    [SerializeField] private Button battle;
    [SerializeField] private Button trade;
    GameObject gobj;

    [SerializeField] private GameObject pickUpIcon;
    void Awake()
    {
        DisableAll();

       
    }

    public void DisableAll()
    {
       
        infoTextRoot.SetActive(false);
        buttonRoot.SetActive(false);
        resource.gameObject.SetActive(false);
        trade.gameObject.SetActive(false);
        search.gameObject.SetActive(false);
        battle.gameObject.SetActive(false);
        build.gameObject.SetActive(false);
        pickUpIcon.SetActive(false);

    }

    public void Bind(GameObject obj)
    {
       

        gobj = obj; //useful if there is a need to save the obj somewhere

        if (obj != null)
        {
            MiniTileUnity miniTile = obj.GetComponent<MiniTileUnity>();
            int distance = PlusCodeUtils.GetChebyshevDistance(miniTile.code, PlayerU.currentPlusCodeLocation);
            if (distance <= 1)
            {
                Debug.Log("within distance");
                pickUpIcon.SetActive(true);
                infoTextRoot.SetActive(true);
                buttonRoot.SetActive(true);

                Debug.Log(miniTile.getPlusCode().Code);
                infoText.text = miniTile.getPlusCode().Code;
                HandleButton(miniTile);
            }

         

        }
      
    }

   
    public void HandleButton(MiniTileUnity miniTileU)
    {
        //depending on the tile content there will be if statements
        //to turn on specific buttons or not
        MiniTile miniTile = miniTileU.getMiniTile(); //for later!

        pickUpIcon.SetActive(true);
        pickUpIcon.transform.position = new Vector3(miniTileU.transform.position.x, miniTileU.transform.position.y + 1, miniTileU.transform.position.z);

        resource.gameObject.SetActive(true);

        resource.gameObject.transform.position = holder1.transform.position;

        battle.gameObject.SetActive(true);
        battle.gameObject.transform.position = holder2.transform.position;

    }
}
