using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModel.Common;
using Google.OpenLocationCode;
using TMPro;
using UnityEngine.UI;
using DataModel.Common.Messages;
using UniRx;
using DataModel.Common.GameModel;
using System.Text;
using System.Linq;

public class QuestTracker : MonoBehaviour
{

 

    public GameObject player;
    public GameObject arrowYellow;
    public GameObject arrowRed;
    public GameObject arrowBlue;
    public GameObject worldQuestTrackerBox;
    public GameObject worldUICanvas;
    public GameObject questCanvas;
    public GameObject questTemplate;
    public GameObject questContentHolder;
    public GameObject errorText;
    public GameObject questDeletionDialog;
    public GameObject areaMarker;


    public TMP_Text worldUIQuestText;
    public TMP_Text worldUIQuestText1;
    public TMP_Text worldUIQuestText2;


    public Sprite buttonPinned;
    public Sprite buttonUnpinned;

    public Dictionary<string, GameObject> questDictionary;
    public Dictionary<string, GameObject> tmpQuestDictionary = new Dictionary<string, GameObject>();
    public Dictionary<int, bool> assignedTrackers;
    public List<QuestU> trackedQuests = new List<QuestU>();


    public ClientConnection clientConnection;
    public Inventory inventory;
    public MapHandler cubeSpawner;


    GameObject callingButtonToDelete;

    int trackedQuestCounter = 0;
    bool questLogOpen = false;
    float compassDir;
    float lastCompassDir;

    List<string> keysToRemove = new List<string>();
    List<GPS> trackedLocations;

  
    public void Initialise()
    {
        worldQuestTrackerBox.SetActive(false);
        assignedTrackers = new Dictionary<int, bool>();
        assignedTrackers.Add(0, false);
        assignedTrackers.Add(1, false);
        assignedTrackers.Add(2, false);
        questDictionary = new Dictionary<string, GameObject>();
        questCanvas.SetActive(false);
        trackedLocations = new List<GPS>();
        arrowYellow.SetActive(false);
        arrowBlue.SetActive(false);
        arrowRed.SetActive(false);
        errorText.SetActive(false);
        questDeletionDialog.SetActive(false);



        RequestQuestList();

        var pickedUpItem = clientConnection.ClientInstance.InboundTraffic.Where(v => v is MapContentTransactionMessage).Select(v => v as MapContentTransactionMessage)
       .Where(v => v.MessageState == MessageState.SUCCESS).StartWith(new MapContentTransactionMessage() { MapContentId = null });


        var questStream = clientConnection.ClientInstance.InboundTraffic.Where(v => v is ActiveUserQuestsMessage).Select(v => v as ActiveUserQuestsMessage);
        var inventoryStream = clientConnection.ClientInstance.InboundTraffic.Where(v => v is InventoryContentMessage).Select(v => v as InventoryContentMessage);





        //an observable to count questitems in the inventory and current quest progress
        questStream.CombineLatest(inventoryStream, (quests, inventar) => new { quests, inventar }).ObserveOnMainThread().Subscribe(v =>
        {
            var inventoryDict = v.inventar.InventoryContent.ToDictionary(k => k.Key, k => k.Value);

            if (v.quests != null)
            {
                if (v.quests.CurrentUserQuests != null)
                {
                    foreach (var quest in v.quests.CurrentUserQuests) //get all current user quests
                    {
                        if (questDictionary.ContainsKey(Encoding.UTF8.GetString(quest.QuestId))) //see if this quest exists in the quest dictionary
                        {
                            var questObj = questDictionary[Encoding.UTF8.GetString(quest.QuestId)]; //get this questObject
                            var questU = questObj.GetComponent<QuestU>();
                            if (inventoryDict.ContainsKey(GetQuestItemDictionaryKey(quest)))
                            {

                                questU.CurrentAmount = inventoryDict[GetQuestItemDictionaryKey(quest)]; //current amount to complete the quest
                                questU.CompletionPercent = questU.CurrentAmount / questU.RequiredAmount;


                            }

                        }
                    }

                    RefreshQuests();
                }
            }
        });





        //an observable for successful quest requests
        clientConnection.ClientInstance.InboundTraffic.Where(v => v is QuestRequestMessage).Select(v => v as QuestRequestMessage).ObserveOnMainThread().Subscribe(v =>
        {

            if (v.MessageState == MessageState.SUCCESS)
            {

                RenderQuest(v.Quest);
            }
        }

        );

        //an observable for the entire list of user quests
        clientConnection.ClientInstance.InboundTraffic.Where(v => v is ActiveUserQuestsMessage).Select(v => v as ActiveUserQuestsMessage).ObserveOnMainThread().Subscribe(v =>
        {

            if (v.MessageState == MessageState.SUCCESS)
            {

                foreach (var quest in v.CurrentUserQuests) //rebuild
                {

                    if (!questDictionary.ContainsKey(Encoding.UTF8.GetString(quest.QuestId))) //if the quest is not yet in the dictionary, add it
                    {

                        RenderQuest(quest);

                    }
                    else //if the value is already in the dictionary, copy it to the tmp dictionary
                    {
                        if (!tmpQuestDictionary.ContainsKey(Encoding.UTF8.GetString(quest.QuestId)))
                            tmpQuestDictionary.Add(Encoding.UTF8.GetString(quest.QuestId), questDictionary[Encoding.UTF8.GetString(quest.QuestId)]);
                    }


                }



                foreach (var keyVal in questDictionary)
                {
                    //remove all quests that are not part of currentUserQuests
                    if (!tmpQuestDictionary.ContainsKey(keyVal.Key))
                    {

                        UntrackQuest(keyVal.Key);
                        Destroy(keyVal.Value);
                        keysToRemove.Add(keyVal.Key);


                    }

                }


                foreach (var key in keysToRemove)
                {
                    questDictionary.Remove(key);
                }
                tmpQuestDictionary.Clear();
                RefreshQuests();
            }
        }


        );
    }

