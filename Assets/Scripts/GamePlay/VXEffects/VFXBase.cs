using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXBase : MonoBehaviour
{
    public float SelfDestroyDutration = -1f;
    public float DestroyDelay = .3f;
    float starttime;
    // Start is called before the first frame update
    void Start()
    {
        starttime = Time.time;  
    }

    public void CallDestroy()
    {
        Invoke("DestroySelf", DestroyDelay);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }


    // Update is called once per frame
    void Update()
    {
        if (SelfDestroyDutration > 0f && Time.time - starttime > SelfDestroyDutration) 
        {
            Destroy(gameObject);
        }   
    }
}
