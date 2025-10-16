using Assets.Scripts.AI;
using Assets.Scripts.BaseUtils;
using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;

public class SpawnRootInfo
{
    public int rootID;
    public List<Vector3Int> move_points;
    public Vector3Int start_point;

    public SpawnRoots sp_config;

    public float spawn_timer = 0;

    public SpawnRootInfo(int rootID , List<Vector3Int> move_point , Vector3Int start_point)
    {
        this.rootID = rootID;
        this.move_points = move_point;
        this.start_point = start_point;

        sp_config = GameTableConfig.Instance.Config_SpawnRoots.FindFirstLine(x => x.RootID == rootID);  
    }
}

public class LevelGridGenerator : MonoSingleton<LevelGridGenerator>
{
    int Height, Width;
    public Tilemap tilemap;
    public Dictionary<Vector3Int, LevelGridTileObject> tile_dictionary = new Dictionary<Vector3Int, LevelGridTileObject>();
    public Dictionary<Vector3Int , string> tile_strings = new Dictionary<Vector3Int , string>();
    public Dictionary<int , SpawnRootInfo> spawnroot_dictionay = new Dictionary<int , SpawnRootInfo>(); 
    public List<List<string>> layer_info = new List<List<string>>();
    private void SpawnWall(Vector3Int pos, string go_name, bool build_colid)
    {
        Vector3Int position = pos;

        Vector3 objpos = tilemap.GetCellCenterWorld(position);

        string tile_obj_name = GameTableConfig.Instance.Config_TileBlocks.FindFirstLine(x => x.TileSpriteName == go_name).BlockObject;

        GameObject new_obj = Resources.Load<GameObject>("GameObjectPrefabs/" + tile_obj_name);
        Debug.Log(new_obj);

        GameObject sp_obj = Instantiate(new_obj, LevelManager.Instance.LevelObjectsRoot);
        sp_obj.transform.position = objpos;

        if (tile_dictionary.ContainsKey(position))
        {
            tile_dictionary[position] = sp_obj.GetComponent<LevelGridTileObject>();
        }
        else
        {
            tile_dictionary.Add(position, sp_obj.GetComponent<LevelGridTileObject>());
        }

        if (build_colid)
        {
            TileBase t = Resources.Load<TileBase>("MapConfig/grid_tile");
            tilemap.SetTile(position, t);
        }
    }

    private void SpawnGOAt(Vector3Int pos , string go_name , bool build_colid)
    {
        string[] g_names = go_name.Split('|');
        foreach (var go in g_names)
        {
            if (go.StartsWith("Wall_"))
            {
                SpawnWall(pos, go, build_colid);
            }
            else if (go.StartsWith("SpawnRoot_"))
            {
                BuildSpawnRootByID( int.Parse( go.Split("_")[1] ) , pos);
            }
        }

    }

    private bool getNextPos(int id , Vector3Int cur_pos , out Vector3Int next_pos , List<Vector3Int> have_search)
    {
        
        string t = "";
        Vector3Int test_vt = new Vector3Int(1, 0, 0);
        if (tile_strings.TryGetValue(cur_pos + test_vt, out t) && !have_search.Contains(cur_pos + test_vt) && t.Contains("Road_" + id))
        {
            next_pos = cur_pos + test_vt;
            return true;
        }

        test_vt = new Vector3Int(-1, 0, 0);
        if (tile_strings.TryGetValue(cur_pos + test_vt, out t) && !have_search.Contains(cur_pos + test_vt) && t.Contains("Road_" + id))
        {
            next_pos = cur_pos + test_vt;
            return true;
        }

        test_vt = new Vector3Int(0, 1, 0);
        if (tile_strings.TryGetValue(cur_pos + test_vt, out t) && !have_search.Contains(cur_pos + test_vt) && t.Contains("Road_" + id))
        {
            next_pos = cur_pos + test_vt;
            return true;
        }

        test_vt = new Vector3Int(0, -1, 0);
        if (tile_strings.TryGetValue(cur_pos + test_vt, out t) && !have_search.Contains(cur_pos + test_vt) && t.Contains("Road_" + id))
        {
            next_pos = cur_pos + test_vt;
            return true;
        }

        next_pos = Vector3Int.zero;
        return false;
    }

