using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
	
	public string enemyType;
	public int countMin=0, countMax=0;

    // Start is called before the first frame update
    private void Start() {
		int count = countMin > countMax? countMin : Random.Range(countMin, countMax+1);
		//Debug.Log(count);
        for (int i = 0; i < count; ++i) {
			Instantiate(Resources.Load<GameObject>("Enemies/" + enemyType), transform.position + new Vector3(0.5f, 0.5f, 0), transform.rotation);
		}
		Destroy(gameObject);
    }
}