using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jumparooni
{
    public class ResetScenery : MonoBehaviour
    {
        public Vector3 defaultPos;

        // Start is called before the first frame update
        void Start()
        {
            ResetSprites();
        }

        public void ResetSprites()
        {
            defaultPos = transform.position;
        }
    }
}
