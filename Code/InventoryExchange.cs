using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModel.Common.Messages;
using TMPro;
using UnityEngine.UI;
using DataModel.Common.GameModel;

public class InventoryExchange : MonoBehaviour
{

    public GameObject root;
    public RectTransform toShift;
    public MapHandler cubespawner;
    public GameObject itemTemplate;
    public GameObject exchangeTemplate;

    public GameObject ownInventoryHolder;
    public GameObject otherInventoryHolder;
    public GameObject exchangeInventoryHolder;

    List<GameObject> allExistingTemplates = new List<GameObject>();
    Dictionary<ResourceType, GameObject> allExchangeObjects = new Dictionary<ResourceType, GameObject>();
    Dictionary<ResourceType, int> transferDict = new Dictionary<ResourceType, int>();

    // Start is called before the first frame update
    void Start()
    {
        root.SetActive(false);
    }


    public void OpenExchange()
    {
       
        root.SetActive(true);
        MiniTileToggle.setMiniTilesActive(false);
    }

    public void CloseExchange()
    {
        root.SetActive(false);
        MiniTileToggle.setMiniTilesActive(true);
        CleanExchangeWindow();
    }


    public void CleanExchangeWindow()
    {
        foreach(var obj in allExistingTemplates)
        {
            Destroy(obj);

        }
        allExistingTemplates.Clear();
        transferDict.Clear();
    }

    public void FetchOwnInventory()
    {
        var item = Instantiate(itemTemplate);
        allExistingTemplates.Add(itemTemplate);
        item.SetActive(true);
        item.transform.SetParent(ownInventoryHolder.transform);
        item.transform.localScale = new Vector3(1, 1, 1);

        TMP_Text itemName = item.transform.Find("ItemName").gameObject.GetComponent<TMP_Text>();
        
        //itemName.text = v + "";

        TMP_Text itemAmount = item.transform.Find("ItemAmount").gameObject.GetComponent<TMP_Text>();

        //itemAmount.text = playerInventory[v] + "";


        Image icon = item.transform.Find("ItemIcon").gameObject.GetComponent<Image>();
        //icon.sprite = cubespawner.iconSpriteDict[((v + "").Substring(0, 1)) + (v + "").ToLower().Substring(1) + "Icon"]; ;

        ItemExchangeData data = item.GetComponent<ItemExchangeData>();
        //get this data properly!!!
        data.type = 0;
        data.image = null;
        data.amount = 0;


    }

    public void FetchOtherInventory()
    {

        var item = Instantiate(itemTemplate);
        allExistingTemplates.Add(itemTemplate);
        item.SetActive(true);
        item.transform.SetParent(otherInventoryHolder.transform);
        item.transform.localScale = new Vector3(1, 1, 1);

        TMP_Text itemName = item.transform.Find("ItemName").gameObject.GetComponent<TMP_Text>();

        //itemName.text = v + "";

        TMP_Text itemAmount = item.transform.Find("ItemAmount").gameObject.GetComponent<TMP_Text>();

        //itemAmount.text = playerInventory[v] + "";


        Image icon = item.transform.Find("ItemIcon").gameObject.GetComponent<Image>();
        //icon.sprite = cubespawner.iconSpriteDict[((v + "").Substring(0, 1)) + (v + "").ToLower().Substring(1) + "Icon"]; 

        ItemExchangeData data = item.GetComponent<ItemExchangeData>();
        //get this data properly!!!
        data.type = 0;
        data.image = null;
        data.amount = 0;


        

    }

    public void RemoveFromExchange()
    {
        GameObject callingButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        GameObject template = callingButton.transform.parent.gameObject;
        ItemExchangeData templateData = callingButton.transform.parent.gameObject.GetComponent<ItemExchangeData>();

        //check if we already added this item to the exchange box before creating it
        if (!allExchangeObjects.ContainsKey(templateData.type))
        {
            allExchangeObjects.Remove(templateData.type);
          
        }
        if (allExistingTemplates.Contains(template))
        {
            Destroy(template);
        }
        GameObject.Destroy(templateData);
    }


    public void AddToExchange()
    {
        GameObject callingButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        ItemExchangeData templateData = callingButton.transform.parent.gameObject.GetComponent<ItemExchangeData>();

        //check if we already added this item to the exchange box before creating it
        if (!allExchangeObjects.ContainsKey(templateData.type))
        {
            var exchangeItem = Instantiate(exchangeTemplate);
            allExistingTemplates.Add(exchangeItem);
            exchangeItem.SetActive(true);
            exchangeItem.transform.SetParent(exchangeInventoryHolder.transform);
            exchangeItem.transform.localScale = new Vector3(1, 1, 1);

            exchangeItem.name = templateData.type + "";

            TMP_Text itemName = exchangeItem.transform.Find("ItemName").gameObject.GetComponent<TMP_Text>();
            itemName.text = templateData.type + "";
            //itemName.text = v + "";

            TMP_Text itemAmountOther = exchangeItem.transform.Find("ItemAmountOther").gameObject.GetComponent<TMP_Text>();

            //itemAmountOther.text = playerInventory[v] + "";

            TMP_Text itemAmountOwn = exchangeItem.transform.Find("ItemAmountOwn").gameObject.GetComponent<TMP_Text>();

            //itemAmountOwn.text = playerInventory[v] + "";


            Image icon = exchangeItem.transform.Find("ItemIcon").gameObject.GetComponent<Image>();
            icon.sprite = templateData.image.sprite;

            allExchangeObjects.Add(templateData.type, exchangeItem);
        }
    }

    public void ReadInputData()
    {
        transferDict.Clear();
        foreach(var exchangeObject in allExchangeObjects)
        {
            
            TMP_InputField inputData = exchangeObject.Value.GetComponent<TMP_InputField>();
            transferDict.Add(exchangeObject.Key, System.Int32.Parse(inputData.text));
        }
    }
    public void SendTransferDataToServer()
    {
        ReadInputData();
        //transferDict has it all now
        CleanExchangeWindow();
    }











    public void LeftScreen()
    {
        /*Left*/
       // toShift.anchoredPosition = new Vector2(-1500f,-500);


        // [ left - bottom ]
        toShift.offsetMin = new Vector2(-50, 0);
        // [ right - top ]
        toShift.offsetMax = new Vector2(500, 0);

    }

    public void MiddleScreen()
    {
        /*Left*/
        // toShift.anchoredPosition = new Vector2(-1500f,-500);


        // [ left - bottom ]
        toShift.offsetMin = new Vector2(-1500, 0);
        // [ right - top ]
        toShift.offsetMax = new Vector2(500, 0);

    }

    public void RightScreen()
    {
        /*Left*/
        // toShift.anchoredPosition = new Vector2(-1500f,-500);


        // [ left - bottom ]
        toShift.offsetMin = new Vector2(-2950, 0);
        // [ right - top ]
        toShift.offsetMax = new Vector2(500, 0);

    }


}
