using UnityEngine;

namespace Assets.Scripts
{
    public class Preview : MonoBehaviour
    {
        public GameObject PrefabI;
        public GameObject PrefabJ;
        public GameObject PrefabL;
        public GameObject PrefabO;
        public GameObject PrefabS;
        public GameObject PrefabT;
        public GameObject PrefabZ;

        private GameObject m_tetraminoPreview;


        public void ShowPreview(TetrominoType tetrominoType)
        {
            if (m_tetraminoPreview != null) Destroy(m_tetraminoPreview);
            switch (tetrominoType)
            {
                case TetrominoType.I: m_tetraminoPreview = Instantiate(PrefabI, transform); break;
                case TetrominoType.J: m_tetraminoPreview = Instantiate(PrefabJ, transform); break;
                case TetrominoType.L: m_tetraminoPreview = Instantiate(PrefabL, transform); break;
                case TetrominoType.O: m_tetraminoPreview = Instantiate(PrefabO, transform); break;
                case TetrominoType.S: m_tetraminoPreview = Instantiate(PrefabS, transform); break;
                case TetrominoType.T: m_tetraminoPreview = Instantiate(PrefabT, transform); break;
                case TetrominoType.Z: m_tetraminoPreview = Instantiate(PrefabZ, transform); break;
            }
        }

    }
}
