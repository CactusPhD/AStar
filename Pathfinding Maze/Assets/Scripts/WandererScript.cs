using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapGen;


public class WandererScript : MonoBehaviour {

    public MapTile[,] map;
    public Vector2 start;
    public Vector2 current;
    public Vector2 goal;
    public List<Tile> path;
    public int xSize;
    public int ySize;
    State state;

    enum State
    {
        Wander,
        Idle
    }

    // Use this for initialization
    void Start () {
        current = randomTile();
        transform.position = new Vector3(current.x * 10, 3, current.y * 10);
        path = new List<Tile>();
        state = State.Wander;
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("State: " + state);
		 switch (state) {
            case State.Wander:
                moveForward();
                if (path.Count == 0)
                {
                    start = current;
                    goal = randomTile();
                    StartCoroutine("calculateAStar");
                }
                break;

            case State.Idle:
                break;
         }
	}

    void moveForward()
    {
        if (path.Count > 0)
        {
            Vector3 dist = new Vector3(10 * path[0].tile.X, transform.position.y, 10 * path[0].tile.Y) - transform.position;
            if (dist.magnitude < 1f)
            {
                current = new Vector2(path[0].tile.X, path[0].tile.Y);
                transform.position = new Vector3(10 * path[0].tile.X, transform.position.y, 10 * path[0].tile.Y);
                //Debug.Log("removing tile (" + path[0].tile.X + "," + path[0].tile.Y + ")");
                path.Remove(path[0]);
            }
            transform.position += dist.normalized * Time.deltaTime * 20;
        }
    }

    void randDirection()
    {
        Tile currentTile = new Tile();
        currentTile.tile = map[(int)current.x,(int)current.y];
        List<MapTile> neighbors = currentTile.adjacents(map, xSize, ySize);
        //Debug.Log("xSize " + xSize);
        //Debug.Log("ySize " + ySize);
        //Debug.Log("# of neighbors " + neighbors.Count);
        List<MapTile> walkNeighbors = new List<MapTile>();
        for(int i = 0; i < neighbors.Count; i++)
        {
            //Debug.Log("Neighbor at (" + neighbors[i].X + "," + neighbors[i].Y + ")");
            if (neighbors[i].Walkable)
            {
                walkNeighbors.Add(neighbors[i]);
                //Debug.Log("is added");
            }
        }
        //Debug.Log("# walkable of neighbors " + walkNeighbors.Count);
        int randIndex = Random.Range(0, walkNeighbors.Count);
        if (walkNeighbors.Count > 0)
        {
            currentTile.tile = walkNeighbors[randIndex];
            path.Add(currentTile);
        }
        else
        {
            state = State.Idle;
        }
    }

    Vector2 randomTile()
    {
        int rX = Random.Range(0, xSize);
        int rY = Random.Range(0, ySize);
        if (!map[rX, rY].Walkable)
        {
            return randomTile();
        }
        else
        {
            return new Vector2(rX, rY);
        }
    }
 
    IEnumerator calculateAStar()
    {
        //Debug.Log("start a*");
        //init stuff
        List<Tile> toDo = new List<Tile>();
        List<Tile> done = new List<Tile>();
        List<Tile> finalList = new List<Tile>();
        Tile startTile = new Tile();
        startTile.tile = map[(int)start.x, (int)start.y];
        startTile.g = 0;
        startTile.setHF(goal);
        toDo.Add(startTile);

        //algorithm
        bool solved = false;
        while (!solved)
        {
            Tile currentTile = toDo[0];
            //Debug.Log("Processing tile (" + currentTile.tile.X + "," + currentTile.tile.Y + ")");
            foreach (Tile item in toDo)
            {
                if (item.f < currentTile.f)
                {
                    currentTile = item;
                }
            }
            List<MapTile> neighbors = currentTile.adjacents(map, xSize, ySize);
            foreach (MapTile item in neighbors)
            {
                if (item.Walkable)
                {
                    //how to not add a duplicates
                    Tile newTile = new Tile();
                    newTile.tile = item;
                    if (!done.Contains(newTile) && !toDo.Contains(newTile))
                    {
                        //Debug.Log("Adding tile (" + currentTile.tile.X + "," + currentTile.tile.Y + ")");
                        newTile.parent = currentTile;
                        newTile.g = currentTile.g + 1;
                        newTile.setHF(goal);
                        toDo.Add(newTile);
                    }
                }
            }
            toDo.Remove(currentTile);
            done.Add(currentTile);
            if (currentTile.tile.X == goal.x && currentTile.tile.Y == goal.y)
            {
                solved = !solved;
            }
            if (toDo.Count == 0)
            {
                //Debug.Log("path failed");
                state = State.Idle;
                yield break;
            }
            //Debug.Log(toDo.Count + " tiles left");
        }

        //return list
        Tile endTile = done[done.Count - 1];
        solved = false;
        while (!solved)
        {
            finalList.Add(endTile);
            if (endTile.tile.X == start.x && endTile.tile.Y == start.y)
            {
                solved = !solved;
            }
            else
            {
                endTile = endTile.parent;
            }
        }
        finalList.Reverse();
        path = finalList;
        //Debug.Log("Path found");
    }

    //private void OnDrawGizmos()
    //{
    //    foreach (Tile t in path)
    //    {
    //        Gizmos.color = Color.magenta;
    //        Gizmos.DrawCube(new Vector3(t.tile.X * 10, 1, t.tile.Y * 10), new Vector3(1, 1, 1));
    //    }
    //}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<PlayerScript>().state = PlayerState.Regen;
        }
    }
}


