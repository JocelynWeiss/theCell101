﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using System.Runtime.ExceptionServices;
using Oculus.Platform.Models;

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
    Material[] m_CubesElemMats = new Material[4];
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
    [ViewOnly] public int m_DeathCount = 0;
    [ViewOnly] public int m_ViewLeft = 0; // Number of possible use of hand scaner per cell
    public float m_MaxGameLength = 1800.0f; // Maximum time allowed for one game in seconds
    [ViewOnly] public bool m_MovingOut = false; // True since both hands trigger moving out and false when entering a new cell

    // Current Game language
    [ViewOnly] public static GameLanguages m_Language = GameLanguages.Undefined;
    public Canvas m_AllNotes = null;
    public Dictionary<string, string> m_LocalizedText;


    static int startingSeed = 0;
    static float startingTime = 0.0f; // Time since the start of a new game in sec
    float m_EndGameTime = 0.0f; // Time to compute score when the player end the game in sec
    public OneCellClass cellClassPrefab;
    public List<OneCellClass> allCells; // All the cells as they are distributed
    public List<int> lookupTab = new List<int>(25); // lookup table, hold a map of cell's id
    [ViewOnly] public int playerCellId = 12; // in which place on the chess the player is. Match the lookup table.
    GameObject playerSphere = null; // a sphere to represent where the player is on the board.
    [ViewOnly] public Canvas m_basicCanvas = null;
	System.Random m_RandomBis = new System.Random();

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
    public GameObject m_OneLookCellModel; // The one look cell model
    public GameObject m_FireCellModel;
    public GameObject m_VortexModel;
    public GameObject m_TunnelCellModel;
    bool m_displayCell_N = false;
    bool m_displayCell_E = false;
    bool m_displayCell_S = false;
    bool m_displayCell_W = false;

    public GameObject m_codes;  // The 4 codes on each wall
    public GameObject m_MenuArea1;  // Menu to select languages.
    public GameObject m_PlayaModel;
    public GameObject m_EndingLights;
    public LocalizationMenu m_LocMenu;

    [HideInInspector] public Vector3 m_waterLevel;
    [HideInInspector] public Vector3 m_waterStuffLevel;

    public CellsModels m_CentreModels;
    public CellsModels m_NorthModels;
    public CellsModels m_EastModels;
    public CellsModels m_SouthModels;
    public CellsModels m_WestModels;

    public List<Material> m_ScreenMats;

    public GameObject Snd_OpenShutters;
    private AudioSource Audio_OpenShutters;

    public GameObject Snd_UseLevers;
    [HideInInspector] public AudioSource Audio_UseLevers;

    public GameObject Snd_SoundBank;
    //0 Death scream
    //1 Cannot move row or column
    //2 On Player exit room
    //3 Element placement
    //4 Hand scanner fail
    //5= 27 Open hatch
    //6 General ambiance
    //7 Beach ambiance
    //8 =9 start room amb1
    //9 =9a start room amb2
    //10= 10 On player exit room
    //11= 12 Hand scanner
    //12= 15 Rifle Jam
    //13= 17 Start teleport
    //14= 16 Line moves
    //15= Female voice 1
    //16= Exit ambiance
    //17= Execute teleport sequence
    //18= Button Down
    //19= Button Up
    //20= 21 Waterfall
    //21= 22 Drowning
    //22= 1 Metal impact
    //23= 19 Warning alarm
    //24= 23 Lasers cut
    //25= 24 Tunnel effect
    [HideInInspector] public AudioSource[] Audio_Bank;
    [HideInInspector] public List<AudioClip> Audio_Voices;
    int m_playedVoice;
    float m_nextVoiceToPlay; // int seconds
    float m_nextSoundTime; // When the next random sound will be triggered
    int m_nextSoundId; // Id of the next random sound to be played
    public List<AudioClip> m_AudioRandomSounds;

    public bool m_ShowMiniMap = true;

    public ParticleSystem m_FxVentilo;
    public GameObject m_FxTopSteam;
    public GameObject m_FxTeleporter;
    public GameObject m_FxSpawner;
    public GameObject m_FxFlame;
    public GameObject m_FxGaz;
    public GameObject m_FxLasers;
    public GameObject m_FxIllusion;
    public GameObject m_FxEmber;
    public GameObject m_FxWater;
    public GameObject m_FXDeathRespawn;
    public GameObject m_FxRespawn;

    List<GameObject> m_ScreenCard; // The list of all small cards on the plan's room screen

    //--- Grabbables ---
    GameObject m_GroupElements;
    GameObject m_ElementPrefab;
    private List<GameObject> m_ElemCubes = new List<GameObject>();
    public int m_ElemCubeNb = 16;
    //--- Grabbables ---

    //--- Consoles ---
    public GameObject m_Console_N;
    public Renderer m_Console_N_Lhand;
    public Renderer m_Console_N_Rhand;
    public GameObject m_Console_E;
    public Renderer m_Console_E_Lhand;
    public Renderer m_Console_E_Rhand;
    public GameObject m_Console_S;
    public Renderer m_Console_S_Lhand;
    public Renderer m_Console_S_Rhand;
    public GameObject m_Console_W;
    public Renderer m_Console_W_Lhand;
    public Renderer m_Console_W_Rhand;
    //--- Consoles ---


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
        playerSphere.SetActive(false);

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
            Vector3 pos = new Vector3(UnityEngine.Random.Range(-1.1f, 1.1f), 0.0f, UnityEngine.Random.Range(-0.7f, 1.1f));
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
        m_AllNotes.enabled = false;

        // Hide consoles
        m_Console_N.SetActive(false);
        m_Console_E.SetActive(false);
        m_Console_S.SetActive(false);
        m_Console_W.SetActive(false);
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
        m_basicCanvas.enabled = false;

        // Init hands
        m_hands[0].handType = OVRHand.Hand.HandLeft;
        m_hands[0].hand = GameObject.Find("OVRCameraRig/TrackingSpace/LeftHandAnchor/OVRHandPrefab").GetComponent<OVRHand>();
        m_hands[1].handType = OVRHand.Hand.HandRight;
        m_hands[1].hand = GameObject.Find("OVRCameraRig/TrackingSpace/RightHandAnchor/OVRHandPrefab").GetComponent<OVRHand>();

        // Sounds
        Audio_OpenShutters = Snd_OpenShutters.GetComponent<AudioSource>();
        Audio_UseLevers = Snd_UseLevers.GetComponent<AudioSource>();
        Audio_Bank = Snd_SoundBank.GetComponents<AudioSource>();
        m_playedVoice = 1;
        m_nextVoiceToPlay = 120.0f;

        n = 1;
        foreach (GameObject obj in m_ElemCubes)
        {
            ElemCubeClass elem = obj.GetComponent<ElemCubeClass>();
            elem.ChangeType((Elements)(n % 4), m_CubesElemMats);
            //obj.transform.Find("air").gameObject.SetActive(true);
            n++;
        }

        // Go to the localization phase
        gameState = GameStates.Localization;
        //GameObject.Find("code_00").gameObject.SetActive(false);
        m_MenuArea1.SetActive(true);
        m_codes.SetActive(false);
        m_PlayaModel.transform.Rotate(Vector3.up, 180.0f);
        m_PlayaModel.transform.position = new Vector3(0.0f, 0.0f, -1.2f);
        Audio_Bank[7].Play();

        Color light_colour = new Color(1.0f, 1.0f, 0.9f, 1.0f);
        SetupLights(m_CentreModels, 6.0f, light_colour);
        SetupLights(m_NorthModels, 0.0f, Color.black);
        SetupLights(m_EastModels, 0.0f, Color.black);
        SetupLights(m_SouthModels, 0.0f, Color.black);
        SetupLights(m_WestModels, 0.0f, Color.black);

        // --- Debug init ---
        // Start the game without the loc panel
        /*
        gameState = GameStates.Undefined;
        GameObject.Find("LocalizationMenu").gameObject.SetActive(false); // Hide loc
        m_codes.SetActive(true);
        InitializeNewGame(startingSeed); // for debug purpose we always start with the same seed
        //*/

        // To debug: Place feeders in front at start and render them (in Exit room)
        /*
        ElemReceiver rA = m_GroupElements.transform.Find("CubeFeedA").GetComponent<ElemReceiver>();
        rA.transform.localPosition = new Vector3(-0.1f, 0.0f, 0.3f);
        rA.transform.GetComponent<MeshRenderer>().enabled = true;
        ElemReceiver rB = m_GroupElements.transform.Find("CubeFeedB").GetComponent<ElemReceiver>();
        rB.transform.localPosition = new Vector3(0.1f, 0.0f, 0.3f);
        rB.transform.GetComponent<MeshRenderer>().enabled = true;
        ElemReceiver rC = m_GroupElements.transform.Find("CubeFeedC").GetComponent<ElemReceiver>();
        rC.transform.localPosition = new Vector3(0.3f, 0.0f, 0.3f);
        rC.transform.GetComponent<MeshRenderer>().enabled = true;
        ElemReceiver rD = m_GroupElements.transform.Find("CubeFeedD").GetComponent<ElemReceiver>();
        rD.transform.localPosition = new Vector3(-0.3f, 0.0f, 0.3f);
        rD.transform.GetComponent<MeshRenderer>().enabled = true;
        */
    }


    // Return the time at which the game started with the current seed in seconds
    public float GetGameStartTime()
    {
        return startingTime;
    }


    // Get cell by its id
    public OneCellClass GetCellById(int uid)
    {
        return allCells[uid];
    }


    // Get cell pos on the chessboard by its id
    public int GetCellPosById(int uid)
    {
        foreach (int pos in lookupTab)
        {
            if (pos == uid)
            {
                return lookupTab.IndexOf(pos);
            }
        }
        return 0;
    }


    // Get cell by its position on the chessboard
    public OneCellClass GetCellByPos(int pos)
    {
        return allCells[lookupTab[pos]];
    }


    // Return the cell the player is in
    public OneCellClass GetCurrentCell()
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

