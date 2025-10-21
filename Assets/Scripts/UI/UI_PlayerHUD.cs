using Assets.Scripts.BaseUtils;
using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class PlayerFollowHUD
{
    public Transform root;
    public List<Slider> observe_xuli_sliders = new List<Slider>();
    public Slider m_Slider_HP;
    public Text m_Text_NowHP;
    
}

public class UI_PlayerHUD : BaseUI<UI_PlayerHUD>
{
    public PlayerCharacterCtrl observe_CharacterCtrl;

    //List<Slider> observe_xuli_sliders = new List<Slider>();

    //Slider m_Slider_HP;
    //Text m_Text_NowHP;

    public Dictionary<CharacterCtrlBase, PlayerFollowHUD> player_follow_dics_hp = new Dictionary<CharacterCtrlBase, PlayerFollowHUD>();
    public Dictionary<CharacterCtrlBase, PlayerFollowHUD> player_follow_dics_mp = new Dictionary<CharacterCtrlBase, PlayerFollowHUD>();

    public override void InitUI()
    {
        base.InitUI();

        player_follow_dics_hp.Clear();
        player_follow_dics_mp.Clear();
        this.nodeDics["m_Item_HP_Tower"].gameObject.SetActive(false);
        this.nodeDics["m_Item_HP_Enemy"].gameObject.SetActive(false);
        this.nodeDics["m_Item_Dish"].gameObject.SetActive(false);

    }

