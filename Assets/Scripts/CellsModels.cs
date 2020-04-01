using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This class contains an instance of each cell types so you can switch to display any of them
// It also has a set of lights for the given room, one on each wall
public class CellsModels : MonoBehaviour
{
    [ViewOnly] public GameObject m_EntryCell;  // The starting room
    [ViewOnly] public GameObject m_GenCellA;   // Generic Room A
    [ViewOnly] public GameObject m_GenCellB;   // Generic Room B
    [ViewOnly] public GameObject m_ExitCell;   // The exit room
    [ViewOnly] public Light m_light_N;
    [ViewOnly] public Light m_light_E;
    [ViewOnly] public Light m_light_S;
    [ViewOnly] public Light m_light_W;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
