using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Preview : MonoBehaviour
{
    public GameObject I_Prefab;
    public GameObject J_Prefab;
    public GameObject L_Prefab;
    public GameObject O_Prefab;
    public GameObject S_Prefab;
    public GameObject T_Prefab;
    public GameObject Z_Prefab;

    private GameObject tetraminoPreview;


    public void ShowPreview(TetrominoType tetrominoType)
    {
        if (tetraminoPreview != null) Destroy(tetraminoPreview);
        switch (tetrominoType)
        {
            case TetrominoType.I: tetraminoPreview = Instantiate(I_Prefab, transform); break;
            case TetrominoType.J: tetraminoPreview = Instantiate(J_Prefab, transform); break;
            case TetrominoType.L: tetraminoPreview = Instantiate(L_Prefab, transform); break;
            case TetrominoType.O: tetraminoPreview = Instantiate(O_Prefab, transform); break;
            case TetrominoType.S: tetraminoPreview = Instantiate(S_Prefab, transform); break;
            case TetrominoType.T: tetraminoPreview = Instantiate(T_Prefab, transform); break;
            case TetrominoType.Z: tetraminoPreview = Instantiate(Z_Prefab, transform); break;
        }
    }

}
