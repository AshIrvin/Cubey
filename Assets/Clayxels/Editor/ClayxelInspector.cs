
#if UNITY_EDITOR // exclude from build

using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

using Clayxels;

namespace Clayxels{
	[CustomEditor(typeof(ClayContainer))]
	public class ClayxelInspector : Editor{
		static bool extrasPanel = false;
		
		public override void OnInspectorGUI(){
			Color defaultColor = GUI.backgroundColor;

			ClayContainer clayContainer = (ClayContainer)this.target;

			EditorGUILayout.LabelField("Clayxels V1.7.6");

			EditorGUILayout.Space();

			#if UNITY_EDITOR_OSX
				if(ClayContainer.getRenderPipe() == "builtin"){
					GUIStyle s = new GUIStyle();
					s.wordWrap = true;
					s.normal.textColor = Color.yellow;
					EditorGUILayout.LabelField("On Mac OS X you are required to use URP or HDRP render pipelines to work with Clayxels <3", s);

					return;
				}
			#endif

			string userWarn = clayContainer.getUserWarning();
			if(userWarn != ""){
				GUIStyle s = new GUIStyle();
				s.wordWrap = true;
				s.normal.textColor = Color.yellow;
				EditorGUILayout.LabelField(userWarn, s);
			}

			if(clayContainer.getNumSolids() > clayContainer.getMaxSolids()){
				GUIStyle s = new GUIStyle();
				s.wordWrap = true;
				s.normal.textColor = Color.yellow;
				EditorGUILayout.LabelField("Max solid count exeeded, open Global Config to tweak settings.");
			}

			if(ClayContainer.getRenderPipe() != "builtin"){
				if(PrefabUtility.IsPartOfAnyPrefab(clayContainer)){
					if(GUILayout.Button((new GUIContent("link prefab instances to this", "Link all other instances of this prefab in scene to this prefab.\nThis becomes the editable master prefab.\nThis will also fix prefab links in scene if something breaks!")))){
						ClayContainer.linkAllPrefabInstances(clayContainer);
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}

					EditorGUILayout.Space();
				}

				if(clayContainer.getInstanceOf() != null){
					ClayContainer newInstance = (ClayContainer)EditorGUILayout.ObjectField(new GUIContent("instance", "Set this to point at another clayContainer in scene to make this into an instance and avoid having to compute the same thing twice."), clayContainer.getInstanceOf(), typeof(ClayContainer), true);
				
					if(newInstance != clayContainer.getInstanceOf() && newInstance != clayContainer){
						clayContainer.setIsInstanceOf(newInstance);
						clayContainer.init();

						if(!Application.isPlaying){
							EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
						}
					}

					EditorGUILayout.Space();
					if(GUILayout.Button((new GUIContent("global config", "")))){
						ClayxelsPrefsWindow.Open();
					}

					return;
				}
			}

			if(clayContainer.isFrozen()){
				if(clayContainer.isFrozenToMesh()){
					EditorGUILayout.LabelField("frozen mesh options:");

					if(!clayContainer.retopoApplied){
						EditorGUILayout.Space();

						GUILayout.BeginHorizontal();

						clayContainer.meshNormalSmooth = EditorGUILayout.Slider("normal smooth", clayContainer.meshNormalSmooth, 0.0f, 180.0f);

						if(GUILayout.Button((new GUIContent("apply", "")))){
							clayContainer.smoothNormalsContainersHierarchy(clayContainer.meshNormalSmooth);
						}

						GUILayout.EndHorizontal();
					}

					#if CLAYXELS_RETOPO
						EditorGUILayout.Space();

						GUILayout.BeginHorizontal();

						clayContainer.retopoMaxVerts = EditorGUILayout.IntField(new GUIContent("vertex count", "-1 will let the tool decide on the best number of vertices."), clayContainer.retopoMaxVerts);

						if(GUILayout.Button((new GUIContent("retopo", "will try to improve the mesh wireframe automatically")))){
							// reset mesh in case it had previous retopo applied
							clayContainer.defrostToLiveClayxels();
							clayContainer.freezeToMesh(clayContainer.getClayxelDetail());
							clayContainer.retopoApplied = true;

							Mesh mesh = clayContainer.gameObject.GetComponent<MeshFilter>().sharedMesh;

							if(mesh != null){
								MeshUtils.weldVertices(mesh);
								mesh.Optimize();

								int targetVertCount = RetopoUtils.getRetopoTargetVertsCount(clayContainer.gameObject, clayContainer.retopoMaxVerts);
								if(targetVertCount == 0){
									return;
								}

								RetopoUtils.retopoMesh(mesh, targetVertCount, -1);
							}
						}

						GUILayout.EndHorizontal();
					#endif

					EditorGUILayout.Space();
				}

				if(GUILayout.Button(new GUIContent("defrost clayxels", "Back to live clayxels."))){
					clayContainer.defrostContainersHierarchy();
				}

				EditorGUILayout.Space();
				if(GUILayout.Button((new GUIContent("global config", "")))){
					ClayxelsPrefsWindow.Open();
				}

				return;
			}

			EditorGUI.BeginChangeCheck();

			int clayxelDetail = EditorGUILayout.IntField(new GUIContent("clayContainer detail", "How coarse or finely detailed is your sculpt. Enable Gizmos in your viewport to see the boundaries."), clayContainer.getClayxelDetail());
			
			if(EditorGUI.EndChangeCheck()){
				ClayContainer._inspectorUpdate();

				Undo.RecordObject(this.target, "changed clayContainer");

				clayContainer.setClayxelDetail(clayxelDetail);
				
				if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
				}

				return;
			}

			GUILayout.BeginHorizontal();

			GUI.backgroundColor = defaultColor;

			if(!clayContainer.isAutoBoundsActive()){
				EditorGUI.BeginChangeCheck();
				Vector3Int boundsScale = EditorGUILayout.Vector3IntField(new GUIContent("bounds scale", "How much work area you have for your sculpt within this container. Enable Gizmos in your viewport to see the boundaries."), clayContainer.getBoundsScale());
				
				if(EditorGUI.EndChangeCheck()){
					ClayContainer._inspectorUpdate();

					clayContainer.setBoundsScale(boundsScale.x, boundsScale.y, boundsScale.z);

					clayContainer.init();
					clayContainer.needsUpdate = true;

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}

					return;
				}

