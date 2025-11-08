using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public PlayerCharacterCtrl follow_characterCtrl;
    public Camera myCamera;
    float zfix = -30;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var tr = new Vector3(
                Mathf.Min(
                    Mathf.Max(follow_characterCtrl.transform.position.x , -0.41f),
                    3.46f),

                follow_characterCtrl.transform.position.y,
                follow_characterCtrl.transform.position.z

            );

        this.transform.position = Vector3.Lerp(
                this.transform.position, tr, 4*Time.deltaTime
            );
        this.transform.position = new Vector3( this.transform.position.x , this.transform.position.y , zfix );

        if(follow_characterCtrl.IsFOVLock.GetValue() == false)
        {
            myCamera.orthographicSize = Mathf.Lerp(
                myCamera.orthographicSize, follow_characterCtrl.targetCameraFOV.GetValue() , .5F * Time.deltaTime
            );
        }
        else
        {

        }

        
    }
}