    public InventoryType GetQuestItemDictionaryKey(Quest quest)
      => new InventoryType() { ContentType = quest.QuestLevel, ResourceType = quest.TypeToPickUp };


    /// <summary>
    /// Updates the quest view every few seconds while its open, updates at a slower rate when closed for quest progress trackers
    /// </summary>
  
    IEnumerator RefreshQuestLogPeriodically()
    {
        while (questLogOpen)
        {

            RefreshQuests();
            yield return new WaitForSeconds(3);
        }
        while (!questLogOpen)
        {
            inventory.RequestInventoryOnce();
            RefreshQuests();
            yield return new WaitForSeconds(10);
        }
       

    }

    /// <summary>
    /// Send a request to the server to update the quest list and inventory every few seconds
    /// </summary>

    /// <returns>The list containing all PlusCodes of this section</returns>
    IEnumerator RequestQuestLogPeriodically()
    {
        while (questLogOpen)
        {

            RequestQuestList();


            inventory.RequestInventoryOnce();

            yield return new WaitForSeconds(10);
        }
    }

    public void OpenQuestLog()
    {
        questLogOpen = true;
        StartCoroutine(RefreshQuestLogPeriodically());
        StartCoroutine(RequestQuestLogPeriodically());

        worldUICanvas.SetActive(false);
        questCanvas.SetActive(true);
        MiniTileToggle.setMiniTilesActive(false);
    }

    public void CloseQuestLog()
    {
        questLogOpen = false;
        worldUICanvas.SetActive(true);
        questCanvas.SetActive(false);
        MiniTileToggle.setMiniTilesActive(true);
    }



    /// <summary>
    /// Requests all of the quests for the player
    /// </summary>

    public void RequestQuestList()
    {
    
        clientConnection.ClientInstance.SendMessage(new ActiveUserQuestsMessage()
        {
            MessageType = MessageType.REQUEST


        });
    }

    /// <summary>
    /// Sends a request to the server to turn in a quest
    /// </summary>

    public void TurnInQuest()
    {
        Debug.Log("handing in quest");
        GameObject callingButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject; //get the turnIn button
        QuestU questU = callingButton.transform.parent.gameObject.GetComponent<QuestU>(); //get the attached QuestU script
        clientConnection.ClientInstance.SendMessage(new TurnInQuestMessage() //send server message to turn in quest with quest ID
        {
            QuestId = questU.QuestID


        });

        RequestQuestList();
    }


    /// <summary>
    /// Sends a request to the server to receive a quest
    /// </summary>
 
