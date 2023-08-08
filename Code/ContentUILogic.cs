using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModel.Common;
using UniRx;
using DataModel.Common.Messages;
using System.Linq;


/// <summary>
/// Class that handles the logic of the contentView
/// </summary>
public class ContentUILogic : MonoBehaviour
{


    public ClientConnection clientConnection;
    public MapHandler cubeSpawner;
    public ContentInformationView contentInformation;
    List<ContentMessage> contentMessages;
    public static string currentOpenContent = "0"; //0 being no content is open! otherwise the miniTile code, turns 0 when ContentInformationView is closed

    // Start is called before the first frame update
    void Start()
    {

    

    }

    public void Initialise()
    {
        var latestState = clientConnection.ClientInstance.InboundTraffic.Where(v => v is BatchContentMessage).Select(v => v as BatchContentMessage);
        var pickedUpItem = clientConnection.ClientInstance.InboundTraffic.Where(v => v is MapContentTransactionMessage).Select(v => v as MapContentTransactionMessage)
            .Where(v => v.MessageState == MessageState.SUCCESS).StartWith(new MapContentTransactionMessage() { MapContentId = null });

        latestState.CombineLatest(pickedUpItem, (state, item) => new { state, item }).ObserveOnMainThread().Subscribe(v =>
        {
            //when an item was picked up and the contentView is open
            if (v.item.MapContentId != null && currentOpenContent != "0")
            {
                //refresh the contentView and map
                var deletedStuff = v.state.ContentList.RemoveAll(e => e.Id.SequenceEqual(v.item.MapContentId));
                cubeSpawner.RebuildContentOnMap(v.state);
                contentMessages = v.state.ContentList;
                if (currentOpenContent != "0")
                {
                    OpenContentForTile(currentOpenContent, v.state.ContentList);
                }
            }


        }
        );
    }

    /// <summary>
    /// This functions is used to identify the button that called it, get content information attached in the MiniTileUnity script and relay it to the rendering function
    /// </summary>
    public void GetLastCalledContentButton()
    {
        GameObject callingButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        GameObject miniTileObject = callingButton.transform.parent.parent.gameObject; //parents: canvas, miniTile
        MiniTileUnity miniTileU = miniTileObject.GetComponent<MiniTileUnity>();


        RenderContentOnScreen(miniTileU.contentList);
        currentOpenContent = miniTileU.code.Code;

    }

    /// <summary>
    /// This function filters the content belonging to a miniTile with the given code and a list of content, and then call the render function
    /// </summary>
    public void OpenContentForTile(string code, List<ContentMessage> content)
    {
        if (content != null)
        {
            RenderContentOnScreen(content.Where(v => v.Location.Equals(code)).ToList());
        }
    }
    public void RenderContentOnScreen(List<ContentMessage> content)
    {
        contentInformation.FilterView(content);

    }

}
