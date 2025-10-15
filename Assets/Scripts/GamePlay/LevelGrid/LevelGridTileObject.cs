using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGridTileObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Outline>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSelect(bool isSelect)
    {
        if (isSelect)
        {
            Debug.Log(transform.name);
        }
        this.GetComponent<Outline>().enabled = isSelect;
    }

    public void TryAttachObject(GameObject go)
    {
        go.transform.SetParent(transform,true);
        go.transform.position = transform.position + new Vector3(0 , 0 ,-0.5f);  
    }
}
