using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;

public abstract class CharacterAttribute { }

[System.Serializable]
public abstract class CharacterAttribute<ValueType> : CharacterAttribute
{
    public ValueType real_value;
    protected CharacterCtrlBase myctrl;
    protected string attribute_name;
    
    public void TakeEffect(CharacterCtrlBase myctrl)
    {
        this.myctrl = myctrl;
        if (myctrl.AttributesDicts.ContainsKey(attribute_name)) 
        {
            myctrl.AttributesDicts[attribute_name] = this;
        }
        else
        {
            myctrl.AttributesDicts.Add(attribute_name, this);   
        }
    }
    public abstract ValueType GetValue();
    public CharacterAttribute( string attribute_name, ValueType startValue, CharacterCtrlBase myctrl = null)
    {
        this.real_value = startValue;
        this.myctrl = myctrl;
        this.attribute_name = attribute_name;
    }
}


public abstract class CharacterAttributeDect : MonoBehaviour { }

public abstract class CharacterAttributeDect<ValueType , DectType> : CharacterAttributeDect where DectType : CharacterAttributeDect<ValueType, DectType>
{
    public ValueType effect_factor;
    public string attribute_name;

    public static List<DectType> getAllDects(GameObject onGameObj , string attribute_name)
    {
        List<DectType> resutlt = new List<DectType>();
        DectType[] rr = (onGameObj.GetComponents<DectType>().ToArray());

        for (int i = 0; i < rr.Length; i++) { 
            if(rr[i].attribute_name == attribute_name)
            {
                resutlt.Add(rr[i]); 
            }
        
        }
        return resutlt;
    }
    //public T DectValue;
    //public abstract T DoDect();
}
[System.Serializable]
public class Character_Bool : CharacterAttribute<bool>
{
    public Character_Bool(string attribute_name, bool startValue) : base(attribute_name, startValue)    { }

    public override bool GetValue()
    {
        //Character_DECT_Bool_Set[] dects = myctrl.GetComponents<Character_DECT_Bool_Set>();
        List<Character_DECT_Bool_Set> dects = Character_DECT_Bool_Set.getAllDects(myctrl.gameObject, attribute_name);
        if (dects.Count > 0) 
        { 
            return dects[dects.Count - 1].effect_factor; 
        }
        return real_value;
    }
}

[System.Serializable]
public class Character_Float : CharacterAttribute<float>
{
    public Character_Float(string attribute_name, float startValue) : base(attribute_name, startValue )
    {
    }

    public override float GetValue()
    {
        List<Character_DECT_Float_Set> set_list = Character_DECT_Float_Set.getAllDects(myctrl.gameObject, attribute_name);
        List<Character_DECT_Float_Add> add_list = Character_DECT_Float_Add.getAllDects(myctrl.gameObject, attribute_name);
        List<Character_DECT_Float_Mul> mul_list = Character_DECT_Float_Mul.getAllDects(myctrl.gameObject, attribute_name);

        if (set_list.Count > 0)
        {
            return set_list[set_list.Count - 1].effect_factor;
        }

        float add_value = 0.0f;
        float mul_value = 1.0f;
        foreach (var item in add_list)
        {
            add_value += item.effect_factor;
        }

        foreach (var item in mul_list)
        {
            mul_value += item.effect_factor;
        }

        return real_value * mul_value + add_value;
        
    }
}
[System.Serializable]
public class Character_Int : CharacterAttribute<int>
{
    public Character_Int(string attribute_name, int startValue) : base(attribute_name, startValue)
    {
    }

    public override int GetValue()
    {
        List<Character_DECT_Int_Set> set_list = Character_DECT_Int_Set.getAllDects(myctrl.gameObject, attribute_name);
        List<Character_DECT_Int_Add> add_list = Character_DECT_Int_Add.getAllDects(myctrl.gameObject, attribute_name);
        List<Character_DECT_Int_Mul> mul_list = Character_DECT_Int_Mul.getAllDects(myctrl.gameObject, attribute_name);

        if (set_list.Count > 0)
        {
            return set_list[set_list.Count - 1].effect_factor;
        }

        int add_value = 0;
        int mul_value = 1;
        foreach (var item in add_list)
        {
            add_value += item.effect_factor;
        }

        foreach (var item in mul_list)
        {
            mul_value += item.effect_factor;
        }

        return real_value * mul_value + add_value;

    }
}

[System.Serializable]
public class Character_Vector2 : CharacterAttribute<Vector2>
{
    public Character_Vector2(string attribute_name, Vector2 startValue) : base(attribute_name, startValue)
    {
    }

    public override Vector2 GetValue()
    {
        List<Character_DECT_Vector2_Set> set_list = Character_DECT_Vector2_Set.getAllDects(myctrl.gameObject, attribute_name);
        List<Character_DECT_Vector2_Add> add_list = Character_DECT_Vector2_Add.getAllDects(myctrl.gameObject, attribute_name);
        if (set_list.Count > 0)
        {
            return set_list[set_list.Count - 1].effect_factor;
        }

        Vector2 add_value = Vector2.zero;
        foreach (var item in add_list)
        {
            add_value += item.effect_factor;
        }

        return real_value + add_value;

    }
}

public class Character_DECT_Bool_Set : CharacterAttributeDect<bool , Character_DECT_Bool_Set> { }
public class Character_DECT_Float_Set : CharacterAttributeDect<float , Character_DECT_Float_Set> { }
public class Character_DECT_Float_Add : CharacterAttributeDect<float , Character_DECT_Float_Add> { }
public class Character_DECT_Float_Mul : CharacterAttributeDect<float, Character_DECT_Float_Mul> { }

public class Character_DECT_Int_Set : CharacterAttributeDect<int, Character_DECT_Int_Set> { }
public class Character_DECT_Int_Add : CharacterAttributeDect<int, Character_DECT_Int_Add> { }
public class Character_DECT_Int_Mul : CharacterAttributeDect<int, Character_DECT_Int_Mul> { }

public class Character_DECT_Vector2_Set : CharacterAttributeDect<Vector2, Character_DECT_Vector2_Set> { }
public class Character_DECT_Vector2_Add : CharacterAttributeDect<Vector2, Character_DECT_Vector2_Add> { }