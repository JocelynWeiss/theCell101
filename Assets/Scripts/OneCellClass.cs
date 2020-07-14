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
    [ViewOnly] public bool m_TunnelEnabled = false;

    [ViewOnly] public HandsPullWheel m_DoorNorth;
    [ViewOnly] public HandsPullWheel m_DoorEast;
    [ViewOnly] public HandsPullWheel m_DoorSouth;
    [ViewOnly] public HandsPullWheel m_DoorWest;
    [ViewOnly] public bool m_DoorsInitialized = false;


    public Vector3 m_MiniGameTranslation;


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
                if ((subId == 1) || (subId == 2))
                {
                    cellSubType = TheCellGameMgr.CellSubTypes.Tunnel;
                    m_TunnelEnabled = true;
                }
                else
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
                if (cellSubType == TheCellGameMgr.CellSubTypes.Tunnel)
                {
                    ret = Color.magenta;
                    break;
                }
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

        //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.North, false);
        //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.East, false);
        //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.South, false);
        //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.West, false);

        switch (cellType)
        {
            case TheCellGameMgr.CellTypes.Start:
                TheCellGameMgr.instance.m_AllNotes.enabled = false;
                TheCellGameMgr.instance.m_FxIllusion.SetActive(false);

                if (TheCellGameMgr.instance.Audio_Bank[6].isPlaying == false)
                {
                    TheCellGameMgr.instance.Audio_Bank[6].Play();
                }

                if (TheCellGameMgr.instance.Audio_Bank[8].isPlaying == true)
                {
                    TheCellGameMgr.instance.Audio_Bank[8].Stop();
                }

                if (TheCellGameMgr.instance.Audio_Bank[9].isPlaying == true)
                {
                    TheCellGameMgr.instance.Audio_Bank[9].Stop();
                }

                break;
            case TheCellGameMgr.CellTypes.Exit:
                TheCellGameMgr.instance.m_PlayaModel.SetActive(false);
                break;
            default:
                break;
        }

        switch (cellSubType)
        {
            case TheCellGameMgr.CellSubTypes.Blind:
                TheCellGameMgr.instance.m_StopHandScaner.SetActive(false);
                break;
            case TheCellGameMgr.CellSubTypes.Gaz:
                TheCellGameMgr.instance.m_FxGaz.SetActive(false);
                break;
            case TheCellGameMgr.CellSubTypes.Fire:
                TheCellGameMgr.instance.m_FxFlame.SetActive(false);
                break;
            case TheCellGameMgr.CellSubTypes.Lasers:
                TheCellGameMgr.instance.m_FxLasers.SetActive(false);
                break;
            case TheCellGameMgr.CellSubTypes.Tunnel:
                m_TunnelEnabled = true; // Activate tunnel again
                TheCellGameMgr.instance.m_StopHandScaner.SetActive(false);
                break;
            case TheCellGameMgr.CellSubTypes.Water:
                TheCellGameMgr.instance.Audio_Bank[20].Stop();
                break;
            default:
                break;
        }
    }


    // Player just enter this cell
    public void OnPlayerEnter(bool setTime = true)
    {
        if (setTime)
        {
            enterTime = Time.fixedTime;
        }

        TheCellGameMgr.instance.m_ViewLeft = 2;

        int idOnChess = TheCellGameMgr.instance.playerCellId;
        if ((cellSubType != TheCellGameMgr.CellSubTypes.Fire) && (cellSubType != TheCellGameMgr.CellSubTypes.Lasers)
            && (cellSubType != TheCellGameMgr.CellSubTypes.Gaz) && (cellSubType != TheCellGameMgr.CellSubTypes.Water))
        {
            if (TheCellGameMgr.instance.GetNorthType(idOnChess) != TheCellGameMgr.CellTypes.Undefined)
            {
                //NorthDoor.SetActive(true);
                StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.North, true));
                //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.North, true);
            }
            else
            {
                //NorthDoor.SetActive(false);
                StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.North, false));
                //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.North, false);
            }

            if (TheCellGameMgr.instance.GetEastType(idOnChess) != TheCellGameMgr.CellTypes.Undefined)
            {
                //EastDoor.SetActive(true);
                StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.East, true));
                //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.East, true);
            }
            else
            {
                //EastDoor.SetActive(false);
                StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.East, false));
                //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.East, false);
            }

            if (TheCellGameMgr.instance.GetSouthType(idOnChess) != TheCellGameMgr.CellTypes.Undefined)
            {
                //SouthDoor.SetActive(true);
                StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.South, true));
                //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.South, true);
            }
            else
            {
                //SouthDoor.SetActive(false);
                StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.South, false));
                //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.South, false);
            }

            if (TheCellGameMgr.instance.GetWestType(idOnChess) != TheCellGameMgr.CellTypes.Undefined)
            {
                //WestDoor.SetActive(true);
                StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.West, true));
                //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.West, true);
            }
            else
            {
                //WestDoor.SetActive(false);
                StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.West, false));
                //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.West, false);
            }
        }

        switch (cellType)
        {
            case TheCellGameMgr.CellTypes.Exit:
                //ExitHatch.SetActive(true); // CellInteract for exit isn't actif anymore
                TheCellGameMgr.instance.m_PlayaModel.SetActive(true);
                break;
            case TheCellGameMgr.CellTypes.Start:
                TheCellGameMgr.instance.m_AllNotes.enabled = true;

                if (TheCellGameMgr.instance.Audio_Bank[6].isPlaying == true)
                {
                    TheCellGameMgr.instance.Audio_Bank[6].Stop();
                }

                if (TheCellGameMgr.instance.Audio_Bank[8].isPlaying == false)
                {
                    TheCellGameMgr.instance.Audio_Bank[8].Play();
                }

                if (TheCellGameMgr.instance.Audio_Bank[9].isPlaying == false)
                {
                    TheCellGameMgr.instance.Audio_Bank[9].Play();
                }

                break;
            default:
                break;
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
            case TheCellGameMgr.CellSubTypes.Lasers:
                TheCellGameMgr.instance.m_FxLasers.SetActive(true);
                StartCoroutine(DelayedDeath());
                break;
            case TheCellGameMgr.CellSubTypes.Fire:
                TheCellGameMgr.instance.m_FxFlame.SetActive(true);
                StartCoroutine(DelayedDeath());
                break;
            case TheCellGameMgr.CellSubTypes.Gaz:
                TheCellGameMgr.instance.m_FxGaz.SetActive(true);
                StartCoroutine(DelayedDeath());
                break;
            case TheCellGameMgr.CellSubTypes.Water:
                TheCellGameMgr.instance.Audio_Bank[20].Play();
                StartCoroutine(TheCellGameMgr.instance.PlayDelayedClip(2.0f, 21));
                StartCoroutine(TheCellGameMgr.instance.RaiseWaterLevel());
                StartCoroutine(DelayedDeath(4.0f, false));
                break;
            case TheCellGameMgr.CellSubTypes.Blind:
                TheCellGameMgr.instance.m_ViewLeft = 0;
                TheCellGameMgr.instance.m_StopHandScaner.SetActive(true);
                TheCellGameMgr.instance.m_FxTopSteam.SetActive(true);
                TheCellGameMgr.instance.m_FxTeleporter.SetActive(true);
                TheCellGameMgr.instance.m_FxSpawner.SetActive(true);
                break;
            case TheCellGameMgr.CellSubTypes.Vortex:
                TheCellGameMgr.instance.m_FxTeleporter.SetActive(true);
                TheCellGameMgr.instance.m_FxSpawner.SetActive(true);
                TheCellGameMgr.instance.m_FxIllusion.SetActive(true);
                TheCellGameMgr.instance.Audio_Bank[13].Play();
                StartCoroutine(TeleportToStart());
                TheCellGameMgr.instance.Audio_Bank[2].Play(100000);
                break;
            case TheCellGameMgr.CellSubTypes.Illusion:
                TheCellGameMgr.instance.m_FxIllusion.SetActive(true);
                int id = TheCellGameMgr.instance.PickRandomCell().cellId;
                TheCellGameMgr.instance.Audio_Bank[13].Play();
                StartCoroutine(TeleportToCell(2.0f, id));
                TheCellGameMgr.instance.Audio_Bank[2].Play(100000);
                break;
            case TheCellGameMgr.CellSubTypes.OneLook:
                TheCellGameMgr.instance.m_ViewLeft = 1;
                TheCellGameMgr.instance.m_FxTopSteam.SetActive(true);
                TheCellGameMgr.instance.m_FxTeleporter.SetActive(true);
                TheCellGameMgr.instance.m_FxSpawner.SetActive(true);
                break;
            case TheCellGameMgr.CellSubTypes.Tunnel:
                TheCellGameMgr.instance.m_ViewLeft = 0;
                TheCellGameMgr.instance.m_StopHandScaner.SetActive(true);
                TheCellGameMgr.instance.m_FxTopSteam.SetActive(true);
                TheCellGameMgr.instance.m_FxTeleporter.SetActive(true);
                TheCellGameMgr.instance.m_FxSpawner.SetActive(true);
                TheCellGameMgr.instance.m_FxIllusion.SetActive(true);
                if (m_TunnelEnabled)
                {
                    StartCoroutine(ActivateTunnel(3.0f));
                }
                break;
            default:
                TheCellGameMgr.instance.m_FxTopSteam.SetActive(true);
                TheCellGameMgr.instance.m_FxTeleporter.SetActive(true);
                TheCellGameMgr.instance.m_FxSpawner.SetActive(true);
                break;
        }
    }


    private IEnumerator DelayedDeath(float seconds = 3.0f, bool playScream = true)
    {
        if (playScream)
        {
            AudioSource snd = TheCellGameMgr.instance.Audio_Bank[0];
            snd.Play();
        }

        yield return new WaitForSecondsRealtime(seconds);

        Debug.Log($"[OneCellClass] Kill the player sub {cellSubType}, go back at start. DeathTime = {Time.fixedTime - TheCellGameMgr.instance.GetGameStartTime()}");
        TheCellGameMgr.instance.IncreaseDeath();

        /*
        OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
        OVRScreenFade _screenFadeScript = rig.GetComponent<OVRScreenFade>();
        if (_screenFadeScript != null)
        {
            _screenFadeScript.fadeColor = new Color(0.5f, 0.0f, 0.0f);
            _screenFadeScript.FadeOut();
        }
        */

        StartCoroutine(TheCellGameMgr.instance.PlayDelayedClip(2.0f, 15)); // play voice 1 in 2sec

        StartCoroutine(TeleportToStart());

        if (cellSubType != TheCellGameMgr.CellSubTypes.Water)
            TheCellGameMgr.instance.m_FxIllusion.SetActive(true);
    }


    private IEnumerator TeleportToStart()
    {
        yield return new WaitForSecondsRealtime(2.0f);

        /*
        OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
        OVRScreenFade _screenFadeScript = rig.GetComponent<OVRScreenFade>();
        if (_screenFadeScript != null)
        {
            _screenFadeScript.OnLevelFinishedLoading(0);
        }
        */

        Debug.Log($"[OneCellClass] Teleport player back at start. Time = {Time.fixedTime - TheCellGameMgr.instance.GetGameStartTime()}");
        OnPlayerExit();
        TheCellGameMgr.instance.TeleportToStart();
    }


    // Teleport the player in waitSec seconds to the cell id
    private IEnumerator TeleportToCell(float waitSec, int id)
    {
        yield return new WaitForSecondsRealtime(waitSec);
        TheCellGameMgr.instance.TeleportToCell(id);
    }


    // Start the tunnel effect so the player is teleported to the other tunnel cell
    private IEnumerator ActivateTunnel(float waitSec)
    {
        int id = TheCellGameMgr.instance.GetTunnelExitId(cellId);

        // Deactivate the tunnel at tp point to avoid feedback
        TheCellGameMgr.instance.allCells[id].m_TunnelEnabled = false;

        /*
        int pos = TheCellGameMgr.instance.GetCellPosById(cellId);
        Debug.Log($"_____________{name} UID:{cellId} in pos {pos}");
        OneCellClass dest = TheCellGameMgr.instance.GetCellById(id);
        pos = TheCellGameMgr.instance.GetCellPosById(id);
        Debug.Log($"___going to {dest.name} UID:{dest.cellId} in pos {pos}");
        */

        yield return new WaitForSecondsRealtime(waitSec);
        TheCellGameMgr.instance.TeleportToCell(id);
    }


    public void EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint cardinal, bool enable)
    {
        enable = false;

        switch (cardinal)
        {
            case TheCellGameMgr.CardinalPoint.North:
                {
                    if (m_DoorNorth != null)
                    {
                        m_DoorNorth.enabled = enable;
                        //Debug.Log($"{m_DoorNorth.transform.parent.parent.name}//{m_DoorNorth.name} set {enable} @ {Time.fixedTime}s");
                    }
                    break;
                }
            case TheCellGameMgr.CardinalPoint.East:
                {
                    if (m_DoorEast != null)
                    {
                        m_DoorEast.enabled = enable;
                        //Debug.Log($"{m_DoorEast.transform.parent.parent.name}//{m_DoorEast.name} set {enable} @ {Time.fixedTime}s");
                    }
                    break;
                }
            case TheCellGameMgr.CardinalPoint.South:
                {
                    if (m_DoorSouth != null)
                    {
                        m_DoorSouth.enabled = enable;
                        //Debug.Log($"{m_DoorSouth.transform.parent.parent.name}//{m_DoorSouth.name} set {enable} @ {Time.fixedTime}s");
                    }
                    break;
                }
            case TheCellGameMgr.CardinalPoint.West:
                {
                    if (m_DoorWest != null)
                    {
                        m_DoorWest.enabled = enable;
                        //Debug.Log($"{m_DoorWest.transform.parent.parent.name}//{m_DoorWest.name} set {enable} @ {Time.fixedTime}s");
                    }
                    break;
                }
        }
    }


    // Activate door system after few secs
    IEnumerator ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint cardinal, bool enable)
    {
        yield return new WaitForSecondsRealtime(2.0f);

        if (cellType == TheCellGameMgr.CellTypes.Exit)
            enable = false; // Deactivate doors in exit room

        switch (cardinal)
        {
            case TheCellGameMgr.CardinalPoint.North:
                {
                    NorthDoor.SetActive(enable);
                    if (enable)
                    {
                        StartCoroutine(DoorHandsTrigger.LitupConsole(NorthDoor));
                    }
                    break;
                }
            case TheCellGameMgr.CardinalPoint.East:
                {
                    EastDoor.SetActive(enable);
                    if (enable)
                    {
                        StartCoroutine(DoorHandsTrigger.LitupConsole(EastDoor));
                    }
                    break;
                }
            case TheCellGameMgr.CardinalPoint.South:
                {
                    SouthDoor.SetActive(enable);
                    if (enable)
                    {
                        StartCoroutine(DoorHandsTrigger.LitupConsole(SouthDoor));
                    }
                    break;
                }
            case TheCellGameMgr.CardinalPoint.West:
                {
                    WestDoor.SetActive(enable);
                    if (enable)
                    {
                        StartCoroutine(DoorHandsTrigger.LitupConsole(WestDoor));
                    }
                    break;
                }
        }
    }


    // ---
    // ---
}