#if UNITY_EDITOR
        GameObject cam = GameObject.Find("OVRCameraRig").gameObject;
        cam.transform.position = new Vector3(0.0f, 1.3f, -0.5f);
#endif

        if (m_ScreenCard != null)
        {
            m_ScreenCard.Clear();
            m_ScreenCard = null;
        }

        if (m_Language == GameLanguages.Undefined)
        {
            // Set default language to French
            m_Language = GameLanguages.French;
        }
        else
        {
            // Load language from file
        }

        m_MenuArea1.SetActive(false);
        m_PlayaModel.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        m_PlayaModel.SetActive(false);
        m_EndingLights.SetActive(false);
        m_LocMenu.m_TransitionStarted = false;

        bool playFirstVoice = false;
        if (startingSeed != gameSeed)
        {
            // New game
            startingSeed = gameSeed;
            startingTime = Time.fixedTime;
            playFirstVoice = true;
            m_nextSoundTime = startingTime + 60.0f + m_RandomBis.Next(5) * 15.0f;
            m_nextSoundId = m_RandomBis.Next(5);
        }
        UnityEngine.Random.InitState(startingSeed);

        int firstRandom = (int)(UnityEngine.Random.value * 100.0f);
        Debug.Log($"[GameMgr][{startingTime}s] new game initialized with seed {startingSeed}, first random {firstRandom}/19.");

        playerCellId = 12; // replace the player in the middle

        // Reserve at least one of the adjacent starting room for a safe one
        int reserveSafe = (int)(UnityEngine.Random.value * 3.0f);
        if (reserveSafe == 0)
            reserveSafe = 7;
        else if (reserveSafe == 1)
            reserveSafe = 11;
        else if (reserveSafe == 2)
            reserveSafe = 13;
        else
            reserveSafe = 17;

        int reserveExit = 24; // make sure we'll always have a valid exit
        List<int> deadly = new List<int>(deadlyCellNb);
        while (deadly.Count < deadlyCellNb)
        {
            int id = (int)(UnityEngine.Random.value * 24.0f);
            if ((deadly.Contains(id)) || (id == 12) || (id == reserveExit) || (id == reserveSafe))
            {
                continue;
            }

            deadly.Add(id);
        }
#if UNITY_EDITOR
        string toto = "";
        foreach (int id in deadly)
        {
            toto += id.ToString() + " ";
        }
        Debug.Log($"[GameMgr] deadly {toto}");
#endif

        List<int> effectCells = new List<int>(effectCellNb);
        while (effectCells.Count < effectCellNb)
        {
            int id = (int)(UnityEngine.Random.value * 24.0f);
            if ((id == 12) || deadly.Contains(id) || effectCells.Contains(id) || (id == reserveExit) || (id == reserveSafe))
            {
                continue;
            }

            effectCells.Add(id);
        }

#if UNITY_EDITOR
        toto = "";
        foreach(int id in effectCells)
        {
            toto += id.ToString() + " ";
        }
        Debug.Log($"[GameMgr] effectCells {toto}");