    public void RequestQuest()
    {

      
        clientConnection.ClientInstance.SendMessage(new QuestRequestMessage()
        {
            MessageType = MessageType.REQUEST


        });
    }


    /// <summary>
    /// Adds new quest elements to the quest UI view (texts, icons etc.) and adds them to a quest dictionary
    /// </summary>
    /// <param name="q">The quest element to add to the UI view.</param>
   
    public void RenderQuest(Quest q)
    {


        //if the quest has not been added yet, add it
        if (!questDictionary.ContainsKey(Encoding.UTF8.GetString(q.QuestId)))
        {

            GameObject quest = Instantiate(questTemplate);
            quest.SetActive(true);
            quest.transform.SetParent(questContentHolder.transform);
            quest.transform.localScale = new Vector3(1, 1, 1);

            TMP_Text questText = quest.transform.Find("QuestText").gameObject.GetComponent<TMP_Text>();

            questText.text = "\nQuestreward: ";
            foreach (var reward in q.QuestReward)
            {
                if (reward.ContentType == ContentType.RESOURCE)
                {
                    questText.text += reward.ResourceType + " " + reward.Amount + "\n";
                }
                else if (reward.ContentType == ContentType.QUESTREWARDPOINTS)
                {
                    questText.text += "Points: " + reward.Amount + "\n";
                }
            }

            TMP_Text questStatus = quest.transform.Find("QuestStatus").gameObject.GetComponent<TMP_Text>();
            questStatus.text = q.TypeToPickUp + " " + q.RequiredAmountForCompletion;

            TMP_Text questDistance = quest.transform.Find("QuestDistance").gameObject.GetComponent<TMP_Text>();
            questDistance.text = System.Math.Round(GPSToDistance(PlusCodeToGPS(PlayerU.currentPlusCodeLocation), PlusCodeToGPS(new PlusCode(q.QuestTargetLocation, 10))), 2) + " KM away";

            QuestU questU = quest.AddComponent<QuestU>();
            questU.QuestText = questText.text;
            questU.StatusText = questStatus.text;
            questU.QuestID = q.QuestId;
            questU.QuestLevel = q.QuestLevel;
            questU.RequiredAmount = q.RequiredAmountForCompletion;
            questU.TypeToPickUp = q.TypeToPickUp;

            questU.TrackedLocation = new PlusCode(q.QuestTargetLocation, 10);
            questU.HandInLocation = new PlusCode(q.QuestTurninLocation, 10);
            questDictionary.Add(Encoding.UTF8.GetString(q.QuestId), quest);
            tmpQuestDictionary.Add(Encoding.UTF8.GetString(q.QuestId), quest);
        }



    }

    /// <summary>
    /// Updates all UI values of all quests in the quest dictionary
    /// </summary>

    public void RefreshQuests()
    {
        Debug.Log("Refreshing quests! Count: " + questDictionary.Count);
        foreach (var questKeyValue in questDictionary)
        {

            QuestU questU = questKeyValue.Value.GetComponent<QuestU>();
            TMP_Text questText = questKeyValue.Value.transform.Find("QuestDistance").gameObject.GetComponent<TMP_Text>();
            questText.text = System.Math.Round(GPSToDistance(PlusCodeToGPS(PlayerU.currentPlusCodeLocation), PlusCodeToGPS(questU.TrackedLocation)), 2) + " KM away";

            TMP_Text questStatus = questKeyValue.Value.transform.Find("QuestStatus").gameObject.GetComponent<TMP_Text>();
            questU.CompletionPercent = questU.CurrentAmount / questU.RequiredAmount;
            questStatus.text = questU.TypeToPickUp + " " + questU.CurrentAmount + "/" + questU.RequiredAmount;
            questU.StatusText = questStatus.text;

            if (questU.CompletionPercent >= 1)
            {
                GameObject questTurnInButton = questKeyValue.Value.transform.Find("TurnInQuestButton").gameObject;
                questTurnInButton.SetActive(true);
            }
            else
            {
                GameObject questTurnInButton = questKeyValue.Value.transform.Find("TurnInQuestButton").gameObject;
                questTurnInButton.SetActive(false);
            }
        }

    }


    /// <summary>
    /// Activates a dialog to ask the player if they want to delete a quest
    /// </summary>
 
    public void AskToDeleteQuest()
    {
        questDeletionDialog.SetActive(true);
        callingButtonToDelete = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
    }

