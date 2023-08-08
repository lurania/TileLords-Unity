using DataModel.Common;
using DataModel.Common.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniTileUnity : MonoBehaviour
{

    public GameObject miniTileObject;
    public GameObject childObject;
    public PlusCode code;
    MiniTile miniTile;
    public List<ContentMessage> contentList = new List<ContentMessage>();
    public List<GameObject> attachedIcons = new List<GameObject>();
    public GameObject multiResource;
    public GameObject oneResource;
    public Image singleIcon;
    public Image icon1;
    public Image icon2;
    public Image icon3;


    public PlusCode getPlusCode()
    {
        return code;
    }
    public void setPlusCode(PlusCode code)
    {
        this.code = code;
    }
    public void setMiniTile(MiniTile miniTile)
    {
        this.miniTile = miniTile;
    }
    public MiniTile getMiniTile()
    {
        return miniTile;
    }
    public void setMiniTile(GameObject g)
    {
        miniTileObject = g;
    }

    public void setChildTile(GameObject g)
    {
        childObject = g;
    }


    public void setMiniTileMesh(Mesh mesh)
    {
        miniTileObject.GetComponent<MeshFilter>().mesh = mesh;
    }
    public void setMiniTileMaterial(Material material)
    {
        miniTileObject.GetComponent<MeshRenderer>().material = material;
    }
    public void setChildTileMeshAndMaterial(GameObject prefab)
    {
        
        MeshFilter childMesh = childObject.GetComponent<MeshFilter>();
   
    
        childMesh.mesh = prefab.GetComponent<MeshFilter>().mesh;
      

    }
    public void setChildTileMaterial(Material material)
    {
        childObject.GetComponent<MeshRenderer>().material = material;
    }

    public void setChildTileRotation(float x, float y, float z)
    {
        childObject.transform.localRotation *= Quaternion.Euler(x, y, z);
    }

    public void setChildTileLocalScale(float x, float y, float z)
    {
        childObject.transform.localScale = new Vector3(x, y, z);
    }

    public void setMiniTileProperties(Mesh mesh, Material material)
    {
        setMiniTileMaterial(material);
        setMiniTileMesh(mesh);
    }




    public GameObject getChild()
    {
        return childObject;
    }

    public GameObject getMiniTileObject()
    {
        return miniTileObject;
    }



 
}
