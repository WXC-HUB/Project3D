using Assets.Scripts.BaseUtils;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class GameFieldBase : MonoBehaviour
{
    Collider2D col2d;
    float tickduration = .2f;
    float cur_tick = 0f;

    public List<CharacterCtrlBase> myHitAttributs;

    bool isFieldActive = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    public virtual void InitField()
    {
        this.isFieldActive = true;

        col2d = this.GetComponent<Collider2D>();
        cur_tick = 0f;

        OnFieldStart(this.getAllColCharacter());

    }

    public List<CharacterCtrlBase> getAllColCharacter()
    {
        List<CharacterCtrlBase> result = new List<CharacterCtrlBase>();
        RaycastHit2D[] ray2D = new RaycastHit2D[100];
        col2d.Cast(Vector2.zero, ray2D, 0f) ;

        for (int i = 0; i < ray2D.Length; i++)
        {
            if (ray2D[i].transform == null) continue;

            GameUtils.DrawCircle(ray2D[i].point, 1f , Color.red);
            GameUtils.DrawCircle(transform.position, (this.col2d as CircleCollider2D).radius, Color.green);

            CharacterCtrlBase aa = ray2D[i].transform.GetComponent<CharacterCtrlBase>();
            if (null != aa)
            {
                result.Add(aa);
            }
        }
        return result;
    }

    public virtual void OnFieldStart(List<CharacterCtrlBase> target_character) 
    {
        Debug.Log("start11111111");
        Debug.Log(target_character.Count);
        for(int i = 0; i < target_character.Count; i++)
        {
            Debug.Log("on start!" + target_character[i].transform.name);
            OnFieldStart(target_character[i]);
        }
    }
    public virtual void OnFieldTick(List<CharacterCtrlBase> target_character) 
    {
        for (int i = 0; i < target_character.Count; i++)
        {
            OnFieldTick(target_character[i]);
        }
    }

    public virtual void OnFieldStart(CharacterCtrlBase target_character) { }
    public virtual void OnFieldTick(CharacterCtrlBase target_character) { }

    // Update is called once per frame
    void Update()
    {
        if (this.isFieldActive)
        {
            cur_tick += Time.deltaTime;
            if (cur_tick >= tickduration)
            {
                OnFieldTick(this.getAllColCharacter());
                cur_tick = 0f;
            }
        }
        
    }

    public void OnTriggerEnter2D(Collider2D hit)
    {
        //Debug.Log(hit.gameObject);
    }
}