    /// <summary>
    /// Hides the dialog that asks to delete a quest and calls the function to delete the quest
    /// </summary>
 
    public void AcceptRemoveQuest()
    {
        questDeletionDialog.SetActive(false);
        RemoveQuest();

    }

    /// <summary>
    /// Hides the dialog that asks to delete a quest
    /// </summary>
    public void CancelRemoveQuest()
    {

        questDeletionDialog.SetActive(false);
    }

    /// <summary>
    /// Deletes a quest from the questlog 
    /// </summary>

    public void RemoveQuest()
    {

        GameObject quest = callingButtonToDelete.transform.parent.gameObject;
        QuestU questU = quest.GetComponent<QuestU>();


        if (questU.IsTracked)
        {
            UntrackQuest(callingButtonToDelete);
        }

        Destroy(quest);
        callingButtonToDelete = null;
    }


    /// <summary>
    /// Function that activates a questtracker object to track a quests location
    /// </summary>

    public void TrackQuest()
    {
        GameObject callingButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        GameObject quest = callingButton.transform.parent.gameObject;
        QuestU questU = quest.GetComponent<QuestU>();


        //if the quest is not tracked yet
        if (!questU.IsTracked)
        {
            if (trackedQuestCounter < 3)
            {
                worldQuestTrackerBox.SetActive(true);
                trackedQuestCounter++;
                questU.IsTracked = true;
                trackedQuests.Add(questU);
                Image buttonImage = callingButton.GetComponent<Image>();
                buttonImage.sprite = buttonPinned;

                for (int i = 0; i < assignedTrackers.Count; i++)
                {
                    //if the tracker is not assigned yet (dictionary has id (0,1,2) and bool (is assigned?)
                    if (!assignedTrackers[i])
                    {
                        assignedTrackers[i] = true;

                        TrackLocation(questU.TrackedLocation);

                        //assign the right text and color corresponding the arrow color 
                        if (i == 0)
                        {
                            questU.assignedTextBox = worldUIQuestText;
                            worldUIQuestText.text = questU.StatusText;
                            worldUIQuestText.color = Color.yellow;
                            arrowYellow.SetActive(true);
                            questU.QuestTrackerID = 0;
                        }
                        else if (i == 1)
                        {
                            questU.assignedTextBox = worldUIQuestText1;
                            worldUIQuestText1.text = questU.StatusText;
                            worldUIQuestText1.color = Color.red;
                            arrowRed.SetActive(true);
                            questU.QuestTrackerID = 1;
                        }
                        else if (i == 2)
                        {
                            questU.assignedTextBox = worldUIQuestText2;
                            worldUIQuestText2.text = questU.StatusText;
                            worldUIQuestText2.color = new Color32(0,200,255,255);
                            arrowBlue.SetActive(true);
                            questU.QuestTrackerID = 2;
                        }

                        break;
                    }

                }
            }
            else
            {
              
                errorText.SetActive(true);


            }
        }


        //otherwise untrack the quest
        else
        {
            UntrackQuest(callingButton);

        }



    }


    /// <summary>
    /// Function to closes the error pop-up when trying to track more than 3 quests
    /// </summary>
    public void AcceptError()
    {
        errorText.SetActive(false);
    }


    /// <summary>
    /// Function that allows to untrack a quest again; disables the questtracker object 
    /// </summary>
    /// <param name="callingButton">The button object that was pressed to untrack the specific quest.</param>

    public void UntrackQuest(GameObject callingButton)
    {
        if (trackedQuestCounter > 0)
        {

            GameObject quest = callingButton.transform.parent.gameObject;
            QuestU questU = quest.GetComponent<QuestU>();
            questU.assignedTextBox = null;
            questU.IsTracked = false;
            questU.marked = false;
            trackedQuests.Remove(questU);
            trackedQuestCounter--;
            Image buttonImage = callingButton.GetComponent<Image>();
            buttonImage.sprite = buttonUnpinned;
            if (questU.marker != null)
            {
                Destroy(questU.marker);
            }


            UntrackLocation(questU.TrackedLocation);
            //unassign the quest tracker
            if (questU.QuestTrackerID == 0)
            {
                assignedTrackers[0] = false;
                worldUIQuestText.text = "";
                arrowYellow.SetActive(false);
            }
            else if (questU.QuestTrackerID == 1)
            {
                assignedTrackers[1] = false;
                worldUIQuestText1.text = "";
                arrowRed.SetActive(false);
            }
            else if (questU.QuestTrackerID == 2)
            {
                assignedTrackers[2] = false;
                worldUIQuestText2.text = "";
                arrowBlue.SetActive(false);
            }


        }
        if(trackedQuestCounter <= 0)
        {
            //disable the box when nothing is tracked
            worldQuestTrackerBox.SetActive(false);
        }


    }


