using Assets.Scripts.BaseUtils;
using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFollowHUD
{
    public Transform root;
    public List<Slider> observe_xuli_sliders = new List<Slider>();
    public Slider m_Slider_HP;
    public Text m_Text_NowHP;
    public Text m_Text_NowNaili;
    public Slider m_Slider_ChargeItem;
    public Slider m_Slider_LastPower;
    public LayoutGroup m_Grid_Charge;
}

public class UI_PlayerHUD : BaseUI<UI_PlayerHUD>
{
    public PlayerCharacterCtrl observe_CharacterCtrl;

    //List<Slider> observe_xuli_sliders = new List<Slider>();

    //Slider m_Slider_HP;
    //Text m_Text_NowHP;

    public Dictionary<PlayerCharacterCtrl, PlayerFollowHUD> player_follow_dics = new Dictionary<PlayerCharacterCtrl, PlayerFollowHUD>();

    public override void InitUI()
    {
        base.InitUI();
        
        player_follow_dics.Clear();
        LevelEvnetManager.Instance.AddListener(EventType_Game2DPlayEvent.CharacterIsReady, this.InitCharacterFollowHUD);
        this.nodeDics["m_FollowPlayerRoot"].gameObject.SetActive(false);    
    }

    public void InitCharacterFollowHUD(BaseEventArgs args)
    {
        PlayerCharacterCtrl pctrl = args.sender.GetComponent<PlayerCharacterCtrl>();    
        if(pctrl == null)
        {
            Debug.LogError("不包含Player！" + args.sender.name);
        }
        else
        {
            PlayerFollowHUD hd = new PlayerFollowHUD();

            GameObject newObj = Instantiate(this.nodeDics["m_FollowPlayerRoot"]);

            hd.root = newObj.transform;
            hd.m_Slider_HP = GameUtils.FindChildInTransform(hd.root , "m_Slider_HP").GetComponent<Slider>();
            hd.m_Text_NowHP = GameUtils.FindChildInTransform(hd.root, "m_Text_NowHP").GetComponent<Text>();
            hd.m_Slider_LastPower = GameUtils.FindChildInTransform(hd.root, "m_Slider_LastPower").GetComponent<Slider>();
            hd.m_Text_NowNaili = GameUtils.FindChildInTransform(hd.root, "m_Text_NowNaili").GetComponent<Text>();
            hd.m_Slider_ChargeItem = GameUtils.FindChildInTransform(hd.root, "m_Slider_ChargeItem").GetComponent<Slider>();
            hd.m_Grid_Charge = GameUtils.FindChildInTransform(hd.root, "m_Grid_Charge").GetComponent<LayoutGroup>();
            hd.m_Slider_ChargeItem.gameObject.SetActive(false);  

            for (int i = 0; i < 3; i++)
            {
                GameObject go = Instantiate(hd.m_Slider_ChargeItem.gameObject);
                go.transform.SetParent(hd.m_Grid_Charge.transform);
                go.SetActive(true);
                hd.observe_xuli_sliders.Add(go.GetComponent<Slider>());
            }

            if(player_follow_dics.ContainsKey( pctrl))
            {
                player_follow_dics[pctrl] = hd;
            }
            else
            {
                player_follow_dics.Add(pctrl, hd);  
            }

            newObj.transform.SetParent(this.transform);
            newObj.gameObject.SetActive(true);

        }
    }

    public void SetSkillFocusPlayer(PlayerCharacterCtrl playerCharacterCtrl)
    {
        observe_CharacterCtrl = playerCharacterCtrl;
    }

    public void UpdatePlayerDate()
    {
        foreach (var item in player_follow_dics) 
        {

            if (item.Key == null) return;

            item.Value.m_Slider_HP.value = (float)item.Key.NowHP / (float)item.Key.MaxHP.GetValue();
            item.Value.m_Text_NowHP.text = string.Format("{0}/{1}", item.Key.NowHP, item.Key.MaxHP.GetValue());


        }
            

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
        

        foreach (var item in player_follow_dics)
        {
            if(item.Key != null)
            {
                item.Value.root.position
                    = Camera.main.WorldToScreenPoint(item.Key.transform.position);
            }
        }

        UpdatePlayerDate();

    }

}
