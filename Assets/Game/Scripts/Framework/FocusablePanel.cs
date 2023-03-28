using UnityEngine;
using UnityEngine.Events;

public class FocusablePanel : MonoBehaviour//, IClosablePanel
{
	public bool visible
	{
		/*get
		{
			return visibleVariable.currentValue;
		}*/

		set
		{
			// visibleVariable.currentValue = value;
		}
	}

	[SerializeField]
	private RectTransform panel;

	// [SerializeField]
	// private SlideDefinition slideDefinition;

	// [SerializeField]
	// private BoolGlobalVariable visibleVariable;
	
	// [SerializeField]
	// private ModalHandle modalHandle;
	
	public UnityEvent onPanelVisible;
	public UnityEvent onPanelHidden;

	/*void Awake()
	{
		visibleVariable.onValueChanged += UpdateVisible;
	}*/
	
	/*void Start()
	{
		UIManager.Instance.InitializeSlide(panel, visible, slideDefinition);
	}*/
	
	/*void OnDestroy()
	{
		visibleVariable.onValueChanged -= UpdateVisible;
	}*/
	
	/*private void UpdateVisible(bool v)
	{
		if (visible)
		{
			UIManager.Instance.SlideIn(panel, slideDefinition);
			onPanelVisible.Invoke();

			if (modalHandle != null)
			{
				modalHandle.Claim(gameObject);
			}
		}
		else
		{
			UIManager.Instance.SlideOut(panel, slideDefinition);
			onPanelHidden.Invoke();

			if (modalHandle != null)
			{
				modalHandle.Release(gameObject);
			}
		}
	}*/

	public void ClosePanel()
	{
		visible = false;
	}
}
