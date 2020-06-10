using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class TheCellGameMgr : MonoBehaviour
{
    // Game language
    public enum GameLanguages
    {
        Undefined = 0,
        English,
        French,
    }


    // Game States
    public enum GameStates
    {
        Undefined = 0,  // not initialized
        Localization,   // In the localization phase
        Starting,       // just started a new game with new seeds
        Running,        // in a middle of a game
        Finishing,      // just get killed, must respawn in starting cell, don't reset seeds
        ExitFound,      // Just found the exit
        CodeAllSet,     // All elements have been placed
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


    // Our 4 Elements in the game
    public enum Elements
    {
        Earth,
        Water,
        Air,
        Fire
    }
    // 3 sequences of code depending on the danger level
    public enum CodeLevels
    {
        Safe,
        Danger,
        Deadly
    }
    Elements[] m_CodeSafe = new Elements[4];
    Elements[] m_CodeDanger = new Elements[4];
    Elements[] m_CodeDeath = new Elements[4];
    Material[] m_ElementsMats = new Material[4];
    [ViewOnly] public uint m_CodeFinalSet = 0; //bitfield specifying wich receiver has been feeded


    public struct MyHands
    {
        public OVRHand.Hand handType;
        public OVRHand hand;
    }


    private int deadlyCellNb = 9;
    private int effectCellNb = 7;


    // Gets the singleton instance.
    public static TheCellGameMgr instance { get; private set; }


    // Current Game state
    public static GameStates gameState { get; private set; }


    // Current Game language
    [ViewOnly] public static GameLanguages m_Language = GameLanguages.Undefined;
    public Canvas m_AllNotes = null;
    public Dictionary<string, string> m_LocalizedText;


    static int startingSeed = 1966;
    static float startingTime = 0.0f; // Time since the start of a new game in sec
    float m_EndGameTime = 0.0f; // Time to compute score when the player end the game in sec
    public OneCellClass cellClassPrefab;
    public List<OneCellClass> allCells; // All the cells as they are distributed
    public List<int> lookupTab = new List<int>(25); // lookup table, hold a map of cell's id
    [ViewOnly] public int playerCellId = 12; // in which place on the chess the player is. Match the lookup table.
    GameObject playerSphere = null; // a sphere to represent where the player is on the board.
    [ViewOnly] public Canvas m_basicCanvas = null;

    private MyHands[] m_hands = new MyHands[2];

    public GameObject m_MiddleCell; // The starting Room
    public GameObject m_GenCellA;   // Generic Room A
    public GameObject m_ExitModel;  // The Exit Room
    public GameObject m_BlindCellModel;  // The blind cell model type
    public GameObject m_IllusionCellModel;
    public GameObject m_WaterCellModel;
    public GameObject m_LaserCellModel;
    public GameObject m_GazCellModel;
    public GameObject m_PlanCellModel; // The screen cell model
    bool m_displayCell_N = false;
    bool m_displayCell_E = false;
    bool m_displayCell_S = false;
    bool m_displayCell_W = false;

    public GameObject m_codes;  // The 4 codes on each wall
    public GameObject m_StopHandScaner; // Object showing no hands scanner availability.
    public LocalizationMenu m_LocMenu;

    public CellsModels m_CentreModels;
    public CellsModels m_NorthModels;
    public CellsModels m_EastModels;
    public CellsModels m_SouthModels;
    public CellsModels m_WestModels;

    public GameObject Snd_OpenShutters;
    private AudioSource Audio_OpenShutters;

    public GameObject Snd_UseLevers;
    [HideInInspector] public AudioSource Audio_UseLevers;

    public GameObject Snd_DeathScream;
    //0 Death scream
    //1 Cannot move row or column
    //2 On Player exit room
    //3 Element placement
    //4 Hand scanner fail
    //5 Open hatch
    [HideInInspector] public AudioSource[] Audio_DeathScream;


    //--- Grabbables ---
    GameObject m_GroupElements;
    GameObject m_ElementPrefab;
    private List<GameObject> m_ElemCubes = new List<GameObject>();
    public int m_ElemCubeNb = 16;
    //--- Grabbables ---


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

            Shader sh = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (sh)
            {
                Renderer rend = playerSphere.GetComponent<Renderer>();
                rend.material = new Material(sh);
                /*
                int n = rend.material.shader.GetPropertyCount();
                for (int i=0; i < n; ++i)
                {
                    Debug.Log($"--- {i} -> {rend.material.HasProperty(i)}, {rend.material.shader.GetPropertyName(i)}");
                }
                rend.material.SetColor("_BaseColor", Color.blue);
                rend.material.SetColor("_BaseMap", Color.blue);
                rend.material.SetColor("_SpecColor", Color.blue);
                */
            }
        }

        LoadElementsMats();

        // --- Elements init ---
        m_GroupElements = GameObject.Find("GroupElements").gameObject;
        m_ElementPrefab = GameObject.Find("GroupElements/element_exit_B").gameObject;
        m_ElementPrefab.SetActive(false);

        for (int i = 0; i < m_ElemCubeNb; ++i)
        {
            //GameObject obj = GameObject.Instantiate(m_ElementPrefab);
            GameObject obj = GameObject.Instantiate(m_ElementPrefab, m_GroupElements.transform);
            obj.name = $"Elem_{i}";
            Vector3 pos = new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f), 0.0f, UnityEngine.Random.Range(-1.0f, 1.0f));
            pos.y = UnityEngine.Random.Range(1.0f, 2.0f);
            obj.transform.SetPositionAndRotation(pos, Quaternion.Euler(pos * 360.0f));
            obj.SetActive(true);
            m_ElemCubes.Add(obj);
        }
        m_GroupElements.SetActive(false); // Need to activate it in exit room

        m_AllNotes = GameObject.Find("AllNotes/LocText").gameObject.GetComponent<Canvas>();
        LoadLocalizedText("localized_fr.json");
        GameObject intro = m_AllNotes.transform.GetChild(0).gameObject;
        intro.GetComponent<TextMeshProUGUI>().text = m_LocalizedText["entry_room_1"];

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
        directions.GetComponent<TextMeshProUGUI>().text = $"Bonne journée\n {Time.fixedTime}s";

        // Init hands
        m_hands[0].handType = OVRHand.Hand.HandLeft;
        m_hands[0].hand = GameObject.Find("OVRCameraRig/TrackingSpace/LeftHandAnchor/OVRHandPrefab").GetComponent<OVRHand>();
        m_hands[1].handType = OVRHand.Hand.HandRight;
        m_hands[1].hand = GameObject.Find("OVRCameraRig/TrackingSpace/RightHandAnchor/OVRHandPrefab").GetComponent<OVRHand>();

        // Sounds
        Audio_OpenShutters = Snd_OpenShutters.GetComponent<AudioSource>();
        Audio_UseLevers = Snd_UseLevers.GetComponent<AudioSource>();
        //Audio_DeathScream = Snd_DeathScream.GetComponent<AudioSource>();
        Audio_DeathScream = Snd_DeathScream.GetComponents<AudioSource>();

        n = 1;
        foreach (GameObject obj in m_ElemCubes)
        {
            ElemCubeClass elem = obj.GetComponent<ElemCubeClass>();
            elem.ChangeType((Elements)(n%4), m_ElementsMats);
            //obj.transform.Find("air").gameObject.SetActive(true);
            n++;
        }

        Debug.Log($"[GameMgr] Start. {gameState}");

        // Go to the localization phase
        //gameState = GameStates.Localization;
        GameObject allLoc = GameObject.Find("LocalizationMenu").gameObject;
        allLoc.SetActive(false);
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


    public MyHands GetHand(OVRHand.Hand type)
    {
        if (type == OVRHand.Hand.HandLeft)
            return m_hands[0];
        else
            return m_hands[1];
    }


    // Start a new fresh game
    void InitializeNewGame(int gameSeed)
    {
        instance = this;
        gameState = GameStates.Starting;

        if (m_Language == GameLanguages.Undefined)
        {
            // Set default language to French
            m_Language = GameLanguages.French;
        }
        else
        {
            // Load language from file
        }
        m_AllNotes = GameObject.Find("AllNotes/LocText").gameObject.GetComponent<Canvas>();

        startingSeed = gameSeed;
        UnityEngine.Random.InitState(startingSeed);

        int firstRandom = (int)(UnityEngine.Random.value * 100.0f);
        startingTime = Time.fixedTime;
        Debug.Log($"[GameMgr][{startingTime}] new game initialized with seed {startingSeed}, first random {firstRandom}/19.");

        playerCellId = 12; // replace the player in the middle

        int reserveExit = 24; // make sure we'll always have a valid exit
        List<int> deadly = new List<int>(deadlyCellNb);
        while (deadly.Count < deadlyCellNb)
        {
            int id = (int)(UnityEngine.Random.value * 24.0f);
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
            int id = (int)(UnityEngine.Random.value * 24.0f);
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
                float aRndNb = UnityEngine.Random.value;

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

        // Randomize code sequence
        ChangeCodes();

        //--- show models ---
        if (m_CentreModels.m_EntryCell == null)
        {
            m_CentreModels.m_EntryCell = GameObject.Instantiate(m_MiddleCell);
            m_CentreModels.m_EntryCell.transform.SetParent(m_CentreModels.transform);
            m_CentreModels.m_EntryCell.SetActive(true);
            m_CentreModels.m_GenCellA = GameObject.Instantiate(m_GenCellA);
            m_CentreModels.m_GenCellA.transform.SetParent(m_CentreModels.transform);
            m_CentreModels.m_GenCellA.name = "GenACentre Model";
            //Debug.Log($"[MgrInit] Awake. {m_CentreModels.m_GenCellA.name}\\{m_CentreModels.m_GenCellA.transform.parent.name} Centre");
            m_CentreModels.m_ExitCell = GameObject.Instantiate(m_ExitModel);
            m_CentreModels.m_ExitCell.transform.SetParent(m_CentreModels.transform);
            m_CentreModels.m_BlindCell = GameObject.Instantiate(m_BlindCellModel);
            m_CentreModels.m_BlindCell.transform.SetParent(m_CentreModels.transform);
            m_CentreModels.m_IllusionCell = GameObject.Instantiate(m_IllusionCellModel);
            m_CentreModels.m_IllusionCell.transform.SetParent(m_CentreModels.transform);
            m_CentreModels.m_WaterCell = GameObject.Instantiate(m_WaterCellModel);
            m_CentreModels.m_WaterCell.transform.SetParent(m_CentreModels.transform);
            m_CentreModels.m_LaserCell = GameObject.Instantiate(m_LaserCellModel);
            m_CentreModels.m_LaserCell.transform.SetParent(m_CentreModels.transform);
            m_CentreModels.m_GazCell = GameObject.Instantiate(m_GazCellModel);
            m_CentreModels.m_GazCell.transform.SetParent(m_CentreModels.transform);
            m_CentreModels.m_PlanCell = GameObject.Instantiate(m_PlanCellModel);
            m_CentreModels.m_PlanCell.transform.SetParent(m_CentreModels.transform);

            m_NorthModels.m_GenCellA = GameObject.Instantiate(m_GenCellA);
            m_NorthModels.m_GenCellA.SetActive(true);
            m_NorthModels.m_GenCellA.transform.SetParent(m_NorthModels.transform);
            m_NorthModels.m_GenCellA.name = "GenANorth Model";
            //Debug.Log($"[MgrInit] Awake. {m_NorthModels.m_GenCellA.name}\\{m_NorthModels.m_GenCellA.transform.parent.name} m_NorthModels");
            m_NorthModels.m_ExitCell = GameObject.Instantiate(m_ExitModel);
            m_NorthModels.m_ExitCell.transform.SetParent(m_NorthModels.transform);
            m_NorthModels.m_EntryCell = GameObject.Instantiate(m_MiddleCell);
            m_NorthModels.m_EntryCell.transform.SetParent(m_NorthModels.transform);
            m_NorthModels.m_BlindCell = GameObject.Instantiate(m_BlindCellModel);
            m_NorthModels.m_BlindCell.transform.SetParent(m_NorthModels.transform);
            m_NorthModels.m_IllusionCell = GameObject.Instantiate(m_IllusionCellModel);
            m_NorthModels.m_IllusionCell.transform.SetParent(m_NorthModels.transform);
            m_NorthModels.m_WaterCell = GameObject.Instantiate(m_WaterCellModel);
            m_NorthModels.m_WaterCell.transform.SetParent(m_NorthModels.transform);
            m_NorthModels.m_LaserCell = GameObject.Instantiate(m_LaserCellModel);
            m_NorthModels.m_LaserCell.transform.SetParent(m_NorthModels.transform);
            m_NorthModels.m_GazCell = GameObject.Instantiate(m_GazCellModel);
            m_NorthModels.m_GazCell.transform.SetParent(m_NorthModels.transform);
            m_NorthModels.m_PlanCell = GameObject.Instantiate(m_PlanCellModel);
            m_NorthModels.m_PlanCell.transform.SetParent(m_NorthModels.transform);
            m_NorthModels.transform.position = new Vector3(0.0f, 0.0f, 2.9f);            

            m_EastModels.m_GenCellA = GameObject.Instantiate(m_GenCellA);
            m_EastModels.m_GenCellA.SetActive(true);
            m_EastModels.m_GenCellA.transform.SetParent(m_EastModels.transform);
            m_EastModels.m_GenCellA.name = "GenAEast Model";
            //Debug.Log($"[MgrInit] Awake. {m_EastModels.m_GenCellA.name}\\{m_EastModels.m_GenCellA.transform.parent.name} m_EastModels");
            m_EastModels.m_EntryCell = GameObject.Instantiate(m_MiddleCell);
            m_EastModels.m_EntryCell.transform.SetParent(m_EastModels.transform);
            m_EastModels.m_ExitCell = GameObject.Instantiate(m_ExitModel);
            m_EastModels.m_ExitCell.transform.SetParent(m_EastModels.transform);
            m_EastModels.m_BlindCell = GameObject.Instantiate(m_BlindCellModel);
            m_EastModels.m_BlindCell.transform.SetParent(m_EastModels.transform);
            m_EastModels.m_IllusionCell = GameObject.Instantiate(m_IllusionCellModel);
            m_EastModels.m_IllusionCell.transform.SetParent(m_EastModels.transform);
            m_EastModels.m_WaterCell = GameObject.Instantiate(m_WaterCellModel);
            m_EastModels.m_WaterCell.transform.SetParent(m_EastModels.transform);
            m_EastModels.m_LaserCell = GameObject.Instantiate(m_LaserCellModel);
            m_EastModels.m_LaserCell.transform.SetParent(m_EastModels.transform);
            m_EastModels.m_GazCell = GameObject.Instantiate(m_GazCellModel);
            m_EastModels.m_GazCell.transform.SetParent(m_EastModels.transform);
            m_EastModels.m_PlanCell = GameObject.Instantiate(m_PlanCellModel);
            m_EastModels.m_PlanCell.transform.SetParent(m_EastModels.transform);
            m_EastModels.transform.position = new Vector3(2.9f, 0.0f, 0.0f);            

            m_SouthModels.m_GenCellA = GameObject.Instantiate(m_GenCellA);
            m_SouthModels.m_GenCellA.SetActive(true);
            m_SouthModels.m_GenCellA.transform.SetParent(m_SouthModels.transform);
            m_SouthModels.m_GenCellA.name = "GenASouth Model";
            //Debug.Log($"[MgrInit] Awake. {m_SouthModels.m_GenCellA.name}\\{m_SouthModels.m_GenCellA.transform.parent.name} m_SouthModels");
            m_SouthModels.m_EntryCell = GameObject.Instantiate(m_MiddleCell);
            m_SouthModels.m_EntryCell.transform.SetParent(m_SouthModels.transform);
            m_SouthModels.m_ExitCell = GameObject.Instantiate(m_ExitModel);
            m_SouthModels.m_ExitCell.transform.SetParent(m_SouthModels.transform);
            m_SouthModels.m_BlindCell = GameObject.Instantiate(m_BlindCellModel);
            m_SouthModels.m_BlindCell.transform.SetParent(m_SouthModels.transform);
            m_SouthModels.m_IllusionCell = GameObject.Instantiate(m_IllusionCellModel);
            m_SouthModels.m_IllusionCell.transform.SetParent(m_SouthModels.transform);
            m_SouthModels.m_WaterCell = GameObject.Instantiate(m_WaterCellModel);
            m_SouthModels.m_WaterCell.transform.SetParent(m_SouthModels.transform);
            m_SouthModels.m_LaserCell = GameObject.Instantiate(m_LaserCellModel);
            m_SouthModels.m_LaserCell.transform.SetParent(m_SouthModels.transform);
            m_SouthModels.m_GazCell = GameObject.Instantiate(m_GazCellModel);
            m_SouthModels.m_GazCell.transform.SetParent(m_SouthModels.transform);
            m_SouthModels.m_PlanCell = GameObject.Instantiate(m_PlanCellModel);
            m_SouthModels.m_PlanCell.transform.SetParent(m_SouthModels.transform);
            m_SouthModels.transform.position = new Vector3(0.0f, 0.0f, -2.9f);            

            m_WestModels.m_GenCellA = GameObject.Instantiate(m_GenCellA);
            m_WestModels.m_GenCellA.SetActive(true);
            m_WestModels.m_GenCellA.transform.SetParent(m_WestModels.transform);
            m_WestModels.m_GenCellA.name = "GenAWest Model";
            //Debug.Log($"[MgrInit] Awake. {m_WestModels.m_GenCellA.name}\\{m_WestModels.m_GenCellA.transform.parent.name} m_WestModels");
            m_WestModels.m_EntryCell = GameObject.Instantiate(m_MiddleCell);
            m_WestModels.m_EntryCell.transform.SetParent(m_WestModels.transform);
            m_WestModels.m_ExitCell = GameObject.Instantiate(m_ExitModel);
            m_WestModels.m_ExitCell.transform.SetParent(m_WestModels.transform);
            m_WestModels.m_BlindCell = GameObject.Instantiate(m_BlindCellModel);
            m_WestModels.m_BlindCell.transform.SetParent(m_WestModels.transform);
            m_WestModels.m_IllusionCell = GameObject.Instantiate(m_IllusionCellModel);
            m_WestModels.m_IllusionCell.transform.SetParent(m_WestModels.transform);
            m_WestModels.m_WaterCell = GameObject.Instantiate(m_WaterCellModel);
            m_WestModels.m_WaterCell.transform.SetParent(m_WestModels.transform);
            m_WestModels.m_LaserCell = GameObject.Instantiate(m_LaserCellModel);
            m_WestModels.m_LaserCell.transform.SetParent(m_WestModels.transform);
            m_WestModels.m_GazCell = GameObject.Instantiate(m_GazCellModel);
            m_WestModels.m_GazCell.transform.SetParent(m_WestModels.transform);
            m_WestModels.m_PlanCell = GameObject.Instantiate(m_PlanCellModel);
            m_WestModels.m_PlanCell.transform.SetParent(m_WestModels.transform);
            m_WestModels.transform.position = new Vector3(-2.9f, 0.0f, 0.0f);            
        }
        //--- show models ---

        //--- set lighting ---
        float light_range = 1.0f;
        Color light_colour = new Color(1.0f, 1.0f, 0.9f, 1.0f);
        SetupLights(m_CentreModels, light_range, light_colour);
        light_colour = Color.red;
        SetupLights(m_NorthModels, light_range, light_colour);
        SetupLights(m_EastModels, light_range, light_colour);
        SetupLights(m_SouthModels, light_range, light_colour);
        SetupLights(m_WestModels, light_range, light_colour);
        //--- set lighting ---

        UpdateCellsModels();
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

            // Update cells models to display
            UpdateCellsModels();
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

            // Update cells models to display
            UpdateCellsModels();
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

        // Update cells models to display
        UpdateCellsModels();
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

        // Update cells models to display
        UpdateCellsModels();
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

        if (gameState == GameStates.Undefined)
        {
            return;
        }

        if (gameState == GameStates.Localization)
        {
            LocalizationUpdate();
            return;
        }

        if (Input.GetKeyUp(KeyCode.Backspace))
        {
            //Cleanup();
            //InitializeNewGame(0);
            InitializeNewGame(System.Environment.TickCount);
        }

        OneCellClass current = GetCurrentCell();

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
        if (Input.GetKey(KeyCode.Keypad6))
        {
            current.MechanismEast.m_forceActionning = true;
        }
        else
        {
            current.MechanismEast.m_forceActionning = false;
        }
        if (Input.GetKey(KeyCode.Keypad4))
        {
            current.MechanismWest.m_forceActionning = true;
        }
        else
        {
            current.MechanismWest.m_forceActionning = false;
        }
        if (Input.GetKey(KeyCode.Keypad8))
        {
            //MoveColumn(true);
            //current.MechanismNorth.TriggerAction();
            current.MechanismNorth.m_forceActionning = true;
        }
        else
        {
            current.MechanismNorth.m_forceActionning = false;
        }
        if (Input.GetKey(KeyCode.Keypad2))
        {
            current.MechanismSouth.m_forceActionning = true;
        }
        else
        {
            current.MechanismSouth.m_forceActionning = false;
        }

        if (gameState != GameStates.CodeAllSet)
        {
            GameObject directions = m_basicCanvas.transform.GetChild(1).gameObject;
            directions.GetComponent<TextMeshProUGUI>().text = $"Counter\n {Time.fixedTime - current.enterTime}s";
        }

        //*
        if (m_CentreModels.m_light_N.range < 5.0f)
        {
            m_CentreModels.m_light_N.range += Time.deltaTime * 2.0f;
            m_CentreModels.m_light_E.range += Time.deltaTime * 2.0f;
            m_CentreModels.m_light_S.range += Time.deltaTime * 2.0f;
            m_CentreModels.m_light_W.range += Time.deltaTime * 2.0f;
        }
        //*/


        if (Input.GetKeyUp(KeyCode.Z))
        {
            ScanTriggerAction(CardinalPoint.North);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            ScanTriggerAction(CardinalPoint.East);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            ScanTriggerAction(CardinalPoint.South);
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            ScanTriggerAction(CardinalPoint.West);
        }

        if (Input.GetKeyUp(KeyCode.End))
        {
            FakeFeedReceivers();
        }
    }


    // Update while in localization phase
    void LocalizationUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Home))
        {
            InitializeNewGame(startingSeed); // for debug purpose we always start with the same seed
        }

        if (Input.GetKeyUp(KeyCode.KeypadPlus))
        {
            if (m_Language == GameLanguages.French)
            {
                m_Language = GameLanguages.English;
                LoadLocalizedText("localized_en.json");
            }
            else
            {
                m_Language = GameLanguages.French;
                LoadLocalizedText("localized_fr.json");
            }

            GameObject intro = m_AllNotes.transform.GetChild(0).gameObject;
            intro.GetComponent<TextMeshProUGUI>().text = m_LocalizedText["entry_room_1"];
        }
    }


    // called once per fixed framerate
    private void FixedUpdate()
    {
        //Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s]");

        if (gameState == GameStates.Undefined)
        {
            return;
        }

        if (gameState == GameStates.Localization)
        {
            return;
        }

        // always update the player pos
        OneCellClass current = GetCurrentCell();
        if (current != null)
        {
            playerSphere.transform.position = current.SmallCell.transform.position + new Vector3(0.0f, 0.1f, 0.0f);

            if (current.cellType == CellTypes.Exit)
            {
                if ((gameState != GameStates.ExitFound) && (gameState != GameStates.CodeAllSet))
                {
                    gameState = GameStates.ExitFound;
                    GameObject title = m_basicCanvas.transform.GetChild(0).gameObject;
                    title.GetComponent<TextMeshProUGUI>().text = $"EXIT found!";
                }
                else
                {
                    if (gameState != GameStates.CodeAllSet)
                    {
                        // Check if all code receiver have been feeded
                        if (m_CodeFinalSet == 15)
                        {
                            //endgame, open door
                            m_EndGameTime = Time.fixedTime;
                            float gameDur = m_EndGameTime - startingTime;
                            float brutScore = Math.Max(0.0f, 1800.0f - gameDur);

                            GameObject txt1 = m_basicCanvas.transform.GetChild(1).gameObject;
                            txt1.GetComponent<TextMeshProUGUI>().text = $"Time: {gameDur}\nBrutScore: {brutScore}";
                            //count code points
                            int codePoints = CountCodePoints();
                            GameObject txt2 = m_basicCanvas.transform.GetChild(2).gameObject;
                            txt2.GetComponent<TextMeshProUGUI>().text = $"code: {codePoints}";
                            gameState = GameStates.CodeAllSet;
                            Debug.Log($"\tBrutScore: {brutScore}. Total: {brutScore + codePoints}");

                            StartCoroutine(OpenExitHatch());
                        }
                    }
                }
            }
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


    public CellTypes GetNorthType(int current)
    {
        OneCellClass cell = GetNorth(current);
        if (cell)
            return cell.cellType;
        else
            return CellTypes.Undefined;
    }


    public OneCellClass GetEast(int current)
    {
        if (current % 5 == 4)
        {
            return null;
        }
        return allCells[lookupTab[current + 1]];
    }


    public CellTypes GetEastType(int current)
    {
        OneCellClass cell = GetEast(current);
        if (cell)
            return cell.cellType;
        else
            return CellTypes.Undefined;
    }


    public OneCellClass GetSouth(int current)
    {
        if (current < 20)
        {
            return allCells[lookupTab[current + 5]];
        }

        return null;
    }


    public CellTypes GetSouthType(int current)
    {
        OneCellClass cell = GetSouth(current);
        if (cell)
            return cell.cellType;
        else
            return CellTypes.Undefined;
    }


    public OneCellClass GetWest(int current)
    {
        if (current % 5 == 0)
        {
            return null;
        }
        return allCells[lookupTab[current - 1]];
    }


    public CellTypes GetWestType(int current)
    {
        OneCellClass cell = GetWest(current);
        if (cell)
            return cell.cellType;
        else
            return CellTypes.Undefined;
    }


    // Update cells models to display when entering a new room
    void UpdateCellsModels()
    {
        OneCellClass current = GetCurrentCell();
        CellTypes curType = current.cellType;
        int curId = current.cellId;
        m_CentreModels.SetActiveModel(curType, current.cellSubType);

        // DeActivate elements
        m_GroupElements.SetActive(false);

        OneCellClass cell = GetNorth(playerCellId);
        if (cell != null)
        {
            m_NorthModels.SetActiveModel(cell.cellType, cell.cellSubType);
            UpdateCodesSections(CardinalPoint.North, cell.cellType);
        }
        else
        {
            m_NorthModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
            UpdateCodesSections(CardinalPoint.North, CellTypes.Undefined);
        }

        cell = GetEast(playerCellId);
        if (cell != null)
        {
            m_EastModels.SetActiveModel(cell.cellType, cell.cellSubType);
            UpdateCodesSections(CardinalPoint.East, cell.cellType);
        }
        else
        {
            m_EastModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
            UpdateCodesSections(CardinalPoint.East, CellTypes.Undefined);
        }

        cell = GetSouth(playerCellId);
        if (cell != null)
        {
            m_SouthModels.SetActiveModel(cell.cellType, cell.cellSubType);
            UpdateCodesSections(CardinalPoint.South, cell.cellType);
        }
        else
        {
            m_SouthModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
            UpdateCodesSections(CardinalPoint.South, CellTypes.Undefined);
        }

        cell = GetWest(playerCellId);
        if (cell != null)
        {
            m_WestModels.SetActiveModel(cell.cellType, cell.cellSubType);
            UpdateCodesSections(CardinalPoint.West, cell.cellType);
        }
        else
        {
            m_WestModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
            UpdateCodesSections(CardinalPoint.West, CellTypes.Undefined);
        }

        if (curType == CellTypes.Exit)
        {
            float light_range = 4.0f;
            Color light_colour = new Color(0.9f, 1.0f, 0.9f, 1.0f);
            SetupLights(m_CentreModels, light_range, light_colour);

            //remove the north wall "Trap_1" from south_cell...
            GameObject southModel = m_SouthModels.GetActiveModel();
            if (southModel)
            {
                /*
                GameObject northWall = southModel.transform.Find("Trap_1").gameObject;
                //Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] {m_SouthModels.transform.name}, model: {northWall.name}");
                if (northWall != null)
                {
                    northWall.SetActive(false);
                }
                */
                m_SouthModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
            }

            UpdateCodesSections(CardinalPoint.South, CellTypes.Undefined);

            // Activate elements
            m_GroupElements.SetActive(true);
        }
        else if (curType == CellTypes.Deadly)
        {
            SetupLightsBySubType(m_CentreModels, current.cellSubType);
        }

        // Activate levers for centre model only
        if (m_CentreModels.m_GenCellA.activeSelf == true)
        {
            //Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] {m_CentreModels.m_GenCellA.name}, MechanismNorth= {current.cellId}");
            /*
            GameObject genAModel = m_CentreModels.m_GenCellA.gameObject;
            GameObject h1 = genAModel.transform.Find("Trap_1").gameObject;
            GameObject h2 = h1.transform.Find("manche_base").gameObject;
            GameObject h3 = h2.transform.Find("manche").gameObject;
            h3.SetActive(true);
            OneCellClass toto = GetCurrentCell();
            MechanismMove mec = h3.GetComponentInChildren<MechanismMove>();
            toto.MechanismNorth = mec;
            */
            if (current.MechanismNorth.m_modelSet == false)
            {
                MechanismMove[] mecs = m_CentreModels.m_GenCellA.GetComponentsInChildren<MechanismMove>();
                foreach (MechanismMove mec in mecs)
                {
                    mec.enabled = true;
                    switch (mec.cardinal)
                    {
                        case CardinalPoint.North:
                            current.MechanismNorth = mec;
                            current.MechanismNorth.m_modelSet = true;
                            break;
                        case CardinalPoint.East:
                            current.MechanismEast = mec;
                            current.MechanismEast.m_modelSet = true;
                            break;
                        case CardinalPoint.South:
                            current.MechanismSouth = mec;
                            current.MechanismSouth.m_modelSet = true;
                            break;
                        case CardinalPoint.West:
                            current.MechanismWest = mec;
                            current.MechanismWest.m_modelSet = true;
                            break;
                        default:
                            Debug.LogWarning($"Wrong cardinal {mec.cardinal} for MechanismMove");
                            break;
                    }
                }
            }
        }

        if (m_CentreModels.m_BlindCell.activeSelf == true)
        {
            if (current.MechanismNorth.m_modelSet == false)
            {
                MechanismMove[] mecs = m_CentreModels.m_BlindCell.GetComponentsInChildren<MechanismMove>();
                foreach (MechanismMove mec in mecs)
                {
                    mec.enabled = true;
                    switch (mec.cardinal)
                    {
                        case CardinalPoint.North:
                            current.MechanismNorth = mec;
                            current.MechanismNorth.m_modelSet = true;
                            break;
                        case CardinalPoint.East:
                            current.MechanismEast = mec;
                            current.MechanismEast.m_modelSet = true;
                            break;
                        case CardinalPoint.South:
                            current.MechanismSouth = mec;
                            current.MechanismSouth.m_modelSet = true;
                            break;
                        case CardinalPoint.West:
                            current.MechanismWest = mec;
                            current.MechanismWest.m_modelSet = true;
                            break;
                        default:
                            Debug.LogWarning($"Wrong cardinal {mec.cardinal} for MechanismMove");
                            break;
                    }
                }
            }
        }

        SetupAdjacentLights(null, 0.0f, Color.red);
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
        if (from == 10) // same row as the start room = impossible
        {
            Audio_DeathScream[1].Play();
            return;
        }

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

        // Update cells models to display
        UpdateCellsModels();
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
        if (from == 2) // same column as the start room = impossible
        {
            Audio_DeathScream[1].Play();
            return;
        }

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

        // Update cells models to display
        UpdateCellsModels();
    }


    // Setup initial lights for a model
    void SetupLights(CellsModels model, float range, Color colour)
    {
        GameObject obj = model.transform.Find(("Point Light North")).gameObject;
        model.m_light_N = obj.GetComponent<Light>();
        model.m_light_N.range = range;
        model.m_light_N.color = colour;

        obj = model.transform.Find(("Point Light East")).gameObject;
        model.m_light_E = obj.GetComponent<Light>();
        model.m_light_E.range = range;
        model.m_light_E.color = colour;

        obj = model.transform.Find(("Point Light South")).gameObject;
        model.m_light_S = obj.GetComponent<Light>();
        model.m_light_S.range = range;
        model.m_light_S.color = colour;

        obj = model.transform.Find(("Point Light West")).gameObject;
        model.m_light_W = obj.GetComponent<Light>();
        model.m_light_W.range = range;
        model.m_light_W.color = colour;
    }


    // Sets lighting when entering a deadly room
    void SetupLightsBySubType(CellsModels model, CellSubTypes subTypes)
    {
        float light_range = 3.0f;
        Color light_colour = new Color(1.0f, 1.0f, 0.9f, 1.0f);
        switch (subTypes)
        {
            case CellSubTypes.Fire:
                light_colour = new Color(1.0f, 0.6f, 0.3f, 1.0f);
                break;
            case CellSubTypes.Gaz:
                light_colour = new Color(0.6f, 1.0f, 0.3f, 1.0f);
                break;
            case CellSubTypes.Water:
                light_colour = new Color(0.3f, 0.6f, 1.0f, 1.0f);
                break;
            case CellSubTypes.Lasers:
                light_colour = new Color(1.0f, 0.6f, 0.6f, 1.0f);
                break;
            case CellSubTypes.Blind:
            case CellSubTypes.Empty:
            case CellSubTypes.Illusion:
            case CellSubTypes.OneLook:
            case CellSubTypes.Screen:
            case CellSubTypes.Tunnel:
            case CellSubTypes.Vortex:
                break;
        }

        SetupLights(model, light_range, light_colour);
    }


    void SetupAdjacentLights(CellsModels model, float range, Color colour)
    {
        model = m_NorthModels;
        model.m_light_N.range = 5.0f;
        model.m_light_N.intensity = 0.5f;
        model.m_light_N.color = colour;
        Vector3 pos = model.m_light_N.transform.localPosition;
        pos.z = 0.5f;
        model.m_light_N.transform.localPosition = pos;

        model.m_light_E.enabled = false;
        model.m_light_E.range = range;

        model.m_light_S.enabled = false;
        model.m_light_S.range = range;

        model.m_light_W.enabled = false;
        model.m_light_W.range = range;

        // -- south
        model = m_SouthModels;
        model.m_light_N.enabled = false;
        model.m_light_N.range = range;

        model.m_light_E.enabled = false;
        model.m_light_E.range = range;

        model.m_light_S.range = 5.0f;
        model.m_light_S.intensity = 0.5f;
        model.m_light_S.color = colour;
        pos = model.m_light_S.transform.localPosition;
        pos.z = -0.5f;
        model.m_light_S.transform.localPosition = pos;

        model.m_light_W.enabled = false;
        model.m_light_W.range = range;

        // -- east
        model = m_EastModels;
        model.m_light_N.enabled = false;
        model.m_light_N.range = range;

        model.m_light_E.range = 5.0f;
        model.m_light_E.intensity = 0.5f;
        model.m_light_E.color = colour;
        pos = model.m_light_E.transform.localPosition;
        pos.x = 0.5f;
        model.m_light_E.transform.localPosition = pos;

        model.m_light_S.enabled = false;
        model.m_light_S.range = range;

        model.m_light_W.enabled = false;
        model.m_light_W.range = range;

        // -- west
        model = m_WestModels;
        model.m_light_N.enabled = false;
        model.m_light_N.range = range;

        model.m_light_E.enabled = false;
        model.m_light_E.range = range;

        model.m_light_S.enabled = false;
        model.m_light_S.range = range;

        model.m_light_W.range = 5.0f;
        model.m_light_W.intensity = 0.5f;
        model.m_light_W.color = colour;
        pos = model.m_light_W.transform.localPosition;
        pos.x = -0.5f;
        model.m_light_W.transform.localPosition = pos;
    }


    public void TeleportToStart()
    {
        InitializeNewGame(startingSeed); // Restart the game with the same previous seed
    }


    /// <summary>
    /// Gets the hand id associated with the index finger of the collider passed as parameter, if any
    /// </summary>
    /// <param name="collider">Collider of interest</param>
    /// <returns>0 if the collider represents the finger tip of left hand, 1 if it is the one of right hand, -1 if it is not an index fingertip</returns>
    public int GetFingerHandId(Collider collider, OVRPlugin.BoneId fingerId)
    {
        //Checking Oculus code, it is possible to see that physics capsules gameobjects always end with _CapsuleCollider
        if (collider.gameObject.name.Contains("_CapsuleCollider"))
        {
            //get the name of the bone from the name of the gameobject, and convert it to an enum value
            string boneName = collider.gameObject.name.Substring(0, collider.gameObject.name.Length - 16);
            OVRPlugin.BoneId boneId = (OVRPlugin.BoneId)System.Enum.Parse(typeof(OVRPlugin.BoneId), boneName);

            //if it is the tip of the Index
            if (boneId == fingerId)
                //check if it is left or right hand, and change color accordingly.
                //Notice that absurdly, we don't have a way to detect the type of the hand
                //so we have to use the hierarchy to detect current hand
                if (collider.transform.IsChildOf(m_hands[0].hand.transform))
                {
                    return 0;
                }
                else if (collider.transform.IsChildOf(m_hands[1].hand.transform))
                {
                    return 1;
                }
        }

        return -1;
    }


    // Open the shutters to look through cells
    public void ScanTriggerAction(CardinalPoint point)
    {
        switch (point)
        {
            case CardinalPoint.North:
                {
                    if (m_displayCell_N == false)
                    {
                        m_displayCell_N = true;

                        GameObject front = null;
                        if (m_CentreModels.m_EntryCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_EntryCell.transform.Find("trap_0").gameObject;
                            front = front.transform.Find("trape_2").gameObject;
                        }
                        else if (m_CentreModels.m_GenCellA.activeSelf == true)
                        {
                            front = m_CentreModels.m_GenCellA.transform.Find("Trap_1").gameObject;
                            front = front.transform.Find("trape_1").gameObject;
                        }
                        else if (m_CentreModels.m_IllusionCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_IllusionCell.transform.Find("Trap_1").gameObject;
                            front = front.transform.Find("trape_1").gameObject;
                        }
                        else if (m_CentreModels.m_BlindCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_BlindCell.transform.Find("Trap_1").gameObject;
                            front = front.transform.Find("trape_1").gameObject;
                        }
                        else if (m_CentreModels.m_PlanCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_PlanCell.transform.Find("Trap_1").gameObject;
                            front = front.transform.Find("trape_1").gameObject;
                        }

                        // Jow: Make sure the back is set considering the right active model as for the front
                        GameObject back = m_NorthModels.GetActiveModel();
                        if (m_NorthModels.m_CurrentType == CellsModels.CellsModelsType.Entry)
                        {
                            back = back.transform.Find("trap_0").gameObject;
                            back = back.transform.Find("trape_2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_NorthModels.m_CurrentType == CellsModels.CellsModelsType.GenA)
                        {
                            back = back.transform.Find("trap_0").gameObject;
                            back = back.transform.Find("trape_2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_NorthModels.m_CurrentType == CellsModels.CellsModelsType.BlindM)
                        {
                            back = back.transform.Find("trap_0").gameObject;
                            back = back.transform.Find("trape_2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_NorthModels.m_CurrentType == CellsModels.CellsModelsType.IllusionM)
                        {
                            back = back.transform.Find("trap_0").gameObject;
                            back = back.transform.Find("trape_0").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_NorthModels.m_CurrentType == CellsModels.CellsModelsType.WaterM)
                        {
                            back = back.transform.Find("trap_0").gameObject;
                            back = back.transform.Find("trape_2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_NorthModels.m_CurrentType == CellsModels.CellsModelsType.LaserM)
                        {
                            back = back.transform.Find("trap_0").gameObject;
                            back = back.transform.Find("trape_2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_NorthModels.m_CurrentType == CellsModels.CellsModelsType.GazM)
                        {
                            back = back.transform.Find("trap_0").gameObject;
                            back = back.transform.Find("trape_2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_NorthModels.m_CurrentType == CellsModels.CellsModelsType.PlanM)
                        {
                            back = back.transform.Find("trap_0").gameObject;
                            back = back.transform.Find("trape_2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else
                        {
                            m_displayCell_N = false;
                            AudioSource snd = Audio_DeathScream[4];
                            if (snd.isPlaying == false)
                            {
                                snd.Play();
                            }
                        }
                    }
                    break;
                }
            case CardinalPoint.East:
                {
                    if (m_displayCell_E == false)
                    {
                        m_displayCell_E = true;

                        GameObject front = null;
                        if (m_CentreModels.m_EntryCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_EntryCell.transform.Find("trap_2").gameObject;
                            front = front.transform.Find("trape_2 1").gameObject;
                        }
                        else if (m_CentreModels.m_GenCellA.activeSelf == true)
                        {
                            front = m_CentreModels.m_GenCellA.transform.Find("trap_3").gameObject;
                            front = front.transform.Find("trape_2 2").gameObject;
                        }
                        else if (m_CentreModels.m_IllusionCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_IllusionCell.transform.Find("trap_3").gameObject;
                            front = front.transform.Find("trape_3").gameObject;
                        }
                        else if (m_CentreModels.m_BlindCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_BlindCell.transform.Find("trap_3").gameObject;
                            front = front.transform.Find("trape_2 2").gameObject;
                        }
                        else if (m_CentreModels.m_PlanCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_PlanCell.transform.Find("trap_3").gameObject;
                            front = front.transform.Find("trape_2 2").gameObject;
                        }

                        GameObject back = m_EastModels.GetActiveModel();
                        if (m_EastModels.m_CurrentType == CellsModels.CellsModelsType.Entry)
                        {
                            back = back.transform.Find("trap_3").gameObject;
                            back = back.transform.Find("trape_2 2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_EastModels.m_CurrentType == CellsModels.CellsModelsType.GenA)
                        {
                            back = back.transform.Find("trap_2").gameObject;
                            back = back.transform.Find("trape_2 1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_EastModels.m_CurrentType == CellsModels.CellsModelsType.Exit)
                        {
                            back = back.transform.Find("trap_2").gameObject;
                            back = back.transform.Find("trape_2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_EastModels.m_CurrentType == CellsModels.CellsModelsType.BlindM)
                        {
                            back = back.transform.Find("trap_2").gameObject;
                            back = back.transform.Find("trape_2 1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_EastModels.m_CurrentType == CellsModels.CellsModelsType.IllusionM)
                        {
                            back = back.transform.Find("trap_2").gameObject;
                            back = back.transform.Find("trape_2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_EastModels.m_CurrentType == CellsModels.CellsModelsType.WaterM)
                        {
                            back = back.transform.Find("trap_2").gameObject;
                            back = back.transform.Find("trape_2 1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_EastModels.m_CurrentType == CellsModels.CellsModelsType.LaserM)
                        {
                            back = back.transform.Find("trap_2").gameObject;
                            back = back.transform.Find("trape_2 1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_EastModels.m_CurrentType == CellsModels.CellsModelsType.GazM)
                        {
                            back = back.transform.Find("trap_2").gameObject;
                            back = back.transform.Find("trape_2 1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_EastModels.m_CurrentType == CellsModels.CellsModelsType.PlanM)
                        {
                            back = back.transform.Find("trap_2").gameObject;
                            back = back.transform.Find("trape_2 1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else
                        {
                            m_displayCell_E = false;
                            AudioSource snd = Audio_DeathScream[4];
                            if (snd.isPlaying == false)
                            {
                                snd.Play();
                            }
                        }
                    }
                    break;
                }
            case CardinalPoint.South:
                {
                    if (m_displayCell_S == false)
                    {
                        m_displayCell_S = true;

                        GameObject front = null;
                        if (m_CentreModels.m_EntryCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_EntryCell.transform.Find("Trap_1").gameObject;
                            front = front.transform.Find("trape_1").gameObject;
                        }
                        else if (m_CentreModels.m_GenCellA.activeSelf == true)
                        {
                            front = m_CentreModels.m_GenCellA.transform.Find("trap_0").gameObject;
                            front = front.transform.Find("trape_2").gameObject;
                        }
                        else if (m_CentreModels.m_IllusionCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_IllusionCell.transform.Find("trap_0").gameObject;
                            front = front.transform.Find("trape_0").gameObject;
                        }
                        else if (m_CentreModels.m_BlindCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_BlindCell.transform.Find("trap_0").gameObject;
                            front = front.transform.Find("trape_2").gameObject;
                        }
                        else if (m_CentreModels.m_PlanCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_PlanCell.transform.Find("trap_0").gameObject;
                            front = front.transform.Find("trape_2").gameObject;
                        }

                        GameObject back = m_SouthModels.GetActiveModel();
                        if (m_SouthModels.m_CurrentType == CellsModels.CellsModelsType.Entry)
                        {
                            back = back.transform.Find("trap_0").gameObject;
                            back = back.transform.Find("trape_2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_SouthModels.m_CurrentType == CellsModels.CellsModelsType.GenA)
                        {
                            back = back.transform.Find("Trap_1").gameObject;
                            back = back.transform.Find("trape_1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_SouthModels.m_CurrentType == CellsModels.CellsModelsType.BlindM)
                        {
                            back = back.transform.Find("Trap_1").gameObject;
                            back = back.transform.Find("trape_1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_SouthModels.m_CurrentType == CellsModels.CellsModelsType.IllusionM)
                        {
                            back = back.transform.Find("Trap_1").gameObject;
                            back = back.transform.Find("trape_1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_SouthModels.m_CurrentType == CellsModels.CellsModelsType.WaterM)
                        {
                            back = back.transform.Find("Trap_1").gameObject;
                            back = back.transform.Find("trape_1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_SouthModels.m_CurrentType == CellsModels.CellsModelsType.LaserM)
                        {
                            back = back.transform.Find("Trap_1").gameObject;
                            back = back.transform.Find("trape_1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_SouthModels.m_CurrentType == CellsModels.CellsModelsType.GazM)
                        {
                            back = back.transform.Find("Trap_1").gameObject;
                            back = back.transform.Find("trape_1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_SouthModels.m_CurrentType == CellsModels.CellsModelsType.PlanM)
                        {
                            back = back.transform.Find("Trap_1").gameObject;
                            back = back.transform.Find("trape_1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else
                        {
                            m_displayCell_S = false;
                            AudioSource snd = Audio_DeathScream[4];
                            if (snd.isPlaying == false)
                            {
                                snd.Play();
                            }
                        }
                    }
                    break;
                }
            case CardinalPoint.West:
                {
                    if (m_displayCell_W == false)
                    {
                        m_displayCell_W = true;

                        GameObject front = null;
                        if (m_CentreModels.m_EntryCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_EntryCell.transform.Find("trap_3").gameObject;
                            front = front.transform.Find("trape_2 2").gameObject;
                        }
                        else if (m_CentreModels.m_GenCellA.activeSelf == true)
                        {
                            front = m_CentreModels.m_GenCellA.transform.Find("trap_2").gameObject;
                            front = front.transform.Find("trape_2 1").gameObject;
                        }
                        else if (m_CentreModels.m_IllusionCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_IllusionCell.transform.Find("trap_2").gameObject;
                            front = front.transform.Find("trape_2").gameObject;
                        }
                        else if (m_CentreModels.m_BlindCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_BlindCell.transform.Find("trap_2").gameObject;
                            front = front.transform.Find("trape_2 1").gameObject;
                        }
                        else if (m_CentreModels.m_PlanCell.activeSelf == true)
                        {
                            front = m_CentreModels.m_PlanCell.transform.Find("trap_2").gameObject;
                            front = front.transform.Find("trape_2 1").gameObject;
                        }

                        GameObject back = m_WestModels.GetActiveModel();
                        if (m_WestModels.m_CurrentType == CellsModels.CellsModelsType.Entry)
                        {
                            back = back.transform.Find("trap_2").gameObject;
                            back = back.transform.Find("trape_2 1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_WestModels.m_CurrentType == CellsModels.CellsModelsType.GenA)
                        {
                            back = back.transform.Find("trap_3").gameObject;
                            back = back.transform.Find("trape_2 2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_WestModels.m_CurrentType == CellsModels.CellsModelsType.Exit)
                        {
                            back = back.transform.Find("trap_3").gameObject;
                            back = back.transform.Find("trape_2 1").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_WestModels.m_CurrentType == CellsModels.CellsModelsType.BlindM)
                        {
                            back = back.transform.Find("trap_3").gameObject;
                            back = back.transform.Find("trape_2 2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_WestModels.m_CurrentType == CellsModels.CellsModelsType.IllusionM)
                        {
                            back = back.transform.Find("trap_3").gameObject;
                            back = back.transform.Find("trape_3").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_WestModels.m_CurrentType == CellsModels.CellsModelsType.WaterM)
                        {
                            back = back.transform.Find("trap_3").gameObject;
                            back = back.transform.Find("trape_3").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_WestModels.m_CurrentType == CellsModels.CellsModelsType.LaserM)
                        {
                            back = back.transform.Find("trap_3").gameObject;
                            back = back.transform.Find("trape_2 2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_WestModels.m_CurrentType == CellsModels.CellsModelsType.GazM)
                        {
                            back = back.transform.Find("trap_3").gameObject;
                            back = back.transform.Find("trape_2 2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else if (m_WestModels.m_CurrentType == CellsModels.CellsModelsType.PlanM)
                        {
                            back = back.transform.Find("trap_3").gameObject;
                            back = back.transform.Find("trape_2 2").gameObject;
                            StartCoroutine(OpenShutters(point, front, back));
                        }
                        else
                        {
                            m_displayCell_W = false;
                            AudioSource snd = Audio_DeathScream[4];
                            if (snd.isPlaying == false)
                            {
                                snd.Play();
                            }
                        }
                    }
                    break;
                }
        }
    }


    private IEnumerator OpenShutters(CardinalPoint point, GameObject front, GameObject back)
    {
        yield return new WaitForSecondsRealtime(0.5f);

        if ((front == null) || (back == null))
        {
            yield return 0;
        }

        //--- Snd ---
        Audio_OpenShutters.transform.SetParent(front.transform);
        Audio_OpenShutters.transform.localPosition = Vector3.zero;
        Audio_OpenShutters.Play();
        //--- Snd ---

        float startTime = Time.time;
        while (Time.time - startTime < 2.0f)
        {
            front.transform.position += Vector3.up * Time.fixedDeltaTime * 0.3f;
            back.transform.position += Vector3.up * Time.fixedDeltaTime * 0.3f;
            yield return new WaitForFixedUpdate();
        }
        Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] {front.name} is open.");
        StartCoroutine(CloseShutters(point, front, back));
    }


    // Close the shutters after x sec
    private IEnumerator CloseShutters(CardinalPoint point, GameObject front, GameObject back)
    {
        yield return new WaitForSecondsRealtime(5.0f);

        //--- Snd ---
        Audio_OpenShutters.Play();
        //--- Snd ---

        float startTime = Time.time;
        while (Time.time - startTime < 2.0f)
        {
            front.transform.position -= Vector3.up * Time.fixedDeltaTime * 0.3f;
            back.transform.position -= Vector3.up * Time.fixedDeltaTime * 0.3f;
            yield return new WaitForFixedUpdate();
        }
        Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] {front.name} is closed.");

        switch (point)
        {
            case CardinalPoint.North:
                {
                    m_displayCell_N = false;
                    break;
                }
            case CardinalPoint.East:
                {
                    m_displayCell_E = false;
                    break;
                }
            case CardinalPoint.South:
                {
                    m_displayCell_S = false;
                    break;
                }
            case CardinalPoint.West:
                {
                    m_displayCell_W = false;
                    break;
                }
        }
    }


    // Load Elements materials
    void LoadElementsMats()
    {
        m_ElementsMats[0] = Resources.Load("Elem_1", typeof(Material)) as Material;
        if (m_ElementsMats[0] == null)
        {
            Debug.LogError("Could not load materials, place it in Resources Folder!");
        }

        m_ElementsMats[1] = Resources.Load("Elem_2", typeof(Material)) as Material;
        if (m_ElementsMats[1] == null)
        {
            Debug.LogError("Could not load materials, place it in Resources Folder!");
        }

        m_ElementsMats[2] = Resources.Load("Elem_3", typeof(Material)) as Material;
        if (m_ElementsMats[2] == null)
        {
            Debug.LogError("Could not load materials, place it in Resources Folder!");
        }

        m_ElementsMats[3] = Resources.Load("Elem_4", typeof(Material)) as Material;
        if (m_ElementsMats[3] == null)
        {
            Debug.LogError("Could not load materials, place it in Resources Folder!");
        }
    }


    // Set new random codes
    public void ChangeCodes()
    {
        for (int i=0; i <4; ++i)
        {
            m_CodeSafe[i] = (Elements)UnityEngine.Random.Range(0, 4);
            m_CodeDanger[i] = (Elements)UnityEngine.Random.Range(0, 4);
            m_CodeDeath[i] = (Elements)UnityEngine.Random.Range(0, 4);
        }
        Debug.Log($"codeSafe: {m_CodeSafe[0]} - {m_CodeSafe[1]} - {m_CodeSafe[2]} - {m_CodeSafe[3]}");
        Debug.Log($"codeDanger: {m_CodeDanger[0]} - {m_CodeDanger[1]} - {m_CodeDanger[2]} - {m_CodeDanger[3]}");
        Debug.Log($"codeDeath: {m_CodeDeath[0]} - {m_CodeDeath[1]} - {m_CodeDeath[2]} - {m_CodeDeath[3]}");

        // For testing purpose we just set one of each
        /*
        ChangeCodesSection("code_N", m_CodeSafe);
        ChangeCodesSection("code_E", m_CodeDanger);
        ChangeCodesSection("code_S", m_CodeDeath);
        ChangeCodesSection("code_W", m_CodeSafe);
        */
    }


    // Initialize code materials with default sequence
    void ChangeCodesSection(string cardinalName, Elements[] mats)
    {
        GameObject section = m_codes.transform.Find(cardinalName).gameObject;
        GameObject code = section.transform.Find("code_1").gameObject;
        MeshRenderer renderer = code.GetComponent<MeshRenderer>();
        if (mats == null)
        {
            code.SetActive(false);
        }
        else
        {
            renderer.material = m_ElementsMats[(int)mats[0]];
            code.SetActive(true);
        }

        code = section.transform.Find("code_2").gameObject;
        renderer = code.GetComponent<MeshRenderer>();
        if (mats == null)
        {
            code.SetActive(false);
        }
        else
        {
            renderer.material = m_ElementsMats[(int)mats[1]];
            code.SetActive(true);
        }

        code = section.transform.Find("code_3").gameObject;
        renderer = code.GetComponent<MeshRenderer>();
        if (mats == null)
        {
            code.SetActive(false);
        }
        else
        {
            renderer.material = m_ElementsMats[(int)mats[2]];
            code.SetActive(true);
        }

        code = section.transform.Find("code_4").gameObject;
        renderer = code.GetComponent<MeshRenderer>();
        if (mats == null)
        {
            code.SetActive(false);
        }
        else
        {
            renderer.material = m_ElementsMats[(int)mats[3]];
            code.SetActive(true);
        }
    }


    // Set proper codes depending on neighbour cells
    void UpdateCodesSections(CardinalPoint cardinal, CellTypes cellType)
    {
        string goName = "";

        switch (cardinal)
        {
            case CardinalPoint.North:
                goName = "code_N";
                break;
            case CardinalPoint.East:
                goName = "code_E";
                break;
            case CardinalPoint.South:
                goName = "code_S";
                break;
            case CardinalPoint.West:
                goName = "code_W";
                break;
        }

        switch (cellType)
        {
            case CellTypes.Deadly:
                ChangeCodesSection(goName, m_CodeDeath);
                break;
            case CellTypes.Effect:
                ChangeCodesSection(goName, m_CodeDanger);
                break;
            case CellTypes.Undefined:
                ChangeCodesSection(goName, null);
                break;
            default:
                ChangeCodesSection(goName, m_CodeSafe);
                break;
        }
    }


    // All receivers have been feeded, count points
    int CountCodePoints()
    {
        ElemReceiver rA = m_GroupElements.transform.Find("CubeFeedA").GetComponent<ElemReceiver>();
        ElemReceiver rB = m_GroupElements.transform.Find("CubeFeedB").GetComponent<ElemReceiver>();
        ElemReceiver rC = m_GroupElements.transform.Find("CubeFeedC").GetComponent<ElemReceiver>();
        ElemReceiver rD = m_GroupElements.transform.Find("CubeFeedD").GetComponent<ElemReceiver>();

        int points = 0;
        // Safe code value 600
        if ((rA.m_ElementType == (Elements)m_CodeSafe[0])
            && (rB.m_ElementType == (Elements)m_CodeSafe[1])
            && (rC.m_ElementType == (Elements)m_CodeSafe[2])
            && (rD.m_ElementType == (Elements)m_CodeSafe[3]))
        {
            points += 600;
        }

        // Danger code value 300
        if ((rA.m_ElementType == (Elements)m_CodeDanger[0])
            && (rB.m_ElementType == (Elements)m_CodeDanger[1])
            && (rC.m_ElementType == (Elements)m_CodeDanger[2])
            && (rD.m_ElementType == (Elements)m_CodeDanger[3]))
        {
            points += 300;
        }

        // Dealdly code value 100
        if ((rA.m_ElementType == (Elements)m_CodeDeath[0])
            && (rB.m_ElementType == (Elements)m_CodeDeath[1])
            && (rC.m_ElementType == (Elements)m_CodeDeath[2])
            && (rD.m_ElementType == (Elements)m_CodeDeath[3]))
        {
            points += 100;
        }

        Debug.Log($"------ Your code points is {points}.");
        return points;
    }


    // Fake feeding all 4 receivers
    void FakeFeedReceivers()
    {
        ElemReceiver rA = m_GroupElements.transform.Find("CubeFeedA").GetComponent<ElemReceiver>();
        ElemReceiver rB = m_GroupElements.transform.Find("CubeFeedB").GetComponent<ElemReceiver>();
        ElemReceiver rC = m_GroupElements.transform.Find("CubeFeedC").GetComponent<ElemReceiver>();
        ElemReceiver rD = m_GroupElements.transform.Find("CubeFeedD").GetComponent<ElemReceiver>();

        List<ElemCubeClass> cubes = new List<ElemCubeClass>(4);
        // Pick 4 elements randomly
        for (int i = 0; i < 4; ++i)
        {
            int rnd = UnityEngine.Random.Range(0, m_ElemCubes.Count);
            ElemCubeClass c = m_ElemCubes[rnd].GetComponent<ElemCubeClass>();
            cubes.Add(c);
        }
        
        cubes[0].transform.SetPositionAndRotation(rA.transform.position, rA.transform.rotation);
        cubes[1].transform.SetPositionAndRotation(rB.transform.position, rB.transform.rotation);
        cubes[2].transform.SetPositionAndRotation(rC.transform.position, rC.transform.rotation);
        cubes[3].transform.SetPositionAndRotation(rD.transform.position, rD.transform.rotation);
    }


    private IEnumerator OpenExitHatch()
    {
        GameObject hatchModel = null;
        GameObject hatch = m_CentreModels.m_ExitCell.transform.Find("trap_exit/door_exit").gameObject;
        if (hatch != null)
        {
            hatchModel = hatch;
            Debug.Log($"[ExitHandsTrigger] Awake. {transform.name}, model: {hatchModel.name} in {hatchModel.transform.position}");
        }
        else
        {
            Debug.LogWarning($"[ExitHandsTrigger] Awake. {transform.name}, model: {hatchModel.name} couldn't be found...");
            yield return 0;
        }

        //--- Snd ---
        Audio_DeathScream[5].transform.SetParent(hatchModel.transform);
        Audio_DeathScream[5].Play();
        //--- Snd ---

        float startTime = Time.time;
        while (Time.time - startTime < 3.5f)
        {
            hatchModel.transform.RotateAround(hatchModel.transform.position, transform.up, Time.deltaTime * -35.0f);
            yield return new WaitForFixedUpdate();
        }
        Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] {hatchModel.name} is open.");
    }

    
    //
    public void LoadLocalizedText(string fileName)
    {
        if (m_LocalizedText != null)
        {
            m_LocalizedText.Clear();
        }

        m_LocalizedText = new Dictionary<string, string>();
        string filePath;
        filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
        www.SendWebRequest();
        while (!www.isDone)
        {
        }
        String jsonString = www.downloadHandler.text;

        if (jsonString != null)
        {
            LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(jsonString);
            for (int i = 0; i < loadedData.items.Length; i++)
            {
                m_LocalizedText.Add(loadedData.items[i].key, loadedData.items[i].value);
            }

            Debug.Log($"Localization dictionary loaded with {m_LocalizedText.Count} entries.");
        }
        else
        {
            Debug.LogError($"Cannot find file {filePath}.");
        }
    }
}