				if(GUILayout.Button(new GUIContent("-", ""))){
					ClayContainer._inspectorUpdate();

					Vector3Int bounds = clayContainer.getBoundsScale();
					clayContainer.setBoundsScale(bounds.x - 1, bounds.y - 1, bounds.z - 1);

					clayContainer.init();
					clayContainer.needsUpdate = true;

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}

					return;
				}

				if(GUILayout.Button(new GUIContent("+", ""))){
					ClayContainer._inspectorUpdate();

					Vector3Int bounds = clayContainer.getBoundsScale();
					clayContainer.setBoundsScale(bounds.x + 1, bounds.y + 1, bounds.z + 1);

					clayContainer.init();
					clayContainer.needsUpdate = true;

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}

					return;
				}

				if(GUILayout.Button(new GUIContent("auto", ""))){
					clayContainer.setAutoBoundsActive(true);

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}
				}
			}
			else{
				GUI.backgroundColor = Color.yellow;

				GUILayout.BeginHorizontal();
				
				EditorGUILayout.LabelField("bounds scale");

				if(GUILayout.Button(new GUIContent("auto", ""))){
					clayContainer.setAutoBoundsActive(false);
				}

				GUILayout.EndHorizontal();
			}

			GUI.backgroundColor = defaultColor;

			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			if(GUILayout.Button(new GUIContent("add clay", "lets get this party started"))){
				ClayObject clayObj = ((ClayContainer)this.target).addClayObject();

				Undo.RegisterCreatedObjectUndo(clayObj.gameObject, "added clayObject");
				UnityEditor.Selection.objects = new GameObject[]{clayObj.gameObject};

				clayContainer.needsUpdate = true;

				if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}

				return;
			}

			if(ClayContainer.getRenderPipe() == "builtin"){
				if(GUILayout.Button(new GUIContent("pick clay ("+ClayContainer.pickingKey+")", "Press p on your keyboard to mouse pick ClayObjects from the viewport. Pressing Shift will add to a previous selection."))){
					ClayContainer.startScenePicking();
				}
			}

			if(GUILayout.Button((new GUIContent("global config", "")))){
				ClayxelsPrefsWindow.Open();
			}

			clayContainer.forceUpdate = EditorGUILayout.Toggle(new GUIContent("animate (forceUpdate)", "Enable if you're animating/moving the container as well as the clayObjects inside it."), clayContainer.forceUpdate);

			EditorGUILayout.Space();

			ClayxelInspector.extrasPanel = EditorGUILayout.Foldout(ClayxelInspector.extrasPanel, "extras", true);

			int currRenderMode = 0;

			if(ClayxelInspector.extrasPanel){
				if(ClayContainer.getRenderPipe() != "builtin"){
					string[] renderLabels = {"polySplat", "microVoxelSplat"};
					currRenderMode = clayContainer.getRenderMode();
	 				int renderMode = EditorGUILayout.Popup("render mode", currRenderMode, renderLabels);
	 				if(currRenderMode != renderMode){
	 					clayContainer.setRenderMode(renderMode);
	 					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	 				}
				}

				if(ClayContainer.getRenderPipe() != "builtin"){
					EditorGUILayout.Space();

					ClayContainer instance = (ClayContainer)EditorGUILayout.ObjectField(new GUIContent("instance", "Set this to point at another clayContainer in scene to make this into an instance and avoid having to compute the same thing twice."), clayContainer.getInstanceOf(), typeof(ClayContainer), true);
					if(instance != clayContainer.getInstanceOf() && instance != clayContainer){
						clayContainer.setIsInstanceOf(instance);

						if(!Application.isPlaying){
							EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
						}
					}
				}

				EditorGUILayout.Space();

				if(clayContainer.storeAssetPath == ""){
					clayContainer.storeAssetPath = clayContainer.gameObject.name;
				}
				clayContainer.storeAssetPath = EditorGUILayout.TextField(new GUIContent("frozen asset name", "Specify an asset name to store frozen mesh or claymation on disk. Files are saved relative to this project's Assets folder."), clayContainer.storeAssetPath);
				string[] paths = clayContainer.storeAssetPath.Split('.');
				if(paths.Length > 0){
					clayContainer.storeAssetPath = paths[0];
				}

				EditorGUILayout.Space();

				if(GUILayout.Button(new GUIContent("freeze mesh", "Switch between live clayxels and a frozen mesh."))){
					clayContainer.freezeContainersHierarchyToMesh();
					
					if(clayContainer.storeAssetPath != ""){
						clayContainer.storeMesh(clayContainer.storeAssetPath);
					}
				}

				EditorGUILayout.Space();

				if(currRenderMode == 0){
					AnimationClip claymationAnimClip = (AnimationClip)EditorGUILayout.ObjectField(new GUIContent("claymation AnimClip", "Freeze an animation to disk using the claymation file format. Leave empty to freeze a single frame claymation point cloud."), clayContainer.claymationAnimClip, typeof(AnimationClip), true);
					if(claymationAnimClip != null && claymationAnimClip != clayContainer.claymationAnimClip){
						clayContainer.claymationStartFrame = 0;
						clayContainer.claymationEndFrame = (int)(claymationAnimClip.length * claymationAnimClip.frameRate);
					}
					clayContainer.claymationAnimClip = claymationAnimClip;

					if(clayContainer.claymationAnimClip != null){
						clayContainer.claymationStartFrame = EditorGUILayout.IntField(new GUIContent("start", ""), clayContainer.claymationStartFrame);
						clayContainer.claymationEndFrame = EditorGUILayout.IntField(new GUIContent("end", ""), clayContainer.claymationEndFrame);
					}

					if(GUILayout.Button(new GUIContent("freeze claymation", "Freeze this container to a point-cloud file stored on disk and skip heavy computing."))){
						clayContainer.freezeClaymation();
					}
				}
					
				EditorGUI.BeginChangeCheck();

				EditorGUILayout.Space();

				bool castShadows = EditorGUILayout.Toggle("cast shadows", clayContainer.getCastShadows());

				if(EditorGUI.EndChangeCheck()){
					ClayContainer._inspectorUpdate();

					clayContainer.setCastShadows(castShadows);

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}
				}

				// end of extras
			}

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();

			Material customMaterial = (Material)EditorGUILayout.ObjectField(new GUIContent("customMaterial", "Custom materials need to use shaders specifically made for clayxels. Use the provided shaders and examples as reference. "), clayContainer.customMaterial, typeof(Material), false);
			
			if(customMaterial == null){
				if(GUILayout.Button(new GUIContent("+", "Create a new material that you can share with other containers."))){
					ClayContainer._inspectorUpdate();

					Undo.RecordObject(this.target, "changed clayContainer");

					this.addCustomMaterial(clayContainer);

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}

					this.inspectMaterial(clayContainer);

					return;
				}
			}

			GUILayout.EndHorizontal();

			if(EditorGUI.EndChangeCheck()){
				ClayContainer._inspectorUpdate();
				
				Undo.RecordObject(this.target, "changed clayContainer");

				if(customMaterial != clayContainer.customMaterial){
					clayContainer.setCustomMaterial(customMaterial);
				}
				
				if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}
			}

			if(!clayContainer.isFrozenToMesh()){
				this.inspectMaterial(clayContainer);
			}
		}

		MaterialEditor materialEditor = null;
		void inspectMaterial(ClayContainer clayContainer){
			this.drawMaterialInspector(clayContainer);
		}

		void drawMaterialInspector(ClayContainer container){
			EditorGUILayout.Space();

			Rect rect = EditorGUILayout.GetControlRect(false, 1);
       		rect.height = 1;
       		EditorGUI.DrawRect(rect, new Color ( 0.5f,0.5f,0.5f, 1 ) );

			Material material = container.getMaterial();

			if(material == null){
				return;
			}

			float splatsLOD = EditorGUILayout.FloatField(new GUIContent("Level Of Detail", "Set Set the visual quality of splats when they get far away from camera."), container.getSplatsLOD());
			container.setSplatsLOD(splatsLOD);

			Shader shader = material.shader;

			string[] priorityNames = new string[]{
				"Smoothness",
				"Metallic",
				"SplatTexture"};

			string[] excludeNames = new string[]{
				"Emission Color",
				"Alpha Cutoff"};

			for(int i = 0; i < priorityNames.Length; ++i){
				for(int id = 0; id < shader.GetPropertyCount(); ++id){
					string desc = shader.GetPropertyDescription(id);

					if(desc == priorityNames[i]){
						this.drawMaterialProperty(material, id);
					}
				}
			}

			for(int i = 0; i < shader.GetPropertyCount(); ++i){
				ShaderPropertyType type = shader.GetPropertyType(i);
				string name = shader.GetPropertyName(i);
				string desc = shader.GetPropertyDescription(i);

				if(desc == ""){
					continue;
				}

				bool shouldSkip = false;
				for(int j = 0; j < excludeNames.Length; ++j){
					if(desc.StartsWith(excludeNames[j])){
						shouldSkip = true;
						break;
					}
				}

				if(shouldSkip){
					continue;
				}

				for(int j = 0; j < priorityNames.Length; ++j){
					if(desc == priorityNames[j]){
						shouldSkip = true;
						break;
					}
				}

				if(shouldSkip){
					continue;
				}
				
				bool userProperty = this.drawMaterialProperty(material, i);
				if(!userProperty){
					break;
				}
			}

			
		}

		void addCustomMaterial(ClayContainer container){
			string assetNameUnique = ClayContainer.defaultAssetsPath + "/" + container.storeAssetPath + container.gameObject.name + "_" + container.GetInstanceID();
			string materialUniqueName = assetNameUnique + "_mat";

			Material storedMat = new Material(container.getMaterial());
			AssetDatabase.CreateAsset(storedMat, "Assets/" + materialUniqueName + ".mat");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			container.setCustomMaterial(storedMat);
		}

		bool drawMaterialProperty(Material material, int id){
			Shader shader = material.shader;
			ShaderPropertyType type = shader.GetPropertyType(id);
			string name = shader.GetPropertyName(id);
			string desc = shader.GetPropertyDescription(id);
			bool isEndParam = false;

			string[] attrs = shader.GetPropertyAttributes(id);
			if(attrs.Length > 0){
				for(int i = 0; i < attrs.Length; ++i){
					string attr = attrs[0];
					if(attr == "ASEEnd"){
						isEndParam = true;
					}

					if(attr.ToUpper() == attr){// all upper case? then it's a keyword
						float value = material.GetFloat(name);

						if(value <= 0.0f){
							material.DisableKeyword(attr);
						}
						else{
							material.EnableKeyword(attr);
						}
					}
				}
			}

			if(type == ShaderPropertyType.Float){
				float value = material.GetFloat(name);
				float newVal = EditorGUILayout.FloatField(desc, value);

				if(value != newVal){
					Undo.RecordObject(material, "changed clayxels material");

					material.SetFloat(name, newVal);

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}
				}
			}
			else if(type == ShaderPropertyType.Range){
				float value = material.GetFloat(name);
				Vector2 range = shader.GetPropertyRangeLimits(id);
				float newVal = EditorGUILayout.Slider(desc, value, range.x, range.y);

				if(value != newVal){
					Undo.RecordObject(material, "changed clayxels material");

					material.SetFloat(name, newVal);

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}
				}
			}
			else if(type == ShaderPropertyType.Texture){
				Texture value = material.GetTexture(name);
				Texture newVal = (Texture)EditorGUILayout.ObjectField(desc, value, typeof(Texture), false);

				if(value != newVal){
					Undo.RecordObject(material, "changed clayxels material");
					
					material.SetTexture(name, newVal);

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}
				}
			}
			else if(type == ShaderPropertyType.Color){
				Color value = material.GetColor(name);
				Color newVal = EditorGUILayout.ColorField(desc, value);

				if(value != newVal){
					Undo.RecordObject(material, "changed clayxels material");
					
					material.SetColor(name, newVal);

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}
				}
			}
			else if(type == ShaderPropertyType.Vector){
				Vector3 value = material.GetVector(name);
				Vector3 newVal = EditorGUILayout.Vector3Field(desc, value);

				if(value != newVal){
					Undo.RecordObject(material, "changed clayxels material");
					
					material.SetVector(name, newVal);

					if(!Application.isPlaying){
						EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
					}
				}
			}

			if(isEndParam){
				return false;
			}

			return true;
		}

		void OnDisable (){
			if(this.materialEditor != null) {
				DestroyImmediate(this.materialEditor);
				this.materialEditor = null;
			}
		}
	}

	[CustomEditor(typeof(ClayObject)), CanEditMultipleObjects]
	public class ClayObjectInspector : Editor{
		public override void OnInspectorGUI(){
			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			
			ClayObject clayObj = (ClayObject)this.targets[0];
			ClayContainer clayContainer = clayObj.getClayContainer();
			if(clayContainer == null){
				return;
			}

			EditorGUI.BeginChangeCheck();
			
			int primitiveType = 0;
			if(clayObj.mode != ClayObject.ClayObjectMode.clayGroup){
				string[] solidsLabels = clayContainer.getSolidsCatalogueLabels();
	 			primitiveType = EditorGUILayout.Popup("type", clayObj.primitiveType, solidsLabels);
	 		}

			float blend = EditorGUILayout.Slider("blend", Mathf.Abs(clayObj.blend) * 100.0f, 0.0f, 100.0f);
			if(clayObj.blend < 0.0f){
				if(blend < 0.001f){
					blend = 0.001f;
				}

				blend *= -1.0f;
			}

			blend *= 0.01f;
			if(blend > 1.0f){
				blend = 1.0f;
			}
			else if(blend < -1.0f){
				blend = -1.0f;
			}

			GUILayout.BeginHorizontal();

			Color defaultColor = GUI.backgroundColor;

			if(clayObj.blend >= 0.0f){
				GUI.backgroundColor = Color.yellow;
			}

			if(GUILayout.Button(new GUIContent("add", "Additive blend"))){
				blend = Mathf.Abs(blend);
			}
			
			GUI.backgroundColor = defaultColor;

			if(clayObj.blend < 0.0f){
				GUI.backgroundColor = Color.yellow;
			}

			if(GUILayout.Button(new GUIContent("sub", "Subtractive blend"))){
				if(blend == 0.0f){
					blend = 0.0001f;
				}

				blend = blend * -1.0f;
			}

			GUI.backgroundColor = defaultColor;

			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();

			GUI.backgroundColor = defaultColor;

			bool isPainter = clayObj.getIsPainter();
			if(isPainter){
				GUI.backgroundColor = Color.yellow;
			}

			if(GUILayout.Button(new GUIContent("painter", "set this clayObject to be a painter that only affects colors on other clayObjects"))){
				clayObj.setIsPainter(!isPainter);
			}

			GUI.backgroundColor = defaultColor;

			bool mirror = clayObj.getMirror();
			if(mirror){
				GUI.backgroundColor = Color.yellow;
			}

			if(GUILayout.Button(new GUIContent("mirror", "mirror this clayObject on the X axis"))){
				clayObj.setMirror(!mirror);
			}

			GUI.backgroundColor = defaultColor;

			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			Color color = Color.white;
			Dictionary<string, float> paramValues = new Dictionary<string, float>();

	 		paramValues["x"] = clayObj.attrs.x;
	 		paramValues["y"] = clayObj.attrs.y;
	 		paramValues["z"] = clayObj.attrs.z;
	 		// paramValues["w"] = clayObj.attrs.w;
	 		paramValues["x2"] = clayObj.attrs2.x;
	 		paramValues["y2"] = clayObj.attrs2.y;
	 		paramValues["z2"] = clayObj.attrs2.z;
	 		paramValues["w2"] = clayObj.attrs2.w;

		 	if(clayObj.mode != ClayObject.ClayObjectMode.clayGroup){
		 		color = EditorGUILayout.ColorField("color", clayObj.color);

		 		List<string[]> parameters = ClayContainer.getSolidsCatalogueParameters(primitiveType);
		 		// List<string> wMaskLabels = new List<string>();
		 		for(int paramIt = 0; paramIt < parameters.Count; ++paramIt){
		 			string[] parameterValues = parameters[paramIt];
		 			string attr = parameterValues[0];
		 			string label = parameterValues[1];
		 			string defaultValue = parameterValues[2];
					
		 			if(primitiveType != clayObj.primitiveType){
		 				// reset to default params when changing primitive type
		 				paramValues[attr] = float.Parse(defaultValue, CultureInfo.InvariantCulture);
		 			}
		 			
		 			// if(attr.StartsWith("w")){
		 			// 	wMaskLabels.Add(label);
		 			// }
		 			// else{
		 				paramValues[attr] = EditorGUILayout.FloatField(label, paramValues[attr] * 100.0f) * 0.01f;
		 			// }
		 		}

		 		// if(wMaskLabels.Count > 0){
		 		// 	paramValues["w"] = (float)EditorGUILayout.MaskField("options", (int)clayObj.attrs.w, wMaskLabels.ToArray());
		 		// }
	 		}

	 		if(EditorGUI.EndChangeCheck()){
	 			ClayContainer._inspectorUpdate();
	 			ClayContainer._skipHierarchyChanges = true;
				
	 			Undo.RecordObjects(this.targets, "changed clayObject");

	 			for(int i = 1; i < this.targets.Length; ++i){
	 				bool somethingChanged = false;
	 				ClayObject currentClayObj = (ClayObject)this.targets[i];
	 				bool shouldAutoRename = false;

	 				if(Mathf.Abs(clayObj.blend - blend) > 0.001f || Mathf.Sign(clayObj.blend) != Mathf.Sign(blend)){
	 					currentClayObj.blend = blend;
	 					somethingChanged = true;
	 					shouldAutoRename = true;
	 				}

	 				if(clayObj.color != color){
	 					currentClayObj.color = color;
	 					somethingChanged = true;
	 				}
					
	 				if(clayObj.primitiveType != primitiveType){
	 					currentClayObj.primitiveType = primitiveType;
	 					somethingChanged = true;
	 					shouldAutoRename = true;
	 				}

	 				if(clayObj.attrs.x != paramValues["x"]){
	 					currentClayObj.attrs.x = paramValues["x"];
	 					somethingChanged = true;
	 				}

	 				if(clayObj.attrs.y != paramValues["y"]){
	 					currentClayObj.attrs.y = paramValues["y"];
	 					somethingChanged = true;
	 				}

	 				if(clayObj.attrs.z != paramValues["z"]){
	 					currentClayObj.attrs2.z = paramValues["z"];
	 					somethingChanged = true;
	 				}

	 				if(clayObj.attrs2.x != paramValues["x2"]){
	 					currentClayObj.attrs2.x = paramValues["x2"];
	 					somethingChanged = true;
	 				}

	 				if(clayObj.attrs2.y != paramValues["y2"]){
	 					currentClayObj.attrs2.y = paramValues["y2"];
	 					somethingChanged = true;
	 				}

	 				if(clayObj.attrs2.z != paramValues["z2"]){
	 					currentClayObj.attrs2.z = paramValues["z2"];
	 					somethingChanged = true;
	 				}

	 				if(clayObj.attrs2.w != paramValues["w2"]){
	 					currentClayObj.attrs2.w = paramValues["w2"];
	 					somethingChanged = true;
	 				}

	 				// if(clayObj.attrs.w != paramValues["w"]){
	 				// 	currentClayObj.attrs.w = paramValues["w"];
	 				// 	somethingChanged = true;
	 				// 	shouldAutoRename = true;
	 				// }

	 				if(somethingChanged){
	 					currentClayObj.getClayContainer().clayObjectUpdated(currentClayObj);

	 					if(shouldAutoRename){
		 					if(currentClayObj.gameObject.name.StartsWith("clay_")){
		 						clayContainer.autoRenameClayObject(currentClayObj);
		 					}
		 				}
	 				}

	 				ClayContainer._skipHierarchyChanges = false;
				}

	 			clayObj.blend = blend;
	 			clayObj.color = color;
	 			clayObj.primitiveType = primitiveType;
	 			clayObj.attrs.x = paramValues["x"];
	 			clayObj.attrs.y = paramValues["y"];
	 			clayObj.attrs.z = paramValues["z"];
	 			// clayObj.attrs.w = paramValues["w"];
	 			clayObj.attrs2.x = paramValues["x2"];
	 			clayObj.attrs2.y = paramValues["y2"];
	 			clayObj.attrs2.z = paramValues["z2"];
	 			clayObj.attrs2.w = paramValues["w2"];

	 			if(clayObj.gameObject.name.StartsWith("clay_")){
					clayContainer.autoRenameClayObject(clayObj);
				}

				clayObj.forceUpdate();
	 			
	 			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
	 			ClayContainer.getSceneView().Repaint();

	 			if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}
			}

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();

			ClayObject.ClayObjectMode mode = (ClayObject.ClayObjectMode)EditorGUILayout.EnumPopup(
				new GUIContent("mode", 
					"change this clayObject into:\n\noffset: a series clones with an offset from each other\n\nspline: series of clones along a spline\n\nclayGroup: this clayObject becomes a group that can nest other clayObjects and blend them as a whole with the rest of your sculpt."), 
				clayObj.mode);
			
			if(EditorGUI.EndChangeCheck()){
				clayObj.setMode(mode);

				if(clayObj.gameObject.name.StartsWith("clay_") && mode == ClayObject.ClayObjectMode.clayGroup){
					clayContainer.autoRenameClayObject(clayObj);
				}

				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();

				if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}
			}

			EditorGUILayout.Space();

			if(clayObj.mode == ClayObject.ClayObjectMode.offset){
				this.drawOffsetMode(clayObj);
			}
			else if(clayObj.mode == ClayObject.ClayObjectMode.spline){
				this.drawSplineMode(clayObj);
			}
			else if(clayObj.mode == ClayObject.ClayObjectMode.clayGroup){
				this.drawClayGroupMode(clayObj);
			}

			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUI.enabled = !clayContainer.isClayObjectsOrderLocked();
			int clayObjectId = EditorGUILayout.IntField("order", clayObj.clayObjectId);
			GUI.enabled = true;

			if(!clayContainer.isClayObjectsOrderLocked()){
				if(clayObjectId != clayObj.clayObjectId){
					int idOffset = clayObjectId - clayObj.clayObjectId; 
					clayContainer.reorderClayObject(clayObj.clayObjectId, idOffset);
				}
			}

			if(GUILayout.Button(new GUIContent("↑", ""))){
				clayContainer.reorderClayObject(clayObj.clayObjectId, -1);
			}
			if(GUILayout.Button(new GUIContent("↓", ""))){
				clayContainer.reorderClayObject(clayObj.clayObjectId, 1);
			}
			if(GUILayout.Button(new GUIContent("⋮", ""))){
				EditorUtility.DisplayPopupMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0), "Component/Clayxels/ClayObject", null);
			}
			GUILayout.EndHorizontal();
		}

		[MenuItem("Component/Clayxels/ClayObject/Mirror Duplicate (m)")]
    	static void MirrorDuplicate(MenuCommand command){
    		ClayContainer.shortcutMirrorDuplicate();
    	}

		[MenuItem("Component/Clayxels/ClayObject/Unlock Order From Hierarchy")]
    	static void OrderFromHierarchyOff(MenuCommand command){
    		if(UnityEditor.Selection.gameObjects.Length > 0){
    			ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
    			if(clayObj != null){
    				clayObj.getClayContainer().setClayObjectsOrderLocked(false);
    			}
    		}
    	}

    	[MenuItem("Component/Clayxels/ClayObject/Lock Order To Hierarchy")]
    	static void OrderFromHierarchyOn(MenuCommand command){
    		if(UnityEditor.Selection.gameObjects.Length > 0){
    			ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
    			if(clayObj != null){
    				clayObj.getClayContainer().setClayObjectsOrderLocked(true);
    			}
    		}
    	}

		[MenuItem("Component/Clayxels/ClayObject/Send Before ClayObject")]
    	static void sendBeforeClayObject(MenuCommand command){
    		if(UnityEditor.Selection.gameObjects.Length > 0){
    			ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
    			if(clayObj != null){
    				clayObj.getClayContainer().selectToReorder(clayObj, 0);
    			}
    		}
    	}

		[MenuItem("Component/Clayxels/ClayObject/Send After ClayObject")]
    	static void sendAfterClayObject(MenuCommand command){
    		if(UnityEditor.Selection.gameObjects.Length > 0){
    			ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
    			if(clayObj != null){
    				clayObj.getClayContainer().selectToReorder(clayObj, 1);
    			}
    		}
    	}

    	[MenuItem("Component/Clayxels/ClayObject/Rename all ClayObjects to Animate")]
    	static void renameToAnimate(MenuCommand command){
    		if(UnityEditor.Selection.gameObjects.Length > 0){
    			ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
    			if(clayObj != null){
    				ClayContainer container = clayObj.getClayContainer();
    				ClayContainer._skipHierarchyChanges = true;// otherwise each rename will trigger onHierarchyChange

    				int numClayObjs = container.getNumClayObjects();

    				for(int i = 0; i < numClayObjs; ++i){
    					ClayObject currentClayObj = container.getClayObject(i);

    					if(currentClayObj.gameObject.name.StartsWith("clay_")){
    						container.autoRenameClayObject(currentClayObj);
    						currentClayObj.name = "(" + i + ")" + currentClayObj.gameObject.name;
    					}
    				}

    				ClayContainer._skipHierarchyChanges = false;
    			}
    		}
    	}

    	void drawClayGroupMode(ClayObject clayObj){
    		if(GUILayout.Button(new GUIContent("add clay", "add clay inside this clayGroup"))){
				ClayObject childClayObj = clayObj.getClayContainer().addClayObject();

				childClayObj.transform.parent = clayObj.transform;
				childClayObj.transform.localPosition = Vector3.zero;
				childClayObj.transform.localEulerAngles = Vector3.zero;

				Undo.RegisterCreatedObjectUndo(childClayObj.gameObject, "added clayObject");
				UnityEditor.Selection.objects = new GameObject[]{childClayObj.gameObject};

				clayObj.getClayContainer().scheduleClayObjectsScan();

				if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}

				return;
			}
    	}

		void drawSplineMode(ClayObject clayObj){
			EditorGUI.BeginChangeCheck();

			int subdivs = EditorGUILayout.IntField("subdivs", clayObj.getSplineSubdiv());

			GUILayout.BeginHorizontal();

			int numPoints = clayObj.splinePoints.Count - 2;
			EditorGUILayout.LabelField("control points: " + numPoints);

			if(GUILayout.Button(new GUIContent("+", ""))){
				clayObj.addSplineControlPoint();
			}

			if(GUILayout.Button(new GUIContent("-", ""))){
				clayObj.removeLastSplineControlPoint();
			}

			GUILayout.EndHorizontal();

			// var list = this.serializedObject.FindProperty("splinePoints");
			// EditorGUILayout.PropertyField(list, new GUIContent("spline points"), true);

			if(EditorGUI.EndChangeCheck()){
				// this.serializedObject.ApplyModifiedProperties();

				clayObj.setSplineSubdiv(subdivs);

				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			}
		}

		void drawOffsetMode(ClayObject clayObj){
			EditorGUI.BeginChangeCheck();
				
			int numSolids = EditorGUILayout.IntField("solids", clayObj.getNumSolids());
			bool allowSceneObjects = true;
			clayObj.offsetter = (GameObject)EditorGUILayout.ObjectField("offsetter", clayObj.offsetter, typeof(GameObject), allowSceneObjects);
			
			if(EditorGUI.EndChangeCheck()){
				if(numSolids < 1){
					numSolids = 1;
				}
				else if(numSolids > 100){
					numSolids = 100;
				}

				clayObj.setOffsetNum(numSolids);
				
				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			}
		}
	}

	[CustomEditor(typeof(Claymation))]
	public class ClaymationInspector : Editor{
		public override void OnInspectorGUI(){
			Claymation claymation = (Claymation)this.target;
			
			if(claymation.instanceOf == null){
				EditorGUI.BeginChangeCheck();

				TextAsset claymationFile = (TextAsset)EditorGUILayout.ObjectField(new GUIContent("claymation asset", ""), claymation.claymationFile, typeof(TextAsset), true);
				
				if(EditorGUI.EndChangeCheck()){
					if(claymation.claymationFile != claymationFile){
						claymation.claymationFile = claymationFile;
						claymation.init();
					}
				}

				if(GUILayout.Button(new GUIContent("reload file", "click this when your claymation file changes and you need to refresh this player"))){
					claymation.init();
				}

				Material material = (Material)EditorGUILayout.ObjectField(new GUIContent("material", "Use the same materials you use on the clayContainers"), claymation.material, typeof(Material), true);
				claymation.material = material;

				if(claymation.getNumFrames() > 1){
					int frameRate = EditorGUILayout.IntField(new GUIContent("frame rate", "how fast will this claymation play"), claymation.frameRate);

					claymation.frameRate = frameRate;

					claymation.playAnim = EditorGUILayout.Toggle(new GUIContent("play anim", "Always play the anim in loop"), claymation.playAnim);
					
					int currentFrame = EditorGUILayout.IntField(new GUIContent("frame", ""), claymation.getFrame());
					if(currentFrame != claymation.getFrame()){
						claymation.loadFrame(currentFrame);
					}
				}
			}

			EditorGUILayout.Space();

			Claymation instance = (Claymation)EditorGUILayout.ObjectField(new GUIContent("instance", "Set this to point at another Claymation in scene to make this into an instance and avoid duplicating memory."), claymation.instanceOf, typeof(Claymation), true);
			
			if(instance != claymation.instanceOf && instance != claymation){
				claymation.instanceOf = instance;
				claymation.init();

				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
				ClayContainer.getSceneView().Repaint();

				if(!Application.isPlaying){
					EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				}
			}
		}
	}
}

