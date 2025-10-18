using UnityEngine;
using System.Collections;

/// <summary>
/// 简单的敌人生成器 - Demo专用
/// 自动在指定位置周围生成敌人
/// </summary>
public class SimpleEnemySpawner : MonoBehaviour
{
    [Header("生成设置")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float waveInterval = 15f;
    
    [Header("生成范围")]
    [SerializeField] private float spawnRadius = 15f;
    [SerializeField] private Vector3 spawnCenter = new Vector3(0, 0, 20);
    
    [Header("敌人设置")]
    [SerializeField] private float enemyMoveSpeed = 3f;
    [SerializeField] private Vector3 targetPosition = Vector3.zero;
    
    private int currentWave = 0;
    private int enemiesSpawned = 0;
    private bool isSpawning = false;
    
    void Start()
    {
        // 如果没有指定敌人Prefab，尝试加载
        if (enemyPrefab == null)
        {
            enemyPrefab = Resources.Load<GameObject>("CharacterPrefabs/CC_Enemy_1");
            if (enemyPrefab == null)
            {
                Debug.LogWarning("⚠ 未找到敌人Prefab，将使用简单cube代替");
            }
        }
        
        StartCoroutine(SpawnWaves());
        Debug.Log("✓ 敌人生成器已激活 - 准备生成第一波");
    }
    
    IEnumerator SpawnWaves()
    {
        while (true)
        {
            currentWave++;
            Debug.Log($"📣 第{currentWave}波开始！准备生成{enemiesPerWave}个敌人");
            
            // 等待准备时间
            if (currentWave > 1)
            {
                Debug.Log($"⏱ 波次间隔{waveInterval}秒...");
                yield return new WaitForSeconds(waveInterval);
            }
            
            // 生成敌人
            yield return StartCoroutine(SpawnWave());
            
            // 等待一小段时间再开始下一波
            yield return new WaitForSeconds(5f);
        }
    }
    
    IEnumerator SpawnWave()
    {
        isSpawning = true;
        enemiesSpawned = 0;
        
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            enemiesSpawned++;
            
            if (i < enemiesPerWave - 1)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }
        
        isSpawning = false;
        Debug.Log($"✓ 第{currentWave}波生成完成！共{enemiesSpawned}个敌人");
    }
    
    void SpawnEnemy()
    {
        // 在圆形范围内随机位置
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = spawnCenter + new Vector3(randomCircle.x, 1f, randomCircle.y);
        
        GameObject enemy;
        
        if (enemyPrefab != null)
        {
            enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            // 创建简单的cube代替
            enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.transform.position = spawnPosition;
            enemy.transform.localScale = Vector3.one * 1.5f;
            enemy.GetComponent<Renderer>().material.color = Color.red;
        }
        
        enemy.name = $"Enemy_{currentWave}_{enemiesSpawned + 1}";
        
        // 添加简单移动
        var mover = enemy.AddComponent<SimpleEnemyMover>();
        mover.moveSpeed = enemyMoveSpeed;
        mover.targetPosition = targetPosition;
        
        // 添加食材掉落（如果有组件）
        var dropper = enemy.GetComponent<Assets.Scripts.Cooking.IngredientDropper>();
        if (dropper == null)
        {
            dropper = enemy.AddComponent<Assets.Scripts.Cooking.IngredientDropper>();
            // 加载掉落表
            var dropTable = Resources.Load<Assets.Scripts.Cooking.IngredientDropTable>("Data/Drops/BasicEnemyDrop");
            if (dropTable != null)
            {
                // 通过反射设置dropTable
                var field = typeof(Assets.Scripts.Cooking.IngredientDropper).GetField("dropTable", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(dropper, dropTable);
                }
            }
        }
        
        Debug.Log($"👾 生成敌人: {enemy.name} 在位置 {spawnPosition}");
    }
    
    void OnDrawGizmos()
    {
        // 绘制生成范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnCenter, spawnRadius);
        
        // 绘制目标位置
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, 2f);
        
        // 绘制从生成点到目标的线
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(spawnCenter, targetPosition);
    }
}

/// <summary>
/// 简单的敌人移动脚本
/// </summary>
public class SimpleEnemyMover : MonoBehaviour
{
    public float moveSpeed = 3f;
    public Vector3 targetPosition = Vector3.zero;
    
    void Update()
    {
        // 朝目标移动
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // 旋转朝向目标
        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // 到达目标后销毁
        if (Vector3.Distance(transform.position, targetPosition) < 2f)
        {
            Debug.Log($"👾 {gameObject.name} 到达目标！");
            Destroy(gameObject);
        }
    }
}

