using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PointOfInterest : MonoBehaviour
{
    //public readonly static HashSet<PointOfInterest> Pool = new HashSet<PointOfInterest>();

    //GameObject[] go;
    //int randomInterest;
    //GameObject newInterestGO;

    //void OnEnable()
    //{
    //    Pool.Add(this);
    //}

    //void OnDisable()
    //{
    //    Pool.Remove(this);
    //}

    void Start()
    {
        //go = GameObject.FindGameObjectsWithTag("PointOfInterest");

    }

    //public static PointOfInterest FindClosestTarget(Vector3 pos)
    //{
        //PointOfInterest result = null;
        //float dist = float.PositiveInfinity;
        //var e = Pool.GetEnumerator();

        //print("hashset count: " + Pool.Count);

        //foreach (var go in Pool)
        //{
        //    print("go: " + go);
        //}

        //// finds the closet gameobject
        //while (e.MoveNext())
        //{
        //    float d = (e.Current.transform.position - pos).sqrMagnitude;
        //    if (d < dist)
        //    {
        //        result = e.Current;
        //        dist = d;
        //    }
        //}
        ////print("returned length: " + result);
        //return result;
    //}

}
