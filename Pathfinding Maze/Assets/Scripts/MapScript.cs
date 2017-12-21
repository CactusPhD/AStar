using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapGen;
using UnityEngine.SceneManagement;

public class MapScript : MonoBehaviour {

    public int xSize;
    public int ySize;
    public float cullingRate;
    MapTile[,] map;
    public GameObject wall;
    public GameObject floor;
    public GameObject player;
    public GameObject enemy;
    public GameObject wanderer;
    public bool requestGen;

	// Use this for initialization
	void Start () {
        map = generateMap();
	}
	
	// Update is called once per frame
	void Update () {
        if (requestGen)
        {
            regenerateMap();
        }
	}

    void regenerateMap()
    {
        SceneManager.LoadScene("AStarPathing");
    }

    MapTile[,] generateMap()
    {
        //change maze
        //remember to change culling rate
        //PrimGenerator pGen = new PrimGenerator();
        PerlinGenerator pGen = new PerlinGenerator();
  
        MapTile[,] newMap = pGen.MapGen(xSize, ySize, cullingRate);
        //draw outside walls
        for (int i = -1; i < xSize + 1; i++)
        {
            (Instantiate(wall, new Vector3(i * 10, 5, -10), Quaternion.identity) as GameObject).transform.parent = transform; ;
        }
        for (int i = -1; i < xSize + 1; i++)
        {
            (Instantiate(wall, new Vector3(i * 10, 5, ySize * 10), Quaternion.identity) as GameObject).transform.parent = transform; ;
        }
        for (int i = -1; i < ySize + 1; i++)
        {
            (Instantiate(wall, new Vector3(-10, 5, i*10), Quaternion.identity) as GameObject).transform.parent = transform; ;
        }
        for (int i = -1; i < ySize + 1; i++)
        {
            (Instantiate(wall, new Vector3(xSize * 10, 5, i*10), Quaternion.identity) as GameObject).transform.parent = transform;
        }

        //make player
        GameObject playerAI = Instantiate(player, Vector3.zero, Quaternion.identity);
        playerAI.GetComponent<PlayerScript>().map = newMap;
        playerAI.GetComponent<PlayerScript>().xSize = xSize;
        playerAI.GetComponent<PlayerScript>().ySize = ySize;
        playerAI.transform.parent = transform;

        //make enemy
        spawnEnemy(1, newMap, playerAI);
        spawnWanderer(1, newMap, playerAI);

        //draw inside
        for (int i = 0; i < xSize; i++)
        {
            for(int j = 0; j < ySize; j++)
            {
                if (newMap[i, j].Walkable)
                {
                    GameObject newFloor = Instantiate(floor, new Vector3(i * 10, 0, j * 10), Quaternion.identity);
                    newFloor.transform.parent = transform;
                    if (newMap[i, j].IsGoal)
                    {
                        //goal is green
                        newFloor.GetComponent<Renderer>().material.color = Color.green;
                        playerAI.GetComponent<PlayerScript>().goal = new Vector2(i, j);
                    }
                    if (newMap[i, j].IsStart)
                    {
                        //start is red
                        newFloor.GetComponent<Renderer>().material.color = Color.red;
                        playerAI.GetComponent<PlayerScript>().start = new Vector2(i, j);
                    }
                }
                else
                {
                    (Instantiate(wall, new Vector3(i * 10, 5, j * 10), Quaternion.identity) as GameObject).transform.parent = transform;
                }
            }
        }
        requestGen = false;
        return newMap;
    }

    void spawnEnemy(int num, MapTile[,] map, GameObject player)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject enemyAI = Instantiate(enemy, Vector3.zero, Quaternion.identity);
            enemyAI.GetComponent<EnemyScript>().map = map;
            enemyAI.GetComponent<EnemyScript>().xSize = xSize;
            enemyAI.GetComponent<EnemyScript>().ySize = ySize;
            enemyAI.GetComponent<EnemyScript>().player = player.GetComponent<PlayerScript>();
            enemyAI.transform.parent = transform;
            player.GetComponent<PlayerScript>().enemies.Add(enemyAI.GetComponent<EnemyScript>());
        }
    }

    void spawnWanderer(int num, MapTile[,] map, GameObject player)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject enemyAI = Instantiate(wanderer, Vector3.zero, Quaternion.identity);
            enemyAI.GetComponent<WandererScript>().map = map;
            enemyAI.GetComponent<WandererScript>().xSize = xSize;
            enemyAI.GetComponent<WandererScript>().ySize = ySize;
            enemyAI.transform.parent = transform;
            player.GetComponent<PlayerScript>().wanderers.Add(enemyAI.GetComponent<WandererScript>());
        }
    }

}
