using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour {
    public string nextSceneName;
    public Image fadeImage;

    private bool inExit = false;
    private bool exitFuncRunning = false;
    private float exitFadeSpeed = 2f;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag == "Mom") {
            inExit = true;
            if (FindObjectsOfType<EnemyAI>().Length == 0
                && collision.GetComponent<PlayerController>().holdingBaby
                && !exitFuncRunning) StartCoroutine(ExitLevel(exitFadeSpeed));
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.tag == "Mom") inExit = false;
    }

    public IEnumerator ExitLevel(float speed) {
        exitFuncRunning = true;
        //Fade to black or clear, depending on if player is in the exit area.
        while ((inExit && fadeImage.color.a < 1) || (!inExit && fadeImage.color.a > 0)) {
            float fadeAmount = fadeImage.color.a + (speed * Time.deltaTime * (inExit ? 1 : -1));
            fadeImage.color = new Color(0, 0, 0, fadeAmount);
            yield return null;
        }
        if (inExit) SceneManager.LoadScene(nextSceneName);
        exitFuncRunning = false;
    }
}