    public string getDishImageByID(int dishID)
    {
        Dish dd = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.DishID == dishID);
        if (dd != null) 
        {
            return dd.IconPath;
        }
        return "";
    }

    public void UpdateRecipe()
    {
        foreach(var dishID in DishSubmissionManager.Instance.dishIdToCount.Keys)
        {
            var newobj = Instantiate(this.nodeDics["m_Item_Dish"]);
            Recipe toDoRecipe = GameTableConfig.Instance.Config_Recipe.FindFirstLine(x => x.CookResult == dishID);
            if (toDoRecipe != null) 
            {
                LoadImageToUI(GameUtils.FindChildInTransform(newobj.transform, "m_Sprite_DishResult").GetComponent<Image>(), getDishImageByID(toDoRecipe.CookResult));

                LoadImageToUI(GameUtils.FindChildInTransform(newobj.transform, "m_Sprite_CookType").GetComponent<Image>(), toDoRecipe.cookTypeIconPath);

                var new_from_obj_item = GameUtils.FindChildInTransform(newobj.transform, "m_Sprite_FromDish");
                var new_from_obj_parent = GameUtils.FindChildInTransform(newobj.transform, "m_Grid_FromDish");

                new_from_obj_item.gameObject.SetActive(false);

                foreach (var fromdis in toDoRecipe.DishList)
                {
                    var to_sp = Instantiate(new_from_obj_item);
                    to_sp.SetParent(new_from_obj_parent);
                    LoadImageToUI(to_sp.GetComponent<Image>(), getDishImageByID(fromdis));

                    to_sp.gameObject.SetActive(true);
                }

            }

            newobj.transform.SetParent(this.nodeDics["m_Grid_Rcipe"].transform);
            newobj.gameObject.SetActive(true);
        }
    }

    public void InitCharacterFollowHUD( CharacterCtrlBase pctrl , InGameCharacterType type)
    {
        
        
        PlayerFollowHUD hd = new PlayerFollowHUD();
        GameObject newObj = null;


        if (type == InGameCharacterType.Tower)
        {

            newObj = Instantiate(this.nodeDics["m_Item_HP_Tower"]);
            hd.root = newObj.transform;
            hd.m_Slider_HP = GameUtils.FindChildInTransform(hd.root, "m_Slider_HP_Tower").GetComponent<Slider>();
            hd.m_Text_NowHP = GameUtils.FindChildInTransform(hd.root, "m_Text_NowHP_Tower").GetComponent<Text>();

            if (player_follow_dics_mp.ContainsKey(pctrl))
            {
                player_follow_dics_mp[pctrl] = hd;
            }
            else
            {
                player_follow_dics_mp.Add(pctrl, hd);
            }
        }
        else if(type == InGameCharacterType.Enemy)
        {
            newObj = Instantiate(this.nodeDics["m_Item_HP_Enemy"]);
            hd.root = newObj.transform;
            hd.m_Slider_HP = GameUtils.FindChildInTransform(hd.root, "m_Slider_HP_Enemy").GetComponent<Slider>();
            hd.m_Text_NowHP = GameUtils.FindChildInTransform(hd.root, "m_Text_NowHP_Enemy").GetComponent<Text>();

            if (player_follow_dics_hp.ContainsKey(pctrl))
            {
                player_follow_dics_hp[pctrl] = hd;
            }
            else
            {
                player_follow_dics_hp.Add(pctrl, hd);
            }
        }

        if (newObj == null) { return; }
        
        

        

        newObj.transform.SetParent(this.transform);
        newObj.gameObject.SetActive(true);

        
    }

    public void SetSkillFocusPlayer(PlayerCharacterCtrl playerCharacterCtrl)
    {
        observe_CharacterCtrl = playerCharacterCtrl;
    }

    public void UpdatePlayerDate()
    {
        List<CharacterCtrlBase> to_del = new List<CharacterCtrlBase>();
        foreach (var item in player_follow_dics_hp) 
        {

            if (item.Key == null)
            {
                to_del.Add(item.Key);

            }
            else
            {

                item.Value.m_Slider_HP.value = (float)item.Key.NowHP / (float)item.Key.MaxHP.GetValue();
                item.Value.m_Text_NowHP.text = string.Format("{0}/{1}", item.Key.NowHP, item.Key.MaxHP.GetValue());

                item.Value.root.transform.position = Camera.main.WorldToScreenPoint(item.Key.transform.position);
            }


        }

        foreach (var item in to_del)
        {
            Destroy(player_follow_dics_hp[item].root.gameObject);
            player_follow_dics_hp.Remove(item);
        }

        foreach (var item in player_follow_dics_mp)
        {

            if (item.Key == null)
            {
                to_del.Add(item.Key);

            }
            else
            {

                item.Value.m_Slider_HP.value = (float)item.Key.NowMP / (float)item.Key.MaxMP.GetValue();
                item.Value.m_Text_NowHP.text = string.Format("{0}/{1}", item.Key.NowMP, item.Key.MaxMP.GetValue());

                item.Value.root.transform.position = Camera.main.WorldToScreenPoint(item.Key.transform.position);
            }


        }

        foreach (var item in to_del)
        {
            if(item == null) continue;
            
            Destroy(player_follow_dics_mp[item].root.gameObject);
            player_follow_dics_mp.Remove(item);
        }

        nodeDics["m_Slider_HP_Level"].GetComponent<Slider>().value = (float)observe_CharacterCtrl.NowHP / (float)observe_CharacterCtrl.MaxHP.GetValue();

        nodeDics["m_Text_NowHP_Level"].GetComponent<Text>().text = string.Format("{0}/{1}", observe_CharacterCtrl.NowHP , observe_CharacterCtrl.MaxHP.GetValue());

    }

    public void UpdatePlayerFollowInfo()
    {
        //this.nodeDics["m_Slider_ChargeItem"].SetActive(false);
        //foreach(var item in observe_xuli_sliders)
        //{
        //    GameObject.Destroy(item.gameObject);
        //}

        //observe_xuli_sliders.Clear();

        //for(int i = 0; i < 3; i++)
        //{
        //    GameObject go = Instantiate(this.nodeDics["m_Slider_ChargeItem"]);

        //    go.transform.SetParent(this.nodeDics["m_Grid_Charge"].transform);
        //    go.SetActive(true);

        //    observe_xuli_sliders.Add(go.GetComponent<Slider>());    
        //}
    }

    // Start is called before the first frame update
    void Start()
    {
        //UpdatePlayerFollowInfo();

        //m_Slider_HP = this.nodeDics["m_Slider_HP"].GetComponent<Slider>();
        //m_Text_NowHP = this.nodeDics["m_Text_NowHP"].GetComponent<Text>();
    }
    // Update is called once per frame
    void Update()
    {
        //if(this.observe_CharacterCtrl != null)
        //{
        //    this.nodeDics["m_FollowPlayerRoot"].transform.position
        //        = Camera.main.WorldToScreenPoint(this.observe_CharacterCtrl.transform.position);
        //}
        


        UpdatePlayerDate();

    }

}
