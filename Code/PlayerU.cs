using DataModel.Common;
using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerU : MonoBehaviour
{

    public Animator animator;
    GameObject avatar { get; set; }
    public static string playerName { get; set; }

    public static PlusCode currentPlusCodeLocation { get; set; }
    public static Dictionary<ResourceType, int> playerResources;
    int compassDir = 0;
    int lastCompassDir = -99999;

    public PlayerU()
    {
        playerResources = new Dictionary<ResourceType, int>();
        for (int i = 0; i < Enum.GetNames(typeof(ResourceType)).Length; i++)
        {
            playerResources.Add((ResourceType)i, 0);
        }
    }

    public static void SetResource(ResourceType resource, int amount)
    {
        playerResources[resource] = amount;
    }

    public static int GetResourceAmount(ResourceType resource)
    {
        return playerResources[resource];
    }


    IEnumerator WaitForUserNameInput()
    {
        while (playerName.Equals(""))
        {
            yield return new WaitForSeconds(3);
        }
        if (!playerName.Equals("") && GetComponentInChildren<TMP_Text>() != null)
        {
            GetComponentInChildren<TMP_Text>().text = playerName;
        }


    }


    public void Start()
    {
        playerName = "";
        StartCoroutine(WaitForUserNameInput());
        StartCoroutine(IdleStance());


    }



    IEnumerator IdleStance()
    {
        
        int i = 0;

        while (true)
        {
            bool run = animator.GetBool("running");
            
      
            if (!run)
            {
                if (i == 10)
                {
                    
                    animator.SetInteger("idle", 2);
                }
                if (i == 15)
                {
                  
                    animator.SetInteger("idle", 1);
                    i = 0;
                }
                i++;
            }
         
            yield return new WaitForSeconds(1);
        }
    }


    public IEnumerator WalkCycle()
    {
        int i = 1;
        while (i > 0)
        {
            i--;
            animator.SetBool("running", true);
            yield return new WaitForSeconds(1);
        }
        animator.SetBool("running", false);

    }
    public void Update()
    {





        int temp = (int)Mathf.Round(-Input.compass.trueHeading);


        if ((Mathf.Abs(temp - lastCompassDir)) > 10)
        {
            compassDir = (int)Mathf.Round(-Input.compass.trueHeading);
            lastCompassDir = compassDir;

        }

        if (compassDir < -90)
        {

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, Mathf.Abs(compassDir + 90f), 0), Time.deltaTime * 2f);
        }
        else
        {

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, Mathf.Abs(360 - (90 - Mathf.Abs(compassDir))), 0), Time.deltaTime * 2f);
        }

    }



}
