using Assets.Scripts.BaseUtils;
using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputBase : MonoBehaviour
{
    public bool isInputEnable = false;  
    public PlayerCharacterCtrl characterCtrl;



    //public int maxSimHitTimes = 1;
    //public float maxSimHitLength = 10f;

    //public Transform AimSimObject;
    //public Collider2D AimSimCollider;

    

    //public Transform AimGuideLineSprite;
    //public float AimSimSpeed = 1.0f;
    //public float AimSimProgress = 0f;
    //public LineRenderer AimLineRenderer = null;
    public List<Vector3> AimSimPoints = new List<Vector3>();
    public bool DoAimSim = false;

    


    // Start is called before the first frame update
    protected void Start()
    {
        

        //AimSimObject = GameUtils.FindChildInTransform(this.transform, "AimSimObject");
        //AimSimCollider = AimSimObject.GetComponent<Collider2D>();

        //AimLineRenderer = GameUtils.FindChildInTransform(this.transform, "AimLineRenderer").GetComponent<LineRenderer>();   

        //AimGuideLineSprite = GameUtils.FindChildInTransform(this.transform, "AimGuideLineSprite");
        //AimGuideLineSprite.SetParent(this.transform.parent);


        characterCtrl = GetComponent<PlayerCharacterCtrl>();


        //characterCtrl.isObserve = false;


        Game2D_GamePlayEvent ready_event = new Game2D_GamePlayEvent(EventType_Game2DPlayEvent.CharacterIsReady, gameObject);
        LevelEventQueue.Instance.EnqueueEvent(ready_event);
        Debug.Log("player ready");
    }

    // Update is called once per frame
    protected void Update()
    {
        
    }

    
    public void SetInputEnable(bool enable)
    {
        this.isInputEnable = enable;
    }
}
