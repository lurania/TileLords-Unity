
using DataModel.Client;
using DataModel.Common;
using UniRx;
using UnityEngine;

public class AndroidCamera : MonoBehaviour
{

    
    public float turnSpeed = 10.0f;
    public GameObject player;
    public float cameraSpeed = 10f;

    private Transform playerTransform;
    private Vector3 offset;
    private float yOffset = 15.0f;
    private float zOffset = 10.0f;

    Vector2 startPos;
    Vector2 direction;
    Vector2 lastPos;
    float swipeDirectionSet = 1f;
    float swipeDir = 0f;

    float oldX = 0;

    


    void Start()
    {
        
        playerTransform = player.transform;
        offset = new Vector3(playerTransform.position.x, playerTransform.position.y + yOffset, playerTransform.position.z + zOffset);


     

    }


    void Update()
    {



        //register touch
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);


            switch (touch.phase)
            {
                //the first contact with the screen
                case TouchPhase.Began:

                    startPos = touch.position;

                    lastPos = startPos;
                    break;

                //the finger is moving over the screen
                case TouchPhase.Moved:
                    //determine the direction that the finger is moving
                    direction = touch.position - startPos;
                    break;

                //the finger is lifted off the screen
                case TouchPhase.Ended:
                    //set the swipe direction to 0
                    swipeDir = 0;

                    break;
            }

            
            //determine the swipe direction
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {

                //swipe to right
                if (direction.x > 0)
                {
                    swipeDir = swipeDirectionSet;

                    //swipe direction has changed
                    if (oldX > direction.x)
                    {

                        swipeDir = 0;
                        startPos = touch.position;

                    }

                }
                //swipe to left
                else
                {
                    swipeDir = -swipeDirectionSet;

                    //swipe direction has changed
                    if (oldX < direction.x)
                    {

                        swipeDir = 0;
                        startPos = touch.position;

                    }
                }
            }


            if (Vector2.Distance(touch.position, startPos) > 30) //threshold for a swipe to be actually executed
            {
                //move the camera in the proper direction, the distance is between the old touch from last frame and this frame
                offset = Quaternion.AngleAxis(swipeDir * Vector2.Distance(touch.position, lastPos) * Time.deltaTime * cameraSpeed, Vector3.up) * offset;
                transform.position = playerTransform.position + offset;
                transform.LookAt(playerTransform.position);
            }


            lastPos = touch.position;
            oldX = direction.x;



        }
        else
        {
           
            transform.position = playerTransform.position + offset;
            transform.LookAt(playerTransform.position);
        }


    }


}

