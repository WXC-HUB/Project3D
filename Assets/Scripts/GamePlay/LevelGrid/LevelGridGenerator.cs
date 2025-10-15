using Assets.Scripts.BaseUtils;
using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGridGenerator : MonoSingleton<LevelGridGenerator>
{
    public Tilemap tilemap;
    public Dictionary<Vector3Int, LevelGridTileObject> tile_dictionary = new Dictionary<Vector3Int, LevelGridTileObject>();
    void TraverseWithGetTilesBlock()
    {
        // 获取Tilemap的单元格边界 :cite[3]
        BoundsInt area = tilemap.cellBounds;

        // 批量获取该边界内的所有瓦片 :cite[2]
        TileBase[] allTiles = tilemap.GetTilesBlock(area);

        // 遍历瓦片数组
        for (int i = 0; i < allTiles.Length; i++)
        {
            TileBase tile = allTiles[i];
            if (tile != null)
            {
                // 将索引转换为位置坐标
                int x = i % area.size.x + area.xMin;
                int y = i / area.size.x + area.yMin;
                Vector3Int position = new Vector3Int(x, y, 0);

                Vector3 objpos = tilemap.GetCellCenterWorld(position);

                Debug.Log($"位置 {position} 上有瓦片: {tile.name}");
                string tile_obj_name = GameTableConfig.Instance.Config_TileBlocks.FindFirstLine(x => x.TileSpriteName == tile.name).BlockObject;
                // 在这里处理你的瓦片逻辑
                GameObject new_obj = Resources.Load<GameObject>("GameObjectPrefabs/" + tile_obj_name);

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

            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        TraverseWithGetTilesBlock();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TryAttach(Vector3Int pos, GameObject obj)
    {

        if (tile_dictionary.ContainsKey(pos))
        {
            tile_dictionary[pos].TryAttachObject(obj);
        }
    }
}
