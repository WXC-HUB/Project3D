using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.BaseUtils;

namespace Assets.Scripts.Core
{



    public class GameModifiers : TableDatabase
    {
        public int ModifierID;
        public int ModifierSubID;
        public string ActType;
        public List<string> ModifierParam;
        public string desc;
    }

    public class GameFields : TableDatabase
    {
        public int FieldID;
        public string FieldPrefabName;
        public string desc;
    }

    public class GameSkills : TableDatabase
    {
        public int SkillID;
        public string SkillName;
        public bool CastImmediate;
        public bool ShowInVirtualInput;
        public int ModifierID;
        public float ModifierDuration;
        public string SkillColor; 
        public string desc;
    }

    public class GameObjects : TableDatabase
    {
        public int ObjectID;
        public string ObjectName;
        public int ObjectType;
        public string BindPrefab;
        public List<int> InitModifier;

    }

    public class TileBlocks : TableDatabase
    {
        public string TileSpriteName;
        public string BlockObject;
        public string desc;
    }

    public class GameTableConfig : Singleton<GameTableConfig>
    {
        public ConfigTable<GameModifiers> Config_GameModifiers = new ConfigTable<GameModifiers>("Configs/GameModifiers.csv");
        public ConfigTable<GameFields> Config_GameFields = new ConfigTable<GameFields>("Configs/GameFields.csv");
        public ConfigTable<GameSkills> Config_GameSkills = new ConfigTable<GameSkills>("Configs/GameSkills.csv");
        public ConfigTable<GameObjects> Config_GameObjects = new ConfigTable<GameObjects>("Configs/GameObjects.csv");
        public ConfigTable<TileBlocks> Config_TileBlocks = new ConfigTable<TileBlocks>("Configs/TileBlocks.csv");
        public void CallBlank() { }
    }


}