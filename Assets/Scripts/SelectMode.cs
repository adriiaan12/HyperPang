using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectMode : MonoBehaviour {
    public Image[] modeImages;
    public Text[] modeTexts;

    private string[] sceneNames = { "Tour_01", "Tour_02", "Tour_03", "Tour_04", "Tour_05", "Tour_06", "Tour_07", "Tour_08" };
    private int modeIndex = 0; // Índice para seleccionar el modo actual

    void Start () {
        UpdateSelection();
    }

    void Update () {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            modeIndex = (modeIndex + 1) % modeImages.Length; // Avanza al siguiente modo
            UpdateSelection();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            modeIndex = (modeIndex - 1 + modeImages.Length) % modeImages.Length; // Retrocede al anterior
            UpdateSelection();
        }
        if (Input.GetKeyDown(KeyCode.Return)) {
            SceneManager.LoadScene(sceneNames[modeIndex]); // Carga la escena correspondiente
        }
    }

    void UpdateSelection() {
        for (int i = 0; i < modeImages.Length; i++) {
            if (i == modeIndex) {
                modeImages[i].color = new Color(1, 1, 1); // Imagen seleccionada brillante
                modeTexts[i].color = new Color(1, 1, 1);
            } else {
                modeImages[i].color = new Color(1, 1, 0, 0.5f); // Imágenes no seleccionadas oscurecidas
                modeTexts[i].color = new Color(1, 1, 0, 0.5f);
            }
        }
    }
}
