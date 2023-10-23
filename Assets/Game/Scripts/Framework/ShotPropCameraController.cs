using UnityEditor;
using UnityEngine;

public class ShotPropCameraController : MonoBehaviour
{
	/*[SerializeField]
	private ShotPropCamera physicalCamera;
	
	private PropController prop;
	private BoundHandler<PropProperties> propertiesUpdatedHandler;

	void Awake()
	{
		prop = GetComponentInParent<PropController>();
		propertiesUpdatedHandler = new BoundHandler<PropProperties>(PropertiesUpdated);
        prop.OnPropertiesUpdated += propertiesUpdatedHandler;

		prop.AssignPhysicalCamera(physicalCamera);

		physicalCamera.onLabelUpdated += LabelUpdated;
		physicalCamera.onSettingsUpdated += SettingsUpdated;
		physicalCamera.onTargetUpdated += TargetUpdated;
		physicalCamera.onFocusUpdated += FocusUpdated;
		physicalCamera.onRotationUpdated += RotationUpdated;
	}

	void OnDestroy()
	{
		prop.UnassignPhysicalCamera(physicalCamera);
        prop.OnPropertiesUpdated -= propertiesUpdatedHandler;

		physicalCamera.onLabelUpdated -= LabelUpdated;
		physicalCamera.onSettingsUpdated -= SettingsUpdated;
		physicalCamera.onTargetUpdated -= TargetUpdated;
		physicalCamera.onFocusUpdated -= FocusUpdated;
		physicalCamera.onRotationUpdated -= RotationUpdated;
	}
	
	private void LabelUpdated()
	{
		prop.properties.SetString("phyLabel", physicalCamera.cameraLabel);
		propertiesUpdatedHandler.Block(prop.PropertiesUpdated);
	}
	
	private void SettingsUpdated()
	{
		var settings = physicalCamera.settings;
		
		prop.properties.SetInt("phySensor", (int)settings.sensor);
		prop.properties.SetInt("phyLens", (int)settings.lens);
		prop.properties.SetInt("phyAperture", (int)settings.aperture);
		prop.properties.SetInt("phyAspect", (int)settings.overlay);
		propertiesUpdatedHandler.Block(prop.PropertiesUpdated);
	}

	private void TargetUpdated()
	{
		var target = physicalCamera.target;
		
		prop.properties.SetBool("phyHasTarget", target.HasValue);
		prop.properties.SetVector3("phyTarget", target.HasValue ? target.Value : Vector3.zero);
		propertiesUpdatedHandler.Block(prop.PropertiesUpdated);
	}
	
	private void FocusUpdated()
	{
		if (!physicalCamera.target.HasValue)
		{
			prop.properties.SetFloat("phyFocus", physicalCamera.focusDistance);
			propertiesUpdatedHandler.Block(prop.PropertiesUpdated);
		}
	}

	private void RotationUpdated()
	{
		if (!physicalCamera.target.HasValue)
		{
			prop.properties.SetVector3("phyRotation", physicalCamera.cameraRotation.eulerAngles);
			propertiesUpdatedHandler.Block(prop.PropertiesUpdated);
		}
	}

	private void PropertiesUpdated(PropProperties properties)
	{
		physicalCamera.cameraLabel = properties.GetString("phyLabel", physicalCamera.cameraLabel);

		var sensor = (PhysicalCameraSettings.Sensor)properties.GetInt(
			"phySensor",
			(int)PhysicalCameraSettings.PropCameraDefault.sensor);
		
		var lens = (PhysicalCameraSettings.Lens)properties.GetInt(
			"phyLens",
			(int)PhysicalCameraSettings.PropCameraDefault.lens);
		
		var aperture = (PhysicalCameraSettings.Aperture)properties.GetInt(
			"phyAperture",
			(int)PhysicalCameraSettings.PropCameraDefault.aperture);

		var overlay = (PhysicalCameraSettings.Overlay)properties.GetInt(
			"phyAspect",
			(int)PhysicalCameraSettings.PropCameraDefault.overlay);

		var aspectratio = (PhysicalCameraSettings.AspectRatios)properties.GetInt(
			"phyAspectRatio",
			(int)PhysicalCameraSettings.PropCameraDefault.aspectRatio);
		
		physicalCamera.settings = new PhysicalCameraSettings(sensor, lens, aperture, overlay, aspectratio);

		if (prop.properties.GetBool("phyHasTarget", false))
		{
			physicalCamera.target = prop.properties.GetVector3("phyTarget", Vector3.zero);
		}
		else
		{
			physicalCamera.target = null;
			physicalCamera.focusDistance = properties.GetFloat("phyFocus", 10f);
			physicalCamera.cameraRotation = Quaternion.Euler(properties.GetVector3("phyRotation", Vector3.zero));
		}
	}*/
}
