using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	private Transform muzzle;
	private GameObject bullet;
	private AudioSource sound;
	private bool firing = false;

	public GameObject bulletPrefab;
	public float muzzleVelocity, spread, fireRate;
	public int projectileCount = 1, damage;
	public bool fullAuto;

    // Start is called before the first frame update
    private void Start() {
		muzzle = transform.GetChild(0);
		sound = GetComponent<AudioSource>();
    }

	public IEnumerator Fire(bool firedByPlayer) {
		if (!firing) {
			firing = true;
			sound.Play();
			for (int i = 0; i < projectileCount; ++i) {
				Vector3 bulletAngle = muzzle.rotation.eulerAngles + new Vector3(0, 0, Random.Range(-spread, spread));
				bullet = Instantiate(bulletPrefab, muzzle.position, Quaternion.Euler(bulletAngle));
				bullet.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(0f, muzzleVelocity));
				bullet.GetComponent<Bullet>().firedByPlayer = firedByPlayer;
				bullet.GetComponent<Bullet>().damage = damage;
			}
			yield return new WaitForSeconds(fireRate);
			firing = false;
		}
	}

	public IEnumerator PutAway() {
		GetComponent<SpriteRenderer>().enabled = false;
		firing = true; //prevents the gun from being fired while waiting for sound to stop
		yield return new WaitUntil(() => !sound.isPlaying);
		Destroy(gameObject);
	}
}

