using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This class contains an instance of each cell types so you can switch to display any of them
// It also has a set of lights for the given room, one on each wall
public class CellsModels : MonoBehaviour
{
    // Cells models type
    public enum CellsModelsType
    {
        None,
        Entry,
        GenA,
        GenB,
        Exit,
        BlindM,
        IllusionM,
        WaterM,
        LaserM,
        GazM,
        PlanM,
        OneLook,
    }

    [ViewOnly] public GameObject m_EntryCell;  // The starting room
    [ViewOnly] public GameObject m_GenCellA;   // Generic Room A
    [ViewOnly] public GameObject m_GenCellB;   // Generic Room B
    [ViewOnly] public GameObject m_ExitCell;   // The exit room
    [ViewOnly] public GameObject m_BlindCell;  // The blind cell model
    [ViewOnly] public GameObject m_IllusionCell;  // The illusion cell model
    [ViewOnly] public GameObject m_WaterCell;  // The water cell model
    [ViewOnly] public GameObject m_LaserCell;   // The laser cell model
    [ViewOnly] public GameObject m_GazCell;     // The gaz cell model
    [ViewOnly] public GameObject m_PlanCell;    // The screen cell model
    [ViewOnly] public GameObject m_OneLookCell; // The screen cell model
    [ViewOnly] public Light m_light_N;
    [ViewOnly] public Light m_light_E;
    [ViewOnly] public Light m_light_S;
    [ViewOnly] public Light m_light_W;
    [ViewOnly] public CellsModelsType m_CurrentType = CellsModelsType.None;
    [ViewOnly] public HandScanTrigger[] m_Scaners; // A list of scaners in the current model, actualized each move


    // ---
    public GameObject GetActiveModel()
    {
        GameObject obj = null;
        switch (m_CurrentType)
        {
            case CellsModelsType.None:
            default:
                break;
            case CellsModelsType.Entry:
                obj = m_EntryCell;
                break;
            case CellsModelsType.GenA:
                obj = m_GenCellA;
                break;
            case CellsModelsType.GenB:
                obj = m_GenCellB;
                break;
            case CellsModelsType.Exit:
                obj = m_ExitCell;
                break;
            case CellsModelsType.BlindM:
                obj = m_BlindCell;
                break;
            case CellsModelsType.IllusionM:
                obj = m_IllusionCell;
                break;
            case CellsModelsType.WaterM:
                obj = m_WaterCell;
                break;
            case CellsModelsType.LaserM:
                obj = m_LaserCell;
                break;
            case CellsModelsType.GazM:
                obj = m_GazCell;
                break;
            case CellsModelsType.PlanM:
                obj = m_PlanCell;
                break;
            case CellsModelsType.OneLook:
                obj = m_OneLookCell;
                break;
        }

        return obj;
    }


    public void SetActiveModel(TheCellGameMgr.CellTypes cellType, TheCellGameMgr.CellSubTypes subType)
    {
        switch (cellType)
        {
            case TheCellGameMgr.CellTypes.Undefined:
                SetActiveModel(CellsModelsType.None);
                break;
            case TheCellGameMgr.CellTypes.Start:
                SetActiveModel(CellsModelsType.Entry);
                break;
            case TheCellGameMgr.CellTypes.Exit:
                SetActiveModel(CellsModelsType.Exit);
                break;
            case TheCellGameMgr.CellTypes.Deadly:
            case TheCellGameMgr.CellTypes.Effect:
            case TheCellGameMgr.CellTypes.Safe:
                {
                    switch (subType)
                    {
                        case TheCellGameMgr.CellSubTypes.Blind:
                            SetActiveModel(CellsModelsType.BlindM);
                            break;
                        case TheCellGameMgr.CellSubTypes.Illusion:
                            SetActiveModel(CellsModelsType.IllusionM);
                            break;
                        case TheCellGameMgr.CellSubTypes.Vortex:
                            SetActiveModel(CellsModelsType.IllusionM);
                            break;
                        case TheCellGameMgr.CellSubTypes.OneLook:
                            SetActiveModel(CellsModelsType.OneLook);
                            break;
                        case TheCellGameMgr.CellSubTypes.Water:
                            SetActiveModel(CellsModelsType.WaterM);
                            break;
                        case TheCellGameMgr.CellSubTypes.Lasers:
                            SetActiveModel(CellsModelsType.LaserM);
                            break;
                        case TheCellGameMgr.CellSubTypes.Gaz:
                            SetActiveModel(CellsModelsType.GazM);
                            break;
                        case TheCellGameMgr.CellSubTypes.Screen:
                            SetActiveModel(CellsModelsType.PlanM);
                            break;
                        default:
                            SetActiveModel(CellsModelsType.GenA);
                            break;
                    }
                    break;
                }
        }
    }


