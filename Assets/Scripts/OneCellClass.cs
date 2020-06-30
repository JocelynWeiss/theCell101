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
    [ViewOnly] public bool m_TunnelEnabled = false; // JowNext...

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
                    cellSubType = TheCellGameMgr.CellSubTypes.Tunnel;
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
        if (TheCellGameMgr.instance.GetNorthType(idOnChess) != TheCellGameMgr.CellTypes.Undefined)
        {
            NorthDoor.SetActive(true);
        }
        else
        {
            NorthDoor.SetActive(false);
        }
        if (TheCellGameMgr.instance.GetEastType(idOnChess) != TheCellGameMgr.CellTypes.Undefined)
        {
            EastDoor.SetActive(true);
        }
        else
        {
            EastDoor.SetActive(false);
        }
        if (TheCellGameMgr.instance.GetSouthType(idOnChess) != TheCellGameMgr.CellTypes.Undefined)
        {
            SouthDoor.SetActive(true);
        }
        else
        {
            SouthDoor.SetActive(false);
        }
        if (TheCellGameMgr.instance.GetWestType(idOnChess) != TheCellGameMgr.CellTypes.Undefined)
        {
            WestDoor.SetActive(true);
        }
        else
        {
            WestDoor.SetActive(false);
        }

        switch (cellType)
        {
            case TheCellGameMgr.CellTypes.Exit:
                SouthDoor.SetActive(false);
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
                //case TheCellGameMgr.CellSubTypes.Water:  //JowTodo: Put back
                TheCellGameMgr.instance.m_FxGaz.SetActive(true);
                StartCoroutine(DelayedDeath());
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
                StartCoroutine(ActivateTunnel(3.0f));
                break;
            default:
                TheCellGameMgr.instance.m_FxTopSteam.SetActive(true);
                TheCellGameMgr.instance.m_FxTeleporter.SetActive(true);
                TheCellGameMgr.instance.m_FxSpawner.SetActive(true);
                break;
        }
    }


    private IEnumerator DelayedDeath()
    {
        AudioSource snd = TheCellGameMgr.instance.Audio_Bank[0];
        snd.Play();

        yield return new WaitForSecondsRealtime(3.0f);

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

        StartCoroutine(TeleportToStart());
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

        yield return new WaitForSecondsRealtime(waitSec);
        TheCellGameMgr.instance.TeleportToCell(id);
    }
}
