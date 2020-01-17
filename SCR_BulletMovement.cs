using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_BulletMovement : MonoBehaviour
{

    [SerializeField] private AudioSource bulletAudioSource;





    public void StartMovement(Vector3 hitPoint, Vector3 startPos, GameObject currentTrail, GameObject hitEffect,AudioClip mHitSoundEffect, float mClipPitchValue)
    {
        StartCoroutine(moveBullet(hitPoint, startPos, currentTrail, hitEffect, mHitSoundEffect, mClipPitchValue));
    }

    public IEnumerator moveBullet(Vector3 hitPoint, Vector3 startPos, GameObject currentTrail, GameObject hitEffect, AudioClip mHitSoundEffect, float mClipPitchValue)
    {
        float distance = Vector3.Distance(startPos, hitPoint);
        float timerVariable = distance / 100.0f;
        float t = 0.0f;

        while (t < timerVariable)
        {
            t += Time.deltaTime;
            currentTrail.transform.position = Vector3.Lerp(startPos, hitPoint, t / timerVariable);
            yield return null;
        }

        Destroy(currentTrail);
        GameObject hitTrail = Instantiate(hitEffect, hitPoint, Quaternion.identity);
        if (distance < 70)
        {
            bulletAudioSource.pitch = mClipPitchValue;
            bulletAudioSource.PlayOneShot(mHitSoundEffect);
        }
        Destroy(hitTrail, 0.3f);
        Destroy(this.gameObject,0.3f);
    }

}



