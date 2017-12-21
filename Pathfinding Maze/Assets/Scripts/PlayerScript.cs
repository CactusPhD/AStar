using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapGen;

public class Tile : System.IEquatable<Tile>
{
    public MapTile tile;
    public Tile parent;
    //to start
    public float g;
    //to goal
    public float h;
    public float f;
    

    public void setHF(Vector2 goal)
    {
        h = Mathf.Abs(tile.X - goal.x) + Mathf.Abs(tile.Y - goal.y);
        f = g + h;
    }

    public List<MapTile> adjacents(MapTile[,] map, int xLength, int yLength)
    {
        List<MapTile> neighbors = new List<MapTile>();
        if (tile.Y < yLength - 1)
            neighbors.Add(map[tile.X, tile.Y + 1]);
        if (tile.X < xLength -1)
            neighbors.Add(map[tile.X + 1, tile.Y]);
        if (tile.Y > 0)
            neighbors.Add(map[tile.X, tile.Y - 1]);
        if (tile.X > 0)
            neighbors.Add(map[tile.X - 1, tile.Y]);
        return neighbors;
    }

    public bool Equals(Tile other)
    {
        if(this.tile.X == other.tile.X && this.tile.Y == other.tile.Y)
        {
            return true;
        }
        return false;
    }
};

public enum PlayerState
{
    Idle,
    Moving,
    Regen,
    Avoidance
};

public class PlayerScript : MonoBehaviour {

    public MapTile[,] map;
    public Vector2 start;
    public Vector2 current;
    public Vector2 goal;
    List<Tile> path;
    public List<EnemyScript> enemies;
    public List<WandererScript> wanderers;
    public int xSize;
    public int ySize;
    public PlayerState state;

    // Use this for initialization
    void Start () {
        current = start;
        transform.position = new Vector3(current.x*10, 3, current.y*10);
        //a* here
        path = new List<Tile>();
        StartCoroutine("calculateAStar");
        state = PlayerState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("State: " + state);
        switch (state) {
            case PlayerState.Idle:
                break;

            //move the player
            case PlayerState.Moving:
                if (path.Count > 0)
                {
                    //lookForEnemies();
                    Vector3 dist = new Vector3(10 * path[0].tile.X, transform.position.y, 10 * path[0].tile.Y) - transform.position;
                    if (dist.magnitude < 1.0f)
                    {
                        current = new Vector2(path[0].tile.X, path[0].tile.Y);
                        transform.position = new Vector3(10 * path[0].tile.X, transform.position.y, 10 * path[0].tile.Y);
                        //Debug.Log("removing tile (" + path[0].tile.X + "," + path[0].tile.Y + ")");
                        path.Remove(path[0]);
                    }
                    transform.position += dist.normalized * Time.deltaTime * 20;
                }
                if (current == goal)
                {
                    state = PlayerState.Regen;
                }
                break;

            //case PlayerState.Avoidance:
            //    if (path.Count > 0)
            //    {
            //        if (path[0].tile.IsGoal)
            //        {
            //            state = PlayerState.Regen;
            //        }
            //        Vector3 dist = new Vector3(10 * path[0].tile.X, transform.position.y, 10 * path[0].tile.Y) - transform.position;
            //        if (dist.magnitude < 1.0f)
            //        {
            //            current = new Vector2(path[0].tile.X, path[0].tile.Y);
            //            transform.position = new Vector3(10 * path[0].tile.X, transform.position.y, 10 * path[0].tile.Y);
            //            //Debug.Log("removing tile (" + path[0].tile.X + "," + path[0].tile.Y + ")");
            //            path.Remove(path[0]);
            //        }
            //        transform.position += dist.normalized * Time.deltaTime * 20;
            //    }
            //    else
            //    {
            //        lookForEnemies();
            //    }
            //    if (current == goal)
            //    {
            //        state = PlayerState.Regen;
            //    }
            //    break;

            //make new map
            case PlayerState.Regen:
                GameObject.Find("Map").GetComponent<MapScript>().requestGen = true;
                state = PlayerState.Idle;
                break;
        }

        if(Input.GetKeyDown(KeyCode.Escape)){
            Application.Quit();
        }

        if(Input.GetKeyDown(KeyCode.R)){
            state = PlayerState.Regen;
        }
	}

    void lookForEnemies()
    {
        bool seen = false;
        foreach (EnemyScript enemy in enemies)
        {
            if (Mathf.Abs(current.x - enemy.current.x) + Mathf.Abs(current.y - enemy.current.y) < 10)
            {
                seen = true;
                
            }
        }
        foreach (WandererScript enemy in wanderers)
        {
            if (Mathf.Abs(current.x - enemy.current.x) + Mathf.Abs(current.y - enemy.current.y) < 10)
            {
                seen = true;
            }
        }
        if (seen)
        {
            StartCoroutine("aStarAvoidance");
        }
        else
        {
            StartCoroutine("calculateAStar");
        }
    }

