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
        Exit
    }

    [ViewOnly] public GameObject m_EntryCell;  // The starting room
    [ViewOnly] public GameObject m_GenCellA;   // Generic Room A
    [ViewOnly] public GameObject m_GenCellB;   // Generic Room B
    [ViewOnly] public GameObject m_ExitCell;   // The exit room
    [ViewOnly] public Light m_light_N;
    [ViewOnly] public Light m_light_E;
    [ViewOnly] public Light m_light_S;
    [ViewOnly] public Light m_light_W;
    [ViewOnly] public CellsModelsType m_CurrentType = CellsModelsType.None;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetActiveModel(TheCellGameMgr.CellTypes cellType)
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
                SetActiveModel(CellsModelsType.GenA);
                break;
        }
    }


    // Set active current model 
    void SetActiveModel(CellsModelsType newType)
    {
        if (m_CurrentType == newType)
            return;

        if (m_EntryCell)
            m_EntryCell.SetActive(false);
        if (m_GenCellA)
            m_GenCellA.SetActive(false);
        if (m_GenCellB)
            m_GenCellB.SetActive(false);
        if (m_ExitCell)
            m_ExitCell.SetActive(false);

        switch (newType)
        {
            case CellsModelsType.None:
                break;
            case CellsModelsType.Entry:
                m_EntryCell.SetActive(true);
                break;
            case CellsModelsType.GenA:
                m_GenCellA.SetActive(true);
                break;
            case CellsModelsType.GenB:
                m_GenCellB.SetActive(true);
                break;
            case CellsModelsType.Exit:
                m_ExitCell.SetActive(true);
                break;
            default:
                Debug.LogWarning($"Wrong model type in SetActiveModel: {newType}");
                break;
        }
    }
}
