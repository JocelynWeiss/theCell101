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
    [ViewOnly] public bool m_DeathTriggered = false;

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
        m_DeathTriggered = false;

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
                if (cellSubType == TheCellGameMgr.CellSubTypes.Vortex)
                {
                    ret = Color.grey;
                    break;
                }
                ret = Color.yellow;
                break;
            case TheCellGameMgr.CellTypes.Deadly:
                if (cellSubType == TheCellGameMgr.CellSubTypes.Illusion)
                {
                    ret = Color.grey * 0.5f;
                    break;
                }
                ret = Color.red;
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
                TheCellGameMgr.instance.m_FXDeathRespawn.SetActive(false);
                TheCellGameMgr.instance.m_FxRespawn.SetActive(false);

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
                TheCellGameMgr.instance.SetupLightsBySubType(TheCellGameMgr.instance.m_CentreModels, TheCellGameMgr.CellSubTypes.Empty);
                break;
            case TheCellGameMgr.CellSubTypes.Gaz:
                TheCellGameMgr.instance.Audio_Bank[27].Stop();
                TheCellGameMgr.instance.m_FxGaz.SetActive(false);
                TheCellGameMgr.instance.SetupLightsBySubType(TheCellGameMgr.instance.m_CentreModels, TheCellGameMgr.CellSubTypes.Empty);
                break;
            case TheCellGameMgr.CellSubTypes.Fire:
                TheCellGameMgr.instance.Audio_Bank[26].Stop();
                TheCellGameMgr.instance.m_FxFlame.SetActive(false);
                break;
            case TheCellGameMgr.CellSubTypes.Lasers:
                TheCellGameMgr.instance.m_FxLasers.SetActive(false);
                break;
            case TheCellGameMgr.CellSubTypes.Tunnel:
                m_TunnelEnabled = true; // Activate tunnel again
                break;
            case TheCellGameMgr.CellSubTypes.Water:
                TheCellGameMgr.instance.Audio_Bank[20].Stop();
                TheCellGameMgr.instance.m_FxWater.SetActive(false);
                // Reset water levels
                CellsModels curModel = TheCellGameMgr.instance.m_CentreModels;
                GameObject water = curModel.m_WaterCell.transform.Find("ground_all/water").gameObject;
                water.transform.position = TheCellGameMgr.instance.m_waterLevel;
                GameObject waterStuff = curModel.m_WaterCell.transform.Find("ground_all/water_stuff").gameObject;
                waterStuff.transform.position = TheCellGameMgr.instance.m_waterStuffLevel;
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

        /*
        CellsModels curModel = TheCellGameMgr.instance.m_CentreModels;
        GameObject shutter = TheCellGameMgr.instance.GetShutterPerCardinal(TheCellGameMgr.CardinalPoint.North, curModel);
        shutter.transform.localPosition = curModel.m_ShutterOriginPos[(int)TheCellGameMgr.CardinalPoint.North];
        shutter = TheCellGameMgr.instance.GetShutterPerCardinal(TheCellGameMgr.CardinalPoint.East, curModel);
        shutter.transform.localPosition = curModel.m_ShutterOriginPos[(int)TheCellGameMgr.CardinalPoint.East];
        shutter = TheCellGameMgr.instance.GetShutterPerCardinal(TheCellGameMgr.CardinalPoint.South, curModel);
        shutter.transform.localPosition = curModel.m_ShutterOriginPos[(int)TheCellGameMgr.CardinalPoint.South];
        shutter = TheCellGameMgr.instance.GetShutterPerCardinal(TheCellGameMgr.CardinalPoint.West, curModel);
        shutter.transform.localPosition = curModel.m_ShutterOriginPos[(int)TheCellGameMgr.CardinalPoint.West];

        CellsModels tmpModel = TheCellGameMgr.instance.m_NorthModels;
        if (tmpModel != null)
        {
            shutter = TheCellGameMgr.instance.GetShutterPerCardinal(TheCellGameMgr.CardinalPoint.South, tmpModel);
            shutter.transform.localPosition = tmpModel.m_ShutterOriginPos[(int)TheCellGameMgr.CardinalPoint.South];
        }
        tmpModel = TheCellGameMgr.instance.m_SouthModels;
        if (tmpModel != null)
        {
            shutter = TheCellGameMgr.instance.GetShutterPerCardinal(TheCellGameMgr.CardinalPoint.North, tmpModel);
            shutter.transform.localPosition = tmpModel.m_ShutterOriginPos[(int)TheCellGameMgr.CardinalPoint.North];
        }
        */


        int idOnChess = TheCellGameMgr.instance.playerCellId;
        if ((m_TunnelEnabled == false) && (cellSubType != TheCellGameMgr.CellSubTypes.Vortex))// don't activate consoles if tunnel is enabled or if in Vortex room
        {
            if (TheCellGameMgr.instance.GetNorthType(idOnChess) != TheCellGameMgr.CellTypes.Undefined)
            {
                //NorthDoor.SetActive(true);
                StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.North, true));
                //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.North, true);
            }
            else
            {
                NorthDoor.SetActive(false);
                TheCellGameMgr.instance.m_Console_N.SetActive(false);
                //StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.North, false));
            }

            if (TheCellGameMgr.instance.GetEastType(idOnChess) != TheCellGameMgr.CellTypes.Undefined)
            {
                //EastDoor.SetActive(true);
                StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.East, true));
                //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.East, true);
            }
            else
            {
                EastDoor.SetActive(false);
                TheCellGameMgr.instance.m_Console_E.SetActive(false);
                //StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.East, false));
            }

            if (TheCellGameMgr.instance.GetSouthType(idOnChess) != TheCellGameMgr.CellTypes.Undefined)
            {
                //SouthDoor.SetActive(true);
                StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.South, true));
                //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.South, true);
            }
            else
            {
                SouthDoor.SetActive(false);
                TheCellGameMgr.instance.m_Console_S.SetActive(false);
                //StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.South, false));
            }

            if (TheCellGameMgr.instance.GetWestType(idOnChess) != TheCellGameMgr.CellTypes.Undefined)
            {
                //WestDoor.SetActive(true);
                StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.West, true));
                //EnableDoorPerCardinal(TheCellGameMgr.CardinalPoint.West, true);
            }
            else
            {
                WestDoor.SetActive(false);
                TheCellGameMgr.instance.m_Console_W.SetActive(false);
                //StartCoroutine(ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint.West, false));
            }
        }

        switch (cellType)
        {
            case TheCellGameMgr.CellTypes.Exit:
                TheCellGameMgr.instance.m_PlayaModel.SetActive(true);
                TheCellGameMgr.instance.m_EndingLights.SetActive(true);
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
                StartCoroutine(StartDelayedFx());
                break;
            case TheCellGameMgr.CellSubTypes.Fire:
                TheCellGameMgr.instance.Audio_Bank[26].Play();
                StartCoroutine(StartDelayedFx());
                break;
            case TheCellGameMgr.CellSubTypes.Gaz:
                TheCellGameMgr.instance.Audio_Bank[27].Play();
                StartCoroutine(StartDelayedFx());
                break;
            case TheCellGameMgr.CellSubTypes.Water:
                TheCellGameMgr.instance.Audio_Bank[20].Play();
                TheCellGameMgr.instance.m_FxWater.SetActive(true);
                StartCoroutine(TheCellGameMgr.instance.PlayDelayedClip(2.0f, 21, false));
                StartCoroutine(TheCellGameMgr.instance.RaiseWaterLevel());
                break;
            case TheCellGameMgr.CellSubTypes.Blind:
                TheCellGameMgr.instance.m_ViewLeft = 0;
                TheCellGameMgr.instance.m_FxTopSteam.SetActive(true);
                TheCellGameMgr.instance.m_FxTeleporter.SetActive(true);
                TheCellGameMgr.instance.m_FxSpawner.SetActive(true);
                break;
            case TheCellGameMgr.CellSubTypes.Vortex:
                TheCellGameMgr.instance.m_FxTeleporter.SetActive(true);
                TheCellGameMgr.instance.m_FxSpawner.SetActive(true);
                TheCellGameMgr.instance.m_FxRespawn.SetActive(true);
                TheCellGameMgr.instance.Audio_Bank[13].Play();
                StartCoroutine(TeleportToStartWithFade(3.0f));
                TheCellGameMgr.instance.Audio_Bank[2].PlayDelayed(2.0f);
                StartCoroutine(TheCellGameMgr.instance.PlayDelayedClip(3.0f, 1, true)); // play voice 2 in 3sec
                break;
            case TheCellGameMgr.CellSubTypes.Illusion:
                TheCellGameMgr.instance.m_FxIllusion.SetActive(true);
                int id = TheCellGameMgr.instance.PickRandomCell(TheCellGameMgr.CellTypes.Exit).cellId;
                TheCellGameMgr.instance.Audio_Bank[13].Play();
                StartCoroutine(TeleportToCell(3.0f, id));
                TheCellGameMgr.instance.Audio_Bank[2].PlayDelayed(2.0f);
                break;
            case TheCellGameMgr.CellSubTypes.OneLook:
                TheCellGameMgr.instance.m_FxTopSteam.SetActive(true);
                TheCellGameMgr.instance.m_FxTeleporter.SetActive(true);
                TheCellGameMgr.instance.m_FxSpawner.SetActive(true);
                break;
            case TheCellGameMgr.CellSubTypes.Tunnel:
                TheCellGameMgr.instance.m_ViewLeft = 0;
                TheCellGameMgr.instance.m_FxTopSteam.SetActive(true);
                TheCellGameMgr.instance.m_FxTeleporter.SetActive(true);
                TheCellGameMgr.instance.m_FxSpawner.SetActive(true);
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

        TheCellGameMgr.instance.m_MovingOut = false;
        Debug.Log($"Player entering cell {name} @ {Time.fixedTime - TheCellGameMgr.instance.GetGameStartTime()}s, pos on chess = {idOnChess}, {cellType} {cellSubType}");
    }


    // [DEPRECATED] Shouldn't use this anymore as we can flee from deadly rooms
    private IEnumerator DelayedDeath(float seconds = 3.0f, bool playScream = true)
    {
        if (playScream)
        {
            AudioSource snd = TheCellGameMgr.instance.Audio_Bank[0];
            snd.Play();
        }

        yield return new WaitForSecondsRealtime(seconds);


        if (TheCellGameMgr.instance.m_MovingOut == true)
        {
            yield return null;
        }

        Debug.Log($"[OneCellClass] Kill the player sub {cellSubType}, go back at start. DeathTime = {Time.fixedTime - TheCellGameMgr.instance.GetGameStartTime()}");
        TheCellGameMgr.instance.IncreaseDeath();

        //StartCoroutine(TheCellGameMgr.instance.PlayDelayedClip(2.0f, 15)); // play voice 1 in 2sec

        StartCoroutine(TeleportToStart());

        yield return new WaitForSecondsRealtime(1.0f); // Wait another seconds before starting teleport fx

        if (cellSubType != TheCellGameMgr.CellSubTypes.Water)
        {
            TheCellGameMgr.instance.m_FXDeathRespawn.SetActive(true);
        }
    }


    // Launch an Fx after seconds based on subtypes
    private IEnumerator StartDelayedFx(float seconds = 2.0f)
    {
        yield return new WaitForSecondsRealtime(seconds);

        switch (cellSubType)
        {
            case TheCellGameMgr.CellSubTypes.Fire:
                TheCellGameMgr.instance.m_FxFlame.SetActive(true);
                break;
            case TheCellGameMgr.CellSubTypes.Lasers:
                TheCellGameMgr.instance.Audio_Bank[24].Play();
                TheCellGameMgr.instance.m_FxLasers.SetActive(true);
                break;
            case TheCellGameMgr.CellSubTypes.Gaz:
                TheCellGameMgr.instance.m_FxGaz.SetActive(true);
                break;
        }
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
        float duration = waitSec;
        float startTime = Time.fixedTime;
        float endTime = startTime + duration;
        while (Time.fixedTime < endTime)
        {
            float p = (Time.fixedTime - startTime) / duration;

            float intensity = 3.0f * p;
            TheCellGameMgr.instance.m_CentreModels.m_light_N.intensity += Time.fixedDeltaTime * intensity;
            TheCellGameMgr.instance.m_CentreModels.m_light_E.intensity += Time.fixedDeltaTime * intensity;
            TheCellGameMgr.instance.m_CentreModels.m_light_S.intensity += Time.fixedDeltaTime * intensity;
            TheCellGameMgr.instance.m_CentreModels.m_light_W.intensity += Time.fixedDeltaTime * intensity;

            //Debug.Log($"___Fade effect {startTime}s current: {Time.fixedTime}s elapsed {Time.fixedTime - startTime} p = {p}%");
            yield return new WaitForFixedUpdate();
        }
        TheCellGameMgr.instance.m_CentreModels.m_light_N.intensity = 0.6f;
        TheCellGameMgr.instance.m_CentreModels.m_light_E.intensity = 0.6f;
        TheCellGameMgr.instance.m_CentreModels.m_light_S.intensity = 0.6f;
        TheCellGameMgr.instance.m_CentreModels.m_light_W.intensity = 0.6f;

        TheCellGameMgr.instance.TeleportToCell(id);
    }


    // Teleport the player to the start cell after x seconds with a fade to white
    private IEnumerator TeleportToStartWithFade(float waitSec)
    {
        float duration = waitSec;
        float startTime = Time.fixedTime;
        float endTime = startTime + duration;
        while (Time.fixedTime < endTime)
        {
            float p = (Time.fixedTime - startTime) / duration;

            float intensity = 3.0f * p;
            TheCellGameMgr.instance.m_CentreModels.m_light_N.intensity += Time.fixedDeltaTime * intensity;
            TheCellGameMgr.instance.m_CentreModels.m_light_E.intensity += Time.fixedDeltaTime * intensity;
            TheCellGameMgr.instance.m_CentreModels.m_light_S.intensity += Time.fixedDeltaTime * intensity;
            TheCellGameMgr.instance.m_CentreModels.m_light_W.intensity += Time.fixedDeltaTime * intensity;

            //Debug.Log($"___Tunnel effect {startTime}s current: {Time.fixedTime}s elapsed {Time.fixedTime - startTime} p = {p}%");
            yield return new WaitForFixedUpdate();
        }
        TheCellGameMgr.instance.m_CentreModels.m_light_N.intensity = 0.6f;
        TheCellGameMgr.instance.m_CentreModels.m_light_E.intensity = 0.6f;
        TheCellGameMgr.instance.m_CentreModels.m_light_S.intensity = 0.6f;
        TheCellGameMgr.instance.m_CentreModels.m_light_W.intensity = 0.6f;

        Debug.Log($"[OneCellClass] Teleport player back at start. Time = {Time.fixedTime - TheCellGameMgr.instance.GetGameStartTime()}");
        OnPlayerExit();
        TheCellGameMgr.instance.TeleportToStart();
    }


    // Start the tunnel effect so the player is teleported to the other tunnel cell
    private IEnumerator ActivateTunnel(float waitSec)
    {
        TheCellGameMgr.instance.m_FxRespawn.SetActive(true);

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
        float duration = waitSec;
        float startTime = Time.fixedTime;
        float endTime = startTime + duration;
        while (Time.fixedTime < endTime)
        {
            float p = (Time.fixedTime - startTime) / duration;

            float intensity = 3.0f * p;
            TheCellGameMgr.instance.m_CentreModels.m_light_N.intensity += Time.fixedDeltaTime * intensity;
            TheCellGameMgr.instance.m_CentreModels.m_light_E.intensity += Time.fixedDeltaTime * intensity;
            TheCellGameMgr.instance.m_CentreModels.m_light_S.intensity += Time.fixedDeltaTime * intensity;
            TheCellGameMgr.instance.m_CentreModels.m_light_W.intensity += Time.fixedDeltaTime * intensity;

            //Debug.Log($"___Tunnel effect {startTime}s current: {Time.fixedTime}s elapsed {Time.fixedTime - startTime} p = {p}%");
            yield return new WaitForFixedUpdate();
        }
        Debug.Log($"Tunnel DONE {startTime}s current: {Time.fixedTime}s elapsed {Time.fixedTime - startTime}s");

        //yield return new WaitForSecondsRealtime(waitSec);
        TheCellGameMgr.instance.Audio_Bank[25].Play();
        TheCellGameMgr.instance.TeleportToCell(id);
        TheCellGameMgr.instance.m_CentreModels.m_light_N.intensity = 0.6f;
        TheCellGameMgr.instance.m_CentreModels.m_light_E.intensity = 0.6f;
        TheCellGameMgr.instance.m_CentreModels.m_light_S.intensity = 0.6f;
        TheCellGameMgr.instance.m_CentreModels.m_light_W.intensity = 0.6f;
    }


    // Deprecated
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


    // Activate door system after few secs (just shows the red hands)
    public IEnumerator ActivateDoorsByCardinal(TheCellGameMgr.CardinalPoint cardinal, bool enable, float delay = 2.0f)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (cellType == TheCellGameMgr.CellTypes.Exit)
            enable = false; // Deactivate doors in exit room

        if ((cellSubType == TheCellGameMgr.CellSubTypes.Lasers) || (cellSubType == TheCellGameMgr.CellSubTypes.Fire))
        {
            enable = false; // Deactivate doors for fire and laser room
        }

        //Debug.Log($"{name} {cardinal} set {enable} @ {Time.fixedTime}s");
        switch (cardinal)
        {
            case TheCellGameMgr.CardinalPoint.North:
                {
                    NorthDoor.SetActive(enable);
                    if (enable)
                    {
                        //StartCoroutine(DoorHandsTrigger.LitupConsole(NorthDoor));
                    }
                    TheCellGameMgr.instance.m_Console_N.SetActive(enable);
                    break;
                }
            case TheCellGameMgr.CardinalPoint.East:
                {
                    EastDoor.SetActive(enable);
                    if (enable)
                    {
                        //StartCoroutine(DoorHandsTrigger.LitupConsole(EastDoor));
                    }
                    TheCellGameMgr.instance.m_Console_E.SetActive(enable);
                    break;
                }
            case TheCellGameMgr.CardinalPoint.South:
                {
                    SouthDoor.SetActive(enable);
                    if (enable)
                    {
                        //StartCoroutine(DoorHandsTrigger.LitupConsole(SouthDoor));
                    }
                    TheCellGameMgr.instance.m_Console_S.SetActive(enable);
                    break;
                }
            case TheCellGameMgr.CardinalPoint.West:
                {
                    WestDoor.SetActive(enable);
                    if (enable)
                    {
                        //StartCoroutine(DoorHandsTrigger.LitupConsole(WestDoor));
                    }
                    TheCellGameMgr.instance.m_Console_W.SetActive(enable);
                    break;
                }
        }
    }


    // ---
    public HandsPullWheel GetWheelByCardinal(TheCellGameMgr.CardinalPoint point)
    {
        switch (point)
        {
            case TheCellGameMgr.CardinalPoint.North:
                return m_DoorNorth;
            case TheCellGameMgr.CardinalPoint.East:
                return m_DoorEast;
            case TheCellGameMgr.CardinalPoint.South:
                return m_DoorSouth;
            case TheCellGameMgr.CardinalPoint.West:
                return m_DoorWest;
            default:
                Debug.LogError($"Wrong cardinal poit to get the wheel: {point}");
                break;
        }
        return null;
    }


    // ---
    // ---
}
