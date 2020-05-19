using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OneCellClass : MonoBehaviour
{
    [ViewOnly] public int cellId = -1;
    [ViewOnly] public TheCellGameMgr.CellTypes cellType = TheCellGameMgr.CellTypes.Undefined;
    [ViewOnly] public TheCellGameMgr.CellSubTypes cellSubType = TheCellGameMgr.CellSubTypes.Empty;
    public float cellRndSource; // A random number set at init [0..1]
    public float enterTime { get; private set; }
    public GameObject SmallCell;
    public GameObject NorthDoor;
    public GameObject EastDoor;
    public GameObject SouthDoor;
    public GameObject WestDoor;
    public GameObject ExitHatch;
    public MechanismMove MechanismNorth;
    public MechanismMove MechanismEast;
    public MechanismMove MechanismSouth;
    public MechanismMove MechanismWest;

    public Vector3 m_MiniGameTranslation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // Initialize the cell
    public void InitCell(TheCellGameMgr.CellTypes type, int subId, float rnd)
    {
        if (type == TheCellGameMgr.CellTypes.Undefined)
        {
            Debug.LogError($"[OneCellClass] wrong type for init!");
            return;
        }

        cellType = type;
        cellRndSource = rnd;

        switch (type)
        {
            case TheCellGameMgr.CellTypes.Deadly:
                {
                    if ((subId == 0) || (subId == 1))
                        cellSubType = TheCellGameMgr.CellSubTypes.Fire;
                    if ((subId == 2) || (subId == 3))
                        cellSubType = TheCellGameMgr.CellSubTypes.Gaz;
                    if ((subId == 4) || (subId == 5))
                        cellSubType = TheCellGameMgr.CellSubTypes.Water;
                    if ((subId == 6) || (subId == 7))
                        cellSubType = TheCellGameMgr.CellSubTypes.Lasers;
                    if (subId == 8)
                        cellSubType = TheCellGameMgr.CellSubTypes.Illusion;
                    break;
                }
            case TheCellGameMgr.CellTypes.Effect:
                if ((subId == 0) || (subId == 1))
                    cellSubType = TheCellGameMgr.CellSubTypes.Blind;
                if ((subId == 2) || (subId == 3))
                    cellSubType = TheCellGameMgr.CellSubTypes.OneLook;
                if ((subId == 4) || (subId == 5))
                    cellSubType = TheCellGameMgr.CellSubTypes.Vortex;
                if (subId == 6)
                    cellSubType = TheCellGameMgr.CellSubTypes.Screen;
                break;
            default:
                cellSubType = TheCellGameMgr.CellSubTypes.Empty;
                break;
        }

        gameObject.SetActive(true);

        //MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        //renderer.material.color = GetColorByType();
        //transform.localScale = new Vector3(80.0f, 80.0f, 80.0f);
        //transform.localScale = new Vector3(8.0f, 8.0f, 8.0f);

        MeshRenderer renderer = SmallCell.GetComponent<MeshRenderer>();
        renderer.material.SetColor("_BaseColor", GetColorByType() * 0.75f);

        SmallCell.transform.position = m_MiniGameTranslation;

        // Now we use 3d models for moving lines of cells so set it to null, it will be dynamically assigned
        MechanismNorth.m_modelSet = false;
    }


    // called once per fixed framerate
    private void FixedUpdate()
    {

    }


    Color GetColorByType()
    {
        Color ret = Color.black;
        switch (cellType)
        {
            case TheCellGameMgr.CellTypes.Undefined:
            case TheCellGameMgr.CellTypes.Start:
                ret = Color.green;
                break;
            case TheCellGameMgr.CellTypes.Exit:
                ret = Color.cyan;
                break;
            case TheCellGameMgr.CellTypes.Safe:
                ret = Color.blue;
                break;
            case TheCellGameMgr.CellTypes.Effect:
                ret = Color.yellow;
                break;
            default:
                ret = Color.red;
                break;
        }
        ret.a = 0.5f;
        return ret;
    }


    // Draw gizmos in editor view
    protected void OnDrawGizmos()
    {
        /*
        Gizmos.color = GetColorByType();

        float size = 1.0f;
        bool wire = false;
        if (wire)
        {
            Gizmos.DrawWireCube(SmallCell.transform.position, SmallCell.transform.localScale * size);
        }
        else
        {
            Gizmos.DrawCube(SmallCell.transform.position, SmallCell.transform.localScale * size);
        }
        */
    }


    // Player just exited this cell
    public void OnPlayerExit()
    {
        NorthDoor.SetActive(false);
        EastDoor.SetActive(false);
        SouthDoor.SetActive(false);
        WestDoor.SetActive(false);
        ExitHatch.SetActive(false);
        /*
        if (MechanismNorth != null)
        {
            //Debug.Log($"================> EXIT {transform.name}//{MechanismNorth.gameObject.name}");
            MechanismNorth.gameObject.SetActive(false);
            MechanismEast.gameObject.SetActive(false);
            MechanismSouth.gameObject.SetActive(false);
            MechanismWest.gameObject.SetActive(false);
        }
        //*/

        TheCellGameMgr.instance.Audio_DeathScream[2].Play();
    }


    // Player just enter this cell
    public void OnPlayerEnter()
    {
        enterTime = Time.fixedTime;

        if (TheCellGameMgr.instance.GetNorthType(cellId) != TheCellGameMgr.CellTypes.Undefined)
        {
            NorthDoor.SetActive(true);
        }
        if (TheCellGameMgr.instance.GetEastType(cellId) != TheCellGameMgr.CellTypes.Undefined)
        {
            EastDoor.SetActive(true);
        }
        if (TheCellGameMgr.instance.GetSouthType(cellId) != TheCellGameMgr.CellTypes.Undefined)
        {
            SouthDoor.SetActive(true);
        }
        if (TheCellGameMgr.instance.GetWestType(cellId) != TheCellGameMgr.CellTypes.Undefined)
        {
            WestDoor.SetActive(true);
        }

        if (cellType == TheCellGameMgr.CellTypes.Exit)
        {
            SouthDoor.SetActive(false);
            ExitHatch.SetActive(true);
        }

        /*
        if (MechanismNorth != null)
        {
            //Debug.Log($"================> ENTER {transform.name}//{MechanismNorth.gameObject.name}");
            if ((cellType == TheCellGameMgr.CellTypes.Safe) || (cellType == TheCellGameMgr.CellTypes.Effect))
            {
                MechanismNorth.gameObject.SetActive(true);
                MechanismEast.gameObject.SetActive(true);
                MechanismSouth.gameObject.SetActive(true);
                MechanismWest.gameObject.SetActive(true);
            }
        }
        //*/

        switch (cellSubType)
        {
            //case TheCellGameMgr.CellSubTypes.Lasers: //JowTodo: Put back
            case TheCellGameMgr.CellSubTypes.Fire:
            case TheCellGameMgr.CellSubTypes.Gaz:
            //case TheCellGameMgr.CellSubTypes.Water:  //JowTodo: Put back
                StartCoroutine(DelayedDeath());
                break;
            default:
                break;
        }
    }


    private IEnumerator DelayedDeath()
    {
        AudioSource snd = TheCellGameMgr.instance.Audio_DeathScream[0];
        snd.Play();

        yield return new WaitForSecondsRealtime(3.0f);

        Debug.Log($"[OneCellClass] Kill the player sub {cellSubType}, go back at start. DeathTime = {Time.fixedTime - TheCellGameMgr.instance.GetGameStartTime()}");

        OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
        OVRScreenFade _screenFadeScript = rig.GetComponent<OVRScreenFade>();
        if (_screenFadeScript != null)
        {
            _screenFadeScript.fadeColor = new Color(0.5f, 0.0f, 0.0f);
            _screenFadeScript.FadeOut();
        }

        StartCoroutine(TeleportToStart());
    }


    private IEnumerator TeleportToStart()
    {
        yield return new WaitForSecondsRealtime(2.0f);

        OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
        OVRScreenFade _screenFadeScript = rig.GetComponent<OVRScreenFade>();
        if (_screenFadeScript != null)
        {
            _screenFadeScript.OnLevelFinishedLoading(0);
        }

        Debug.Log($"[OneCellClass] Teleport player back at start. Time = {Time.fixedTime - TheCellGameMgr.instance.GetGameStartTime()}");
        TheCellGameMgr.instance.TeleportToStart();
    }
}