    /// <summary>
    /// Function that allows to untrack a quest again; disables the questtracker object 
    /// </summary>
    /// <param name="callingButton">The questID of the quest that will be untracked.</param>
    public void UntrackQuest(string questId)
    {
        if (trackedQuestCounter > 0)
        {

            if (questDictionary.ContainsKey(questId))
            {
                GameObject quest = questDictionary[questId];
                QuestU questU = quest.GetComponent<QuestU>();
                questU.assignedTextBox = null;
                questU.IsTracked = false;
                questU.marked = false;
                trackedQuests.Remove(questU);
                trackedQuestCounter--;
         
                if (questU.marker != null)
                {
                    Destroy(questU.marker);
                }


                UntrackLocation(questU.TrackedLocation);
                //unassign the quest tracker
                if (questU.QuestTrackerID == 0)
                {
                    assignedTrackers[0] = false;
                    worldUIQuestText.text = "";
                    arrowYellow.SetActive(false);
                }
                else if (questU.QuestTrackerID == 1)
                {
                    assignedTrackers[1] = false;
                    worldUIQuestText1.text = "";
                    arrowRed.SetActive(false);
                }
                else if (questU.QuestTrackerID == 2)
                {
                    assignedTrackers[2] = false;
                    worldUIQuestText2.text = "";
                    arrowBlue.SetActive(false);
                }
            }

        }

        if (trackedQuestCounter <= 0)
        {
            //disable the box when nothing is tracked
            worldQuestTrackerBox.SetActive(false);
        }


    }


    /// <summary>
    /// Function to calculate the bearing between two gps coordinates
    /// </summary>
    /// <param name="from">The first gps coordinate.</param>
    /// <param name="to">The second gps coordinate.</param>
    /// <returns>The bearing between the two gps coordinates in degrees</returns>
    public static double CalcBearing(GPS from, GPS to)
    {

        //lat lon

        {
            double x = System.Math.Cos(DegreesToRadians(from.Lat)) * System.Math.Sin(DegreesToRadians(to.Lat)) - System.Math.Sin(DegreesToRadians(from.Lat)) * System.Math.Cos(DegreesToRadians(to.Lat)) * System.Math.Cos(DegreesToRadians(to.Lon - from.Lon));
            double y = System.Math.Sin(DegreesToRadians(to.Lon - from.Lon)) * System.Math.Cos(DegreesToRadians(to.Lat));


            return (System.Math.Atan2(y, x) + System.Math.PI * 2) % (System.Math.PI * 2) * (180.0 / System.Math.PI);
        }



    }

 
    public static double DegreesToRadians(double angle)
    {
        return angle * System.Math.PI / 180.0d;
    }


    public GPS PlusCodeToGPS(PlusCode code)
    {
        CodeArea codeArea = OpenLocationCode.Decode(code.Code);
        GPS gpsCenter = new GPS(codeArea.Center.Latitude, codeArea.Center.Longitude);
        return gpsCenter;

    }

    public void TrackLocation(GPS trackedLocation)
    {

        trackedLocations.Add(trackedLocation);
    }

    public void TrackLocation(PlusCode trackedLocation)
    {
        trackedLocations.Add(PlusCodeToGPS(trackedLocation));
    }

    public void UntrackLocation(int index)
    {
        trackedLocations.RemoveAt(index);


    }
    public void UntrackLocation(PlusCode trackedLocation)
    {
        PlusCodeToGPS(trackedLocation);
        trackedLocations.Remove(PlusCodeToGPS(trackedLocation));


    }