    private void BuildSpawnRootByID(int id , Vector3Int start_pos)
    {
        Vector3Int curPos = start_pos;
        List<Vector3Int> pos_list = new List<Vector3Int>(); 
        pos_list.Add(curPos);
        Vector3Int next_pos;
        while (getNextPos(id, curPos , out next_pos , pos_list))
        {
            pos_list.Add(next_pos);
            curPos = next_pos;
        }

        SpawnRootInfo sp = new SpawnRootInfo(id, pos_list, start_pos);

        if(!spawnroot_dictionay.TryAdd(id , sp))
        {
            spawnroot_dictionay[id] = sp;
        }

        Debug.Log(sp.move_points.Count);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var spawn_pair in spawnroot_dictionay)
        {
            SpawnRootInfo sp = spawn_pair.Value;
            if (sp.sp_config != null)
            {
                sp.spawn_timer += Time.deltaTime;
                if (sp.spawn_timer > sp.sp_config.SpawnGap)
                {
                    string enemy_obj_name = GameTableConfig.Instance.Config_GameCharacters.FindFirstLine(x => x.ObjectID == sp.sp_config.EnemyID).BindPrefab;
                    GameObject newobj = Resources.Load<GameObject>("CharacterPrefabs/" + enemy_obj_name);
                    Debug.Log("CharacterPrefabs/" + enemy_obj_name);
                    if (newobj != null)
                    {
                        GameObject sp_obj = Instantiate(newobj, LevelManager.Instance.LevelObjectsRoot);
                        sp_obj.transform.position = tilemap.GetCellCenterWorld(sp.start_point);
                        if (sp_obj.GetComponent<AIAgentBase>() != null)
                        {
                            sp_obj.GetComponent<AIAgentBase>().followPath = sp.rootID;
                        }
                    }

                    sp.spawn_timer = 0;
                }
            }
        }
    }

    public void TryAttach(Vector3Int pos, GameObject obj)
    {

        if (tile_dictionary.ContainsKey(pos))
        {
            tile_dictionary[pos].TryAttachObject(obj);
        }
    }

    public List< List<string>> getLayerByFileName(string fileName)
    {
        // 1. 从Resources文件夹加载CSV文件
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);

        // 2. 分割行并初始化二维数组
        string[] lines = csvFile.text.Split('\n');
        List<List<string>> dataList = new List<List<string>>();

        // 3. 处理每一行数据
        for (int i = 0; i < lines.Length; i++)
        {
            // 移除可能的回车符并检查空行
            string line = lines[i].TrimEnd('\r', '\n');
            if (string.IsNullOrEmpty(line)) continue;

            // 分割列
            List<string> columns = line.Split(',').ToList();
            columns.RemoveAt(0);
            dataList.Add(columns);
        }

        dataList.RemoveAt(0);
        return dataList;
    }

    public void LoadLevelByID(int id)
    {
        LevelTileLoad layer = GameTableConfig.Instance.Config_LevelTileLoad.FindFirstLine( x => x.LevelID  == id );

        string csvFileName = layer.TileMapPath;
        layer_info = getLayerByFileName(csvFileName);
        Height = layer_info.Count;
        Width = layer_info[0].Count;

        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                string goName = layer_info[i][j];
                if (goName == null || goName == "")
                {
                    continue;
                }
                if(!tile_strings.TryAdd(new Vector3Int(-Width / 2 + j, -Height / 2 + i), goName))
                {
                    tile_strings[new Vector3Int(-Width / 2 + j, -Height / 2 + i)] = goName;
                }
            }
        }

        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                string goName = layer_info[i][j];
                if (goName == null || goName == "")
                {
                    continue;
                }
                SpawnGOAt(new Vector3Int(-Width / 2 + j, -Height / 2 + i), goName, true);
            }
        }

    }
}
