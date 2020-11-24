using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnItems : MonoBehaviour
{
    public GameObject spawnVolume;
    public GameObject item;
    public float timer = 1;

    // Update is called once per frame
    void Update()
    {
        if (spawnVolume.activeSelf)
        {
            timer -= Time.fixedDeltaTime;

            if (timer < 0)
            {
                timer = 1;
                RainItems();
            }
        }

    }

    private void RainItems()
    {
        if (spawnVolume.activeSelf)
        {
            var posX = Random.Range(-4f, 4f);
            var pos = new Vector3(posX, spawnVolume.transform.position.y, 0);

            var spawnedItem = Instantiate(item, spawnVolume.transform.parent.transform);
            spawnedItem.transform.position = pos;

        }
    }
}