    IEnumerator calculateAStar()
    {
        yield return 0;
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
            //Debug.Log("Processing tile (" +currentTile.tile.X + "," + currentTile.tile.Y + ")");
            //find smallest f
            foreach (Tile item in toDo)
            {
                if (item.f < currentTile.f)
                {
                    currentTile = item;
                }
            }
            //add walkalbe neighbors to todo
            List<MapTile> neighbors = currentTile.adjacents(map, xSize, ySize);
            foreach (MapTile item in neighbors)
            {
                if (item.Walkable)
                {
                    //how to not add duplicates
                    Tile newTile = new Tile();
                    newTile.tile = item;
                    if (!done.Contains(newTile) && !toDo.Contains(newTile)) {
                        newTile.parent = currentTile;
                        newTile.g = currentTile.g + 1;
                        newTile.setHF(goal);
                        toDo.Add(newTile);
                    }
                }
            }
            //move from todo to done
            toDo.Remove(currentTile);
            done.Add(currentTile);
            //check for solution
            if (currentTile.tile.IsGoal)
            {
                solved = !solved;
            }
            //check for exhaustion
            if (toDo.Count == 0)
            {
                //Debug.Log("path failed");
                state = PlayerState.Regen;
                yield break;
            }
            //Debug.Log(toDo.Count + " tiles left");
            //yield return 0;
        }        

        //return list
        Tile endTile = done[done.Count - 1];
        solved = false;
        while (!solved)
        {
            finalList.Add(endTile);
            if (!endTile.tile.IsStart)
            {
                endTile = endTile.parent;
            }
            else
            {
                solved = !solved;
            }
        }
        finalList.Reverse();
        path = finalList;
        //Debug.Log("found path");
        state = PlayerState.Moving;
    }

    IEnumerator aStarAvoidance()
    {
        yield return 0;
        Debug.Log("start a* with avioidance");
        //init stuff
        List<Tile> toDo = new List<Tile>();
        List<Tile> done = new List<Tile>();
        List<Tile> finalList = new List<Tile>();
        Tile startTile = new Tile();
        startTile.tile = map[(int)current.x, (int)current.y];
        startTile.g = 0;
        startTile.setHF(goal);
        toDo.Add(startTile);

        //algorithm
        bool solved = false;
        while (!solved)
        {
            Tile currentTile = toDo[0];
            Debug.Log("Processing tile (" +currentTile.tile.X + "," + currentTile.tile.Y + ")");
            //find smallest f
            foreach (Tile item in toDo)
            {
                if (item.f < currentTile.f)
                {
                    currentTile = item;
                }
            }
            //add walkalbe neighbors to todo
            List<MapTile> neighbors = currentTile.adjacents(map, xSize, ySize);
            foreach (MapTile item in neighbors)
            {
                bool occpied = false;
                foreach(EnemyScript enemy in enemies)
                {
                    if (enemy.path.Contains(currentTile) && enemy.path.IndexOf(currentTile) <= 2)
                    {
                        occpied = true;
                    }
                }
                foreach (WandererScript enemy in wanderers)
                {
                    if (enemy.path.Contains(currentTile) && enemy.path.IndexOf(currentTile) <=2)
                    {
                        occpied = true;
                    }
                }
                if (item.Walkable && !occpied)
                {
                    //how to not add duplicates
                    Tile newTile = new Tile();
                    newTile.tile = item;
                    if (!done.Contains(newTile) && !toDo.Contains(newTile))
                    {
                        newTile.parent = currentTile;
                        newTile.g = currentTile.g + 1;
                        newTile.setHF(goal);
                        toDo.Add(newTile);
                    }
                }
            }
            //move from todo to done
            toDo.Remove(currentTile);
            done.Add(currentTile);
            //check for solution
            if (currentTile.tile.IsGoal)
            {
                solved = !solved;
            }
            //check for exhaustion
            if (toDo.Count == 0)
            {
                Debug.Log("path failed");
                path = new List<Tile>();
                state = PlayerState.Avoidance;
                yield break;
            }
            //Debug.Log(toDo.Count + " tiles left");
            //yield return 0;
        }

        //return list
        Tile endTile = done[done.Count - 1];
        solved = false;
        while (!solved)
        {
            finalList.Add(endTile);
            if (!endTile.tile.IsStart)
            {
                endTile = endTile.parent;
            }
            else
            {
                solved = !solved;
            }
        }
        finalList.Reverse();
        path = finalList;
        Debug.Log("found path");
        state = PlayerState.Avoidance;
    }

    private void OnDrawGizmos()
    {
        foreach (Tile t in path)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(new Vector3(t.tile.X * 10, 1, t.tile.Y * 10), new Vector3(1, 1, 1));
        }
    }

}
