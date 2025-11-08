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
using static UnityEditor.Progress;

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
        SpawnRoot,
        TeamMate,
        Block,
    }


    //一个关卡对应一个LevelManager，允许跨场景使用。但是玩家切换关卡时，必须重新初始化LevelManager。如果需要跨大关卡传参，那么往更上层的GameManager缓存里放。
    public class LevelManager : MonoSingleton<LevelManager>
    {

        public Transform LevelObjectsRoot;

        public int currentLevelID;
        public int focusSublLevelID;

        [Header("初始防御塔配置")]
        [Tooltip("是否在游戏开始时自动生成三种防御塔")]
        public bool spawnStartingTowers = true;
        [Tooltip("基础防御塔位置")]
        public Vector3 basicTowerPosition = new Vector3(-3f, 0f, 0f);
        [Tooltip("散射防御塔位置")]
        public Vector3 scatterTowerPosition = new Vector3(0f, 0f, 0f);
        [Tooltip("减速防御塔位置")]
        public Vector3 slowTowerPosition = new Vector3(3f, 0f, 0f);
        
        // 防止重复生成的标志
        private bool hasInitializedTowers = false;

        public List<RoundGameTeamInfo> levelTeams = new List<RoundGameTeamInfo>();

        public int NowWorkTeamI;

        public int NowRoundID = 0;
        public int MaxRoundID = 1;
        public bool isRoundGameStart = false;

        public List<CharacterCtrlBase> RoundNextFlag = new List<CharacterCtrlBase>();   

        public Dictionary<InGameCharacterType, List<CharacterCtrlBase>> Character_Dict = new Dictionary<InGameCharacterType, List<CharacterCtrlBase>>();

        public CharacterCtrlBase MyHero;


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
            Debug.Log("游戏胜利！");
            if (UI_GameState.instance != null)
            {
                UI_GameState.instance.ShowVictory();
            }
        }

        public void LevelLose()
        {
            Debug.Log("游戏失败！");
            if (UI_GameState.instance != null)
            {
                UI_GameState.instance.ShowDefeat();
            }
        }

        public void RegCharacterAsType(CharacterCtrlBase newsp , InGameCharacterType characterType)
        {
            newsp.MyObjectLayer = characterType;
            if (!Character_Dict.ContainsKey(characterType))
            {
                Character_Dict.Add(characterType, new List<CharacterCtrlBase>());
            }
            Character_Dict[characterType].Add(newsp);
        }

        public T_CHar SpawnCharacterByID<T_CHar>(int ID , SkillUseInfo call_by_skill = null) where T_CHar : CharacterCtrlBase
        {
            GameCharacters g_config = GameTableConfig.Instance.Config_GameCharacters.FindFirstLine(x => x.ObjectID == ID);
            
            // 检查配置是否存在
            if (g_config == null)
            {
                Debug.LogError($"❌ 找不到ObjectID={ID}的配置！请运行: Tools → 更新防御塔配置表");
                return null;
            }
            
            string enemy_obj_name = g_config.BindPrefab;
            InGameCharacterType characterType = (InGameCharacterType)Enum.Parse(typeof(InGameCharacterType) , g_config.ObjectType);
            GameObject newobj = Resources.Load<GameObject>("CharacterPrefabs/" + enemy_obj_name);

            if (newobj != null)
            {
                GameObject sp_obj = Instantiate(newobj, LevelManager.Instance.LevelObjectsRoot);
                
                T_CHar newsp = sp_obj.GetComponent<T_CHar>();
                foreach (int buff in g_config.InitModifier)
                {

                    SkillDispatchCenter.Instance.AddModifierToCharacter(newsp, -1, buff);
                }
                
                RegCharacterAsType(newsp , characterType);  
                newsp.MyGameObjectID = ID;
                

                if(characterType is InGameCharacterType.Tower || characterType is InGameCharacterType.Enemy)
                {
                    UI_PlayerHUD.instance.InitCharacterFollowHUD(newsp, characterType);
                }

                int dish_t = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == ID)?.DishID ?? 0;
                newsp.dishID = dish_t;

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
            UIManager.Instance.CreateUIByName<UI_GameState>("UI_GameState");
            UIManager.Instance.CreateUIByName<UI_cooktime>("UI_cooktime");
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
            
            // 显示游戏开始提示
            if (UI_GameState.instance != null)
            {
                UI_GameState.instance.ShowGameStart();
            }
            
            // 注释：场景中已有防御塔，不需要代码生成
            // 如果需要代码生成，取消下面这行注释
            // InitializeStartingTowers();
            
            GoNextRound();
            UI_PlayerHUD.instance.UpdateRecipe();

            //SpawnCharacterByID<PlayerCharacterCtrl>(2001);
        }

        /// <summary>
        /// 在游戏开始时生成三种防御塔各一个
        /// </summary>
        private void InitializeStartingTowers()
        {
            // 防止重复生成
            if (hasInitializedTowers)
            {
                Debug.LogWarning("⚠️ 防御塔已经生成过，跳过重复生成");
                return;
            }
            
            if (!spawnStartingTowers)
            {
                Debug.Log("跳过初始防御塔生成（spawnStartingTowers = false）");
                return;
            }

            // 检查必要的依赖
            if (LevelObjectsRoot == null)
            {
                Debug.LogError("❌ LevelObjectsRoot 未设置，无法生成防御塔！请在Inspector中指定LevelObjectsRoot");
                return;
            }

            if (GameTableConfig.Instance == null || GameTableConfig.Instance.Config_GameCharacters == null)
            {
                Debug.LogError("❌ 配置表未加载，无法生成防御塔！");
                return;
            }

            try
            {
                Debug.Log("开始初始化防御塔...");

                // 生成基础防御塔 (ObjectID = 2)
                var basicTower = SpawnCharacterByID<CharacterCtrlBase>(2);
                if (basicTower != null)
                {
                    basicTower.transform.position = basicTowerPosition;
                    basicTower.gameObject.name = "BasicTower_1";
                    Debug.Log($"✅ 基础防御塔已生成 at {basicTowerPosition}");
                }
                else
                {
                    Debug.LogWarning("⚠️ 基础防御塔生成失败（预制体可能不存在）");
                }

                // 生成散射防御塔 (ObjectID = 10)
                var scatterTower = SpawnCharacterByID<CharacterCtrlBase>(10);
                if (scatterTower != null)
                {
                    scatterTower.transform.position = scatterTowerPosition;
                    scatterTower.gameObject.name = "ScatterTower_1";
                    Debug.Log($"✅ 散射防御塔已生成 at {scatterTowerPosition}");
                }
                else
                {
                    Debug.LogWarning("⚠️ 散射防御塔生成失败（请先运行: Tools → 更新防御塔配置表）");
                }

                // 生成减速防御塔 (ObjectID = 11)
                var slowTower = SpawnCharacterByID<CharacterCtrlBase>(11);
                if (slowTower != null)
                {
                    slowTower.transform.position = slowTowerPosition;
                    slowTower.gameObject.name = "SlowTower_1";
                    Debug.Log($"✅ 减速防御塔已生成 at {slowTowerPosition}");
                }
                else
                {
                    Debug.LogWarning("⚠️ 减速防御塔生成失败（请先运行: Tools → 更新防御塔配置表）");
                }
                
                // 设置标志，防止重复生成
                hasInitializedTowers = true;
                Debug.Log("✅ 防御塔初始化完成，共生成3个防御塔");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 初始化防御塔时发生错误: {e.Message}\n堆栈: {e.StackTrace}");
            }
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