    // Set active current model 
    void SetActiveModel(CellsModelsType newType)
    {
        if (m_CurrentType == newType)
        {
            GameObject cur = GetActiveModel();
            if (cur != null)
            {
                m_Scaners = cur.GetComponentsInChildren<HandScanTrigger>();
                LitupScanner(true);
            }
            return;
        }

        if (m_EntryCell)
            m_EntryCell.SetActive(false);
        if (m_GenCellA)
            m_GenCellA.SetActive(false);
        if (m_GenCellB)
            m_GenCellB.SetActive(false);
        if (m_ExitCell)
            m_ExitCell.SetActive(false);
        if (m_BlindCell)
            m_BlindCell.SetActive(false);
        if (m_IllusionCell)
            m_IllusionCell.SetActive(false);
        if (m_WaterCell)
            m_WaterCell.SetActive(false);
        if (m_LaserCell)
            m_LaserCell.SetActive(false);
        if (m_GazCell)
            m_GazCell.SetActive(false);
        if (m_PlanCell)
            m_PlanCell.SetActive(false);
        if (m_OneLookCell)
            m_OneLookCell.SetActive(false);

        GameObject current = null;
        switch (newType)
        {
            case CellsModelsType.None:
                break;
            case CellsModelsType.Entry:
                m_EntryCell.SetActive(true);
                current = m_EntryCell;
                break;
            case CellsModelsType.GenA:
                m_GenCellA.SetActive(true);
                current = m_GenCellA;
                break;
            case CellsModelsType.GenB:
                m_GenCellB.SetActive(true);
                current = m_GenCellB;
                break;
            case CellsModelsType.Exit:
                m_ExitCell.SetActive(true);
                current = m_ExitCell;
                break;
            case CellsModelsType.BlindM:
                m_BlindCell.SetActive(true);
                current = m_BlindCell;
                break;
            case CellsModelsType.IllusionM:
                m_IllusionCell.SetActive(true);
                current = m_IllusionCell;
                break;
            case CellsModelsType.WaterM:
                m_WaterCell.SetActive(true);
                current = m_WaterCell;
                break;
            case CellsModelsType.LaserM:
                m_LaserCell.SetActive(true);
                current = m_LaserCell;
                break;
            case CellsModelsType.GazM:
                m_GazCell.SetActive(true);
                current = m_GazCell;
                break;
            case CellsModelsType.PlanM:
                m_PlanCell.SetActive(true);
                current = m_PlanCell;
                break;
            case CellsModelsType.OneLook:
                m_OneLookCell.SetActive(true);
                current = m_OneLookCell;
                break;
            default:
                Debug.LogWarning($"Wrong model type in SetActiveModel: {newType}");
                break;
        }

        if (current != null)
        {
            m_Scaners = current.GetComponentsInChildren<HandScanTrigger>();
            LitupScanner(true);
        }

        m_CurrentType = newType;
    }


    // Change the scaner's colour on/off
    public void LitupScanner(bool turnOn)
    {
        Color col = Color.red;
        if (turnOn)
        {
            col = Color.green * 2.0f;
        }

        foreach (HandScanTrigger scaner in m_Scaners)
        {
            scaner.SwitchOnOff(turnOn);
            Renderer rend = scaner.transform.GetComponent<Renderer>();
            if (rend)
            {
                rend.material.SetColor("_EmissionColor", col);
            }
        }
    }


    // Change the scaner's colour on/off
    public void SwitchOffScanner(bool turnOn, TheCellGameMgr.CardinalPoint cardinal)
    {
        Color col = Color.red;
        if (turnOn)
        {
            col = Color.green * 2.0f;
        }

        foreach (HandScanTrigger scaner in m_Scaners)
        {
            if (scaner.m_cardinal == cardinal)
            {
                scaner.SwitchOnOff(turnOn);
                Renderer rend = scaner.transform.GetComponent<Renderer>();
                if (rend)
                {
                    rend.material.SetColor("_EmissionColor", col);
                }
            }
        }
    }
}
