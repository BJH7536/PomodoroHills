using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;

[ExecuteInEditMode]
public class targetAdder : MonoBehaviour
{
    public List<Transform> tiles;
    public CinemachineTargetGroup CinemachineTargetGroup;
    
    [ContextMenu(nameof(addTiles))]
    public void addTiles()
    {
        CinemachineTargetGroup.m_Targets = new CinemachineTargetGroup.Target[tiles.Count];
        for (int i = 0; i < tiles.Count; i++)
        {
            CinemachineTargetGroup.m_Targets[i] = new CinemachineTargetGroup.Target()
                { target = tiles[i], weight = 1, radius = 0f };
        }
    }
}
