using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownUILogic : MonoBehaviour
{

    public QuestTracker quest;
    public GameObject townCanvas;


    public void Start()
    {
        townCanvas.SetActive(false);
    }


    public void OpenTownUI()
    {
        townCanvas.SetActive(true);
        MiniTileToggle.setMiniTilesActive(false);
    }

    public void CloseTownUI()
    {
        townCanvas.SetActive(false);
        MiniTileToggle.setMiniTilesActive(true);
    }

    public void ReceiveQuestFromTown()
    {
        quest.RequestQuest();
    }
}
