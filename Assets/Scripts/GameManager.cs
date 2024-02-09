using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	public float babyMaxHealth = 100;
	public float babyHealth;
	public bool level = false;
	public Slider healthBar;

    private void Start() {
		babyHealth = babyMaxHealth;
	}

    private void Update() {
		if (level) {
			if(babyHealth <= 0) LoadScene("GameOver");
			else if(babyHealth > babyMaxHealth) babyHealth = babyMaxHealth;

			healthBar.value = babyHealth;
		}
    }

	public void LoadScene(string sceneName) {
		SceneManager.LoadScene(sceneName);
	}

	public void QuitGame() {
		Application.Quit();
	}
}