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

        public bool RunNode()
        {
            return RunNode(root);
        }

        private bool RunNode(string from_node_key)
        {
            BTNode cur_node = nodes[from_node_key];

            if( cur_node.category == NodeCategory.Sequence)
            {
                foreach(string next_node in cur_node.children)
                {
                    bool node_result = RunNode(next_node);
                    if( node_result )
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
            else if( cur_node.category == NodeCategory.Selector)
            {
                foreach (string next_node in cur_node.children)
                {
                    bool node_result = RunNode(next_node);
                    if (node_result)
                    {
                        return true;
                    }
                    else
                    {
                        continue ;
                    }
                }
                return false ;
            }
            else if( cur_node.category == NodeCategory.Action)
            {
                return bindAgent.DoAction(cur_node);
            }
            else if( cur_node.category == NodeCategory.Condition)
            {
                return bindAgent.DoCondition(cur_node); 
            }
            else
            {
                Debug.LogError("未知的行为树节点" + cur_node.name + cur_node.id);
                return false;
            }
        }

        public BTree(JSONObject rawTree , AIAgentBase bindAgent)
        {
            this.root = rawTree["root"].stringValue;
            this.bindAgent = bindAgent; 
            foreach (var item in rawTree["custom_nodes"]) 
            {
                costumeNodeConfigs.Add(JsonUtility.FromJson<CostumeNodeConfig>((item.ToString()) ));    
            }
            foreach (string item_key in rawTree["nodes"].keys)
            {
                BTNode newItem = JsonUtility.FromJson<BTNode>(rawTree["nodes"][item_key].ToString());

                CostumeNodeConfig thisConfig = costumeNodeConfigs.Find((CostumeNodeConfig x) => x.name == newItem.name);

                if (thisConfig != null) 
                {
                    if(thisConfig.category == "condition")
                    {
                        newItem.category = NodeCategory.Condition;
                    }
                    else if (thisConfig.category == "action")
                    {
                        newItem.category = NodeCategory.Action;
                    }
                }
                else
                {
                    if(newItem.name == "Sequence")
                    {
                        newItem.category = NodeCategory.Sequence;
                    }
                    else if(newItem.name == "Selector")
                    {
                        newItem.category= NodeCategory.Selector;
                    }
                    else
                    {
                        newItem.category = NodeCategory.Unknown;
                    }
                }



                if (nodes.ContainsKey(item_key))
                {
                    this.nodes[item_key] = newItem;
                }
                else
                {
                    this.nodes.Add(item_key, newItem );
                }
            }

        }
    }

    public static class AIUtiles
    {
        public static BTree LoadBTreeFromFile(string filepath , AIAgentBase bindAgent)
        {
            //加载资源
            TextAsset obj = Resources.Load<TextAsset>(filepath);

            BTree result_tree = null;
            if (obj != null)
            {
                //解析
                //BTree m_Data = JsonUtility.FromJson<BTree>(obj.text);
                JSONObject jsonObj = new JSONObject(obj.text);
                Debug.Log(jsonObj.ToString());
                Debug.Log(jsonObj["version"].ToString());

                result_tree = new BTree(jsonObj , bindAgent);
                Debug.Log(result_tree);
            }

            return result_tree;

        }
    }
}
