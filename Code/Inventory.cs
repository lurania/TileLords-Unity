using DataModel.Common;
using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{


    public GameObject itemTemplate;
    public GameObject itemHolder;

    public Sprite iconTemp;

    public GameObject root;
    public ClientConnection clientConnection;
    public List<GameObject> inventoryItems = new List<GameObject>();
    List<System.IDisposable> disposable = new List<System.IDisposable>();
    Dictionary<InventoryType, int> playerInventory = new Dictionary<InventoryType, int>();
    byte[] clientId = null;
    public MapHandler cubespawner;

    void Start()
    {
  
    }

    public void Initialise()
    {
        root.SetActive(false);
        var idRequest = clientConnection.ClientInstance.InboundTraffic.Where(v => v is UserActionMessage)
                           .Select(v => v as UserActionMessage)
                           .Where(v => v.MessageContext == MessageContext.LOGIN && v.MessageState == MessageState.SUCCESS)
                           .ObserveOnMainThread()
                           .Subscribe(v => { clientId = v.AdditionalInfo; });

        var ownInventoryUpdate = clientConnection.ClientInstance.InboundTraffic.Where(v => v is InventoryContentMessage)
                                                                               .Select(v => v as InventoryContentMessage)
                                                                               //.Do(v => Debug.Log(v.MessageState + ""))
                                                                               .Where(v => v.MessageState == MessageState.SUCCESS)
                                                                               .Where(v => clientId != null)
                                                                               .Where(v => v.InventoryOwner.SequenceEqual(clientId))
                                                                               .ObserveOnMainThread()
                                                                               .Subscribe(v =>
                                                                               {
                                                                                   playerInventory = v.InventoryContent.ToDictionary(x => x.Key, x => x.Value);
                                                                                   BuildInventory();
                                                                               });
        disposable.Add(idRequest);
        disposable.Add(ownInventoryUpdate);
        StartCoroutine(RequestInventory());

    }

    /// <summary>
    /// Coroutine that periodically updates the inventory for the client
    /// </summary>
    IEnumerator RequestInventory()
    {

        yield return new WaitForSeconds(20);
        while (true)
        {
            if (clientId != null)
            {
                clientConnection.ClientInstance.SendMessage(new InventoryContentMessage()
                {
                    InventoryContent = null,
                    InventoryOwner = clientId,
                    MessageState = MessageState.NONE,
                    Type = MessageType.REQUEST

                });
            }

            yield return new WaitForSeconds(20);
        }
    }


    /// <summary>
    /// Used by the quest tracker to show the actual quest progress (instead of requesting periodically)
    /// </summary>

    public void RequestInventoryOnce()
    {
        if (clientId != null)
        {
            clientConnection.ClientInstance.SendMessage(new InventoryContentMessage()
            {
                InventoryContent = null,
                InventoryOwner = clientId,
                MessageState = MessageState.NONE,
                Type = MessageType.REQUEST

            });
        }
    }

    public void BuildInventory()
    {



        //Destroy items from old build
        foreach (var obj in inventoryItems)
        {
            Destroy(obj);
        }
        inventoryItems.Clear();

        //Rebuild Inventory with new items
        playerInventory.Keys.ToList().ForEach(v =>
        {

            if (playerInventory[v] != 0)
            {
                var item = Instantiate(itemTemplate);

                inventoryItems.Add(item);
                item.SetActive(true);
                item.transform.SetParent(itemHolder.transform);
                item.transform.localScale = new Vector3(1, 1, 1);



                TMP_Text itemName = item.transform.Find("ItemName").gameObject.GetComponent<TMP_Text>();
                itemName.text = v.ResourceType + "";

                TMP_Text itemAmount = item.transform.Find("ItemAmount").gameObject.GetComponent<TMP_Text>();
                itemAmount.text = playerInventory[v] + "";


                Image icon = item.transform.Find("ItemIcon").gameObject.GetComponent<Image>();
                if (v.ResourceType != ResourceType.NONE)
                {
                    icon.sprite = cubespawner.iconSpriteDict[((v.ResourceType + "").Substring(0, 1)) + (v.ResourceType + "").ToLower().Substring(1) + "Icon"];
                }

                if(v.ContentType == ContentType.QUESTREWARDPOINTS)
                {
                    icon.sprite = cubespawner.iconSpriteDict["PointsIcon"];
                    itemName.text = v.ContentType + "";
                }

                if (v.ContentType == ContentType.QUESTLEVEL1)
                {
                    GameObject iconQuestObj = item.transform.Find("ItemQuestIcon").gameObject;
                    Image iconQuest = iconQuestObj.GetComponent<Image>();
                    iconQuest.sprite = cubespawner.iconSpriteDict["QuestIcon"];
                    iconQuestObj.SetActive(true);

                }
            }

            /*Button button = item.transform.Find("Button").gameObject.GetComponent<Button>();
            button.onClick.AddListener(delegate () { var action = typeActionLookup.Invoke(type); action.Invoke(); }); */





        });

    }

    private void OnDestroy()
    {
        disposable.ForEach(v => v.Dispose());
    }
    public void OpenInventory()
    {
        //request the inventory
        if (clientId != null)
        {
            clientConnection.ClientInstance.SendMessage(new InventoryContentMessage()
            {
                InventoryContent = null,
                InventoryOwner = clientId,
                MessageState = MessageState.NONE,
                Type = MessageType.REQUEST

            });
        }
        root.SetActive(true);

        MiniTileToggle.setMiniTilesActive(false);
    }

    public void CloseInventory()
    {
        root.SetActive(false);
        MiniTileToggle.setMiniTilesActive(true);
    }


}
