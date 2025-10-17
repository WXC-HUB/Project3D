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
        Player,
        Tower,
        Enemy,
        Bullet
    }


    //一个关卡对应一个LevelManager，允许跨场景使用。但是玩家切换关卡时，必须重新初始化LevelManager。如果需要跨大关卡传参，那么往更上层的GameManager缓存里放。
    public class LevelManager : MonoSingleton<LevelManager>
    {

        public Transform LevelObjectsRoot;

        public int currentLevelID;
        public int focusSublLevelID;

        public List<RoundGameTeamInfo> levelTeams = new List<RoundGameTeamInfo>();

        public int RoundCnt;
        public int NowWorkTeamI;

        public Dictionary<InGameCharacterType, List<CharacterCtrlBase>> Character_Dict = new Dictionary<InGameCharacterType, List<CharacterCtrlBase>>();

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
                return newsp;
            }

            return null;

            
        } 

        private void Awake()
        {
            base.Awake();

            InitLevelEventManager();
            InitLevelEventQueue();

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
        }
    }
}