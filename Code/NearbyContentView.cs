using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NearbyContentView : MonoBehaviour
{
    

    public MapHandler cubeSpawner;
    public GameObject nearbySpawnPanel;
    public GameObject nearbySpawnTemplate;
    List<GameObject> cubeListCopy = new List<GameObject>();
    List<GameObject> toDelete = new List<GameObject>();
    public GameObject openButton;
    public GameObject closeButton;

    public int howManyIconsCanFit = 50;
    public int refreshRate = 10;


    void Start()
    {
        nearbySpawnPanel.SetActive(false);
        closeButton.SetActive(false);
        openButton.SetActive(true);
        StartCoroutine(RefreshContent());
    }

    public void OpenNearby()
    {
        closeButton.SetActive(true);
        openButton.SetActive(false);
        nearbySpawnPanel.SetActive(true);
    }

    public void CloseNearby()
    {

        closeButton.SetActive(false);
        openButton.SetActive(true);
        nearbySpawnPanel.SetActive(false);
    }

    //Shows what is around the player on nearby screen
    IEnumerator RefreshContent()
    {
        while (true)
        {

            yield return new WaitForSeconds(refreshRate);
            foreach (var obj in toDelete)
            {
                Destroy(obj);
            }
            toDelete.Clear();


            cubeListCopy = cubeSpawner.cubeList;
            foreach (var obj in cubeListCopy)
            {

                MiniTileUnity miniU = obj.GetComponent<MiniTileUnity>();

                foreach (var content in miniU.contentList)
                {
                    if (toDelete.Count < howManyIconsCanFit)
                    {

                        GameObject nearbySpawn = Instantiate(nearbySpawnTemplate);
                        nearbySpawn.transform.SetParent(nearbySpawnPanel.transform);
                        nearbySpawn.SetActive(true);
                        nearbySpawn.transform.localScale = new Vector3(1, 1, 1);
                        toDelete.Add(nearbySpawn);

                        Image icon = nearbySpawn.transform.Find("NearbyIcon").gameObject.GetComponent<Image>();

                        if (content.Type == ContentType.RESOURCE)
                        {

                            icon.sprite = cubeSpawner.iconSpriteDict[((content.ResourceType + "").Substring(0, 1)) + (content.ResourceType + "").ToLower().Substring(1) + "Icon"];

                        }
                        else if (content.Type == ContentType.PLAYER)
                        {

                            icon.sprite = cubeSpawner.iconSpriteDict["PlayerIcon"];

                        }
                        else if (content.Type == ContentType.QUESTLEVEL1)
                        {

                            icon.sprite = cubeSpawner.iconSpriteDict["QuestIcon"];

                        }
                    }
                }

            }

        }
    }
}

