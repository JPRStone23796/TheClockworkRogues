using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

public class SCR_BossBomb : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image fillTimer;
    [SerializeField] private GameObject cannon;
    [SerializeField] private GameObject boomTextEffect;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float fadeInTimer, fadeOutTimer;
    [SerializeField] private GameObject timerParentObj;
    [SerializeField] private float ExplosionLength = 5.0f;
    [SerializeField] float damageRadius = 4.0f, maximumDamage = 50.0f;
    float detonationTimer = 0.0f;

    [Space(20)]
    [SerializeField] private AudioSource cannonFireEffect;
    [Range(0.7f,1.5f)]
    [SerializeField] private float minimumPitchValue, maximumPitchValue;

    public void StartBombTimer(float mTimer)
    {
        detonationTimer = mTimer;
        float pitchValue = Random.Range(minimumPitchValue, maximumPitchValue);
        cannonFireEffect.pitch = pitchValue;
        cannonFireEffect.Play();
        StartCoroutine(StartDetonationProcess());     
    }



    IEnumerator StartDetonationProcess()
    {
        RectTransform currentRect = fillTimer.rectTransform;
        currentRect.localScale = new Vector3(0,1,0);
        float initalTimer = detonationTimer;

        GameObject currentCannon = Instantiate(cannon, transform.position, Quaternion.identity);
        currentCannon.transform.parent = transform.parent;
        Vector3 startPos = currentCannon.transform.position + currentCannon.transform.up * 230;
        Vector3 endPos = transform.position;
        Camera mainCamera = Camera.main;
        while (detonationTimer >= 0)
        {
            detonationTimer -= Time.deltaTime;
            timerText.text = detonationTimer.ToString("f1");
            timerParentObj.transform.LookAt(mainCamera.transform.position);
            float scaleValue = 1 - (detonationTimer / initalTimer);
            currentRect.localScale = new Vector3(scaleValue,scaleValue,0);
            Vector3 current = Vector3.Lerp(startPos, endPos, scaleValue);
            currentCannon.transform.position = current;
            yield return null;
        }
        timerText.text = "";
        Destroy(currentCannon);
        StartCoroutine(FadeOutBomb());
        StartCoroutine(boomEffectStart());
    }


    IEnumerator boomEffectStart()
    {
        GameObject boomTextObject = Instantiate(boomTextEffect, transform.position, Quaternion.identity);
        GameObject currentExplostionEffect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        TextMeshProUGUI boomTextEffectUI = boomTextObject.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        currentExplostionEffect.transform.parent = transform.parent;
        Destroy(currentExplostionEffect, ExplosionLength);
        boomTextObject.transform.parent = transform.parent;
        boomTextObject.transform.localScale = Vector3.zero;






        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);
        bool player1Hit = false, player2Hit = false;
        foreach (Collider other in hitColliders)
        {
            if ((other.transform.gameObject.CompareTag("Player1") && !player1Hit) || (other.transform.gameObject.CompareTag("Player2") && !player2Hit))
            {
                float distance = Vector3.Distance(other.transform.position, transform.position);

                float damageDealt = Mathf.Clamp((1 - distance / damageRadius) * maximumDamage, 20.0f, maximumDamage);
                other.transform.GetComponent<SCR_PlayerHealth>().DamagePlayer(damageDealt);
                if (other.gameObject.CompareTag("Player1"))
                {
                    player1Hit = true;
                }
                else
                {
                    player2Hit = true;
                }
              
            }
          
        }




        Camera currentCam = Camera.main;
        float timer = fadeInTimer;
        float startingMultiplier = 20;
        float affect = Random.Range(0.92f, 0.99f);
        float endSpeedMax = Random.Range(8.2f, 16.2f);
        
        while (timer>0)
        {
            timer -= (Time.deltaTime);

            float speed = Time.deltaTime * startingMultiplier;
            boomTextObject.transform.position += new Vector3(0, speed, 0);
            startingMultiplier = Mathf.Clamp(startingMultiplier * affect, 0.2f, endSpeedMax);
            Vector3 currentScale = boomTextObject.transform.localScale;
            float scaleValue = 2 - (timer/fadeInTimer);
            currentScale.x = scaleValue;
            currentScale.y = scaleValue;
            currentScale.z = scaleValue;
            boomTextObject.transform.localScale = currentScale;
            boomTextObject.transform.LookAt(currentCam.transform.position);
            yield return null;
        }
        timer = fadeOutTimer;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            float alpha = (timer / fadeOutTimer) * 255;
            Color32 currentFace = boomTextEffectUI.faceColor;
            currentFace.a = (byte)alpha;
            boomTextEffectUI.faceColor = currentFace;
            Color32 currentOutline = boomTextEffectUI.outlineColor;
            currentOutline.a = (byte)alpha;
            boomTextEffectUI.outlineColor = currentOutline;
            boomTextObject.transform.LookAt(currentCam.transform.position);
            
            Color currentVertexColour = boomTextEffectUI.color;
            currentVertexColour.a = alpha;
            boomTextEffectUI.color = currentVertexColour;

            yield return null;
        }

       
        Destroy(boomTextObject);
        Destroy(this.gameObject);
        
    }


    IEnumerator FadeOutBomb()
    {

        float t = 1.0f;
        while (t>0)
        {
            t -= Time.deltaTime / fadeOutTimer;
            transform.localScale = new Vector3(t,1,t);
            yield return null;
        }
        transform.localScale = new Vector3(0, 1, 0);
    }
}
