using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGridTileObject : CharacterCtrlBase
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

        this.GetComponent<Outline>().enabled = isSelect;
    }

    public override bool TryAttachObject(CharacterCtrlBase attach_obj)
    {
        attach_obj.transform.SetParent(transform, true);

        attach_obj.transform.position = transform.position + new Vector3(0, 0, -1.6f);
        nowAttachList.Add(attach_obj);
        attach_obj.isAttachedToOther = true;
        return true;
    }
}
