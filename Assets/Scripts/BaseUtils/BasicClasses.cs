using System.Collections;
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;



namespace Assets.Scripts.BaseUtils
{
    public static class PhysicUtils
    {
        public static Vector3 getProgressInVector3List(List<Vector3> points , float progress)
        {
            Vector3 result = points[0];
            if(points.Count == 1)
            {
                return result;
            }
            float all_leng = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                all_leng += (points[i + 1] - points[i]).magnitude;
            }

            float cur_leng = 0.0000001f;
            for (int i = 0; i < points.Count - 1; i++)
            {
                cur_leng += (points[i + 1] - points[i]).magnitude;
                if((points[i + 1] - points[i]).magnitude <= 0.00001f)
                {
                    continue;
                }
                if(cur_leng >= all_leng * progress)
                {
                    float p_end = cur_leng / all_leng;
                    float p_st = (cur_leng - (points[i + 1] - points[i]).magnitude ) / all_leng;
                    result = points[i] + (points[i + 1] - points[i]) * ((progress - p_st) / (p_end - p_st));

                    break;
                }
            }
            return result;
        }
        public static Vector3 getNewPositionAfterCircleHit2D(Rigidbody2D MoveRB, RaycastHit2D hitInfo) 
        {
            if(MoveRB.GetComponent<CircleCollider2D>() == null)
            {
                Debug.LogError("没有找到合适的Circle碰撞体: " + MoveRB.gameObject.name); 
                return Vector3.zero;
            }

            return getNewPositionAfterCircleHit2D(MoveRB.GetComponent<CircleCollider2D>().radius, hitInfo);
        
        }

        public static Vector3 getNewPositionAfterCircleHit2D(   float rad , RaycastHit2D hitInfo )
        {
            Vector3 start_pos;
            start_pos = hitInfo.point + hitInfo.normal * rad;

            return start_pos;
        }




        public static Vector2 getNewMoveSpeedAfterCircleHit2D( Rigidbody2D MoveRB , RaycastHit2D hitInfo )
        {
            return getNewMoveSpeedAfterCircleHit2D(MoveRB.velocity , hitInfo );
        }

        public static Vector2 getNewMoveSpeedAfterCircleHit2D(Vector2 MoveSpeed, RaycastHit2D hitInfo)
        {
            Vector2 rot_input = MoveSpeed;
            rot_input = Vector2.Reflect(rot_input, hitInfo.normal);

            return rot_input * 0.1f;
        }