#endif

        // Init a board
        bool exitChosen = false;
        int exitCount = 0;
        int safeNb = 0;

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
                    if (((i != 2) && (j != 2)) && ((i == 0) || (j == 0) || (i == 4) || (j == 4)))
                    {
                        exitCount++;
                        if ((exitCount >= (int)(aRndNb * 10.0f)) || (id == 24))
                        {
                            cell.InitCell(CellTypes.Exit, 0, aRndNb);
                            exitChosen = true;
                            continue;
                        }
                    }
                }

                cell.InitCell(CellTypes.Safe, safeNb++, aRndNb);
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
            m_CentreModels.m_OneLookCell = GameObject.Instantiate(m_OneLookCellModel);
            m_CentreModels.m_OneLookCell.transform.SetParent(m_CentreModels.transform);
            m_CentreModels.m_FireCell = GameObject.Instantiate(m_FireCellModel);
            m_CentreModels.m_FireCell.transform.SetParent(m_CentreModels.transform);
            m_CentreModels.m_VortexCell = GameObject.Instantiate(m_VortexModel);
            m_CentreModels.m_VortexCell.transform.SetParent(m_CentreModels.transform);
            m_CentreModels.m_TunnelCell = GameObject.Instantiate(m_TunnelCellModel);
            m_CentreModels.m_TunnelCell.transform.SetParent(m_CentreModels.transform);

            // Save initial water position
            Transform waterT = m_CentreModels.m_WaterCell.transform.Find("ground_all/water");
            m_waterLevel = waterT.position;
            m_waterStuffLevel = m_CentreModels.m_WaterCell.transform.Find("ground_all/water_stuff").position;
            m_FxWater.transform.parent = waterT;
            m_FxWater.transform.localPosition = m_FxWater.transform.localPosition + new Vector3(0.0f, -3.0f, 0.0f);

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
            m_NorthModels.m_OneLookCell = GameObject.Instantiate(m_OneLookCellModel);
            m_NorthModels.m_OneLookCell.transform.SetParent(m_NorthModels.transform);
            m_NorthModels.m_FireCell = GameObject.Instantiate(m_FireCellModel);
            m_NorthModels.m_FireCell.transform.SetParent(m_NorthModels.transform);
            m_NorthModels.m_VortexCell = GameObject.Instantiate(m_VortexModel);
            m_NorthModels.m_VortexCell.transform.SetParent(m_NorthModels.transform);
            m_NorthModels.m_TunnelCell = GameObject.Instantiate(m_TunnelCellModel);
            m_NorthModels.m_TunnelCell.transform.SetParent(m_NorthModels.transform);
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
            m_EastModels.m_OneLookCell = GameObject.Instantiate(m_OneLookCellModel);
            m_EastModels.m_OneLookCell.transform.SetParent(m_EastModels.transform);
            m_EastModels.m_FireCell = GameObject.Instantiate(m_FireCellModel);
            m_EastModels.m_FireCell.transform.SetParent(m_EastModels.transform);
            m_EastModels.m_VortexCell = GameObject.Instantiate(m_VortexModel);
            m_EastModels.m_VortexCell.transform.SetParent(m_EastModels.transform);
            m_EastModels.m_TunnelCell = GameObject.Instantiate(m_TunnelCellModel);
            m_EastModels.m_TunnelCell.transform.SetParent(m_EastModels.transform);
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
            m_SouthModels.m_OneLookCell = GameObject.Instantiate(m_OneLookCellModel);
            m_SouthModels.m_OneLookCell.transform.SetParent(m_SouthModels.transform);
            m_SouthModels.m_FireCell = GameObject.Instantiate(m_FireCellModel);
            m_SouthModels.m_FireCell.transform.SetParent(m_SouthModels.transform);
            m_SouthModels.m_VortexCell = GameObject.Instantiate(m_VortexModel);
            m_SouthModels.m_VortexCell.transform.SetParent(m_SouthModels.transform);
            m_SouthModels.m_TunnelCell = GameObject.Instantiate(m_TunnelCellModel);
            m_SouthModels.m_TunnelCell.transform.SetParent(m_SouthModels.transform);
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
            m_WestModels.m_OneLookCell = GameObject.Instantiate(m_OneLookCellModel);
            m_WestModels.m_OneLookCell.transform.SetParent(m_WestModels.transform);
            m_WestModels.m_FireCell = GameObject.Instantiate(m_FireCellModel);
            m_WestModels.m_FireCell.transform.SetParent(m_WestModels.transform);
            m_WestModels.m_VortexCell = GameObject.Instantiate(m_VortexModel);
            m_WestModels.m_VortexCell.transform.SetParent(m_WestModels.transform);
            m_WestModels.m_TunnelCell = GameObject.Instantiate(m_TunnelCellModel);
            m_WestModels.m_TunnelCell.transform.SetParent(m_WestModels.transform);
            m_WestModels.transform.position = new Vector3(-2.9f, 0.0f, 0.0f);

            m_FxVentilo.gameObject.SetActive(true);
            m_FxVentilo.Play();
        }
        //--- show models ---

        //--- set lighting ---
        float light_range = 1.0f;
        Color light_colour = new Color(1.0f, 1.0f, 0.9f, 1.0f);
        SetupLights(m_CentreModels, light_range, light_colour);
        m_CentreModels.m_light_N.intensity = 0.1f;
        m_CentreModels.m_light_E.intensity = 0.0f;
        m_CentreModels.m_light_S.intensity = 0.0f;
        m_CentreModels.m_light_W.intensity = 0.0f;
        light_colour = Color.red;
        SetupLights(m_NorthModels, light_range, light_colour);
        SetupLights(m_EastModels, light_range, light_colour);
        SetupLights(m_SouthModels, light_range, light_colour);
        SetupLights(m_WestModels, light_range, light_colour);
        //--- set lighting ---

        // Activate the minimap in english only
        if (m_Language == GameLanguages.English)
            m_ShowMiniMap = true;
        ShowMiniMap(m_ShowMiniMap);

        // Reset water levels
        GameObject water = m_CentreModels.m_WaterCell.transform.Find("ground_all/water").gameObject;
        water.transform.position = m_waterLevel;
        GameObject waterStuff = m_CentreModels.m_WaterCell.transform.Find("ground_all/water_stuff").gameObject;
        waterStuff.transform.position = m_waterStuffLevel;

        UpdateCellsModels();
        gameState = GameStates.Running;
        OneCellClass current = GetCurrentCell();
        current.OnPlayerEnter();

        if (Audio_Bank[7].isPlaying)
        {
            Audio_Bank[7].Stop();
        }

        if (playFirstVoice == true)
        {
            AudioSource.PlayClipAtPoint(Audio_Voices[0], Vector3.up);
        }
        else if (m_DeathCount == 1)
        {
            AudioSource.PlayClipAtPoint(Audio_Voices[1], Vector3.up);
        }

        // Flickering lights
        InvokeRepeating("CheckLightState", 3.0f, 2.0f);											   
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
            //MovePlayerNorth();
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            //MovePlayerSouth();
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            //MovePlayerEast();
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            //MovePlayerWest();
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
        if (m_LocMenu.m_TransitionStarted == true)
        {
            float inc = 3.0f;
            m_CentreModels.m_light_N.intensity += Time.deltaTime * inc;
            m_CentreModels.m_light_E.intensity += Time.deltaTime * inc;
            m_CentreModels.m_light_S.intensity += Time.deltaTime * inc;
            m_CentreModels.m_light_W.intensity += Time.deltaTime * inc;
            return;
        }

        if (m_CentreModels.m_light_N.intensity < 1.0f)
        {
            float inc = 0.5f;
            m_CentreModels.m_light_N.intensity += Time.deltaTime * inc;
            m_CentreModels.m_light_E.intensity += Time.deltaTime * inc;
            m_CentreModels.m_light_S.intensity += Time.deltaTime * inc;
            m_CentreModels.m_light_W.intensity += Time.deltaTime * inc;
        }

        if (Input.GetKeyUp(KeyCode.KeypadPlus))
        {
            /*
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
            */
            m_LocMenu.ChangeLanguageSelection(m_LocMenu.m_FlagUk);
            m_LocMenu.m_StartButton.LaunchTheGame();
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

            // Check if running out of time
            //m_MaxGameLength = 6.0f; // To test
            float gameLength = Time.fixedTime - startingTime;
            if ((gameState != GameStates.CodeAllSet) && (gameLength > m_MaxGameLength))
            {
                // Fade out & go back at start
                if (gameLength < m_MaxGameLength + 2.0f)
                {
                    float intensity = 3.0f;
                    m_CentreModels.m_light_N.intensity += Time.fixedDeltaTime * intensity;
                    m_CentreModels.m_light_E.intensity += Time.fixedDeltaTime * intensity;
                    m_CentreModels.m_light_S.intensity += Time.fixedDeltaTime * intensity;
                    m_CentreModels.m_light_W.intensity += Time.fixedDeltaTime * intensity;
                }
                if (gameLength > m_MaxGameLength + 3.0f)
                {
                    Debug.Log($"-----GameOver----- after {gameLength}s");
                    gameState = GameStates.Localization;
                    m_CentreModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
                    m_NorthModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
                    m_EastModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
                    m_SouthModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
                    m_WestModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
                    m_Console_N.SetActive(false);
                    m_Console_E.SetActive(false);
                    m_Console_S.SetActive(false);
                    m_Console_W.SetActive(false);
                    m_codes.SetActive(false);
                    m_MenuArea1.SetActive(true);
                    m_LocMenu.m_StartButton.enabled = false;
                    m_LocMenu.m_FlagFr.transform.gameObject.SetActive(false);
                    m_LocMenu.m_FlagUk.transform.gameObject.SetActive(false);
                    m_LocMenu.m_GameoverMesh.SetActive(true);
                    m_LocMenu.enabled = true;
                    m_LocMenu.transform.gameObject.SetActive(true);
                    // switch light off, they will lit up in the loc update
                    m_CentreModels.m_light_N.intensity = 0.1f;
                    m_CentreModels.m_light_E.intensity = 0.1f;
                    m_CentreModels.m_light_S.intensity = 0.1f;
                    m_CentreModels.m_light_W.intensity = 0.1f;
                    GameObject intro = m_AllNotes.transform.GetChild(0).gameObject;
                    intro.GetComponent<TextMeshProUGUI>().text = m_LocalizedText["gameOver_1"];
                    StartCoroutine(TheCellGameMgr.instance.PlayDelayedClip(2.0f, 15, false)); // play voice 1 in 2sec
                }
            }

            if (gameState == GameStates.Running)
            {
                if (current.cellType == CellTypes.Deadly)
                {
                    if (m_MovingOut == false)
                    {
                        float t = Time.fixedTime - current.enterTime;
                        float secBeforeDeath = 5.0f;
                        float secDead = 2.0f;
                        if ((t > secBeforeDeath) && (current.m_DeathTriggered == false))
                        {
                            Audio_Bank[0].Play(); // play scream

                            current.m_DeathTriggered = true;
                            IncreaseDeath();
                            Debug.Log($"+++ +++ +++ Kill the player in {current.cellSubType}, go back at start Death {m_DeathCount}. DeathTime = {Time.fixedTime - TheCellGameMgr.instance.GetGameStartTime()}");
                            m_CentreModels.m_light_N.color = Color.red;
                            m_CentreModels.m_light_E.color = Color.red;
                            m_CentreModels.m_light_S.color = Color.red;
                            m_CentreModels.m_light_W.color = Color.red;
                        }
                        if ((t > secBeforeDeath) && (current.m_DeathTriggered == true)) // red fade out
                        {
                            float intensity = 3.0f;
                            m_CentreModels.m_light_N.intensity += Time.fixedDeltaTime * intensity;
                            m_CentreModels.m_light_E.intensity += Time.fixedDeltaTime * intensity;
                            m_CentreModels.m_light_S.intensity += Time.fixedDeltaTime * intensity;
                            m_CentreModels.m_light_W.intensity += Time.fixedDeltaTime * intensity;
                        }
                        if ((t > secBeforeDeath + secDead) && (current.m_DeathTriggered == true))
                        {
                            current.OnPlayerExit();
                            //StartCoroutine(PlayDelayedClip(2.0f, 15)); // play voice 1 in 2sec
                            m_FXDeathRespawn.SetActive(true);
                            TeleportToStart();
                            current.m_DeathTriggered = false;
                        }
                    }
                }
                else
                {
                    // Play a voice every x minutes
                    float playVoiceEachSec = 120.0f;
                    if (gameLength > m_nextVoiceToPlay)
                    {
                        m_nextVoiceToPlay += playVoiceEachSec + Audio_Voices[m_playedVoice].length;
                        m_playedVoice++;
                        if (m_playedVoice == 9)
                        {
                            m_playedVoice = 2;
                        }
                        //Debug.Log($"Playing voice at {gameLength}, id {m_playedVoice} / {Audio_Voices.Count}");
                        AudioSource.PlayClipAtPoint(Audio_Voices[m_playedVoice], Vector3.up);
                    }
                    UpdateAudioManager();
                }
            }
            
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
                            float brutScore = Math.Max(0.0f, m_MaxGameLength - gameDur);

                            GameObject txt1 = m_basicCanvas.transform.GetChild(1).gameObject;
                            txt1.GetComponent<TextMeshProUGUI>().text = $"Time: {gameDur}\nBrutScore: {brutScore}";
                            //count code points
                            int codePoints = CountCodePoints();
                            GameObject txt2 = m_basicCanvas.transform.GetChild(2).gameObject;
                            txt2.GetComponent<TextMeshProUGUI>().text = $"code: {codePoints}";
                            gameState = GameStates.CodeAllSet;
                            Debug.Log($"StartTime {startingTime}, end {m_EndGameTime} -> BrutScore: {brutScore}. Total: {brutScore + codePoints}");

                            // Change audio ambiance
                            if (Audio_Bank[6].isPlaying)
                            {
                                Audio_Bank[6].Stop();
                                Audio_Bank[16].Play();
                            }

                            // Change exit colour					 
                            MeshRenderer exitRender = m_CentreModels.m_ExitCell.transform.Find("trap_exit/exit_panel").gameObject.GetComponent<MeshRenderer>();
                            exitRender.material.SetColor("_EmissionColor", Color.green * 2.0f);
                            StartCoroutine(OpenExitHatch());

                            OneCellClass exitCell = GetCurrentCell();
                            //exitCell.NorthDoor.SetActive(false);
                            //exitCell.EastDoor.SetActive(false);
                            exitCell.SouthDoor.SetActive(false);
                            exitCell.WestDoor.SetActive(false);

                            // Print score
                            RectTransform rec0 = m_AllNotes.GetComponent<RectTransform>();
                            rec0.sizeDelta = new Vector2(5.0f, 2.0f);
                            rec0.SetPositionAndRotation(new Vector3(-1.0f, -0.9f, -4.27f), Quaternion.Euler(0.0f, -180.0f, 0.0f));

                            GameObject intro = m_AllNotes.transform.GetChild(0).gameObject;
                            RectTransform rec = intro.GetComponent<RectTransform>();
                            rec.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                            rec.sizeDelta = new Vector2(5.0f, 2.0f);
                            rec.transform.localPosition = Vector3.zero;
                            rec.transform.localRotation = Quaternion.identity;

                            // Compensate because I cannot set rec pos properly:
                            rec0.SetPositionAndRotation(new Vector3(-1.25f, 1.1f, -4.27f), Quaternion.Euler(0.0f, -180.0f, 0.0f));

                            TextMeshProUGUI tmp = intro.GetComponent<TextMeshProUGUI>();
                            tmp.fontSize = 0.35f;
                            tmp.color = Color.green;
                            string duration = Mathf.Floor(gameDur / 60.0f).ToString("00") + "m";
                            duration += (gameDur % 60.0f).ToString("00") + "s";
                            string timeLabel = m_LocalizedText["time"];
                            string scoreLabel = m_LocalizedText["score"];
                            string deathsLabel = m_LocalizedText["deaths"];
                            tmp.text = $"{timeLabel} {duration}\n{scoreLabel} {brutScore + codePoints}\n{deathsLabel} {m_DeathCount}";
                            m_AllNotes.enabled = true;
                        }
                    }
                    else
                    {
                        // All remaining elements should fall down
                        foreach (GameObject obj in m_ElemCubes)
                        {
                            ElemCubeClass elem = obj.GetComponent<ElemCubeClass>();
                            if (elem.m_State == 0)
                            {
                                int c = instance.m_RandomBis.Next(99);
                                if (c < 2)
                                {
                                    elem.SetUseGravity();
                                    break;
                                }
                            }
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
        //int curId = current.cellId;
        m_CentreModels.SetActiveModel(curType, current.cellSubType);
        InitDoorsScript(current, m_CentreModels.GetActiveModel());
        //Debug.Log($"UpdateCellsModels: {current}, type: {curType}, id: {curId}, playerCellId: {playerCellId}");

        // DeActivate elements
        m_GroupElements.SetActive(false);

        OneCellClass cell = GetNorth(playerCellId);
        if (cell != null)
        {
            m_NorthModels.SetActiveModel(cell.cellType, cell.cellSubType);
            UpdateCodesSections(CardinalPoint.North, cell.cellType);
            //InitDoorsScript(cell, m_NorthModels.GetActiveModel());
            //current.EnableDoorPerCardinal(CardinalPoint.North, true);
            m_CentreModels.SwitchOffScanner(1, CardinalPoint.North);
            SwitchScaners(m_NorthModels, GetCellPosById(cell.cellId));
        }
        else
        {
            m_NorthModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
            UpdateCodesSections(CardinalPoint.North, CellTypes.Undefined);
            m_CentreModels.SwitchOffScanner(2, CardinalPoint.North);
            //current.EnableDoorPerCardinal(CardinalPoint.North, false);
        }

        cell = GetEast(playerCellId);
        if (cell != null)
        {
            m_EastModels.SetActiveModel(cell.cellType, cell.cellSubType);
            UpdateCodesSections(CardinalPoint.East, cell.cellType);
            //InitDoorsScript(cell, m_EastModels.GetActiveModel());
            //current.EnableDoorPerCardinal(CardinalPoint.East, true);
            m_CentreModels.SwitchOffScanner(1, CardinalPoint.East);
            SwitchScaners(m_EastModels, GetCellPosById(cell.cellId));
        }
        else
        {
            m_EastModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
            UpdateCodesSections(CardinalPoint.East, CellTypes.Undefined);
            m_CentreModels.SwitchOffScanner(2, CardinalPoint.East);
            //current.EnableDoorPerCardinal(CardinalPoint.East, false);
        }

        cell = GetSouth(playerCellId);
        if (cell != null)
        {
            m_SouthModels.SetActiveModel(cell.cellType, cell.cellSubType);
            UpdateCodesSections(CardinalPoint.South, cell.cellType);
            //InitDoorsScript(cell, m_SouthModels.GetActiveModel());
            //current.EnableDoorPerCardinal(CardinalPoint.South, true);
            m_CentreModels.SwitchOffScanner(1, CardinalPoint.South);
            SwitchScaners(m_SouthModels, GetCellPosById(cell.cellId));
        }
        else
        {
            m_SouthModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
            UpdateCodesSections(CardinalPoint.South, CellTypes.Undefined);
            m_CentreModels.SwitchOffScanner(2, CardinalPoint.South);
            //current.EnableDoorPerCardinal(CardinalPoint.South, false);
        }

        cell = GetWest(playerCellId);
        if (cell != null)
        {
            m_WestModels.SetActiveModel(cell.cellType, cell.cellSubType);
            UpdateCodesSections(CardinalPoint.West, cell.cellType);
            //InitDoorsScript(cell, m_WestModels.GetActiveModel());
            //current.EnableDoorPerCardinal(CardinalPoint.West, true);
            m_CentreModels.SwitchOffScanner(1, CardinalPoint.West);
            SwitchScaners(m_WestModels, GetCellPosById(cell.cellId));
        }
        else
        {
            m_WestModels.SetActiveModel(CellTypes.Undefined, CellSubTypes.Empty);
            UpdateCodesSections(CardinalPoint.West, CellTypes.Undefined);
            m_CentreModels.SwitchOffScanner(2, CardinalPoint.West);
            //current.EnableDoorPerCardinal(CardinalPoint.West, false);
        }

        if (curType == CellTypes.Exit)
        {
            float light_range = 4.0f;
            Color light_colour = new Color(0.9f, 1.0f, 0.9f, 1.0f);
            SetupLights(m_CentreModels, light_range, light_colour);

			// Change exit panel colour
            MeshRenderer exitRender = m_CentreModels.m_ExitCell.transform.Find("trap_exit/exit_panel").gameObject.GetComponent<MeshRenderer>();
            exitRender.material.SetColor("_EmissionColor", Color.red);
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
        if (current.cellSubType == CellSubTypes.Blind)
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
            ActivateCellMechanism(current, m_CentreModels.m_GenCellA, true);
        }

        if (m_CentreModels.m_BlindCell.activeSelf == true)
        {
            ActivateCellMechanism(current, m_CentreModels.m_BlindCell, true);
        }

        if (m_CentreModels.m_PlanCell.activeSelf == true)
        {
            UpdatePlanScreen();
            ActivateCellMechanism(current, m_CentreModels.m_PlanCell, true);
        }

        if (m_CentreModels.m_OneLookCell.activeSelf == true)
        {
            ActivateCellMechanism(current, m_CentreModels.m_OneLookCell, true);
        }

        if (m_CentreModels.m_TunnelCell.activeSelf == true)
        {
            ActivateCellMechanism(current, m_CentreModels.m_TunnelCell, true);
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
    public void MoveRow(int from, bool onEast)
    {
        if (from == 10) // same row as the start room = impossible
        {
            Audio_Bank[12].Play();
            return;
        }

        if ((from != 0) && (from != 5) && (from != 15) && (from != 20))
        {
            Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] MoveRow should start from the beginning of a row not {from}.");
            return;
        }

        Audio_Bank[14].Play();

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
        GetCurrentCell().OnPlayerEnter(false);

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
    public void MoveColumn(int from, bool onNorth)
    {
        if (from == 2) // same column as the start room = impossible
        {
            Audio_Bank[12].Play();
            return;
        }

        if ((from != 0) && (from != 1) && (from != 3) && (from != 4))
        {
            Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] MoveColumn should start from the beginning of a column not {from}.");
            return;
        }

        Audio_Bank[14].Play();

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
        GetCurrentCell().OnPlayerEnter(false);

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
    public void SetupLightsBySubType(CellsModels model, CellSubTypes subTypes)
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
                light_colour = new Color(0.8f, 0.5f, 1.0f, 1.0f);
                break;
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


    public void TeleportToStart(bool newSeed = false)
    {
        int seed = startingSeed;
        if (newSeed)
        {
            seed = System.Environment.TickCount;
            //seed = 1966;
            //seed = 13068546;
        }
        InitializeNewGame(seed);
    }


    // Teleport the player from the current cell to the cell Id
    public void TeleportToCell(int id)
    {
        if ((id < 0) || (id >= allCells.Count))
        {
            Debug.LogWarning($"Cannot teleport player to cell id {id} !");
            return;
        }

        OneCellClass current = GetCurrentCell();
        current.OnPlayerExit();
        m_FxIllusion.SetActive(false);

        int pos = GetCellPosById(id);
        playerCellId = pos;

        current = GetCurrentCell();
        Debug.Log($"Teleport player to cell {id}: {current.name} {current.cellType} {current.cellSubType} in pos {pos}");
        current.OnPlayerEnter();

        // Update cells models to display
        UpdateCellsModels();
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
        if (m_ViewLeft <= 0)
        {
            if (GetCurrentCell().cellSubType == CellSubTypes.Blind)
            {
                if (Audio_Bank[23].isPlaying == false)
                {
                    Audio_Bank[23].Play();
                }
                m_CentreModels.SwitchOffScanner(0, CardinalPoint.North);
                m_CentreModels.SwitchOffScanner(0, CardinalPoint.East);
                m_CentreModels.SwitchOffScanner(0, CardinalPoint.South);
                m_CentreModels.SwitchOffScanner(0, CardinalPoint.West);
            }
            else
            {
                AudioSource.PlayClipAtPoint(Audio_Bank[1].clip, transform.position);
            }
            return;
        }

        // Warning alarm in OneLook at the second time
        OneCellClass cell = GetCurrentCell();
        if (cell.cellSubType == CellSubTypes.OneLook)
        {
            if (m_ViewLeft == 1)
            {
                instance.m_ViewLeft = 0;
                Audio_Bank[23].Play();
                m_CentreModels.SwitchOffScanner(0, CardinalPoint.North);
                m_CentreModels.SwitchOffScanner(0, CardinalPoint.East);
                m_CentreModels.SwitchOffScanner(0, CardinalPoint.South);
                m_CentreModels.SwitchOffScanner(0, CardinalPoint.West);
                return;
            }
        }

        switch (point)
        {
            case CardinalPoint.North:
                {
                    if (m_displayCell_N == false)
                    {
                        m_displayCell_N = true;

                        GameObject front = GetShutterPerCardinal(point, m_CentreModels);
                        GameObject back = GetShutterPerCardinal(GetOppositeCardinalPoint(point), m_NorthModels);
                        HandsPullWheel wheel = cell.GetWheelByCardinal(point);
                        if ((front != null) && (back != null))
                        {
                            StartCoroutine(OpenShutters(point, front, back, m_NorthModels, wheel));
                            m_ViewLeft--;
                        }
                        else
                        {
                            m_displayCell_N = false;
                            AudioSource snd = Audio_Bank[4];
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

                        GameObject front = GetShutterPerCardinal(point, m_CentreModels);
                        GameObject back = GetShutterPerCardinal(GetOppositeCardinalPoint(point), m_EastModels);
                        HandsPullWheel wheel = cell.GetWheelByCardinal(point);
                        if ((front != null) && (back != null))
                        {
                            StartCoroutine(OpenShutters(point, front, back, m_EastModels, wheel));
                            m_ViewLeft--;
                        }
                        else
                        {
                            m_displayCell_E = false;
                            AudioSource snd = Audio_Bank[4];
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

                        GameObject front = GetShutterPerCardinal(point, m_CentreModels);
                        GameObject back = GetShutterPerCardinal(GetOppositeCardinalPoint(point), m_SouthModels);
                        HandsPullWheel wheel = cell.GetWheelByCardinal(point);
                        if ((front != null) && (back != null))
                        {
                            StartCoroutine(OpenShutters(point, front, back, m_SouthModels, wheel));
                            m_ViewLeft--;
                        }
                        else
                        {
                            m_displayCell_S = false;
                            AudioSource snd = Audio_Bank[4];
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

                        GameObject front = GetShutterPerCardinal(point, m_CentreModels);
                        GameObject back = GetShutterPerCardinal(GetOppositeCardinalPoint(point), m_WestModels);
                        HandsPullWheel wheel = cell.GetWheelByCardinal(point);
                        if ((front != null) && (back != null))
                        {
                            StartCoroutine(OpenShutters(point, front, back, m_WestModels, wheel));
                            m_ViewLeft--;
                        }
                        else
                        {
                            m_displayCell_W = false;
                            AudioSource snd = Audio_Bank[4];
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


    // Return the shutter on point for model if any or null
    public GameObject GetShutterPerCardinal(CardinalPoint point, CellsModels model)
    {
        GameObject ret = null;

        if ((model == null) || (model.m_CurrentType == CellsModels.CellsModelsType.None))
        {
            return ret;
        }

        GameObject obj = model.GetActiveModel();
        switch (point)
        {
            case CardinalPoint.North:
                {
                    switch (model.m_CurrentType)
                    {
                        case CellsModels.CellsModelsType.Entry:
                            ret = obj.transform.Find("trap_0/trape_2").gameObject;
                            break;
                        case CellsModels.CellsModelsType.Exit:
                            ret = obj.transform.Find("Trap_1/trape_1").gameObject;
                            break;
                        case CellsModels.CellsModelsType.GenA:
                            ret = obj.transform.Find("Trap_1/trape_1").gameObject;
                            break;
                        case CellsModels.CellsModelsType.IllusionM:
                            ret = obj.transform.Find("Trap_1/trape_1").gameObject;
                            break;
                        case CellsModels.CellsModelsType.BlindM:
                            ret = obj.transform.Find("Trap_1/trape_1").gameObject;
                            break;
                        case CellsModels.CellsModelsType.WaterM:
                            ret = obj.transform.Find("Trap_1/trape_1").gameObject;
                            break;
                        case CellsModels.CellsModelsType.LaserM:
                        case CellsModels.CellsModelsType.GazM:
                        case CellsModels.CellsModelsType.PlanM:
                        case CellsModels.CellsModelsType.FireM:
                        case CellsModels.CellsModelsType.VortexM:
                        case CellsModels.CellsModelsType.TunnelM:
                            ret = obj.transform.Find("Trap_1/trape_1").gameObject;
                            break;
                        case CellsModels.CellsModelsType.OneLook:
                            ret = obj.transform.Find("Trap_1/trape_1").gameObject;
                            break;
                        default:
                            ret = obj.transform.Find("Trap_1/trape_1").gameObject;
                            break;
                    }

                    break;
                }
            case CardinalPoint.East:
                {
                    switch (model.m_CurrentType)
                    {
                        case CellsModels.CellsModelsType.Entry:
                            ret = obj.transform.Find("trap_2/trape_2 1").gameObject;
                            break;
                        case CellsModels.CellsModelsType.Exit:
                            ret = obj.transform.Find("trap_3/trape_2 1").gameObject;
                            break;
                        case CellsModels.CellsModelsType.GenA:
                            ret = obj.transform.Find("trap_3/trape_2 2").gameObject;
                            break;
                        case CellsModels.CellsModelsType.IllusionM:
                            ret = obj.transform.Find("trap_3/trape_3").gameObject;
                            break;
                        case CellsModels.CellsModelsType.BlindM:
                            ret = obj.transform.Find("trap_3/trape_2 2").gameObject;
                            break;
                        case CellsModels.CellsModelsType.WaterM:
                        case CellsModels.CellsModelsType.VortexM:
                        case CellsModels.CellsModelsType.TunnelM:
                            ret = obj.transform.Find("trap_3/trape_3").gameObject;
                            break;
                        case CellsModels.CellsModelsType.LaserM:
                        case CellsModels.CellsModelsType.GazM:
                        case CellsModels.CellsModelsType.PlanM:
                        case CellsModels.CellsModelsType.FireM:
                            ret = obj.transform.Find("trap_3/trape_2 2").gameObject;
                            break;
                        case CellsModels.CellsModelsType.OneLook:
                            ret = obj.transform.Find("trap_3/trape_2 2").gameObject;
                            break;
                        default:
                            ret = obj.transform.Find("trap_3/trape_2 2").gameObject;
                            break;
                    }

                    break;
                }
            case CardinalPoint.South:
                {
                    switch (model.m_CurrentType)
                    {
                        case CellsModels.CellsModelsType.Entry:
                            ret = obj.transform.Find("Trap_1/trape_1").gameObject;
                            break;
                        case CellsModels.CellsModelsType.Exit:
                            //ret = obj.transform.Find("trap_3/trape_2 1").gameObject;
                            break;
                        case CellsModels.CellsModelsType.GenA:
                            ret = obj.transform.Find("trap_0/trape_2").gameObject;
                            break;
                        case CellsModels.CellsModelsType.IllusionM:
                        case CellsModels.CellsModelsType.VortexM:
                        case CellsModels.CellsModelsType.TunnelM:
                            ret = obj.transform.Find("trap_0/trape_0").gameObject;
                            break;
                        case CellsModels.CellsModelsType.BlindM:
                            ret = obj.transform.Find("trap_0/trape_2").gameObject;
                            break;
                        case CellsModels.CellsModelsType.WaterM:
                            ret = obj.transform.Find("trap_0/trape_2").gameObject;
                            break;
                        case CellsModels.CellsModelsType.LaserM:
                        case CellsModels.CellsModelsType.GazM:
                        case CellsModels.CellsModelsType.PlanM:
                        case CellsModels.CellsModelsType.FireM:
                            ret = obj.transform.Find("trap_0/trape_2").gameObject;
                            break;
                        case CellsModels.CellsModelsType.OneLook:
                            ret = obj.transform.Find("trap_0/trape_2").gameObject;
                            break;
                        default:
                            ret = obj.transform.Find("trap_0/trape_2").gameObject;
                            break;
                    }

                    break;
                }
            case CardinalPoint.West:
                {
                    switch (model.m_CurrentType)
                    {
                        case CellsModels.CellsModelsType.Entry:
                            ret = obj.transform.Find("trap_3/trape_2 2").gameObject;
                            break;
                        case CellsModels.CellsModelsType.Exit:
                            ret = obj.transform.Find("trap_2/trape_2").gameObject;
                            break;
                        case CellsModels.CellsModelsType.GenA:
                            ret = obj.transform.Find("trap_2/trape_2 1").gameObject;
                            break;
                        case CellsModels.CellsModelsType.IllusionM:
                        case CellsModels.CellsModelsType.VortexM:
                        case CellsModels.CellsModelsType.TunnelM:
                            ret = obj.transform.Find("trap_2/trape_2").gameObject;
                            break;
                        case CellsModels.CellsModelsType.BlindM:
                        case CellsModels.CellsModelsType.WaterM:
                        case CellsModels.CellsModelsType.LaserM:
                        case CellsModels.CellsModelsType.GazM:
                        case CellsModels.CellsModelsType.FireM:
                            ret = obj.transform.Find("trap_2/trape_2 1").gameObject;
                            break;
                        case CellsModels.CellsModelsType.PlanM:
                            ret = obj.transform.Find("trap_2/trape_2 1").gameObject;
                            break;
                        case CellsModels.CellsModelsType.OneLook:
                            ret = obj.transform.Find("trap_2/trape_2 1").gameObject;
                            break;
                        default:
                            ret = obj.transform.Find("trap_2/trape_2 1").gameObject;
                            break;
                    }
                    break;
                }
        }

        if (ret == null)
        {
            Debug.LogWarning($"Couldn't find shutter in {model.name} on cardinal {point}, type: {model.m_CurrentType}");
            // JowTodo: Set hand scanner to red
        }

        // Save origin position of shutters
        if (model.m_ShutterPosSaved[(int)point] == false)
        {
            model.m_ShutterOriginPos[(int)point] = ret.transform.localPosition;
            model.m_ShutterPosSaved[(int)point] = true;
        }
        
        return ret;
    }


    private IEnumerator OpenShutters(CardinalPoint point, GameObject front, GameObject back, CellsModels adjModel, HandsPullWheel lockerWheel)
    {
        yield return new WaitForSecondsRealtime(0.5f);

        if ((front == null) || (back == null))
        {
            yield return 0;
        }

        if (lockerWheel == null)
        {
            Debug.LogError($"lockerWheel is null in OpenShutters {GetCurrentCell().name} {GetCurrentCell().cellType} {GetCurrentCell().cellSubType}");
            yield return 0;
        }

        //--- Snd ---
        Audio_OpenShutters.transform.SetParent(front.transform);
        Audio_OpenShutters.transform.localPosition = Vector3.zero;
        Audio_OpenShutters.Play();
        //--- Snd ---

        if (m_ViewLeft <= 0)
        {
            m_CentreModels.LitupScanner(0);
        }

        float startTime = Time.time;
        while (Time.time - startTime < 2.0f)
        {
            front.transform.position += Vector3.up * Time.fixedDeltaTime * 0.3f;
            back.transform.position += Vector3.up * Time.fixedDeltaTime * 0.3f;

            Vector3 forward = lockerWheel.GetForwardToDoor();
            lockerWheel.transform.RotateAround(lockerWheel.transform.position, forward, Time.fixedDeltaTime * 100.0f);

            yield return new WaitForFixedUpdate();
        }
        Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] {front.name} is open.");
        StartCoroutine(CloseShutters(point, front, back, adjModel, lockerWheel));
    }


    // Close the shutters after x sec
    private IEnumerator CloseShutters(CardinalPoint point, GameObject front, GameObject back, CellsModels adjModel, HandsPullWheel lockerWheel)
    {
        yield return new WaitForSecondsRealtime(5.0f);

        //--- Snd ---
        Audio_OpenShutters.Play();
        //--- Snd ---

        float duration = 2.0f;
        float startTime = Time.fixedTime;
        float endTime = startTime + duration;
        Vector3 frontOrigin = m_CentreModels.m_ShutterOriginPos[(int)point];
        CardinalPoint invCard = GetOppositeCardinalPoint(point);
        Vector3 backOrigin = adjModel.m_ShutterOriginPos[(int)invCard];

        while (Time.fixedTime < endTime)
        {
            float p = (Time.fixedTime - startTime) / (duration * 12.0f);
            front.transform.localPosition = Vector3.Lerp(front.transform.localPosition, frontOrigin, p);
            back.transform.localPosition = Vector3.Lerp(back.transform.localPosition, backOrigin, p);
            //*
            Vector3 forward = lockerWheel.GetForwardToDoor();
            p = (1.0f - p * 2.0f);
            if (p > 0.0f)
                lockerWheel.transform.RotateAround(lockerWheel.transform.position, forward, Time.fixedDeltaTime * -150.0f * p);
            //*/
            //Debug.Log($"[GameMgr][{startTime}s to {endTime}s]--- {Time.fixedTime}s {Time.fixedTime - startTime} closing {p}.");
            yield return new WaitForFixedUpdate();
        }
        Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] {front.name} is closed.");

        Audio_Bank[22].Play();

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


    public void AnimateShuttersOpen(CardinalPoint cardinal)
    {
        GameObject front = GetShutterPerCardinal(cardinal, m_CentreModels);
        CardinalPoint inverseCardinal = GetOppositeCardinalPoint(cardinal);
        GameObject back = null;
        CellsModels model = m_CentreModels;
        if (cardinal == CardinalPoint.North)
        {
            model = m_NorthModels;
            if (m_displayCell_N == true)
            {
                return;
            }
        }
        else if (cardinal == CardinalPoint.East)
        {
            model = m_EastModels;
            if (m_displayCell_E == true)
            {
                return;
            }
        }
        else if (cardinal == CardinalPoint.South)
        {
            model = m_SouthModels;
            if (m_displayCell_S == true)
            {
                return;
            }
        }
        else if (cardinal == CardinalPoint.West)
        {
            model = m_WestModels;
            if (m_displayCell_W == true)
            {
                return;
            }
        }

        back = GetShutterPerCardinal(inverseCardinal, model);
        if ((front != null) && (back != null))
        {
            OneCellClass cell = GetCurrentCell();
            HandsPullWheel wheel = cell.GetWheelByCardinal(cardinal);
            StartCoroutine(AnimateShutters(cardinal, front, back, model, wheel));
        }
    }


    IEnumerator AnimateShutters(CardinalPoint point, GameObject front, GameObject back, CellsModels backModel, HandsPullWheel lockerWheel)
    {
        if ((front == null) || (back == null))
        {
            yield return 0;
        }

        //--- Snd ---
        Audio_OpenShutters.transform.SetParent(front.transform);
        Audio_OpenShutters.transform.localPosition = Vector3.zero;
        Audio_OpenShutters.Play();
        //--- Snd ---

        //Vector3 frontPos = front.transform.localPosition;
        Vector3 frontPos = m_CentreModels.m_ShutterOriginPos[(int)point];
        Vector3 backPos = backModel.m_ShutterOriginPos[(int)point];
        //Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] {front.name} pos {frontPos}");

        float startTime = Time.time;
        while (Time.time - startTime < 2.0f)
        {
            front.transform.position += Vector3.up * Time.fixedDeltaTime * 0.3f;
            back.transform.position += Vector3.up * Time.fixedDeltaTime * 0.3f;

            Vector3 forward = lockerWheel.GetForwardToDoor();
            lockerWheel.transform.RotateAround(lockerWheel.transform.position, forward, Time.fixedDeltaTime * 100.0f);

            yield return new WaitForFixedUpdate();
        }
        Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] {front.name} is going back to {frontPos}");
        front.transform.localPosition = frontPos;
        back.transform.localPosition = backPos;
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

        m_CubesElemMats[0] = Resources.Load("Elem_exit_1", typeof(Material)) as Material;
        m_CubesElemMats[1] = Resources.Load("Elem_exite_2", typeof(Material)) as Material;
        m_CubesElemMats[2] = Resources.Load("Elem_exit_3", typeof(Material)) as Material;
        m_CubesElemMats[3] = Resources.Load("Elem_exit_4", typeof(Material)) as Material;
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

        List<int> pickedNb = new List<int>(4);
        List<GameObject> cubes = new List<GameObject>(4);
        // Pick 4 uniq elements randomly
        while (cubes.Count < 4)
        {
            int rnd = UnityEngine.Random.Range(0, m_ElemCubes.Count);
            if (pickedNb.Contains(rnd))
                continue;
            pickedNb.Add(rnd);
            GameObject c = m_ElemCubes[rnd];
            cubes.Add(c);
        }

        cubes[0].transform.SetPositionAndRotation(rA.transform.position, rA.transform.rotation);
        cubes[1].transform.SetPositionAndRotation(rB.transform.position, rB.transform.rotation);
        cubes[2].transform.SetPositionAndRotation(rC.transform.position, rC.transform.rotation);
        cubes[3].transform.SetPositionAndRotation(rD.transform.position, rD.transform.rotation);

        Transform doorT = m_CentreModels.m_ExitCell.transform.Find("trap_exit/door_exit");
        cubes[0].transform.SetParent(doorT);
        cubes[1].transform.SetParent(doorT);
        cubes[2].transform.SetParent(doorT);
        cubes[3].transform.SetParent(doorT);
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
        Audio_Bank[5].transform.SetParent(hatchModel.transform);
        Audio_Bank[5].Play();
        //--- Snd ---

        float startTime = Time.time;
        while (Time.time - startTime < 3.5f)
        {
            hatchModel.transform.RotateAround(hatchModel.transform.position, transform.up, Time.deltaTime * -35.0f);
            yield return new WaitForFixedUpdate();
        }
        Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] {hatchModel.name} is open.");
    }


    public IEnumerator RaiseWaterLevel()
    {
        GameObject water = null;
        GameObject waterStuff = null;
        water = m_CentreModels.m_WaterCell.transform.Find("ground_all/water").gameObject;
        waterStuff = m_CentreModels.m_WaterCell.transform.Find("ground_all/water_stuff").gameObject;
        if ((water == null) || (waterStuff == null))
        {
            Debug.LogWarning($"{transform.name}, {water} & {waterStuff}");
            yield return 0;
        }

        //--- Snd ---
        //Audio_Bank[5].transform.SetParent(hatchModel.transform);
        //Audio_Bank[5].Play();
        //--- Snd ---

        float startTime = Time.time;
        while (Time.time - startTime < 3.5f)
        {
            water.transform.position += new Vector3(0.0f, Time.fixedDeltaTime * 0.4f, 0.0f);
            waterStuff.transform.position += new Vector3(0.0f, Time.fixedDeltaTime * 0.4f, 0.0f);
            //Debug.Log($"raising {water.name} @ {Time.fixedTime}s.");
            yield return new WaitForFixedUpdate();
        }
        Debug.Log($"[GameMgr][{Time.fixedTime - startingTime}s] {water.name} is raised.");
    }


    // Should be called every x sec to check if a light is going off
    void CheckLightState()
    {
        int c = instance.m_RandomBis.Next(99);
        if (c < 33)
        {
            Light light = null;
            int r = (int)instance.m_RandomBis.Next(4); // [[
            if (r == 0)
                light = m_CentreModels.m_light_N;
            else if (r == 1)
                light = m_CentreModels.m_light_E;
            else if (r == 2)
                light = m_CentreModels.m_light_S;
            else if (r == 3)
                light = m_CentreModels.m_light_W;

            if (light != null)
            {
                light.intensity = 0.0f;
                StartCoroutine(SwitchLightOn(0.1f, light));
            }
            else
            {
                Debug.LogError($"Pb random = {r}");
            }
        }
    }


    // Switch light back on in x seconds
    private IEnumerator SwitchLightOn(float xSec, Light light)
    {
        yield return new WaitForSeconds(xSec);
        light.intensity = 0.6f;
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

            Debug.Log($"Localization dictionary {fileName} loaded with {m_LocalizedText.Count} entries.");

            // Just update the starting text but shouldn't need it here
            GameObject intro = m_AllNotes.transform.GetChild(0).gameObject;
            intro.GetComponent<TextMeshProUGUI>().text = m_LocalizedText["entry_room_1"];
        }
        else
        {
            Debug.LogError($"Cannot find file {filePath}.");
        }

        // Load voices
        if (m_Language != GameLanguages.Undefined)
        {
            string path = "voices_";
            if (m_Language == GameLanguages.French)
                path += "fr";
            if (m_Language == GameLanguages.English)
                path += "en";

            UnityEngine.Object[] clips;
            clips = Resources.LoadAll(path, typeof(AudioClip));
            Audio_Voices = new List<AudioClip>(clips.Length);
            foreach (var t in clips)
            {
                Audio_Voices.Add(t as AudioClip);
                //Debug.Log($"{Audio_Voices[Audio_Voices.Count-1]}");
            }
        }
    }


    // Attach and activate or deactivate moving mechanisms for one active model
    void ActivateCellMechanism(OneCellClass cell, GameObject model, bool turnOn)
    {
        if (cell.MechanismNorth.m_modelSet == false)
        {
            MechanismMove[] mecs = model.GetComponentsInChildren<MechanismMove>();
            foreach (MechanismMove mec in mecs)
            {
                //mec.enabled = true;
                switch (mec.cardinal)
                {
                    case CardinalPoint.North:
                        cell.MechanismNorth = mec;
                        cell.MechanismNorth.m_modelSet = true;
                        break;
                    case CardinalPoint.East:
                        cell.MechanismEast = mec;
                        cell.MechanismEast.m_modelSet = true;
                        break;
                    case CardinalPoint.South:
                        cell.MechanismSouth = mec;
                        cell.MechanismSouth.m_modelSet = true;
                        break;
                    case CardinalPoint.West:
                        cell.MechanismWest = mec;
                        cell.MechanismWest.m_modelSet = true;
                        break;
                    default:
                        Debug.LogWarning($"Wrong cardinal {mec.cardinal} for MechanismMove");
                        break;
                }
            }
        }

        cell.MechanismNorth.enabled = turnOn;
        cell.MechanismNorth.m_IsOn = turnOn;
        
        cell.MechanismEast.enabled = turnOn;
        cell.MechanismEast.m_IsOn = turnOn;

        cell.MechanismSouth.enabled = turnOn;
        cell.MechanismSouth.m_IsOn = turnOn;

        cell.MechanismWest.enabled = turnOn;
        cell.MechanismWest.m_IsOn = turnOn;
    }


    // retreive all pullers from models
    void InitDoorsScript(OneCellClass cell, GameObject model)
    {
        if (cell.m_DoorsInitialized)
            return;

        HandsPullWheel[] pullers = model.GetComponentsInChildren<HandsPullWheel>();
        foreach (HandsPullWheel puller in pullers)
        {
            switch (puller.m_cardinal)
            {
                case CardinalPoint.North:
                    cell.m_DoorNorth = puller;
                    cell.m_DoorNorth.enabled = false;
                    break;
                case CardinalPoint.East:
                    cell.m_DoorEast = puller;
                    cell.m_DoorEast.enabled = false;
                    break;
                case CardinalPoint.South:
                    cell.m_DoorSouth = puller;
                    cell.m_DoorSouth.enabled = false;
                    break;
                case CardinalPoint.West:
                    cell.m_DoorWest = puller;
                    cell.m_DoorWest.enabled = false;
                    break;
                default:
                    Debug.LogWarning($"Wrong cardinal {puller.m_cardinal} for HandsPullWheel");
                    break;
            }
        }

        cell.m_DoorsInitialized = true;
    }


    // Set the plan screen according to the board
    void UpdatePlanScreen()
    {
        if (m_ScreenCard == null)
        {
            m_ScreenCard = new List<GameObject>(25);

            string path = "Screen/group9/room/";
            string roomName = "";
            for (int i = 0; i < 25; ++i)
            {
                roomName = "room_" + i;
                //Debug.Log($"{path}{roomName}");
                GameObject go = m_CentreModels.m_PlanCell.transform.Find(path+roomName).gameObject;
                m_ScreenCard.Add(go);
            }
        }

        int id = 0;
        foreach (GameObject card in m_ScreenCard)
        {
            MeshRenderer mr = card.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                /*
                Color col = Color.red;
                CellTypes type = allCells[lookupTab[id]].cellType;
                if (type == CellTypes.Start)
                    col = Color.green;
                else if (type == CellTypes.Exit)
                    col = Color.cyan;
                else if (type == CellTypes.Safe)
                    col = Color.blue;
                else if (type == CellTypes.Effect)
                    col = Color.yellow;

                mr.material.SetColor("_BaseColor", col);
                */

                CellTypes type = allCells[lookupTab[id]].cellType;
                CellSubTypes subtype = allCells[lookupTab[id]].cellSubType;
                mr.material = GetScreenMat(type, subtype);

                /*
                Color col = Color.yellow;
                mr.material.EnableKeyword("_EMISSION");
                mr.material.SetColor("_EmissionColor", col);
                */
            }
            id++;
        }
    }


    // Return the good screen card material from types
    Material GetScreenMat(CellTypes type, CellSubTypes subType)
    {
        Material matos = null;
        if (m_ScreenMats.Count == 0)
            return matos;

        switch (type)
        {
            case CellTypes.Start:
                matos = m_ScreenMats[9];
                break;
            case CellTypes.Exit:
                matos = m_ScreenMats[3];
                break;
            case CellTypes.Safe:
            case CellTypes.Deadly:
            case CellTypes.Undefined:
            case CellTypes.Effect:
                {
                    switch (subType)
                    {
                        case CellSubTypes.Blind:
                            matos = m_ScreenMats[1];
                            break;
                        case CellSubTypes.Fire:
                            matos = m_ScreenMats[4];
                            break;
                        case CellSubTypes.Empty:
                            matos = m_ScreenMats[8];
                            break;
                        case CellSubTypes.Gaz:
                            matos = m_ScreenMats[5];
                            break;
                        case CellSubTypes.Illusion:
                            matos = m_ScreenMats[6];
                            break;
                        case CellSubTypes.Lasers:
                            matos = m_ScreenMats[7];
                            break;
                        case CellSubTypes.OneLook:
                            matos = m_ScreenMats[0];
                            break;
                        case CellSubTypes.Screen:
                            matos = m_ScreenMats[2];
                            break;
                        case CellSubTypes.Tunnel:
                            matos = m_ScreenMats[10];
                            break;
                        case CellSubTypes.Vortex:
                            matos = m_ScreenMats[11];
                            break;
                        case CellSubTypes.Water:
                            matos = m_ScreenMats[12];
                            break;
                    }
                }
                break;
        }

        return matos;
    }


    // Pick a random room 
    public OneCellClass PickRandomCell()
    {
        int id = instance.m_RandomBis.Next(allCells.Count);
        return allCells[id];
    }


    // Increase the number of death count
    public void IncreaseDeath()
    {
        int newDeathCount = m_DeathCount + 1;
        GameObject intro = m_AllNotes.transform.GetChild(0).gameObject;
        string txt = intro.GetComponent<TextMeshProUGUI>().text;
        switch (newDeathCount)
        {
            case 1:
                txt += "\n" + m_LocalizedText["hintDeath_1"];
                break;
            case 2:
                txt += "\n" + m_LocalizedText["hintDeath_2"];
                break;
            case 3:
                txt += "\n" + m_LocalizedText["hintDeath_3"];
                break;
            case 4:
                txt += "\n" + m_LocalizedText["hintDeath_4"];
                break;
            case 5:
                txt += "\n" + m_LocalizedText["hintDeath_5"];
                break;
            default:
                break;
        }
        intro.GetComponent<TextMeshProUGUI>().text = txt;

        m_DeathCount = newDeathCount;
    }


    public static CardinalPoint GetOppositeCardinalPoint(CardinalPoint cardinal)
    {
        switch (cardinal)
        {
            case CardinalPoint.North:
                return CardinalPoint.South;
            case CardinalPoint.East:
                return CardinalPoint.West;
            case CardinalPoint.South:
                return CardinalPoint.North;
            case CardinalPoint.West:
                return CardinalPoint.East;
        }

        return CardinalPoint.North;
    }


    // Return the tunnel linked to id
    public int GetTunnelExitId(int startId)
    {
        foreach(OneCellClass cell in allCells)
        {
            if (cell.cellSubType == CellSubTypes.Tunnel)
            {
                if (cell.cellId != startId)
                    return cell.cellId;
            }
        }

        return startId;
    }

    
    // Play a clip in sec seconds
    public IEnumerator PlayDelayedClip(float sec, int clipIndex, bool isVoice)
    {
        yield return new WaitForSecondsRealtime(sec);
        if (isVoice == true)
        {
            AudioSource.PlayClipAtPoint(Audio_Voices[clipIndex], Vector3.up);
        }
        else
        {
            Audio_Bank[clipIndex].Play();
        }
    }



    // hand=0 for left hand, 1 -> right hand, 2 -> both hands
    public void SwitchHandConsole(CardinalPoint point, bool turnOn, int hand=2)
    {
        Color colour = Color.red;
        if (turnOn)
        {
            colour = Color.green;
        }

        switch (point)
        {
            case CardinalPoint.North:
                if (hand == 0)
                    m_Console_N_Lhand.material.SetColor("_BaseColor", colour);
                else if (hand == 1)
                    m_Console_N_Rhand.material.SetColor("_BaseColor", colour);
                else
                {
                    m_Console_N_Lhand.material.SetColor("_BaseColor", colour);
                    m_Console_N_Rhand.material.SetColor("_BaseColor", colour);
                }
                break;
            case CardinalPoint.East:
                if (hand == 0)
                    m_Console_E_Lhand.material.SetColor("_BaseColor", colour);
                else if (hand == 1)
                    m_Console_E_Rhand.material.SetColor("_BaseColor", colour);
                else
                {
                    m_Console_E_Lhand.material.SetColor("_BaseColor", colour);
                    m_Console_E_Rhand.material.SetColor("_BaseColor", colour);
                }
                break;
            case CardinalPoint.South:
                if (hand == 0)
                    m_Console_S_Lhand.material.SetColor("_BaseColor", colour);
                else if (hand == 1)
                    m_Console_S_Rhand.material.SetColor("_BaseColor", colour);
                else
                {
                    m_Console_S_Lhand.material.SetColor("_BaseColor", colour);
                    m_Console_S_Rhand.material.SetColor("_BaseColor", colour);
                }
                break;
            case CardinalPoint.West:
                if (hand == 0)
                    m_Console_W_Lhand.material.SetColor("_BaseColor", colour);
                else if (hand == 1)
                    m_Console_W_Rhand.material.SetColor("_BaseColor", colour);
                else
                {
                    m_Console_W_Lhand.material.SetColor("_BaseColor", colour);
                    m_Console_W_Rhand.material.SetColor("_BaseColor", colour);
                }
                break;

        }
    }


    // Turn on/off scaners of a model based on a cellId checking for neighbours
    void SwitchScaners(CellsModels model, int fromCellId)
    {
        OneCellClass neighbours = GetNorth(fromCellId);
        if (neighbours)
            model.SwitchOffScanner(1, CardinalPoint.North);
        else
            model.SwitchOffScanner(2, CardinalPoint.North);
        neighbours = GetEast(fromCellId);
        if (neighbours)
            model.SwitchOffScanner(1, CardinalPoint.East);
        else
            model.SwitchOffScanner(2, CardinalPoint.East);
        neighbours = GetSouth(fromCellId);
        if (neighbours)
            model.SwitchOffScanner(1, CardinalPoint.South);
        else
            model.SwitchOffScanner(2, CardinalPoint.South);
        neighbours = GetWest(fromCellId);
        if (neighbours)
            model.SwitchOffScanner(1, CardinalPoint.West);
        else
            model.SwitchOffScanner(2, CardinalPoint.West);
    }


    // Hide or show mini map cheat
    void ShowMiniMap(bool show)
    {
        playerSphere.SetActive(show);

        foreach (OneCellClass cell in allCells)
        {
            cell.SmallCell.SetActive(show);
        }
    }


    // Return the game object of the lock wheel for model at point
    public GameObject GetWheelPerCardinal(CardinalPoint point, CellsModels model)
    {
        GameObject shutter = GetShutterPerCardinal(point, model);
        GameObject locker = null;
        if (shutter == null)
            return null;

        switch (point)
        {
            case CardinalPoint.North:
                locker = shutter.transform.Find("lock_1 1").gameObject;
                break;
            case CardinalPoint.East:
                locker = shutter.transform.Find("lock_1 2").gameObject;
                break;
            case CardinalPoint.South:
                locker = shutter.transform.Find("lock_1").gameObject;
                break;
            case CardinalPoint.West:
                locker = shutter.transform.Find("lock_1 3").gameObject;
                break;
        }

        return locker;
    }


    // Handle playing random sounds etc...
    void UpdateAudioManager()
    {
        float gameLength = Time.fixedTime - startingTime;

        if (gameLength > m_nextSoundTime)
        {
            int roll = m_RandomBis.Next(999);
            if (roll < 10)
            {
                // Choose a sound
                if (m_nextSoundId == 2)
                    m_nextSoundId = 3;
                else
                    m_nextSoundId = m_RandomBis.Next(3);
                m_nextSoundTime = Time.fixedTime + 30.0f + m_RandomBis.Next(5) * 15.0f; // [30..90]s
                AudioSource.PlayClipAtPoint(m_AudioRandomSounds[m_nextSoundId], Vector3.up);
                Debug.Log($"£££££££ m_nextSoundId {m_nextSoundId} will be played in {(m_nextSoundTime - gameLength) / 60.0f}m, gameLength {gameLength}s. nextSoundTime {m_nextSoundTime}s");
            }
        }
    }


    // ---
}
