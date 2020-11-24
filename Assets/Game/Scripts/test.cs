using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{

    RaycastHit[] hits;
    List<Vector3> currentHitPositions = new List<Vector3>();
    Vector3 pos;
    bool newPosition;


    void CheckPosition()
    {
        // Loops through all the hits in the raycast array
        foreach (RaycastHit hit in hits)
        {
            // Starts the list 
            if (currentHitPositions.Count == 0)
                AddToList(pos);

            // loops through the saved positions
            for (int i = 0; i < currentHitPositions.Count; i++)
            {
                if (hit.transform.position == currentHitPositions[i])
                {
                    // position already in list
                } else
                {
                    // Add new position to list
                    pos = hit.transform.position;
                    newPosition = true;
                }
            }
        }

        // After the loops, this adds the position to the list
        if (newPosition)
        {
            AddToList(pos);
        }
    }

    // increases the size of the list to be checked against
    void AddToList(Vector3 position)
    {
        currentHitPositions.Add(position);
        newPosition = false;
    }
}
