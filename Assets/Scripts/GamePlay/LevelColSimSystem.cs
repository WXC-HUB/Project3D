using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.BaseUtils;
using System.Linq;


public class LevelColSimSystem : MonoSingleton<LevelColSimSystem>
{

    GameObject TempObj;
    public List<Vector3> SimCol(GameObject trySimObject , Vector2 DirInpt , float max_leng , int max_times )
    {
        CircleCollider2D oricollider = trySimObject.GetComponent<CircleCollider2D>();
        if (oricollider == null)
        {
            Debug.LogError("传入了非圆形碰撞！");
            return null;
        }
        List<Vector3> list = new List<Vector3>();

        GameObject tempobj = TempObj;
        tempobj.transform.SetParent(this.transform);
        tempobj.layer = LayerMask.NameToLayer("IgnoreAll");
        tempobj.transform.position = trySimObject.transform.position;



        CircleCollider2D tempcollider;
        if(tempobj.GetComponent<CircleCollider2D>() != null)
        {
            tempcollider = tempobj.GetComponent<CircleCollider2D>();
        }
        else
        {
            tempcollider = tempobj.AddComponent<CircleCollider2D>();
        }
        
        
        tempcollider.radius = oricollider.radius;
        tempcollider.isTrigger = true;  
        float rad = tempcollider.radius;    
        Vector2 rot_input = (UI_VirtualInput.instance as UI_VirtualInput).GetDir("Right");
        float last_leng = max_leng;
        Vector2 start_pos = trySimObject.transform.position;
        List<Vector3> nodeList = new List<Vector3>();
        nodeList.Add(start_pos);

        for (int i = 0; (i < max_times && last_leng > 0); i++)
        {
            tempobj.transform.position = start_pos;

            RaycastHit2D[] result = new RaycastHit2D[10];

            
            //int aa = tempcollider.Cast(rot_input, con, result, Mathf.Infinity);
            Debug.DrawRay(tempcollider.transform.position , rot_input , Color.yellow , .5f);
            //GameUtils.DrawCircle(tempcollider.transform.position, .5F, Color.yellow);

            result = Physics2D.CircleCastAll(start_pos, rad, rot_input , last_leng , LayerMask.GetMask("MapBlock") | LayerMask.GetMask("Player"));

            List<RaycastHit2D> result_array =  result.ToList();
            RaycastHit2D r1 = result_array.Find(it => it.transform != null && it.transform.gameObject != trySimObject);

            if (r1.transform != null)
            {
                //rot_input = r1.normal;
                //start_pos = r1.point + r1.normal.normalized * rad;
                //start_pos = r1.point + r1.normal * rad ;

                start_pos = PhysicUtils.getNewPositionAfterCircleHit2D(rad, r1);
                rot_input = PhysicUtils.getNewMoveSpeedAfterCircleHit2D( rot_input, r1);
               // GameUtils.DrawCircle(r1.point, rad, Color.blue);
               // GameUtils.DrawCircle(start_pos , rad, Color.green);

                //Debug.DrawLine(start_pos, start_pos + 10 * rot_input, Color.yellow, 10000f);
                //Debug.DrawLine(start_pos, start_pos + 10 * r1.normal, Color.blue, 10000f);
                nodeList.Add(start_pos);
            }
            else
            {
                nodeList.Add(start_pos + rot_input.normalized * last_leng);
                break;
            }
        }

        for (int i = 0; i < nodeList.Count; i++)
        {
            if (i + 1 < nodeList.Count)
            {
                Debug.DrawLine(nodeList[i], nodeList[i + 1], Color.red, 0.5f);
            }
        }

        //GameObject.Destroy(tempobj.gameObject);

        if (nodeList.Count > 0)
        {
            return nodeList;
        }
        
        return null;    
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        TempObj = transform.Find("TempObj").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
