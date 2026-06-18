using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using NUnit.Framework;
using JetBrains.Annotations;
using System.Collections;

public class Oxygen : MonoBehaviour
{
    [SerializeField] private Image _oxyBarSprite;

    public float currentOxygen; //might change to int;
    public float maxOxygen;
    public float tickTime;

    public bool isBreathing = true;

    public void Awake()
    {
        StartCoroutine(Tick());
        currentOxygen = maxOxygen;
    }

    public void Update()
    {   
        //temp testing keys until a proper dev console is made
        if(Keyboard.current != null)
        {
            if (Keyboard.current.numpadPlusKey.wasPressedThisFrame || Keyboard.current.equalsKey.wasPressedThisFrame)
            {
                //plus 5 oxygen
                currentOxygen += 5;
                Debug.Log("Cheat mode: +5 oxygen.");
            }
            if (Keyboard.current.numpadMinusKey.wasPressedThisFrame || Keyboard.current.minusKey.wasPressedThisFrame)
            {
                //minus 5 oxygen
                currentOxygen -= 5;
                Debug.Log("Cheat mode: -5 oxygen.");
            }
            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                Debug.Log("Cheat mode: oxygen restored.");
                RefillOxygen();   
            }
            if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                if (!isBreathing)
                {
                    isBreathing = true;
                    StartCoroutine(Tick());
                    Debug.Log("Cheat mode: started breathing. (oxygen will now deplete)");
                }
                else
                {
                    isBreathing = false;
                    Debug.Log("Cheat mode: stopped breathing. (oxygen will no longer deplete)");
                }
            }
        }

        //if oxygen ever gets depleted, game over
        if (currentOxygen <= 0 && isBreathing)
        {
            Die();
        }

        //update ui
        UpdateBar(maxOxygen, currentOxygen);
    }

    private IEnumerator Tick() //tick down oxygen
    {
        while(isBreathing)
        {
            yield return new WaitForSeconds(tickTime);
            currentOxygen -= 1;
        }
    }

    public void UpdateBar(float max, float current) //update UI
    {
        float _oxyMeter = current / max;
        _oxyBarSprite.fillAmount = _oxyMeter;
    }

    public void RefillOxygen() //quickly refill oxygen to max
    {
        currentOxygen = maxOxygen;
        //UpdateBar(maxOxygen, currentOxygen);
    }

    public void Die() //i kept this as a debug line because I assume this functionality will later be done by a game manager
    {
        isBreathing = false;
        Debug.Log("Oxygen Depleted. Game Over.");
    }
}
