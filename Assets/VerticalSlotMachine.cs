using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerticalSlotMachine : MonoBehaviour 
{
    [SerializeField] private List<WeaponScripteableObject> weapons;
    [Space]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private RectTransform itemsContainer;
    [SerializeField] private RectTransform maskArea;
    [Space]
    [SerializeField] private float startSpeed = 3000f;
    [SerializeField] private float deceleration = 800f;
    [SerializeField] private int totalIcons = 4;
    [SerializeField] private float slotSpacing = 100f;
    

    private WeaponScripteableObject selectedWeapon;
    
    public float topLimit;
    public float bottomLimit;

    private float currentSpeed;
    private bool isSpinning;

    private void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            Spin();
    }

    public void Spin()
    {
        if (isSpinning) return;
        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;

        // Limpia items anteriores
        foreach (Transform child in itemsContainer)
            Destroy(child.gameObject);

        // Crear slots espaciados verticalmente
        for (int i = 0; i < totalIcons; i++)
        {
            var randomWeapon = weapons[Random.Range(0, weapons.Count)];
            var slot = Instantiate(slotPrefab, itemsContainer).GetComponent<RectTransform>();
            slot.GetComponent<Image>().sprite = randomWeapon.icon;

            // Espaciado inicial
            slot.anchoredPosition = new Vector2(0, i * -slotSpacing);
        }

        selectedWeapon = ChooseWeaponByChance();

        currentSpeed = startSpeed;

        while (currentSpeed > 0)
        {
            foreach (Transform child in itemsContainer)
            {
                RectTransform rect = child.GetComponent<RectTransform>();

                // Mover hacia arriba
                rect.anchoredPosition += new Vector2(0, currentSpeed * Time.deltaTime);

                // Si se sale por arriba, reciclar abajo
                if (rect.anchoredPosition.y > topLimit)
                {
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, bottomLimit);

                    var randomWeapon = weapons[Random.Range(0, weapons.Count)];
                    child.GetComponent<Image>().sprite = randomWeapon.icon;

                    Debug.Log("Reapareció: " + randomWeapon.weaponName);
                }
            }

            currentSpeed -= deceleration * Time.deltaTime;
            yield return null;
        }

        isSpinning = false;
        Debug.Log("Has ganado: " + selectedWeapon.weaponName);
    }

    WeaponScripteableObject ChooseWeaponByChance()
    {
        float total = 0;
        foreach (var w in weapons)
            total += w.dropChance;

        float random = Random.value * total;
        float cumulative = 0;
        foreach (var w in weapons)
        {
            cumulative += w.dropChance;
            if (random <= cumulative)
                return w;
        }

        return weapons[weapons.Count - 1];
    }
}
