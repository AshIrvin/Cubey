using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cubey/LevelList")]
public class LevelList : GenericList<LevelMetaData>
{
    public string pathName = "Assets/Game/Data/Chapters/Chapter00";
}