    /// <summary>
    /// Function to get the distance in km between two gps
    /// </summary>
    /// <param name="from">The first gps coordinate.</param>
    /// <param name="to">The second gps coordinate.</param>
    /// <returns>The distance in km between the two gps coordinates</returns>
    public double GPSToDistance(GPS from, GPS to)
    {


        var earthRadiusKm = 6371;

        var dLat = DegreesToRadians(to.Lat - from.Lat);
        var dLon = DegreesToRadians(to.Lon - from.Lon);

        var lat1 = DegreesToRadians(from.Lat);
        var lat2 = DegreesToRadians(to.Lat);

        var a = System.Math.Sin(dLat / 2) * System.Math.Sin(dLat / 2) +
                System.Math.Sin(dLon / 2) * System.Math.Sin(dLon / 2) * System.Math.Cos(lat1) * System.Math.Cos(lat2);
        var c = 2 * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1 - a));
        return earthRadiusKm * c;

    }




    public void Update()
    {
        if (clientConnection.ClientInstance != null)
        {
            //change the text of the tracked quests to represent the right distance to the quest
            //also set a quest area marker on the position of the quest location
            foreach (var questU in trackedQuests)
            {
                questU.assignedTextBox.text = questU.StatusText + " Distance: " + System.Math.Round(GPSToDistance(PlusCodeToGPS(PlayerU.currentPlusCodeLocation), PlusCodeToGPS(questU.TrackedLocation)), 2) + "";

                if (cubeSpawner.plusCodeMiniTileDict.ContainsKey(questU.TrackedLocation))
                {
                    GameObject miniTile = cubeSpawner.plusCodeMiniTileDict[questU.TrackedLocation].gameObject;
                    //update position if quest marker exists
                    if (questU.marker != null)
                    {
                        questU.marker.transform.position = new Vector3(miniTile.transform.position.x, miniTile.transform.position.y + 1, miniTile.transform.position.z);
                    }
                    //create a quest marker if in range and set that the quest is marked
                    if (!questU.marked)
                    {

                        GameObject marker = Instantiate(areaMarker);
                        marker.transform.position = new Vector3(miniTile.transform.position.x, miniTile.transform.position.y + 1, miniTile.transform.position.z);
                        marker.SetActive(true);
                        questU.marked = true;
                        questU.marker = marker;

                    }
                }
                else //unmark the quest, remove the quest marker
                {
                    questU.marked = false;
                    if (questU.marker != null)
                    {
                        Destroy(questU.marker);
                    }
                }

            }

            //point the quest trackers in the right direction
            for (int i = 0; i < trackedLocations.Count; i++)
            {
                GPS location = trackedLocations[i];
                if (i == 0)
                {

                    TrackQuest(arrowYellow, location, 0, 0.7f);
                }
                else if (i == 1)
                {
                    TrackQuest(arrowRed, location, 0.7f, 0);

                }
                else if (i == 2)
                {
                    TrackQuest(arrowBlue, location, -0.7f, 0);

                }
            }
        }
    }

    /// <summary>
    /// Function to point the quest tracker to the quest location
    /// </summary>
    /// <param name="arrow">The arrow gameObject.</param>
    /// <param name="location">The quest location.</param>
    /// <param name="offsetX">The offset on x to the player of the arrow gameObject (used to position the arrow properly).</param>
    /// <param name="offsetZ">The offset on z to the player of the arrow gameObject (used to position the arrow properly).</param>

    public void TrackQuest(GameObject arrow, GPS location, float offsetX, float offsetZ)
    {
        arrow.transform.position = new Vector3(player.transform.position.x + offsetX, player.transform.position.y + 1f, player.transform.position.z + offsetZ);
        compassDir = (float)CalcBearing(PlusCodeToGPS(PlayerU.currentPlusCodeLocation), location);

        int temp = (int)Mathf.Round(-Input.compass.trueHeading);


        if ((Mathf.Abs(temp - lastCompassDir)) > 10)
        {

            lastCompassDir = compassDir;

        }


        if (compassDir < -90)
        {

            arrow.transform.rotation = Quaternion.Slerp(arrow.transform.rotation, Quaternion.Euler(0, Mathf.Abs(compassDir + 90f), 90), Time.deltaTime * 2f);
        }
        else
        {

            arrow.transform.rotation = Quaternion.Slerp(arrow.transform.rotation, Quaternion.Euler(0, Mathf.Abs(360 - (90 - Mathf.Abs(compassDir))), 90), Time.deltaTime * 2f);
        }
    }

}
