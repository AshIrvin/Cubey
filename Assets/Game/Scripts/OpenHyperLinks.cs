using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class OpenHyperLinks : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI hyperLink;
    
    public void OnPointerClick(PointerEventData eventData) 
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(hyperLink, Input.mousePosition, Camera.main);
        
        if( linkIndex != -1 ) 
        { // was a link clicked?
            TMP_LinkInfo linkInfo = hyperLink.textInfo.linkInfo[linkIndex];

            // open the link id as a url, which is the metadata we added in the text field
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}