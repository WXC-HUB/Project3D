using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using UnityEngine;
using Defective.JSON;

namespace Assets.Scripts.AI
{
    public enum NodeCategory
    {
        Unknown,
        Condition,
        Action,
        Sequence,
        Selector
    }

    

    [System.Serializable]
    public class CostumeNodeConfig
    {
        public string name;
        public string category;
    }
    
    [System.Serializable]
    public class BTNodeParams
    {
        public string NodeType;
        public string pName;
        public string pValue;
        public string getType;
        public string toDic;
        public string target;
    }

    [System.Serializable]
    public class BTNode
    {
        public string id;
        public string name;
        public string title;
        public BTNodeParams properties;
        public List<string> children;
        public NodeCategory category;

    }
    [System.Serializable]
    public class BTree
    {
        //public List<BTNode> nodes;
        public Dictionary<string, BTNode> nodes = new Dictionary<string, BTNode>();
        public string root;
        public List<CostumeNodeConfig> costumeNodeConfigs = new List<CostumeNodeConfig>();

        public AIAgentBase bindAgent;

    }

    public static class AIUtiles
    {
        
    }
}