public class ClayxelsPrefsWindow : EditorWindow{
	static ClayxelsPrefs prefs;
	static bool somethingChanged = false;

    [MenuItem("Component/Clayxels/Config")]
    public static void Open(){
    	if(Application.isPlaying){
    		return;
    	}

    	ClayxelsPrefsWindow.somethingChanged = false;

    	ClayxelsPrefsWindow.prefs = ClayContainer.loadPrefs();

        ClayxelsPrefsWindow window = (ClayxelsPrefsWindow)EditorWindow.GetWindow(typeof(ClayxelsPrefsWindow));
        window.Show();
    }

    void OnLostFocus(){
    	if(Application.isPlaying){
    		return;
    	}
    	
    	ClayContainer.savePrefs(ClayxelsPrefsWindow.prefs);
    	ClayContainer.reloadAll();
    }

    void OnGUI(){
    	if(Application.isPlaying){
    		return;
    	}

    	if(ClayxelsPrefsWindow.prefs == null){
    		ClayxelsPrefsWindow.prefs = ClayContainer.loadPrefs();
    	}

    	EditorGUI.BeginChangeCheck();

    	Color boundsColor = new Color((float)ClayxelsPrefsWindow.prefs.boundsColor[0] / 255.0f, (float)ClayxelsPrefsWindow.prefs.boundsColor[1] / 255.0f, (float)ClayxelsPrefsWindow.prefs.boundsColor[2] / 255.0f, (float)ClayxelsPrefsWindow.prefs.boundsColor[3] / 255.0f);
    	boundsColor = EditorGUILayout.ColorField(new GUIContent("boundsColor", "Color of the bounds indicator in the viewport, enable Gizmos in the viewport to see this."), boundsColor);
    	ClayxelsPrefsWindow.prefs.boundsColor[0] = (byte)(boundsColor.r * 255);
    	ClayxelsPrefsWindow.prefs.boundsColor[1] = (byte)(boundsColor.g * 255);
    	ClayxelsPrefsWindow.prefs.boundsColor[2] = (byte)(boundsColor.b * 255);
    	ClayxelsPrefsWindow.prefs.boundsColor[3] = (byte)(boundsColor.a * 255);

    	if(ClayContainer.getRenderPipe() != "builtin"){
    		ClayxelsPrefsWindow.prefs.directPickEnabled = EditorGUILayout.Toggle(new GUIContent("direct picking", "Enable this to pick clay without having to use a picking shortcut, doesn't work with poly-splats"), ClayxelsPrefsWindow.prefs.directPickEnabled);
    	}

    	ClayxelsPrefsWindow.prefs.pickingKey = EditorGUILayout.TextField(new GUIContent("picking shortcut", "Press this shortcut to pick/select containers and clayObjects in scene."), ClayxelsPrefsWindow.prefs.pickingKey);
    	ClayxelsPrefsWindow.prefs.mirrorDuplicateKey = EditorGUILayout.TextField(new GUIContent("mirrorDuplicate shortcut", "Press this shortcut to duplicate and mirror a clayObject on the X axis."), ClayxelsPrefsWindow.prefs.mirrorDuplicateKey);

    	string[] pointCountPreset = new string[]{"low", "mid", "high"};
    	ClayxelsPrefsWindow.prefs.maxPointCount = EditorGUILayout.Popup(new GUIContent("pointCloud memory", "Preset to allocate video ram to handle bigger point clouds."), ClayxelsPrefsWindow.prefs.maxPointCount, pointCountPreset);
    	
    	string[] solidsCountPreset = new string[]{"low", "mid", "high"};
    	ClayxelsPrefsWindow.prefs.maxSolidsCount = EditorGUILayout.Popup(new GUIContent("clayObjects memory", "Preset to allocate video ram to handle more clayObjects per container."), ClayxelsPrefsWindow.prefs.maxSolidsCount, solidsCountPreset);
    	
    	string[] solidsPerVoxelPreset = new string[]{"best performance", "balanced", "max sculpt detail"};
    	ClayxelsPrefsWindow.prefs.maxSolidsPerVoxel = EditorGUILayout.Popup(new GUIContent("clayObjects per voxel", "Preset to handle more clayObjects per voxel, it might fix some artifacts caused by having a lot of clayObjects all close to each other."), ClayxelsPrefsWindow.prefs.maxSolidsPerVoxel, solidsPerVoxelPreset);
    	
    	int frameSkip = EditorGUILayout.IntField(new GUIContent("frame skip", ""), ClayxelsPrefsWindow.prefs.frameSkip);
    	if(frameSkip < 0){
    		frameSkip = 0;
    	}
    	else if(frameSkip > 100){
    		frameSkip = 100;
    	}
    	ClayxelsPrefsWindow.prefs.frameSkip = frameSkip;

    	int maxBounds = EditorGUILayout.IntField(new GUIContent("max bounds size", "Smaller bounds use less video memory but give you less space to work with."), ClayxelsPrefsWindow.prefs.maxBounds);
    	if(maxBounds < 1){
    		maxBounds = 1;
    	}
    	else if(maxBounds > 3){
    		maxBounds = 3;
    	}
    	ClayxelsPrefsWindow.prefs.maxBounds = maxBounds;

    	ClayxelsPrefsWindow.prefs.globalBlend = EditorGUILayout.Slider(new GUIContent("global blend", 
    		"The max amount of blend between clayObjects. Reduce it to increase performance when updating clay."), 
    		ClayxelsPrefsWindow.prefs.globalBlend, 0.0f, 2.0f);

    	if(ClayContainer.getRenderPipe() != "builtin"){
    		ClayxelsPrefsWindow.prefs.renderSize = EditorGUILayout.Vector2IntField(new GUIContent("render target resolution", "Set the output pixel resolution used by the microvoxelSplats renderer. For best performance set this to half of your output resolution and enable Temporal Anti Aliasing on the main Camera."), ClayxelsPrefsWindow.prefs.renderSize);
    	}

    	ClayxelsPrefsWindow.prefs.vramLimitEnabled = EditorGUILayout.Toggle(new GUIContent("video ram limit enabled", "When this limit is enabled you won't be able to exceed your available vram when creating new container."), ClayxelsPrefsWindow.prefs.vramLimitEnabled);

    	if(EditorGUI.EndChangeCheck()){
    		ClayxelsPrefsWindow.somethingChanged = true;
    	}

    	EditorGUILayout.Space();

    	// LOD dismissed, microvoxels does it automatically
    	// EditorGUI.BeginChangeCheck();
		// EditorGUILayout.MinMaxSlider(new GUIContent("LOD: " + Mathf.Round(ClayxelsPrefsWindow.prefs.lodNear) + " - " + Mathf.Round(ClayxelsPrefsWindow.prefs.lodFar), "Level Of Detail in scene unit, measures the distance from the camera to automatically reduce the amount of points rendered."), 
		// 	ref ClayxelsPrefsWindow.prefs.lodNear, ref ClayxelsPrefsWindow.prefs.lodFar, 0.0f, 1000.0f);
		// if(EditorGUI.EndChangeCheck()){
		// 	ClayContainer.savePrefs(ClayxelsPrefsWindow.prefs);
		// 	ClayContainer.setAutoLOD(ClayxelsPrefsWindow.prefs.lodNear, ClayxelsPrefsWindow.prefs.lodFar);
		// }

	    int[] memStats = ClayContainer.getMemoryStats();
    	EditorGUILayout.LabelField("- vram rough usage -");
	    EditorGUILayout.LabelField("upfront vram allocated: " + memStats[0] + "MB");
	    EditorGUILayout.LabelField("containers in scene: " + memStats[1] + "MB");

	    EditorGUILayout.Space();

	    Color defaultColor = GUI.backgroundColor;
	    if(ClayxelsPrefsWindow.somethingChanged){
	    	GUI.backgroundColor = Color.yellow;
	    }

		if(GUILayout.Button((new GUIContent("reload all", "This is necessary after you make changes to the shaders or to the claySDF file.")))){
			ClayContainer.savePrefs(ClayxelsPrefsWindow.prefs);
			ClayContainer.reloadAll();

			ClayxelsPrefsWindow.prefs = ClayContainer.loadPrefs();

			ClayxelsPrefsWindow.somethingChanged = false;
		}

		GUI.backgroundColor = defaultColor;
    }

