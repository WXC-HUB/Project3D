using System.Collections;
using UnityEngine;
using Assets.Scripts.BaseUtils;
using Assets.Scripts.Core;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Assets.Scripts.UI;
using TMPro;
using Assets.Scripts.AI;
using UnityEngine.Tilemaps;
using System;
using System.Linq;

namespace Assets.Scripts.Core
{
    public enum LevelState
    {
        Pause = 1 , 
        Playing = 2 ,
    }

    [System.Serializable]
    public class RoundGameTeamInfo
    {
        public int TeamID;
        public CharacterInputBase his_Character;
    }

    public enum InGameCharacterType
    {
        None,
        Player,
        Tower,
        Enemy,
        Bullet,
        Ingredient,
        Dish,
        SpawnRoot
    }


    //一个关卡对应一个LevelManager，允许跨场景使用。但是玩家切换关卡时，必须重新初始化LevelManager。如果需要跨大关卡传参，那么往更上层的GameManager缓存里放。
    public class LevelManager : MonoSingleton<LevelManager>
    {

        public Transform LevelObjectsRoot;

        public int currentLevelID;
        public int focusSublLevelID;

        public List<RoundGameTeamInfo> levelTeams = new List<RoundGameTeamInfo>();

        public int NowWorkTeamI;

        public int NowRoundID = 0;
        public int MaxRoundID = 1;
        public bool isRoundGameStart = false;

        public List<CharacterCtrlBase> RoundNextFlag = new List<CharacterCtrlBase>();   

        public Dictionary<InGameCharacterType, List<CharacterCtrlBase>> Character_Dict = new Dictionary<InGameCharacterType, List<CharacterCtrlBase>>();


        public void AddRoundNextFlag(CharacterCtrlBase ch , int MaxID)
        {
            RoundNextFlag.Add(ch);  
            this.MaxRoundID = Math.Max(this.MaxRoundID, MaxID);    
        }

        public void GoNextRound()
        {
            // 防止延迟调用时 Instance 已被销毁
            if (LevelGridGenerator.Instance == null)
            {
                Debug.LogWarning("GoNextRound called but LevelGridGenerator.Instance is null");
                return;
            }

            LevelGridGenerator.Instance.StartSpawn(true);   
            NowRoundID += 1;
        }

        public bool TestNextRound()
        {
            if (!this.isRoundGameStart)
            {
                return false;
            }

            // 过滤掉已被销毁的对象
            if(RoundNextFlag.All( x => x != null && x.isReadyForNextRound))
            {
                if (LevelGridGenerator.Instance == null)
                {
                    Debug.LogWarning("TestNextRound: LevelGridGenerator.Instance is null");
                    return false;
                }

                Invoke("GoNextRound", 5f);
                LevelGridGenerator.Instance.GoNextRound();
                if(NowRoundID > MaxRoundID)
                {
                    LevelWin();
                }
                
                return true;
            }
            else
            {
                return false;
            }
        }

        public void LevelWin()
        {
            
        }

        public T_CHar SpawnCharacterByID<T_CHar>(int ID , SkillUseInfo call_by_skill = null) where T_CHar : CharacterCtrlBase
        {
            GameCharacters g_config = GameTableConfig.Instance.Config_GameCharacters.FindFirstLine(x => x.ObjectID == ID);
            string enemy_obj_name = g_config.BindPrefab;
            InGameCharacterType characterType = (InGameCharacterType)Enum.Parse(typeof(InGameCharacterType) , g_config.ObjectType);
            GameObject newobj = Resources.Load<GameObject>("CharacterPrefabs/" + enemy_obj_name);

            if (newobj != null)
            {
                GameObject sp_obj = Instantiate(newobj, LevelManager.Instance.LevelObjectsRoot);
                if (!Character_Dict.ContainsKey(characterType))
                {
                    Character_Dict.Add(characterType , new List<CharacterCtrlBase>());
                }
                T_CHar newsp = sp_obj.GetComponent<T_CHar>();
                foreach (int buff in g_config.InitModifier)
                {

                    SkillDispatchCenter.Instance.AddModifierToCharacter(newsp, -1, buff);
                }
                Character_Dict[characterType].Add(newsp);
                newsp.MyGameObjectID = ID;
                newsp.MyObjectLayer = characterType;

                if(characterType is InGameCharacterType.Tower || characterType is InGameCharacterType.Enemy)
                {
                    UI_PlayerHUD.instance.InitCharacterFollowHUD(newsp, characterType); 
                }


                return newsp;
            }
            else
            {
                Debug.LogError("尝试生成未定义的物体:" + "CharacterPrefabs/" + enemy_obj_name);
            }

            return null;

        }

        public void ClearCharDic()
        {
            // 清理全局角色字典中的 null 引用
            foreach (var list_i in Character_Dict.Values)
            {
                list_i.RemoveAll(i => null==i);
            }
        }

        private void Awake()
        {
            base.Awake();

            InitLevelEventManager();
            InitLevelEventQueue();

            // 初始化数据
            DishSubmissionManager.Instance.InitializeDishes();

            // 初始化UI

            UIManager.Instance.InitUIManager();
            UIManager.Instance.CreateUIByName<UI_VirtualInput>("UI_VirtualInput");
            UIManager.Instance.CreateUIByName<UI_PlayerHUD>("UI_PlayerHUD");
        }


       
        /// <summary>
        /// 用这个函数来启动关卡
        /// </summary>
        /// <param name="levelID"></param>
        /// <param name="subLevelID"></param>
        //public void LoadLevelConfigFromTable(int levelID , int subLevelID)
        //{
        //    LevelConfig levelSetting =  GameTableConfig.Instance.allLevelConfigs.FindFirstLine(t => ( t.LevelID == levelID && t.subLevelID == subLevelID )  );

        //    //RoleSetting newLevelHeroData = GameTableConfig.Instance.roleConifg.FindFirstLine(t => t.RoleID == levelSetting.LevelHeroRoleID);

        //    //初始化地图网格

        //    //buildingSystem.InitGrids();

        //    //buildingSystem.InitGrids(levelSetting.LineX , levelSetting.LineY );

        //}

        private GameObject getHeroGameObject()
        {
            return null;
        }

        // Use this for initialization
        void Start()
        {
            //StartRoundGame();
            //Invoke("StartRoundGame", 1);
            //LevelEvnetManager.Instance.AddListener(EventType_Game2DCMD.GoNextPlayer, GameCMD_GoNextPlayer);
            //LevelEvnetManager.Instance.AddListener(EventType_Game2DPlayEvent.CharacterEndMove , GameCMD_GoNextPlayer);

            Invoke("StartMyGame", 1);
        }

        void StartMyGame()
        {
            LevelGridGenerator.Instance.LoadLevelByID(1);
            isRoundGameStart = true;
            GoNextRound();
            UI_PlayerHUD.instance.UpdateRecipe();
        }

        //事件系统通过回调，严格控制游戏的整体运行流程。但由于事件的触发分布在各个GameObject中，所以无法严格保证先后顺序
        private void InitLevelEventManager()
        {
           
        }

        //事件队列单线程地处理所有事件，用来处理必须严格有先后顺序的功能
        private void InitLevelEventQueue()
        {
            LevelEventQueue.Instance.InitEventQueue();
        }
        

        // Update is called once per frame
        void Update()
        {
            //每帧清空事件队列
            LevelEventQueue.Instance.EventQueueTick();
            TestNextRound();
        }
    }
}