using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

/// <summary>
/// Class handling visual UI components of "ContentInformation"
/// </summary>
public class ContentInformationView : MonoBehaviour
{
  
    public GameObject contentInfoRoot;


    public GameObject contentTemplate;
    public GameObject contentPanel;
    public List<GameObject> allContent = new List<GameObject>();
    public MapHandler cubespawner;
    LocalizeStringEvent localizedString;
    public ClientConnection clientConnection;
    public List<System.IDisposable> disposables = new List<System.IDisposable>();



    public void Start()
    {
     
        contentInfoRoot.SetActive(false);


    }
  
    public void ShowContentInfo()
    {


        MiniTileToggle.setMiniTilesActive(false);
        contentInfoRoot.SetActive(true);
        
    }

    /// <summary>
    /// Disables a button so that a player can not click it multiple times while waiting for server confirmation.
    /// </summary>
    public IEnumerator DisableAndEnableButton(GameObject button)
    {
        if (button != null)
        {
            button.SetActive(false);
        }
        yield return new WaitForSeconds(3);
        if (button != null)
        {
            button.SetActive(true);
        }
    }

    /// <summary>
    /// This function creates a visual representation of content when given a list of the content to display 
    /// </summary>
    public void FilterView(List<ContentMessage> filterList)
    {
        ShowContentInfo();
       
        //delete the view from previous calls
        foreach (var obj in allContent)
        {
            Destroy(obj);
        }
        allContent.Clear();

        //rebuild the view
        foreach (var content in filterList)
        {

            GameObject currentDisplay = Instantiate(contentTemplate);
            currentDisplay.transform.SetParent(contentPanel.transform);
            currentDisplay.transform.localScale = new Vector3(1, 1, 1);
            currentDisplay.SetActive(true);

            allContent.Add(currentDisplay);

            TMP_Text contentName = currentDisplay.transform.Find("ContentName").GetComponent<TMP_Text>();
            TMP_Text contentAmount = currentDisplay.transform.Find("ContentAmount").GetComponent<TMP_Text>();
            Image contentImage = currentDisplay.transform.Find("ContentImage").GetComponent<Image>();
            Button contentInteraction = currentDisplay.transform.Find("ContentInteractionButton").GetComponent<Button>();

            //translation of text
            localizedString = contentInteraction.transform.Find("ContentButtonText").GetComponent<LocalizeStringEvent>();
            localizedString.StringReference.TableReference = "UIText";
            contentName.text = content.ToString();



            if (content.Type == ContentType.PLAYER)
            {

                contentImage.sprite = cubespawner.iconSpriteDict["PlayerIcon"];
                localizedString.StringReference.TableEntryReference = "Trade";
            }
            else if (content.Type == ContentType.QUESTLEVEL1)
            {

                contentImage.sprite = cubespawner.iconSpriteDict["QuestIcon"];
                localizedString.StringReference.TableEntryReference = "Interact";

                if(content.ResourceType != ResourceType.NONE) //if the quest has a resource type it is a resource!
                {
                    contentName.text += " " + content.ResourceType;
                   var msg = new MapContentTransactionMessage() { MapContentId = content.Id, MessageState = MessageState.NONE, MessageType = MessageType.REQUEST };
                    contentInteraction.onClick.AddListener(delegate () { clientConnection.ClientInstance.SendMessage(msg); StartCoroutine(DisableAndEnableButton(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject)); });
                }
            }
            else if (content.Type == ContentType.RESOURCE)
            {
                localizedString.StringReference.TableEntryReference = "PickUp";
                contentImage.sprite = cubespawner.iconSpriteDict[((content.ResourceType + "").Substring(0, 1)) + (content.ResourceType + "").ToLower().Substring(1) + "Icon"];

                //attach to the button a function to send a pickup request for the item to the server
                var msg = new MapContentTransactionMessage() { MapContentId = content.Id, MessageState = MessageState.NONE, MessageType = MessageType.REQUEST };
                contentInteraction.onClick.AddListener(delegate () { clientConnection.ClientInstance.SendMessage(msg); StartCoroutine(DisableAndEnableButton(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject)); });
            }


        }

    }

    public void CloseContentInfo()
    {

        ContentUILogic.currentOpenContent = "0";
        MiniTileToggle.setMiniTilesActive(true);

        contentInfoRoot.SetActive(false);
    }



}
