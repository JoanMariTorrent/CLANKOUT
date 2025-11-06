using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalSlotMachine : MonoBehaviour 
{
    [SerializeField] private List<WeaponScripteableObject> weapons;
    [Space]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private RectTransform  itemsContainer;
    [Space]
    [SerializeField] private float startSpeed = 3000f;
    [SerializeField] private float deceleration = 800f;
    [SerializeField] private int totalIcons = 10;

    private WeaponScripteableObject selectedWeapons;
    private float currentSpeed;
    private bool isSpinning;


    public void Spin()
    {
        if(isSpinning) return;
        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;

        // Limpia items anteriores
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < totalIcons; i++)
        {
        }
        



        yield return null;
    }


    
}