using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class TheCellGameMgr : MonoBehaviour
{
    // Game States
    public enum GameStates
    {
        Undefined = 0,  // not initialized
        Starting,       // just started a new game with new seeds
        Running,        // in a middle of a game
        Finishing,      // just get killed, must respawn in starting cell, don't reset seeds
    }


    // Cell types
    public enum CellTypes
    {
        Undefined = 0,  // not initialized
        Start,          // first cell
        Exit,           // exit cell (winning)
        Safe,           // empty and safe
        Effect,         // a cell with a non deadly effect
        Deadly,         // a cell with a deadly effect
    }


    // Cell sub types
    public enum CellSubTypes
    {
        Fire = 0,
        Gaz,
        Water,
        Lasers,
        Illusion,
        Blind,
        Screen,
        Vortex,
        OneLook,
        Empty,
        Tunnel,
    }


    // Cardinal direction as North = +Z, East = +X, South = -Z, West = -X
    public enum CardinalPoint
    {
        North,
        East,
        South,
        West
    }


    private int deadlyCellNb = 9;
    private int effectCellNb = 7;


    // Gets the singleton instance.
    public static TheCellGameMgr instance { get; private set; }


    // Gets the singleton instance.
    public static GameStates gameState { get; private set; }

    static int startingSeed = 1966;
    static float startingTime = 0; // Time since the start of a new game in sec
    public OneCellClass cellClassPrefab;
    public List<OneCellClass> allCells; // All the cells as they are distributed
    public List<int> lookupTab = new List<int>(25); // lookup table, hold a map of cell's id
    int playerCellId = 12; // in which place on the chess the player is. Match the lookup table.
    GameObject playerSphere = null; // a sphere to represent where the player is on the board.
    Canvas m_basicCanvas = null;

    public GameObject m_MiddleCell; // The Room
    bool displayCell_N = false;
    GameObject cell_N = null;

    public Light m_light_N;
    public Light m_light_E;
    public Light m_light_S;
    public Light m_light_W;


    void Awake()
    {
        Debug.Log($"[GameMgr] Awake. {gameState}");
        transform.position = new Vector3(-1.0f, 0.0f, 1.2f); // position of the mini game

        // add a sphere to represent the player     
        if (playerSphere == null)
        {
            playerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            playerSphere.transform.position = transform.position + new Vector3(0.0f, 0.1f, 0.0f);
            playerSphere.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
        }

        InitializeNewGame(startingSeed); // for debug purpose we always start with the same seed
        //InitializeNewGame(System.Environment.TickCount);
    }


    // Start is called before the first frame update
    void Start()
    {
        // Grab text from canvas and change it
        //GameObject canvas = GameObject.Find("Canvas");
        m_basicCanvas = GameObject.FindObjectOfType<Canvas>();
        int n = m_basicCanvas.transform.childCount;
        GameObject directions = m_basicCanvas.transform.GetChild(1).gameObject;
        string toto = directions.GetComponent<TextMeshProUGUI>().text;
        Debug.Log($"----------------- {toto}");
        directions.GetComponent<TextMeshProUGUI>().text = $"Bonne journée\n {Time.fixedTime}s";

        // Switch off the light
        m_light_N.range = 0.0f;
        m_light_E.range = 0.0f;
        m_light_S.range = 0.0f;
        m_light_W.range = 0.0f;

        Debug.Log($"[GameMgr] Start. {gameState}");
    }


    // Return the time at which the game started with the current seed in seconds
    public float GetGameStartTime()
    {
        return startingTime;
    }


    // Return the cell the player is in
    OneCellClass GetCurrentCell()
    {
        OneCellClass current = allCells[lookupTab[playerCellId]];
        return current;
    }


    // Start a new fresh game
    void InitializeNewGame(int gameSeed)
    {
        instance = this;
        gameState = GameStates.Starting;

        startingSeed = gameSeed;
        Random.InitState(startingSeed);

        int firstRandom = (int)(Random.value * 100.0f);
        startingTime = Time.fixedTime;
        Debug.Log($"[GameMgr][{startingTime}] new game initialized with seed {startingSeed}, first random {firstRandom}/19.");

        playerCellId = 12; // replace the player in the middle

        int reserveExit = 24; // make sure we'll always have a valid exit
        List<int> deadly = new List<int>(deadlyCellNb);
        while (deadly.Count < deadlyCellNb)
        {
            int id = (int)(Random.value * 24.0f);
            if ((deadly.Contains(id)) || (id == 12) || (id == reserveExit))
            {
                continue;
            }

            deadly.Add(id);
        }
        string toto = "";
        foreach (int id in deadly)
        {
            toto += id.ToString() + " ";
        }
        Debug.Log($"[GameMgr] deadly {toto}");


        List<int> effectCells = new List<int>(effectCellNb);
        while (effectCells.Count < effectCellNb)
        {
            int id = (int)(Random.value * 24.0f);
            if ((id == 12) || deadly.Contains(id) || effectCells.Contains(id) || (id == reserveExit))
            {
                continue;
            }

            effectCells.Add(id);
        }

        toto = "";
        foreach(int id in effectCells)
        {
            toto += id.ToString() + " ";
        }
        Debug.Log($"[GameMgr] effectCells {toto}");


        // Init a board
        bool exitChosen = false;
        int exitCount = 0;

        if (allCells == null)
        {
            allCells = new List<OneCellClass>(25);
        }

        for (int i=0; i < 5; ++i)
        {
            for (int j = 0; j < 5; ++j)
            {
                OneCellClass cell = null;
                int id = i * 5 + j;
                if (allCells.Count == 25)
                {
                    cell = allCells[id];
                    lookupTab[id] = id;
                }
                else
                {
                    cell = Instantiate<OneCellClass>(cellClassPrefab);
                    cell.cellId = id;
                    allCells.Add(cell);
                    lookupTab.Add(id);
                }
                cell.name = "Cell_" + id;
                float z = i;
                float x = j;
                cell.m_MiniGameTranslation = new Vector3(x * 0.1f, 0.0f, z * -0.1f) + transform.position;
                float aRndNb = Random.value;

                if ((i == 2) && (j == 2))
                {
                    cell.InitCell(CellTypes.Start, 0, aRndNb);
                    playerSphere.transform.position = cell.SmallCell.transform.position + new Vector3(0.0f, 0.1f, 0.0f);
                    continue;
                }

                // Choose deadly ones
                if (deadly.Contains(id))
                {
                    int index = deadly.IndexOf(id);
                    cell.InitCell(CellTypes.Deadly, index, aRndNb);
                    continue;
                }

                // Choose effect ones
                if (effectCells.Contains(id))
                {
                    int index = effectCells.IndexOf(id);
                    cell.InitCell(CellTypes.Effect, index, aRndNb);
                    continue;
                }

                // Choose an exit
                if (!exitChosen)
                {
                    if ((i == 0) || (j == 0) || (i == 4) || (j == 4) || ((i != 2) && (j != 2)))
                    {
                        exitCount++;
                        if ((exitCount >= (int)(aRndNb * 20.0f)) || (id == 24))
                        {
                            cell.InitCell(CellTypes.Exit, 0, aRndNb);
                            exitChosen = true;
                            continue;
                        }
                    }
                }

                cell.InitCell(CellTypes.Safe, 0, aRndNb);
            }
        }

        gameState = GameStates.Running;
        OneCellClass current = GetCurrentCell();
        current.OnPlayerEnter();
    }


    // Move the player position on the board to the north +Z
    public void MovePlayerNorth()
    {
        if (playerCellId > 4)
        {
            OneCellClass current = GetCurrentCell();
            current.OnPlayerExit();

            playerCellId -= 5;

            current = GetCurrentCell();
            current.OnPlayerEnter();
        }
    }


    // Move the player position on the board to the south -Z
    public void MovePlayerSouth()
    {
        if (playerCellId < 20)
        {
            OneCellClass current = GetCurrentCell();
            current.OnPlayerExit();

            playerCellId += 5;

            current = GetCurrentCell();
            current.OnPlayerEnter();
        }
    }


    // Move the player position on the board to the east +X
    public void MovePlayerEast()
    {
        if (playerCellId % 5 == 4)
        {
            return;
        }
        OneCellClass current = GetCurrentCell();
        current.OnPlayerExit();

        playerCellId++;

        current = GetCurrentCell();
        current.OnPlayerEnter();
    }


    // Move the player position on the board to the west -X
    public void MovePlayerWest()
    {
        if (playerCellId % 5 == 0)
        {
            return;
        }
        OneCellClass current = GetCurrentCell();
        current.OnPlayerExit();

        playerCellId--;

        current = GetCurrentCell();
        current.OnPlayerEnter();
    }


    // Make sure the player is in the correct cell
    void SetPlayerLookupId(int cellId)
    {
        OneCellClass current = GetCurrentCell();
        int currentCellId = current.cellId;
        if (currentCellId != cellId)
        {
            int i = 0;
            foreach (int id in lookupTab)
            {
                if (id == cellId)
                {
                    playerCellId = i;
                }
                i++;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log($"[GameMgr] not initialized ! {instance}.");
        }

        if (Input.GetKeyUp(KeyCode.Backspace))
        {
            //Cleanup();
            //InitializeNewGame(0);
            InitializeNewGame(System.Environment.TickCount);
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            MovePlayerNorth();
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            MovePlayerSouth();
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            MovePlayerEast();
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            MovePlayerWest();
        }
        if (Input.GetKeyUp(KeyCode.Keypad6))
        {
            TestGetNorth();
            MoveRow(true);
            TestGetNorth();
        }
        if (Input.GetKeyUp(KeyCode.Keypad4))
        {
            TestGetNorth();
            MoveRow(false);
            TestGetNorth();
        }
        if (Input.GetKeyUp(KeyCode.Keypad8))
        {
            MoveColumn(true);
        }
        if (Input.GetKeyUp(KeyCode.Keypad2))
        {
            MoveColumn(false);
        }

        OneCellClass current = GetCurrentCell();
        GameObject directions = m_basicCanvas.transform.GetChild(1).gameObject;
        directions.GetComponent<TextMeshProUGUI>().text = $"Counter\n {Time.fixedTime - current.enterTime}s";

        if (m_light_N.range < 10.0f)
        {
            m_light_N.range += Time.deltaTime * 2.0f;
            m_light_E.range += Time.deltaTime * 2.0f;
            m_light_S.range += Time.deltaTime * 2.0f;
            m_light_W.range += Time.deltaTime * 2.0f;
        }


        if (Input.GetKeyUp(KeyCode.Z))
        {
            if (displayCell_N == false)
            {
                displayCell_N = true;
                //cell_N = GameObject.Find("Cube_00");

                // Open the shutters
                GameObject obj = m_MiddleCell.transform.Find(("wall_pipe")).gameObject;
                obj.transform.Find(("pPlane8")).gameObject.SetActive(false);

                cell_N = GameObject.Instantiate(m_MiddleCell);
                cell_N.transform.position = new Vector3(0.0f, 0.0f, 2.8f);
                obj = cell_N.transform.Find(("wall_pipe")).gameObject;
                obj.transform.Find(("pPlane10")).gameObject.SetActive(false);

                // Launch close sequence
                StartCoroutine(CloseShutters());
            }
        }
    }


    // called once per fixed framerate
    private void FixedUpdate()
    {
        //Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s]");

        // always update the player pos
        OneCellClass current = GetCurrentCell();
        if (current != null)
        {
            playerSphere.transform.position = current.SmallCell.transform.position + new Vector3(0.0f, 0.1f, 0.0f);
        }
    }


    private void OnDestroy()
    {
        Cleanup();
        //Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] DESTROY !!!");
    }


    // Delete all cells and clear the list
    private void Cleanup()
    {
        gameState = GameStates.Finishing;

        for (int i = allCells.Count-1; i >= 0; i--)
        {
            OneCellClass cell = allCells[i];
            Destroy(cell);
        }

        allCells.Clear();
    }


    // Return the cell that is on the north wall
    public OneCellClass GetNorth(int current)
    {
        if (current > 4)
        {
            return allCells[lookupTab[current - 5]];
        }

        return null;
    }


    private void OnDrawGizmos()
    {
        if (gameState != GameStates.Running)
            return;

        OneCellClass current = GetCurrentCell();
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(current.SmallCell.transform.position, current.SmallCell.transform.localScale);
    }


    // Move an entire row to the east or west from player position
    public void MoveRow(bool onEast)
    {
        int from = playerCellId / 5 * 5;
        MoveRow(from, onEast);
    }


    // Move an entire row to the east or west
    void MoveRow(int from, bool onEast)
    {
        if ((from != 0) && (from != 5) && (from != 15) && (from != 20))
        {
            Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] MoveRow should start from the beginning of a row not {from}.");
            return;
        }

        int currentCellId = lookupTab[playerCellId];
        List<int> row = new List<int>(5);
        for (int i=0; i < 5; ++i)
        {
            row.Add(lookupTab[from + i]);
        }

        if (onEast)
        {
            lookupTab[from + 0] = row[4];
            lookupTab[from + 1] = row[0];
            lookupTab[from + 2] = row[1];
            lookupTab[from + 3] = row[2];
            lookupTab[from + 4] = row[3];
        }
        else
        {
            lookupTab[from + 0] = row[1];
            lookupTab[from + 1] = row[2];
            lookupTab[from + 2] = row[3];
            lookupTab[from + 3] = row[4];
            lookupTab[from + 4] = row[0];
        }

        float z = from / 5;
        for (int j = 0; j < 5; ++j)
        {
            float x = j;
            OneCellClass cell = allCells[lookupTab[from + j]];
            cell.m_MiniGameTranslation = new Vector3(x * 0.1f, 0.0f, z * -0.1f) + transform.position;
            cell.SmallCell.transform.position = cell.m_MiniGameTranslation;
        }

        // reposition the player
        SetPlayerLookupId(currentCellId);
    }


    void TestGetNorth()
    {
        string msg = "";
        int id = 10;
        for (int i=0; i < 5; ++i)
        {
            OneCellClass current = GetNorth(id+i);
            if (current != null)
            {
                msg += current.cellId.ToString() + " ";
            }
            else
            {
                msg += " .. ";
            }
        }
        Debug.Log($"[GameMgr] north of 10 -> {msg}");
    }


    // Move an entire column to the north or south from player position
    public void MoveColumn(bool onNorth)
    {
        int from = playerCellId % 5;
        MoveColumn(from, onNorth);
    }


    // Move an entire column to the north or south
    void MoveColumn(int from, bool onNorth)
    {
        if ((from != 0) && (from != 1) && (from != 3) && (from != 4))
        {
            Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] MoveColumn should start from the beginning of a column not {from}.");
            return;
        }

        int currentCellId = lookupTab[playerCellId];
        List<int> column = new List<int>(5);
        for (int i = 0; i < 5; ++i)
        {
            column.Add(lookupTab[from + i * 5]);
        }

        if (onNorth)
        {
            lookupTab[from + 0] = column[1];
            lookupTab[from + 5] = column[2];
            lookupTab[from + 10] = column[3];
            lookupTab[from + 15] = column[4];
            lookupTab[from + 20] = column[0];
        }
        else
        {
            lookupTab[from + 0] = column[4];
            lookupTab[from + 5] = column[0];
            lookupTab[from + 10] = column[1];
            lookupTab[from + 15] = column[2];
            lookupTab[from + 20] = column[3];
        }

        float x = from;
        for (int j = 0; j < 5; ++j)
        {
            float z = j;
            OneCellClass cell = allCells[lookupTab[from + j * 5]];
            cell.m_MiniGameTranslation = new Vector3(x * 0.1f, 0.0f, z * -0.1f) + transform.position;
            cell.SmallCell.transform.position = cell.m_MiniGameTranslation;
        }

        // reposition the player
        SetPlayerLookupId(currentCellId);
    }


    private IEnumerator CloseShutters()
    {
        yield return new WaitForSecondsRealtime(5.0f);

        GameObject obj = m_MiddleCell.transform.Find(("wall_pipe")).gameObject;
        obj.transform.Find(("pPlane8")).gameObject.SetActive(true);
    }
}
