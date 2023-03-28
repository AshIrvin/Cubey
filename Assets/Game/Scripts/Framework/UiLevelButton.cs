using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Cubey/LevelButton")]
public class UiLevelButton : ScriptableObject
{
    [SerializeField] private GameObject levelButton ;

    public GameObject LevelButton => levelButton;

}
