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

    public class InGameObjects : TableDatabase
    {
        public int ObjectID;
        public string ObjectName;
        public string ObjectType;
        public string BindPrefab;
        public List<int> InitModifier;

    }

    public class GameCharacters : TableDatabase
    {
        public int ObjectID;
        public string ObjectName;
        public string ObjectType;
        public string BindPrefab;
        public List<int> InitModifier;

    }

    public class TileBlocks : TableDatabase
    {
        public string TileSpriteName;
        public string BlockObject;
        public string desc;
    }

    public class LevelTileLoad : TableDatabase
    {
        public int LevelID;
        public int LayerID;
        public string TileMapType;
        public string TileMapPath;
        public string ExtraParam;
    }

    public class SpawnRoots : TableDatabase
    {
        public int RootID;
        public int EnemyID;
        public float SpawnGap;
    }

    public class GameTableConfig : Singleton<GameTableConfig>
    {
<<<<<<< Updated upstream
        public ConfigTable<GameModifiers> Config_GameModifiers = new ConfigTable<GameModifiers>("Configs/GameModifiers");
        public ConfigTable<GameFields> Config_GameFields = new ConfigTable<GameFields>("Configs/GameFields");
        public ConfigTable<GameSkills> Config_GameSkills = new ConfigTable<GameSkills>("Configs/GameSkills");
        public ConfigTable<InGameObjects> Config_GameObjects = new ConfigTable<InGameObjects>("Configs/GameObjects");
        public ConfigTable<TileBlocks> Config_TileBlocks = new ConfigTable<TileBlocks>("Configs/TileBlocks");
        public ConfigTable<LevelTileLoad> Config_LevelTileLoad = new ConfigTable<LevelTileLoad>("Configs/LevelTileLoad");
        public ConfigTable<SpawnRoots> Config_SpawnRoots = new ConfigTable<SpawnRoots>("Configs/SpawnRoots");
        public ConfigTable<GameCharacters> Config_GameCharacters = new ConfigTable<GameCharacters>("Configs/GameCharacters");
=======
        public ConfigTable<GameModifiers> Config_GameModifiers = new ConfigTable<GameModifiers>("Configs/GameModifiers.csv");
        public ConfigTable<GameFields> Config_GameFields = new ConfigTable<GameFields>("Configs/GameFields.csv");
        public ConfigTable<GameSkills> Config_GameSkills = new ConfigTable<GameSkills>("Configs/GameSkills.csv");
        public ConfigTable<TileBlocks> Config_TileBlocks = new ConfigTable<TileBlocks>("Configs/TileBlocks.csv");
        public ConfigTable<LevelTileLoad> Config_LevelTileLoad = new ConfigTable<LevelTileLoad>("Configs/LevelTileLoad.csv");
        public ConfigTable<SpawnRoots> Config_SpawnRoots = new ConfigTable<SpawnRoots>("Configs/SpawnRoots.csv");
        public ConfigTable<GameCharacters> Config_GameCharacters = new ConfigTable<GameCharacters>("Configs/GameCharacters.csv");
>>>>>>> Stashed changes


        public void CallBlank() { }
    }


}