    public class ClayxelsMessageWindow : EditorWindow{
    	public delegate void ClayxelsMessageWindoCallback();
		static public ClayxelsMessageWindoCallback onClosedCallback = null;

		static string message = "";

	    public static void Open(string msg){
	    	if(EditorWindow.HasOpenInstances<ClayxelsMessageWindow>() || msg == ""){
	    		return;
	    	}

	    	ClayxelsMessageWindow.message = msg;

	        ClayxelsMessageWindow window = (ClayxelsMessageWindow)EditorWindow.GetWindow(typeof(ClayxelsMessageWindow));
	        window.titleContent = new GUIContent("Clayxels Message");
	        window.minSize = new Vector2(500, 200);
	        window.maxSize = new Vector2(500, 200);
	        window.Show();
	    }

	    void OnGUI(){
	    	EditorGUILayout.Space();

	    	GUIStyle s = new GUIStyle();
			s.wordWrap = true;

	    	EditorGUILayout.LabelField(ClayxelsMessageWindow.message, s);

	    	EditorGUILayout.Space();
	    	EditorGUILayout.Space();
			
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Ok")){
				this.OnDestroy();

				this.Close();
			}
    	}

    	void OnDestroy(){
    		if(ClayxelsMessageWindow.onClosedCallback != null){
    			ClayxelsMessageWindow.onClosedCallback();

    			ClayxelsMessageWindow.onClosedCallback = null;
    		}
    	}
    }

    [InitializeOnLoad]
	public class ClayxelsEditorInit{
	    static ClayxelsEditorInit(){
		    if(EditorSettings.enterPlayModeOptions == EnterPlayModeOptions.DisableDomainReload){
		    	Debug.Log("Clayxels Warning: Domain Reload is disabled and will cause issues with resetting some of clayxels internal variables.");
		    }

		    string renderPipeAsset = "";

		    try{
				if(GraphicsSettings.renderPipelineAsset != null){
					renderPipeAsset = GraphicsSettings.renderPipelineAsset.GetType().Name;
				}
			}
			catch{
			}
			
			if(renderPipeAsset == "HDRenderPipelineAsset"){
				#if UNITY_EDITOR_WIN
		    		PlayerSettings.allowUnsafeCode = true;
			    	ClayxelsEditorInit.setDefine("CLAYXELS_RETOPO");
		    	#endif 

				ClayxelsEditorInit.setDefine("CLAYXELS_HDRP");
				ClayxelsEditorInit.checkPackage("HDRP");

				bool isNewHDRP = false;
				int majorVersion = int.Parse(Application.unityVersion.Split('.')[0]);
            	int minorVersion = int.Parse(Application.unityVersion.Split('.')[1]);
            	if(majorVersion > 2020){
        			isNewHDRP = true;
        		}
        		else if(majorVersion >= 2020 && minorVersion > 1){
            		isNewHDRP = true;
            	}
            
            	if(isNewHDRP){
            		string[] oldShaderAssets = AssetDatabase.FindAssets("ClayxelHDRPMeshShader.shader");
            		if(oldShaderAssets.Length > 0){
            			string oldShader = AssetDatabase.GUIDToAssetPath(oldShaderAssets[0]);
            			AssetDatabase.DeleteAsset(oldShader);
            		}
            		
            		oldShaderAssets = AssetDatabase.FindAssets("clayxelHDRPShader.shader");
            		if(oldShaderAssets.Length > 0){
            			string oldShader = AssetDatabase.GUIDToAssetPath(oldShaderAssets[0]);
            			AssetDatabase.DeleteAsset(oldShader);
            		}

            		oldShaderAssets = AssetDatabase.FindAssets("ClayxelHDRPShaderMicroVoxelASE.shader");
            		if(oldShaderAssets.Length > 0){
            			string oldShader = AssetDatabase.GUIDToAssetPath(oldShaderAssets[0]);
            			AssetDatabase.DeleteAsset(oldShader);
            		}
            	}
			}
			else if(renderPipeAsset == "UniversalRenderPipelineAsset"){
				#if UNITY_EDITOR_WIN
		    		PlayerSettings.allowUnsafeCode = true;
			    	ClayxelsEditorInit.setDefine("CLAYXELS_RETOPO");
		    	#endif 

				ClayxelsEditorInit.setDefine("CLAYXELS_URP");
				ClayxelsEditorInit.checkPackage("URP");
			}
			else{
				if(!ClayxelsEditorInit.checkDefined("CLAYXELS_BUILTIN")){
					string msg = "";

					#if UNITY_EDITOR_WIN
						msg = "Hi! Clayxels has a faster and better looking renderer\nwhen using the URP and HDRP render pipelines.\nPlease consider switching pipeline to get the best out of this tool.";
					#else
						msg = "Hi! Clayxels on Mac OS is meant to be used from the URP or HDRP render pipelines.\n";
					#endif

					// this errors on 2021.1
					// ClayxelsMessageWindow.Open();
					// ClayxelsMessageWindow.onClosedCallback = ClayxelsEditorInit.finalizeBuiltinInit;
					
					if(EditorUtility.DisplayDialog("Clayxels Message", msg, "ok")){
						ClayxelsEditorInit.finalizeBuiltinInit();
					}
				}
			}
	    }

	    static void finalizeBuiltinInit(){
	    	#if UNITY_EDITOR_WIN
	    		PlayerSettings.allowUnsafeCode = true;
		    	ClayxelsEditorInit.setDefine("CLAYXELS_RETOPO");
	    	#endif 

	    	ClayxelsEditorInit.setDefine("CLAYXELS_BUILTIN");
	    	ClayxelsEditorInit.checkPackage("BuiltIn");
	    }

	    static bool checkDefined(string defstr){
	    	string currDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup);
	        if(!currDefines.Contains(defstr)){
	        	return false;
	        }

	        return true;
	    }

	    static void setDefine(string defstr){
	    	string currDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup);
	        if(!currDefines.Contains(defstr)){
	    		PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defstr + ";" + currDefines);
	    	}
	    }

		static void checkPackage(string renderPipe){
			string packageName = "clayxelShaders" + renderPipe;

			string[] packageAssets = AssetDatabase.FindAssets(packageName);

			string[] shaderAssets = AssetDatabase.FindAssets("clayxel" + renderPipe + "Shader");
			
			if(packageAssets.Length == 0 && shaderAssets.Length == 0){
				Debug.Log("Clayxels: you appear to be missing the relevant package for this render pipeline, please reimport Clayxels from the Asset Store making sure to include this package: " + packageName);
			}
			else if(shaderAssets.Length == 0 && packageAssets.Length > 0){
				AssetDatabase.ImportPackage(AssetDatabase.GUIDToAssetPath(packageAssets[0]), false);
			}
		}
	}
}

#endif // end if UNITY_EDITOR
