using UnityEngine;

public class Cheats : MonoBehaviour
{
    [SerializeField] private ItemData spaceshipEngine;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            InventoryManager.Instance.CollectItem(spaceshipEngine, 1);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            StormManager.Instance.SetTimeToNextWave(0f);
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            StormManager.Instance.IncreaseCurrentWave();
        }
    }
}