        public static Vector2 getMoveSpeedAfterHit( float mass_go , Vector2 v_go , float mass_be , Vector2 v_be , RaycastHit2D hitInfo)
        {
            Vector2 diff = v_be - v_go;
            Debug.Log(diff);
            Vector2 flect_dir = Vector2.Reflect(diff, hitInfo.normal).normalized;
            Debug.Log(flect_dir);
            Debug.Log(string.Format("go:{0} be:{1}", mass_go, mass_be));
            Debug.Log(v_go);
            return v_go + (mass_be / (mass_go + mass_be)) * diff;
        }
    }

    public static class GameUtils
    {
        public static void DrawCircle(Vector2 center , float radius, Color color)
        {
            Debug.DrawLine(center, center + radius * Vector2.left, color, 10000f);
            Debug.DrawLine(center, center + radius * Vector2.right, color, 10000f);
            Debug.DrawLine(center, center + radius * Vector2.up, color, 10000f);
            Debug.DrawLine(center, center + radius * Vector2.down, color, 10000f);
        }
        public static void getAllChilds(GameObject gameObject, ref List<GameObject> aa)
        {
            foreach (Transform child in gameObject.transform)
            {
                aa.Add(child.gameObject);
                getAllChilds(child.gameObject, ref aa);
            }
        }

        public static void getAllChilds<T>(GameObject gameObject, ref List<T> aa) where T : MonoBehaviour
        {
            foreach (Transform child in gameObject.transform)
            {
                //aa.Add(child.gameObject);
                T tx = child.GetComponent<T>();   
                if(tx != null) aa.Add(tx);
                getAllChilds(child.gameObject, ref aa);
            }
        }

        public static Transform FindChildInTransform(Transform parent, string child)
        {
            List<GameObject> s = new List<GameObject>();  
            getAllChilds(parent.gameObject, ref s);
            foreach (var item in s)
            {
                if(item.transform.name == child)
                {
                    return item.transform;
                }
            }

            return null;    
        }

    }

    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        public static T Instance { get; private set; }

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = (T)this;
                // DontDestroyOnLoad 只能用于根对象
                // 如果当前对象不是根对象，则应用到根对象
                if (transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }
                else
                {
                    DontDestroyOnLoad(transform.root.gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }


    public class Singleton<T> where T : class
    {
        private static T _Instance;
        private static readonly object padlock = new object();
        public static T Instance
        {
            get
            {
                if (null == _Instance)
                {
                    lock (padlock)
                    {
                        if (null == _Instance)
                        {
                            _Instance = Activator.CreateInstance(typeof(T), true) as T;
                        }
                    }
                }
                return _Instance;
            }
        }

    }

    public static class Pathfinding
    {
        // 移动方向：上、右、下、左
        private static readonly Vector3Int[] directions = new Vector3Int[]
        {
        new Vector3Int(0, 1, 0),   // 上
        new Vector3Int(1, 0, 0),   // 右
        new Vector3Int(0, -1, 0),  // 下
        new Vector3Int(-1, 0, 0)   // 左
        };

        /// <summary>
        /// 获取接近另一个角色的路径
        /// </summary>
        /// <param name="source">来源物体</param>
        /// <param name="target">目标物体</param>
        /// <param name="obstacleTilemap">障碍物Tilemap</param>
        /// <returns>路径点列表，如果没有路径则返回空列表</returns>
        public static List<Vector3Int> GetClosetoCharacter(GameObject source, GameObject target, Tilemap obstacleTilemap)
        {
            if (source == null || target == null) return new List<Vector3Int>();

            // 获取来源物体和目标物体的网格位置
            Vector3Int startPos = GetGridPosition(source.transform.position, obstacleTilemap);
            Vector3Int targetPos = GetGridPosition(target.transform.position, obstacleTilemap);

            // 如果已经在目标位置
            if (startPos == targetPos)
                return new List<Vector3Int> { startPos };

            // 使用A*算法寻找路径
            return FindPathAStar(startPos, targetPos, obstacleTilemap);
        }

        /// <summary>
        /// 使用A*算法寻路
        /// </summary>
        private static List<Vector3Int> FindPathAStar(Vector3Int start, Vector3Int target, Tilemap obstacleTilemap)
        {
            // 如果目标点不可行走，尝试寻找最近的可行走点
            if (!IsWalkable(target, obstacleTilemap))
            {
                Vector3Int closestWalkable = FindClosestWalkablePosition(target, obstacleTilemap);
                if (closestWalkable == start)
                    return new List<Vector3Int> { start }; // 如果最近的可行走点就是起点，直接返回

                target = closestWalkable;
            }

            // 开放列表和关闭列表
            Dictionary<Vector3Int, Node> openSet = new Dictionary<Vector3Int, Node>();
            Dictionary<Vector3Int, Node> closedSet = new Dictionary<Vector3Int, Node>();

            // 创建起始节点
            Node startNode = new Node(start, null, 0, GetHeuristic(start, target));
            openSet[start] = startNode;

            while (openSet.Count > 0)
            {
                // 获取F值最小的节点
                Node currentNode = GetLowestFNode(openSet);

                // 如果到达目标，重构路径
                if (currentNode.position == target)
                {
                    return ReconstructPath(currentNode);
                }

                // 将当前节点从开放列表移到关闭列表
                openSet.Remove(currentNode.position);
                closedSet[currentNode.position] = currentNode;

                // 检查所有相邻节点
                foreach (Vector3Int direction in directions)
                {
                    Vector3Int neighborPos = currentNode.position + direction;

                    // 跳过不可行走的节点
                    if (!IsWalkable(neighborPos, obstacleTilemap))
                        continue;

                    // 跳过已在关闭列表中的节点
                    if (closedSet.ContainsKey(neighborPos))
                        continue;

                    // 计算新的G值
                    int newG = currentNode.gCost + 1;

                    // 检查邻居节点是否在开放列表中
                    if (openSet.TryGetValue(neighborPos, out Node neighborNode))
                    {
                        // 如果找到更短的路径，更新节点
                        if (newG < neighborNode.gCost)
                        {
                            neighborNode.gCost = newG;
                            neighborNode.parent = currentNode;
                        }
                    }
                    else
                    {
                        // 创建新节点并加入开放列表
                        int hCost = GetHeuristic(neighborPos, target);
                        Node newNode = new Node(neighborPos, currentNode, newG, hCost);
                        openSet[neighborPos] = newNode;
                    }
                }
            }

            // 没有找到路径
            return new List<Vector3Int>();
        }

        /// <summary>
        /// 寻找最近的可行走位置
        /// </summary>
        private static Vector3Int FindClosestWalkablePosition(Vector3Int target, Tilemap obstacleTilemap)
        {
            // 使用广度优先搜索寻找最近的可行走位置
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

            queue.Enqueue(target);
            visited.Add(target);

            while (queue.Count > 0)
            {
                Vector3Int current = queue.Dequeue();

                if (IsWalkable(current, obstacleTilemap))
                    return current;

                // 检查所有相邻节点
                foreach (Vector3Int direction in directions)
                {
                    Vector3Int neighbor = current + direction;

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // 如果没有找到可行走位置，返回目标位置（作为后备方案）
            return target;
        }

        /// <summary>
        /// 获取启发式距离（曼哈顿距离）
        /// </summary>
        private static int GetHeuristic(Vector3Int a, Vector3Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// 获取F值最小的节点
        /// </summary>
        private static Node GetLowestFNode(Dictionary<Vector3Int, Node> openSet)
        {
            Node lowestNode = null;
            foreach (var node in openSet.Values)
            {
                if (lowestNode == null || node.FCost < lowestNode.FCost)
                    lowestNode = node;
            }
            return lowestNode;
        }

        /// <summary>
        /// 重构路径
        /// </summary>
        private static List<Vector3Int> ReconstructPath(Node endNode)
        {
            List<Vector3Int> path = new List<Vector3Int>();
            Node currentNode = endNode;

            while (currentNode != null)
            {
                path.Add(currentNode.position);
                currentNode = currentNode.parent;
            }

            path.Reverse(); // 反转路径，从起点到终点
            return path;
        }

        /// <summary>
        /// 检查网格位置是否可行走
        /// </summary>
        private static bool IsWalkable(Vector3Int gridPosition, Tilemap obstacleTilemap)
        {
            // 如果有障碍物Tilemap，检查该位置是否有障碍物
            if (obstacleTilemap != null)
            {
                TileBase tile = obstacleTilemap.GetTile(gridPosition);
                // 如果有障碍物瓦片，不可行走；否则可行走
                return tile == null;
            }

            // 如果没有障碍物Tilemap，所有位置都可行走
            return true;
        }

        /// <summary>
        /// 将世界坐标转换为网格坐标
        /// </summary>
        private static Vector3Int GetGridPosition(Vector3 worldPosition, Tilemap obstacleTilemap)
        {
            if (obstacleTilemap != null)
                return obstacleTilemap.WorldToCell(worldPosition);
            else
                return Vector3Int.FloorToInt(worldPosition);
        }

        /// <summary>
        /// A*算法节点类
        /// </summary>
        private class Node
        {
            public Vector3Int position;
            public Node parent;
            public int gCost; // 从起点到当前节点的成本
            public int hCost; // 从当前节点到终点的启发式成本
            public int FCost => gCost + hCost; // 总成本

            public Node(Vector3Int position, Node parent, int gCost, int hCost)
            {
                this.position = position;
                this.parent = parent;
                this.gCost = gCost;
                this.hCost = hCost;
            }
        }
    }


}