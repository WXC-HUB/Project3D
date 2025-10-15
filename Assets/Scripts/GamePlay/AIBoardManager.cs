using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIInput : CharacterInputBase
{
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        SkillDispatchCenter.Instance.AddModifierToCharacter(characterCtrl, -1, 3);
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();   
    }
}
