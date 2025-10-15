using Assets.Scripts.BaseUtils;
using UnityEngine;

namespace Assets.Scripts.Core
{
    //和整个游戏相关的东西从这里走，只允许GameManager处理Level逻辑
    public class GameManager : MonoSingleton<GameManager>
    {


        private void Awake()
        {
            base.Awake();
            UI.UIManager.Instance.InitUIManager();

            //调用一个空函数，确保表格管理器加载出来
            GameTableConfig.Instance.CallBlank();
        }

        // Use this for initialization
        void Start()
        {
            //LevelManager.Instance.LoadLevelConfigFromTable(1, 1);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}