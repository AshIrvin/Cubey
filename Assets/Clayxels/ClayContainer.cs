
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Reflection;
#endif

namespace Clayxels{
	
	/* This class is the main interface to work with Clayxels, it is designed to work in editor and in game.
		Each container nests one or more ClayObject as children to generate the final clay result.
	*/
	[ExecuteInEditMode, DefaultExecutionOrder(-999)]
	public class ClayContainer : MonoBehaviour{
		
		/* CustomMaterial: specify a material that is not the default one. It will need a special shader as shown in the examples provided.*/
		public Material customMaterial = null;

		/* Enable this if you want to animate the clayObjects inside this container or if you need to move them via code. */
		public bool forceUpdate = false;

		/* By default all containers are not interactive unless forceUpdate is set to true.
			When the game starts clayObjects are disabled and the memory used by the container is optimized to limit the use of vram.
			Use this method to re-eanble all clayObjects and perform changes at runtime, then invoke it again with a state of false to optimize the container.
			Only non-interative containers can perform auto-LOD.
			 */
		public void setInteractive(bool state){
			this.forceUpdate = state;

			if(state){
				this.enableAllClayObjects(state);
			}
			else{
				// first optimize currently computed clay
				this.optimizeMemory();

				// then disable clayObjects and avoid triggering a new update
				this.enableAllClayObjects(false);
				this.needsUpdate = false;
			}
		}

		public bool isInteractive(){
			return this.memoryOptimized;
		}

		public void setVisible(bool state){
			this.visible = state;
			this.enabled = state;
		}

		public bool isVisible(){
			return this.visible;
		}

		/* Check if this container will set the bounds automatically. */
		public bool isAutoBoundsActive(){
			return this.autoBounds;
		}

		/* Set this container to update its bounds automatically.
			The bounds of a clayContainer are used to increase the work area in wich clay is processed and displayed. */
		public void setAutoBoundsActive(bool state){
			this.autoBounds = state;

			this.needsInit = true;
			this.init();
		}

		/* Max number of points per chunk (number of chunks is set via boundsScale on the inspector)
        	this only affects video memory while sculpting or moving clayObjects at runtime.
        	num: a bounds size of 1,1,1 can have a max of (256^3) points. 
        	Since 3,3,3 bounds is the max allowed, you can have a max of (256*256*256) * (3*3*3) points.
        	If the user runs out of points, a warning will be issued and logged using clayContainer.getUserWarning();
        	bufferSizeReduceFactor: allows for a size reduction of some of the heavier buffers, min 0.5 (half zize) to max 1.0 (no size reduction)*/
		public static void setPointCloudLimit(int num, float bufferSizeReduceFactor = 1.0f){
			if(num < 256 * 256 * 256){
				num = 256 * 256 * 256;
			}
			else if(num > (256 * 256 * 256) * 27){
				num = (256 * 256 * 256) * 27;
			}

			if(bufferSizeReduceFactor < 0.5f){
				bufferSizeReduceFactor = 0.5f;
			}
			else if(bufferSizeReduceFactor > 1.0f){
				bufferSizeReduceFactor = 1.0f;
			}

			ClayContainer.maxPointCount = num;
			ClayContainer.bufferSizeReduceFactor = bufferSizeReduceFactor;
			ClayContainer.globalDataNeedsInit = true;
		}
		
		/* Skip N frames before updating to reduce stress on GPU and increase FPS count. 
			See ClayxelPrefs.cs */
		public static void setUpdateFrameSkip(int frameSkip){
			ClayContainer.frameSkip = frameSkip;
		}

		/* Set this from 0.0 to 1.0 in order to reduce the max blend that a clayObject can have,
			the smaller the number the better performance will be when evaluating clay. */
		public static void setGlobalBlend(float value){
			if(value < 0.0f){
				value = 0.0f;
			}

			ClayContainer.globalBlend = value;
		}

		/* How many soldis can this container work with in total.
			Valid values: 64, 128, 256, 512, 1024, 4096, 16384
			See ClayxelPrefs.cs */
		public static void setMaxSolids(int num){
			if(!ClayContainer.prefsOverridden){
				ClayContainer.prefsOverridden = true;
				ClayContainer.applyPrefs();
			}

			ClayContainer.maxSolids = num;
			ClayContainer.globalDataNeedsInit = true;
		}

		/* Upon creating a container, clayxels will check if there is enough vram available.
        	The check is a rough estimate, disable this check to get past this limit. */
		public static void enableVideoRamSafeLimit(bool state){
			ClayContainer.vramLimitEnabled = state;
		}

		/* Limit the bounds in x,y,z dimentions. 
			A value of 1 will get you a small area to work with but will use very little video ram.
			A value of 4 is the max limit and it will give you a very large area at the expense of more video ram.
			Video ram needed is occupied upfront, you don't pay this cost for each new container.*/
		public static void setMaxBounds(int value){
			if(!ClayContainer.prefsOverridden){
				ClayContainer.prefsOverridden = true;
				ClayContainer.applyPrefs();
			}

			if(value < 1){
				value = 1;
			}
			else if(value > 4){
				value = 4;
			}

			ClayContainer.maxChunkX = value;
			ClayContainer.maxChunkY = value;
			ClayContainer.maxChunkZ = value;
			ClayContainer.totalMaxChunks = value * value * value;
			ClayContainer.globalDataNeedsInit = true;
		}

		/* How many solids can stay one next to another while occupying the same voxel.
			Keeping this value low will increase overall performance but will cause disappearing clayxels if the number is exceeded.
			Valid values: 32, 64, 128, 256, 512, 1024, 2048
			See ClayxelPrefs.cs */
		public static void setMaxSolidsPerVoxel(int num){
			if(!ClayContainer.prefsOverridden){
				ClayContainer.prefsOverridden = true;
				ClayContainer.applyPrefs();
			}

			ClayContainer.maxSolidsPerVoxel = num;
			ClayContainer.globalDataNeedsInit = true;
		}

		/* Sets how finely detailed are your clayxels, range 0 to 100.*/
		public void setClayxelDetail(int value){
			if(value == this.clayxelDetail || this.frozen || this.needsInit){
				return;
			}
			
			if(ClayContainer.lastUpdatedContainerId != this.GetInstanceID()){
				this.switchComputeData();
			}

			if(value < 0){
				value = 0;
			}
			else if(value > 100){
				value = 100;
			}

			this.clayxelDetail = value;

			this.updateInternalBounds();

			this.forceUpdateAllSolids();
			this.computeClay();
		}

		/* Get the value specified by setClayxelDetail()*/		
		public int getClayxelDetail(){
			return this.clayxelDetail;
		}

		/* Determines how much work area you have for your sculpt within this container.
			These values are not expressed in scene units, 
			the final size of this container is determined by the value specified with setClayxelDetail().
			Performance tip: The bigger the bounds, the slower this container will be to compute clay in-game.*/
		public void setBoundsScale(int x, int y, int z){
			this.chunksX = x;
			this.chunksY = y;
			this.chunksZ = z;
			this.limitChunkValues();

			this.needsInit = true;

			ClayContainer.totalChunksInScene = 0;
		}

		/* Get the values specified by setBoundsScale()*/		
		public Vector3Int getBoundsScale(){
			return new Vector3Int(this.chunksX, this.chunksY, this.chunksZ);
		}

		/* How many solids can a container work with.*/
		public int getMaxSolids(){
			return ClayContainer.maxSolids;
		}

		/* How many solids are currently used in this container.*/
		public int getNumSolids(){
			return this.solids.Count;
		}

		/* How many ClayObjects currently in this container, each ClayObject will spawn a certain amount of Solids.*/
		public int getNumClayObjects(){
			return  this.clayObjects.Count;
		}

		/* Invoke this after adding a new ClayObject in scene to have the container notified instantly.*/
		public void scanClayObjectsHierarchy(){
			this.clayObjects.Clear();
			this.solidsUpdatedDict.Clear();
			this.solids.Clear();
			
			List<ClayObject> collectedClayObjs = new List<ClayObject>();
			this.scanRecursive(this.transform, collectedClayObjs);
			
			for(int i = 0; i < collectedClayObjs.Count; ++i){
				this.collectClayObject(collectedClayObjs[i]);
			}

			this.solidsHierarchyNeedsScan = false;

			if(this.numChunks == 1){
				this.genericIntBufferArray[0] = this.solids.Count;
				ClayContainer.numSolidsPerChunkBuffer.SetData(this.genericIntBufferArray);
			}
		}

		/* Get and own the list of solids in this container. 
			Useful when you don't want a heavy hierarchy of ClayObject in scene (ex. working with particles). */
		public List<Solid> getSolids(){
			return this.solids;
		}

		/* If you work directly with the list of solids in this container, invoke this to notify when a solid has changed.*/
		public void solidUpdated(int id){
			if(id < ClayContainer.maxSolids){
				this.solidsUpdatedDict[id] = 1;

				this.needsUpdate = true;
			}
		}

		/* If you are manipulating the internal list of solids, use this after you add or remove solids in the list.*/
		public void updatedSolidCount(){
			if(this.numChunks == 1){
				this.genericIntBufferArray[0] = this.solids.Count;
				ClayContainer.numSolidsPerChunkBuffer.SetData(this.genericIntBufferArray);
			}
			
			for(int i = 0; i < this.solids.Count; ++i){
				Solid solid = this.solids[i];
				solid.id = i;
				
				if(solid.id < ClayContainer.maxSolids){
					this.solidsUpdatedDict[solid.id] = 1;
				}
				else{
					break;
				}
			}
		}

		/* Set a material with a clayxels-compatible shader or set it to null to return to the standard clayxels shader.*/
		public void setCustomMaterial(Material material){
			this.customMaterial = material;
			this.material = material;

			this.initMaterialProperties();
		}

		/* Automatically invoked once when the game starts, 
			you only need to invoke this yourself if you change what's declared in ClayxelsPrefs.cs at runtime.*/
		static public void initGlobalData(){
			if(!ClayContainer.globalDataNeedsInit){
				return;
			}

			ClayContainer.containersToRender.Clear();
			ClayContainer.containersInScene.Clear();

			ClayContainer.reassignContainerIds();

			#if UNITY_EDITOR
				ClayContainer.checkPrefsIntegrity();
			#endif

			ClayContainer.globalDataNeedsInit = false;

			if(!ClayContainer.prefsOverridden){
				ClayContainer.applyPrefs();
			}

			ClayContainer.totalChunksInScene = 0;

			ClayContainer.numThreadsComputeStartRes = 64 / ClayContainer.maxThreads;
			ClayContainer.numThreadsComputeFullRes = 256 / ClayContainer.maxThreads;

			string renderPipeAsset = "";
			if(GraphicsSettings.renderPipelineAsset != null){
				renderPipeAsset = GraphicsSettings.renderPipelineAsset.GetType().Name;
			}
			
			if(renderPipeAsset == "HDRenderPipelineAsset"){
				ClayContainer.renderPipe = "hdrp";
			}
			else if(renderPipeAsset == "UniversalRenderPipelineAsset"){
				ClayContainer.renderPipe = "urp";
			}
			else{
				ClayContainer.renderPipe = "builtin";
			}

			#if UNITY_EDITOR
				if(!Application.isPlaying){
					ClayContainer.setupScenePicking();
					ClayContainer.pickingMode = false;
					ClayContainer.pickedObj = null;
				}
			#endif

			ClayContainer.reloadSolidsCatalogue();

			ClayContainer.lastUpdatedContainerId = -1;

			ClayContainer.releaseGlobalBuffers();

			#if UNITY_EDITOR// fix reimport issues on unity 2020+
				if(Resources.Load("clayCoreLock") != null){
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Resources.Load("clayCoreLock")), ImportAssetOptions.ForceUpdate);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Resources.Load("clayxelMicroVoxelUtils")), ImportAssetOptions.ForceUpdate);
				}
			#endif

			UnityEngine.Object clayCore = Resources.Load("clayCoreLock");
			if(clayCore == null){
				#if UNITY_EDITOR// fix reimport issues on unity 2020+
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Resources.Load("clayCore")), ImportAssetOptions.ForceUpdate);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Resources.Load("clayxelMicroVoxelUtils")), ImportAssetOptions.ForceUpdate);
				#endif

				clayCore = Resources.Load("clayCore");
			}

			ClayContainer.claycoreCompute = (ComputeShader)Instantiate(clayCore);
			
			ClayContainer.gridDataBuffer = new ComputeBuffer(256 * 256 * 256, sizeof(float) * 3);
			ClayContainer.globalCompBuffers.Add(ClayContainer.gridDataBuffer);

			ClayContainer.gridDataLowResBuffer = new ComputeBuffer(64 * 64 * 64, sizeof(float) * 2);
			ClayContainer.globalCompBuffers.Add(ClayContainer.gridDataLowResBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "gridDataLowRes", ClayContainer.gridDataLowResBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridMip3, "gridDataLowRes", ClayContainer.gridDataLowResBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloudMicroVoxels, "gridDataLowRes", ClayContainer.gridDataLowResBuffer);

			ClayContainer.prefilteredSolidIdsBuffer = new ComputeBuffer((64 * 64 * 64) * ClayContainer.maxSolidsPerVoxel, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.prefilteredSolidIdsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "prefilteredSolidIds", ClayContainer.prefilteredSolidIdsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridMip3, "prefilteredSolidIds", ClayContainer.prefilteredSolidIdsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridForMesh, "prefilteredSolidIds", ClayContainer.prefilteredSolidIdsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloudMicroVoxels, "prefilteredSolidIds", ClayContainer.prefilteredSolidIdsBuffer);
			
			int maxSolidsPerVoxelMask = ClayContainer.maxSolidsPerVoxel / 32;
			ClayContainer.solidsFilterBuffer = new ComputeBuffer((64 * 64 * 64) * maxSolidsPerVoxelMask, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsFilterBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "solidsFilter", ClayContainer.solidsFilterBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridMip3, "solidsFilter", ClayContainer.solidsFilterBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridForMesh, "solidsFilter", ClayContainer.solidsFilterBuffer);
			
			ClayContainer.claycoreCompute.SetInt("maxSolidsPerVoxel", maxSolidsPerVoxel);
			ClayContainer.claycoreCompute.SetInt("maxSolidsPerVoxelMask", maxSolidsPerVoxelMask);

			ClayContainer.claycoreCompute.SetFloat("globalBlendReduce", 1.0f - ClayContainer.globalBlend);
			
			ClayContainer.triangleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.triangleConnectionTable);

			ClayContainer.triangleConnectionTable.SetData(MeshUtils.TriangleConnectionTable);
			
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "triangleConnectionTable", ClayContainer.triangleConnectionTable);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeMesh, "triangleConnectionTable", ClayContainer.triangleConnectionTable);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloudMicroVoxels, "triangleConnectionTable", ClayContainer.triangleConnectionTable);

			ClayContainer.claycoreCompute.SetInt("maxSolids", ClayContainer.maxSolids);

			int numKernels = Enum.GetNames(typeof(Kernels)).Length;
			for(int i = 0; i < numKernels; ++i){
				ClayContainer.claycoreCompute.SetBuffer(i, "gridData", ClayContainer.gridDataBuffer);
			}

			ClayContainer.numSolidsPerChunkBuffer = new ComputeBuffer(64, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridMip3, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridForMesh, "numSolidsPerChunk", ClayContainer.numSolidsPerChunkBuffer);
			
			ClayContainer.solidsUpdatedBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsUpdatedBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "solidsUpdated", ClayContainer.solidsUpdatedBuffer);

			int maxChunks = 64;
			ClayContainer.solidsPerChunkBuffer = new ComputeBuffer(ClayContainer.maxSolids * maxChunks, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridMip3, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridForMesh, "solidsPerChunk", ClayContainer.solidsPerChunkBuffer);
			
			ClayContainer.solidsInSingleChunkArray = new int[ClayContainer.maxSolids];
			for(int i = 0; i < ClayContainer.maxSolids; ++i){
				ClayContainer.solidsInSingleChunkArray[i] = i;
			}

			ClayContainer.meshIndicesBuffer = null;
			ClayContainer.meshVertsBuffer = null;
			ClayContainer.meshColorsBuffer = null;

			ClayContainer.claycoreCompute.SetInt("maxPointCount", ClayContainer.maxPointCount);
			
			// polySplat data
			ClayContainer.pointCloudDataToSolidIdBuffer = new ComputeBuffer(ClayContainer.maxPointCount * ClayContainer.totalMaxChunks, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.pointCloudDataToSolidIdBuffer);

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.compactPointCloud, "pointCloudDataToSolidId", ClayContainer.pointCloudDataToSolidIdBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizePointCloud, "pointCloudDataToSolidId", ClayContainer.pointCloudDataToSolidIdBuffer);

			ClayContainer.chunkPointCloudDataToSolidIdBuffer = new ComputeBuffer(ClayContainer.maxPointCount * ClayContainer.totalMaxChunks, sizeof(int));
			ClayContainer.globalCompBuffers.Add(ClayContainer.chunkPointCloudDataToSolidIdBuffer);

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "chunkPointCloudDataToSolidId", ClayContainer.chunkPointCloudDataToSolidIdBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.compactPointCloud, "chunkPointCloudDataToSolidId", ClayContainer.chunkPointCloudDataToSolidIdBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizePointCloud, "chunkPointCloudDataToSolidId", ClayContainer.chunkPointCloudDataToSolidIdBuffer);

			ClayContainer.claycoreCompute.SetInt("storeSolidId", 0);

			ClayContainer.chunkPointCloudDataBuffer = new ComputeBuffer(ClayContainer.maxPointCount * ClayContainer.totalMaxChunks, sizeof(int) * 2);
			ClayContainer.globalCompBuffers.Add(ClayContainer.chunkPointCloudDataBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "chunkPointCloudData", ClayContainer.chunkPointCloudDataBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.compactPointCloud, "chunkPointCloudData", ClayContainer.chunkPointCloudDataBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizePointCloud, "chunkPointCloudData", ClayContainer.chunkPointCloudDataBuffer);
			
			ClayContainer.pointsInChunkDefaultValues = new int[maxChunks];
			ClayContainer.updateChunksDefaultValues = new int[maxChunks];
			ClayContainer.indirectChunk1DefaultValues = new int[maxChunks * 3];
			ClayContainer.indirectChunk2DefaultValues = new int[maxChunks * 3];

			ClayContainer.microvoxelBoundingBoxData = new int[maxChunks * 6];

			int indirectChunkSize1 = 64 / ClayContainer.maxThreads;
			int indirectChunkSize2 = 256 / ClayContainer.maxThreads;
			
			for(int i = 0; i < maxChunks; ++i){
				ClayContainer.pointsInChunkDefaultValues[i] = 0;

				ClayContainer.updateChunksDefaultValues[i] = 1;

				int indirectChunkId = i * 3;
				ClayContainer.indirectChunk1DefaultValues[indirectChunkId] = indirectChunkSize1;
				ClayContainer.indirectChunk1DefaultValues[indirectChunkId + 1] = indirectChunkSize1;
				ClayContainer.indirectChunk1DefaultValues[indirectChunkId + 2] = indirectChunkSize1;

				ClayContainer.indirectChunk2DefaultValues[indirectChunkId] = indirectChunkSize2;
				ClayContainer.indirectChunk2DefaultValues[indirectChunkId + 1] = indirectChunkSize2;
				ClayContainer.indirectChunk2DefaultValues[indirectChunkId + 2] = indirectChunkSize2;

				ClayContainer.microvoxelBoundingBoxData[i * 6] = 64;
				ClayContainer.microvoxelBoundingBoxData[(i * 6) + 1] = 64;
				ClayContainer.microvoxelBoundingBoxData[(i * 6) + 2] = 64;
				ClayContainer.microvoxelBoundingBoxData[(i * 6) + 3] = 0;
				ClayContainer.microvoxelBoundingBoxData[(i * 6) + 4] = 0;
				ClayContainer.microvoxelBoundingBoxData[(i * 6) + 5] = 0;
			}

			ClayContainer.microvoxelBoundingBoxData[ClayContainer.totalMaxChunks * 6] = 0; // storing the chunkId offset used by the first chunk, which is always zero

			if(ClayContainer.renderPipe != "builtin"){
				ClayContainer.initMicroVoxelGlobal();
			}

			ClayContainer.globalDataNeedsInit = false;

			#if UNITY_EDITOR
				// make sure all containers are initialized after this call
				ClayContainer[] containers = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
	    		for(int i = 0; i < containers.Length; ++i){
	    			ClayContainer container = containers[i];
	    			container.needsInit = true;
	    		}
        	#endif

			#if UNITY_EDITOR_OSX
				// on mac disable warnings about missing bindings
				PlayerSettings.enableMetalAPIValidation = false;
			#endif
		}

		/* If you happen to change one of the global settings at runtime, 
			this will make sure all containers are properly reinitialized to reflect those changes. */
		static public void forceAllContainersInit(){
			ClayContainer[] containers = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
    		for(int i = 0; i < containers.Length; ++i){
    			ClayContainer container = containers[i];
    			container.needsInit = true;

    			if(!container.frozen && container.gameObject.activeSelf && container.enabled && container.instanceOf == null){
    				container.enableAllClayObjects(true);
    				container.needsInit = true;
    				container.init();
    			}
    		}
		}

		/* Automatically invoked once when the game starts, 
			you only need to invoke this yourself if you change chunkSize or chunksX,Y,Z attributes.*/
		public void init(){
			if(!this.needsInit){
				return;
			}

			this.needsInit = false;
			
			this.checkContainerId();

			#if UNITY_EDITOR
				if(!Application.isPlaying){
					ClayContainer.checkNeedsGlobalInit();

					this.reinstallEditorEvents();

					if(PrefabUtility.IsPartOfAnyPrefab(this.gameObject)){
						this.setupPrefab();
					}
				}
			#endif

			if(ClayContainer.globalDataNeedsInit){
				ClayContainer.initGlobalData();
			}

			this.needsInit = false;

			if(ClayContainer.renderPipe == "builtin"){
				this.renderMode = ClayContainer.RenderModes.polySplat;
			}

			this.pointCount = 0;

			this.memoryOptimized = false;

			this.editingThisContainer = false;

			this.userWarning = "";

			this.releaseBuffers();

			this.addToScene();

			if(this.instanceOf != null){
				this.initInstance();

				return;
			}

			this.instancingOtherContainer = false;
			
			if(this.frozen){
				this.releaseBuffers();
				return;
			}

			bool vramAvailable = this.checkVRam();
			if(!vramAvailable){
				this.enabled = false;
				Debug.Log("Clayxels: you have reached the maximum amount of containers for your available video ram.\nTo increase this limit, open ClayxelsPrefs.cs and lower the maximum amount of chunks from ClayContainer.setMaxBounds().");
				return;
			}

			this.memoryOptimized = false;

			this.chunkSize = (int)Mathf.Lerp(40.0f, 4.0f, (float)this.clayxelDetail / 100.0f);
			this.limitChunkValues();

			this.clayObjects.Clear();
			this.solidsUpdatedDict.Clear();

			this.solidsHierarchyNeedsScan = true;
			this.scanClayObjectsHierarchy();

			this.voxelSize = (float)this.chunkSize / 256;
			this.splatRadius = this.voxelSize * ((this.transform.lossyScale.x + this.transform.lossyScale.y + this.transform.lossyScale.z) / 3.0f);

			this.initChunks();

			this.autoFrameSkip = this.numChunks / 5;
			this.autoBoundsChunkSize = 0;
			if(this.autoBounds){
				this.initAutoBounds();
			}

			this.globalSmoothing = this.voxelSize;
			ClayContainer.claycoreCompute.SetFloat("globalRoundCornerValue", this.globalSmoothing);
			
			this.genericNumberBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
			this.compBuffers.Add(this.genericNumberBuffer);

			this.needsUpdate = true;
			ClayContainer.lastUpdatedContainerId = -1;

			this.initMaterialProperties();

			this.initSolidsData();

			this.computeClay();

			this.updateFrame = 0;

			if(ClayContainer.renderPipe != "builtin"){
				this.initMicroVoxelBuffer();
			}

			#if UNITY_EDITOR
				if(!Application.isPlaying){
					if(UnityEditor.Selection.Contains(this.gameObject)){
						this.editingThisContainer = true;
					}
				}
			#endif

			if(this.clayObjects.Count > 0 && !this.forceUpdate && !this.editingThisContainer){
				this.optimizeMemory();

				if(Application.isPlaying){
					this.enableAllClayObjects(false);
				}

				this.microvoxelsEditorDelayedOptimize = true;
			}

			this.needsInit = false;
		}

		/* Spawn a new ClayObject in scene under this container.*/
		public ClayObject addClayObject(){
			GameObject clayObj = new GameObject("clay_cube+");
			clayObj.transform.parent = this.transform;
			clayObj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

			ClayObject clayObjComp = clayObj.AddComponent<ClayObject>();
			clayObjComp.clayxelContainerRef = new WeakReference(this);
			clayObjComp.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

			this.collectClayObject(clayObjComp);

			this.needsUpdate = true;

			return clayObjComp;
		}

		/* Get a ClayObject inside this container by id.*/
		public ClayObject getClayObject(int id){
			return this.clayObjects[id];
		}

		/* Scan for ClayObjects in this container at the next update.*/
		public void scheduleClayObjectsScan(){
			this.solidsHierarchyNeedsScan = true;
			this.needsUpdate = true;
		}

		/* Invoke this when you need all solids in a container to be updated, ex. if you change the material attributes.*/
		public void forceUpdateAllSolids(){
			for(int i = 0; i < this.solids.Count; ++i){
				int id = this.solids[i].id;
				if(id < ClayContainer.maxSolids){
					this.solidsUpdatedDict[id] = 1;
				}
				else{
					break;
				}
			}

			this.needsUpdate = true;
		}

		/* Notify this container that one of the nested ClayObject has changed.*/
		public void clayObjectUpdated(ClayObject clayObj){
			if(!this.transform.hasChanged || this.forceUpdate){
				for(int i = 0; i < clayObj.getNumSolids(); ++i){
					int id = clayObj.getSolid(i).id;
					if(id < ClayContainer.maxSolids){
						this.solidsUpdatedDict[id] = 1;
					}
				}

				this.needsUpdate = true;
			}
		}

		/* Set the visual quality of splats when they get far away from camera. 
			Goes from 0.0f (low quality even if close to camera) to 1.0f (high quality even if far away from camera)*/
		public void setSplatsLOD(float value){
			this.splatsLOD = (1.0f - Mathf.Clamp(value, 0.0f, 1.0f)) * 100.0f;
		}

		public float getSplatsLOD(){
			return 1.0f - (this.splatsLOD / 100.0f);
		}
		
		/* Get the material currently in use by this container. */
		public Material getMaterial(){
			return this.material;
		}

		/* Force this container to compute the final clay result now.
			Useful if you have set frame skips or limited the chunks to update per frame.*/
		public void computeClay(){
			if(this.needsInit){
				return;
			}

			if(this.solidsHierarchyNeedsScan){
				this.scanClayObjectsHierarchy();
			}

			if(ClayContainer.lastUpdatedContainerId != this.GetInstanceID()){
				this.switchComputeData();
			}
			
			if(this.autoBounds){
				float boundsSize = this.computeBoundsSize();
				this.updateAutoBounds(boundsSize);
			}

			if(this.memoryOptimized){
				this.expandMemory();
			}
			
			this.needsUpdate = false;
			this.updateFrame = 0;

			this.updateSolids();

			if(this.renderMode == ClayContainer.RenderModes.polySplat){
				this.computeClayPolySplat();
			}
			else{
				this.computeClayMicroVoxel();
			}
		}

		void setLOD(ClayContainer instance, int level){
			// if(level < 0){
			// 	level = 0;
			// }
			// else if(level > 100){
			// 	level = 100;
			// }

			// instance.LODLevel = level;

			// float t = (float)level / 100;
			// float invT = 1.0f - t;

			// float curveT = 1.0f - (invT * invT * invT * invT);
			// int pointReducer = (int)Mathf.Round(Mathf.Lerp(1, 20, t));
			// float splatMultiplier = Mathf.Lerp(1.5f, 5.0f, curveT);

			// int numPoints = this.pointCount / pointReducer;
			
			// ClayContainer.indirectArgsData[0] = numPoints * 3;

			// instance.indirectDrawArgsBuffer2.SetData(ClayContainer.indirectArgsData);
			// instance.splatRadius = (this.voxelSize * ((instance.transform.lossyScale.x + instance.transform.lossyScale.y + instance.transform.lossyScale.z) / 3.0f)) * splatMultiplier;
		}

		/* */
		public void setCastShadows(bool state){
			if(state){
				this.castShadows = ShadowCastingMode.On;
			}
			else{
				this.castShadows = ShadowCastingMode.Off;
			}
		}

		/* */
		public bool getCastShadows(){
			if(this.castShadows == ShadowCastingMode.On){
				return true;
			}

			return false;
		}

		/* */
		public void setReceiveShadows(bool state){
			this.receiveShadows = state;
		}

		/* */
		public bool getReceiveShadows(){
			return this.receiveShadows;
		}

		/* Schedule a draw call, 
			this is only useful if you disable this container's Update and want to manually draw its content.
			Instance can be set to "this" if you want to draw this container, or to another container entirely if
			you want to draw an instance of that other container.
			When you pass another container as instance, only some of its attributes will be used such (position,rotation,scale)
			thus making it irrelevant on the vram budget.
		*/
		public void drawClayxels(ClayContainer instance){
			if(this.renderMode == ClayContainer.RenderModes.polySplat){
				this.drawClayxelsPolySplat(instance);
			}
			else{
				this.drawClayxelsMicroVoxel();
			}
		}

		/* Returns a mesh at the specified level of detail, clayxelDetail will range from 0 to 100.
			Useful to generate mesh colliders, to improve performance leave colorizeMesh and generateNormals to false.*/
		public Mesh generateMesh(int detail, bool colorizeMesh = false, bool computeNormals = false, float smoothNormalAngle = 100.0f){
			if(ClayContainer.lastUpdatedContainerId != this.GetInstanceID()){
				this.switchComputeData();
			}

			this.bindSolidsBuffers((int)Kernels.computeGridForMesh);

			int prevDetail = this.clayxelDetail;

			if(detail != this.clayxelDetail){
				this.setClayxelDetail(detail);
			}

			if(computeNormals){
				colorizeMesh = true;
			}
			
			if(ClayContainer.meshIndicesBuffer == null){
				ClayContainer.meshIndicesBuffer = new ComputeBuffer(ClayContainer.maxPointCount*6, sizeof(int) * 3, ComputeBufferType.Counter);
				ClayContainer.globalCompBuffers.Add(ClayContainer.meshIndicesBuffer);
				
				ClayContainer.meshVertsBuffer = new ComputeBuffer(ClayContainer.maxPointCount*6, sizeof(float) * 3);
				ClayContainer.globalCompBuffers.Add(ClayContainer.meshVertsBuffer);

				ClayContainer.meshColorsBuffer = new ComputeBuffer(ClayContainer.maxPointCount*6, sizeof(float) * 4);
				ClayContainer.globalCompBuffers.Add(ClayContainer.meshColorsBuffer);
			}

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeMesh, "meshOutIndices", ClayContainer.meshIndicesBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeMesh, "meshOutPoints", ClayContainer.meshVertsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeMesh, "meshOutColors", ClayContainer.meshColorsBuffer);

			List<Vector3> totalVertices = null;
			List<int> totalIndices = null;
			List<Color> totalColors = null;

			if(this.numChunks > 1){
				totalVertices = new List<Vector3>();
				totalIndices = new List<int>();

				if(colorizeMesh){
					totalColors = new List<Color>();
				}
			}

			int totalNumVerts = 0;

			Mesh mesh = new Mesh();
			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			ClayContainer.claycoreCompute.SetInt("numSolids", this.solids.Count);
			ClayContainer.claycoreCompute.SetFloat("chunkSize", (float)this.chunkSize);

			this.forceUpdateAllSolids();

			ClayContainer.claycoreCompute.SetFloat("seamOffsetMultiplier", 3.0f);// special case for seems in meshed chunks

			this.computeClay();

			this.userWarning = "";

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridForMesh, "chunksCenter", this.chunksCenterBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeMesh, "chunksCenter", this.chunksCenterBuffer);

			for(int chunkIt = 0; chunkIt < this.numChunks; ++chunkIt){
				ClayContainer.meshIndicesBuffer.SetCounterValue(0);

				ClayContainer.claycoreCompute.SetInt("chunkId", chunkIt);

				ClayContainer.claycoreCompute.Dispatch((int)Kernels.computeGridForMesh, ClayContainer.numThreadsComputeStartRes, ClayContainer.numThreadsComputeStartRes, ClayContainer.numThreadsComputeStartRes);
				
				ClayContainer.claycoreCompute.SetInt("outMeshIndexOffset", totalNumVerts);
				ClayContainer.claycoreCompute.Dispatch((int)Kernels.computeMesh, ClayContainer.numThreadsComputeFullRes, ClayContainer.numThreadsComputeFullRes, ClayContainer.numThreadsComputeFullRes);

				int numTris = this.getBufferCount(ClayContainer.meshIndicesBuffer);
				int numVerts = numTris * 3;

				if(numVerts > ClayContainer.maxPointCount * 6){
					this.userWarning = "max point count exceeded, increase limit from Global Config window";
					Debug.Log("Clayxels: container " + this.gameObject.name + " has exceeded the limit of points allowed, increase limit from Global Config window");
					mesh = null;

					break;
				}

				totalNumVerts += numVerts;
				
				if(mesh != null){
					if(this.numChunks > 1){
						Vector3[] vertices = new Vector3[numVerts];
						ClayContainer.meshVertsBuffer.GetData(vertices);

						int[] indices = new int[numVerts];
						ClayContainer.meshIndicesBuffer.GetData(indices);

						totalVertices.AddRange(vertices);
						totalIndices.AddRange(indices);

						if(colorizeMesh){
							Color[] colors = new Color[numVerts];
							ClayContainer.meshColorsBuffer.GetData(colors);

							totalColors.AddRange(colors);
						}
					}
				}
			}

			if(mesh != null){
				if(this.numChunks > 1){
					mesh.vertices = totalVertices.ToArray();
					mesh.triangles = totalIndices.ToArray();

					if(colorizeMesh){
						mesh.colors = totalColors.ToArray();
					}
				}
				else{
					Vector3[] vertices = new Vector3[totalNumVerts];
					ClayContainer.meshVertsBuffer.GetData(vertices);

					mesh.vertices = vertices;

					int[] indices = new int[totalNumVerts];
					ClayContainer.meshIndicesBuffer.GetData(indices);

					mesh.triangles = indices;

					if(colorizeMesh){
						Color[] colors = new Color[totalNumVerts];
						meshColorsBuffer.GetData(colors);

						mesh.colors = colors;
					}
				}

				if(computeNormals){
					MeshUtils.smoothNormals(mesh, smoothNormalAngle);
				}
			}

			if(prevDetail != this.clayxelDetail){
				this.setClayxelDetail(prevDetail);
			}

			return mesh;
		}

		/* Freeze this container to a mesh. Specify meshDetail from 0 to 100.*/
		public void freezeToMesh(int meshDetail){
			if(this.needsInit){
				this.init();
			}

			if(this.gameObject.GetComponent<MeshFilter>() == null){
				this.gameObject.AddComponent<MeshFilter>();
			}
			
			MeshRenderer render = this.gameObject.GetComponent<MeshRenderer>();
			if(render == null){
				render = this.gameObject.AddComponent<MeshRenderer>();
			}
			render.enabled = true;

			Material mat = render.sharedMaterial;
			if(mat == null){
				if(ClayContainer.renderPipe == "hdrp"){
					string renderModeMatSuffix = "";
					if(ClayContainer.renderPipe == "hdrp"){
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
		            		renderModeMatSuffix += "_2020_2";
		            	}
		            }
		            
					render.sharedMaterial = new Material(Shader.Find("Clayxels/ClayxelHDRPMeshShader" + renderModeMatSuffix));
				}
				else if(ClayContainer.renderPipe == "urp"){
					render.sharedMaterial = new Material(Shader.Find("Clayxels/ClayxelURPMeshShader"));
				}
				else{
					render.sharedMaterial = new Material(Shader.Find("Clayxels/ClayxelBuiltInMeshShader"));
				}
			}
			
			if(meshDetail < 0){
				meshDetail = 0;
			}
			else if(meshDetail > 100){
				meshDetail = 100;
			}

			bool vertexColors = true;
			bool smoothNormals = true;
			Mesh mesh = this.generateMesh(meshDetail, vertexColors, smoothNormals, this.meshNormalSmooth);
			if(mesh == null){
				return;
			}

			this.frozen = true;
			this.enabled = false;
			
			this.gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;

			this.releaseBuffers();

			this.enableAllClayObjects(false);
		}

		/* Transfer every material attribue found with the same name from this container's material, to the generated mesh material. */
		public void transferMaterialPropertiesToMesh(){
			MeshRenderer render = this.gameObject.GetComponent<MeshRenderer>();
			if(render == null){
				return;
			}
			
			for(int propertyId = 0; propertyId < this.material.shader.GetPropertyCount(); ++propertyId){
				ShaderPropertyType type = this.material.shader.GetPropertyType(propertyId);
				string name = this.material.shader.GetPropertyName(propertyId);
				
				if(render.sharedMaterial.shader.FindPropertyIndex(name) != -1){
					if(type == ShaderPropertyType.Color || type == ShaderPropertyType.Vector){
						render.sharedMaterial.SetVector(name, this.material.GetVector(name));
					}
					else if(type == ShaderPropertyType.Float || type == ShaderPropertyType.Range){
						render.sharedMaterial.SetFloat(name, this.material.GetFloat(name));
					}
					else if(type == ShaderPropertyType.Texture){
						render.sharedMaterial.SetTexture(name, this.material.GetTexture(name));
					}
				}
			}
		}

		/* Is this container using a mesh filter to display a mesh? */
		public bool isFrozenToMesh(){
			if(this.frozen && this.gameObject.GetComponent<MeshFilter>() != null){
				return true;
			}

			return false;
		}

		public bool isFrozen(){
			return this.frozen;
		}

		/* Disable the frozen state and get back to live clayxels. */
		public void defrostToLiveClayxels(){
			this.frozen = false;
			this.needsInit = true;
			this.enabled = true;

			#if UNITY_EDITOR
				this.retopoApplied = false;
			#endif

			if(this.gameObject.GetComponent<MeshFilter>() != null){
				DestroyImmediate(this.gameObject.GetComponent<MeshFilter>());
			}

			if(this.gameObject.GetComponent<MeshRenderer>() != null){
				this.gameObject.GetComponent<MeshRenderer>().enabled = false;
			}

			Claymation claymation = this.gameObject.GetComponent<Claymation>();
			if(claymation != null){
				claymation.enabled = false;
			}

			this.enableAllClayObjects(true);
		}

		/* ClayContainers that have forceUpdate set to false will disable all their clayObjects when the game starts.
			Use this method to re-enable all clayObjects in a container while the game is running. */
		public void enableAllClayObjects(bool state){
			List<GameObject> objs = new List<GameObject>();
			ClayContainer.collectClayObjectsRecursive(this.gameObject, ref objs);
			
			for(int i = 0; i < objs.Count; ++i){
				objs[i].SetActive(state);
			}
		}

		/* Freeze this container plus all the containers that are part of this hierarchy. */
		public void freezeContainersHierarchyToMesh(){
			ClayContainer[] containers = this.GetComponentsInChildren<ClayContainer>();
			for(int i = 0; i < containers.Length; ++i){
				ClayContainer container = containers[i];
				container.needsInit = true;
				container.editingThisContainer = false;
				container.freezeToMesh(this.clayxelDetail);

				container.transferMaterialPropertiesToMesh();
			}
		}

		/* Smooth normals on this container after it got frozen to mesh, plus all the containers that are part of this hierarchy. */
		public void smoothNormalsContainersHierarchy(float smoothNormalAngle){
			ClayContainer[] containers = this.GetComponentsInChildren<ClayContainer>();
			for(int i = 0; i < containers.Length; ++i){
				ClayContainer container = containers[i];
				if(container.isFrozenToMesh()){
					MeshUtils.smoothNormals(container.GetComponent<MeshFilter>().sharedMesh, smoothNormalAngle);
				}
			}
		}

		/* Defrost this container plus all the containers that are part of this hierarchy. */
		public void defrostContainersHierarchy(){
			ClayContainer[] containers = this.GetComponentsInChildren<ClayContainer>();
			for(int i = 0; i < containers.Length; ++i){
				containers[i].defrostToLiveClayxels();
			}
		}

		/* Set this container to be an instance of another container, or set this to null to remove an old instance link.
			Instances have the advantage of not consuming vram budget and they won't increase draw calls. */
		public void setIsInstanceOf(ClayContainer sourceContainer){
			if(ClayContainer.renderPipe == "builtin"){
				return;
			}

			if(ClayContainer.containersToRender.Contains(this)){
				ClayContainer.containersToRender.Remove(this);
			}

			if(this.instanceOf == sourceContainer){
				return;
			}

			this.needsInit = true;

			ClayContainer oldSourceContainer = this.instanceOf;
			
			this.instanceOf = sourceContainer;
			this.instancingOtherContainer = true;

			if(this.instanceOf != null){
				if(!this.instanceOf.instances.Contains(this)){
					this.instanceOf.instances.Add(this);
				}

				this.instanceOf.initInstancesData();
			}
			else{
				this.addToScene();
			}

			if(oldSourceContainer != null){
				List<ClayContainer> oldSourceInstances = new List<ClayContainer>();

				for(int i = 0; i < oldSourceContainer.instances.Count; ++i){
					if(oldSourceContainer.instances[i].instanceOf == oldSourceContainer){
						oldSourceInstances.Add(oldSourceContainer.instances[i]);
					}
				}

				oldSourceContainer.instances = oldSourceInstances;
				oldSourceContainer.initInstancesData();
			}
		}

		public ClayContainer getInstanceOf(){
			return this.instanceOf;
		}

		public Bounds getRenderBounds(){
			return this.renderBounds;
		}

		/* When using microvoxels clayxels will render to a texture and you can override its resolution.
		 	Higher resolutions will decrease performance at runtime and improve visual quality. 
		 	Using this method at turntime will disable using the value from the globalPrefs. */
		public static void setOutputRenderTextureSize(int width, int height){
			if(!ClayContainer.prefsOverridden){
				ClayContainer.prefsOverridden = true;
				ClayContainer.applyPrefs();
			}

			ClayContainer.microvoxelRTSizeOverride = new Vector2Int(width, height);
			ClayContainer.globalDataNeedsInit = true;
		}

		/* used to communicate with the user, for example if user runs out of points as specified by clayContainer.setPointCloudLimit()
		*/
		public string getUserWarning(){
			return ClayContainer.globalUserWarning + this.userWarning;
		}

		// public members for internal use

		public static void reloadSolidsCatalogue(){
			ClayContainer.solidsCatalogueLabels.Clear();
			ClayContainer.solidsCatalogueParameters.Clear();

			int lastParsed = -1;
			try{
				string claySDF = ((TextAsset)Resources.Load("claySDF", typeof(TextAsset))).text;
				ClayContainer.parseSolidsAttrs(claySDF, ref lastParsed);

				string numThreadsDef = "MAXTHREADS";
				ClayContainer.maxThreads = (int)char.GetNumericValue(claySDF[claySDF.IndexOf(numThreadsDef) + numThreadsDef.Length + 1]);
			}
			catch{
				Debug.Log("error trying to parse parameters in claySDF.compute, solid #" + lastParsed);
			}
		}

		public string[] getSolidsCatalogueLabels(){
			return ClayContainer.solidsCatalogueLabels.ToArray();
		}
		
		public static List<string[]> getSolidsCatalogueParameters(int solidId){
			return ClayContainer.solidsCatalogueParameters[solidId];
		}

		public bool isClayObjectsOrderLocked(){
			return this.clayObjectsOrderLocked;
		}

		public void setClayObjectsOrderLocked(bool state){
			this.clayObjectsOrderLocked = state;
		}

		public void reorderClayObject(int clayObjOrderId, int offset){
			List<ClayObject> tmpList = new List<ClayObject>(this.clayObjects.Count);
			for(int i = 0; i < this.clayObjects.Count; ++i){
				tmpList.Add(this.clayObjects[i]);
			}
			
			int newOrderId = clayObjOrderId + offset;
			if(newOrderId < 0){
				newOrderId = 0;
			}
			else if(newOrderId > this.clayObjects.Count - 1){
				newOrderId = this.clayObjects.Count - 1;
			}
			
			ClayObject clayObj1 = tmpList[clayObjOrderId];
			ClayObject clayObj2 = tmpList[newOrderId];

			tmpList.Remove(clayObj1);
			tmpList.Insert(newOrderId, clayObj1);

			clayObj1.clayObjectId = tmpList.IndexOf(clayObj1);
			clayObj2.clayObjectId = tmpList.IndexOf(clayObj2);

			if(this.clayObjectsOrderLocked){
				clayObj1.transform.SetSiblingIndex(tmpList.IndexOf(clayObj1));
			}
			
			this.scanClayObjectsHierarchy();
		}

		public bool checkVRam(){
			if(!ClayContainer.vramLimitEnabled){
				return true;
			}

			if(ClayContainer.totalChunksInScene <= 0){
				ClayContainer.totalChunksInScene = 0;
				ClayContainer[] clayxelObjs = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
				for(int i = 0; i < clayxelObjs.Length; ++i){
					ClayContainer container = clayxelObjs[i];
					if(container.enabled && container.instanceOf == null){
						ClayContainer.totalChunksInScene += container.numChunks;
					}
				}
			}

			int mbPerContainer = (64 * ClayContainer.maxPointCount) / 8000000;// bit to mb
			int sceneContainersMb = mbPerContainer * ClayContainer.totalChunksInScene;
			int memoryLimit = SystemInfo.graphicsMemorySize - mbPerContainer;

			bool vramOk = true;
			if(sceneContainersMb > memoryLimit){
				vramOk = false;
			}

			return vramOk;
		}

		// deprecated: microvoxels has auto LOD by default
		public static float getAutoLODNear(){
			return ClayContainer.autoLODNear;
		}

		// deprecated: microvoxels has auto LOD by default
		public static float getAutoLODFar(){
			return ClayContainer.autoLODFar;
		}

		// deprecated: microvoxels has auto LOD by default
		public static void setAutoLOD(float near, float far){
			Debug.Log("deprecated: microvoxels has auto LOD by default");

			// ClayContainer.autoLODNear = near;
			// ClayContainer.autoLODFar = far;

			// if(ClayContainer.autoLODFar - ClayContainer.autoLODNear > 0.0f){
			// 	ClayContainer.lodActive = true;
			// }
			// else{
			// 	ClayContainer.lodActive = false;
			// }
		}

		public ComputeBuffer getPointCloudBuffer(){
			Debug.Log("getPointCloudBuffer is being deprecated and will only work for poly-splats");

			return this.pointCloudDataMip3Buffer;
		}

		// deprecated
		public static void setupPicking(){
			ClayContainer.pickingCommandBuffer = new CommandBuffer();
			
			ClayContainer.pickingTextureResult = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			ClayContainer.pickingRect = new Rect(0, 0, 1, 1);

			if(ClayContainer.pickingRenderTexture != null){
				ClayContainer.pickingRenderTexture.Release();
				ClayContainer.pickingRenderTexture = null;
			}

			ClayContainer.pickingRenderTexture = new RenderTexture(1024, 768, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			ClayContainer.pickingRenderTexture.Create();
			ClayContainer.pickingRenderTextureId = new RenderTargetIdentifier(ClayContainer.pickingRenderTexture);

			if(ClayContainer.pickingMaterialPolySplat ==  null){
				Shader pickingShader = Shader.Find("Clayxels/ClayxelPickingShader");
				ClayContainer.pickingMaterialPolySplat = new Material(pickingShader);
				ClayContainer.pickingMaterialPropertiesPolySplat = new MaterialPropertyBlock();
			}
		}

		private static bool PerformingAsyncRead = false;
		private static Action<int , int> CurrentCompleteAction;
		public static void performPickingAsync( ClayContainer[] containers , bool shouldPickClayObjects , Camera camera , float mousePosX , float mousePosY , bool invertVerticalMouseCoords , Action<int,int> OnCompleteAction )
		{
			if( PerformingAsyncRead )
				return;

			if( ( mousePosX < 0 || mousePosX >= camera.pixelWidth ||
				mousePosY < 0 || mousePosY >= camera.pixelHeight ) )
			{
				return;
			}

			//Aguiar: Pulling this out of the for loop
			int rectWidth = (int)( 1024.0f * ( (float)mousePosX / (float)camera.pixelWidth ) );
			int rectHeight = (int)( 768.0f * ( (float)mousePosY / (float)camera.pixelHeight ) );
			if( invertVerticalMouseCoords )
			{
				rectHeight = 768 - rectHeight;
			}

			//Aguiar: Also testing this edge case to prevent out of bounds
			if( ( rectWidth + 1 ) > 1024 || ( rectHeight + 1 ) > 768 )
			{
				return;
			}

			PerformingAsyncRead = true;
			if( containers == null )
				containers = GameObject.FindObjectsOfType<ClayContainer>();

			ClayContainer.pickingRect.Set(
				rectWidth ,
				rectHeight ,
				1 , 1 );


			if( shouldPickClayObjects )
			{
				if( ClayContainer.lastPickedContainerId > -1 && ClayContainer.pickedContainerId != ClayContainer.lastPickedContainerId && ClayContainer.lastPickedContainerId < containers.Length )
				{
					ClayContainer lastContainer = containers[ ClayContainer.lastPickedContainerId ];
					lastContainer.pickingThis = false;
					ClayContainer.lastPickedContainerId = -1;
				}
				if( ClayContainer.pickedContainerId > -1 && ClayContainer.pickedContainerId < containers.Length )
				{
					ClayContainer container = containers[ ClayContainer.pickedContainerId ];
					ClayContainer.lastPickedContainerId = ClayContainer.pickedContainerId;
					if( !container.pickingThis )
					{
						ClayContainer.claycoreCompute.SetInt( "storeSolidId" , 1 );
						container.forceUpdateAllSolids();
						container.computeClay();
						ClayContainer.claycoreCompute.SetInt( "storeSolidId" , 0 );
					}
					container.pickingThis = true;
				}
			}
			ClayContainer.pickedClayObjectId = -1;
			ClayContainer.pickedContainerId = -1;
			ClayContainer.pickingCommandBuffer.Clear();
			ClayContainer.pickingCommandBuffer.SetRenderTarget( ClayContainer.pickingRenderTexture );
			ClayContainer.pickingCommandBuffer.ClearRenderTarget( true , true , Color.black , 1.0f );
			for( int i = 0 ; i < containers.Length ; ++i )
			{
				ClayContainer container = containers[ i ];
				if( container.enabled )
				{
					container.drawClayxelPicking( i , ClayContainer.pickingCommandBuffer , shouldPickClayObjects );
				}
			}
			ClayContainer.pickingCommandBuffer.RequestAsyncReadback( ClayContainer.pickingRenderTexture , 0 , rectWidth , 1 , rectHeight , 1 , 0 , 1 , TextureFormat.ARGB32 , CheckAsyncRead );
			//ClayContainer.pickingCommandBuffer.WaitAllAsyncReadbackRequests();
			Graphics.ExecuteCommandBuffer( ClayContainer.pickingCommandBuffer );
			CurrentCompleteAction = OnCompleteAction;
		}

		private static void CheckAsyncRead( AsyncGPUReadbackRequest request )
		{
			PerformingAsyncRead = false;
			if( request.hasError )
			{
				Debug.Log( "GPU readback error detected." );
				return;
			}

			NativeArray<byte> data = request.GetData<byte>();

			int pickId = (int)( ( data[ 1 ] + data[ 2 ] * 255.0f + data[ 3 ] * 255.0f ) );
			ClayContainer.pickedClayObjectId = pickId - 1;
			ClayContainer.pickedContainerId = data[ 0 ];
			if( ClayContainer.pickedContainerId >= 255 )
			{
				ClayContainer.pickedContainerId = -1;
			}

			if( CurrentCompleteAction != null )
			{
				CurrentCompleteAction( pickedContainerId , pickedClayObjectId );
			}

		}
		
		public static void performPicking( ClayContainer[] containers, bool shouldPickClayObjects, Camera camera, float mousePosX, float mousePosY, bool invertVerticalMouseCoords, out int pickedContainerId, out int pickedClayObjectId){
			if(mousePosX < 0 || mousePosX >= camera.pixelWidth || 
				mousePosY < 0 || mousePosY >= camera.pixelHeight){
				pickedContainerId = -1;
				pickedClayObjectId = -1;
				return;
			}
			//Aguiar: Major optimization, we do our own cache and send it instead of always doing GameObject.FindObjectsOfType
			if( containers == null )
				containers = GameObject.FindObjectsOfType<ClayContainer>();
			if(shouldPickClayObjects){
				if(ClayContainer.lastPickedContainerId > -1 && ClayContainer.pickedContainerId != ClayContainer.lastPickedContainerId && ClayContainer.lastPickedContainerId < containers.Length){
					ClayContainer lastContainer = containers[ClayContainer.lastPickedContainerId];
					lastContainer.pickingThis = false;
					ClayContainer.lastPickedContainerId = -1;
				}

				if(ClayContainer.pickedContainerId > -1 && ClayContainer.pickedContainerId < containers.Length){
					ClayContainer container = containers[ClayContainer.pickedContainerId];
					ClayContainer.lastPickedContainerId = ClayContainer.pickedContainerId;

					if(container.renderMode == ClayContainer.RenderModes.polySplat){
						if(!container.pickingThis){
							ClayContainer.claycoreCompute.SetInt("storeSolidId", 1);
							container.forceUpdateAllSolids();
				  			container.computeClay();
				  			ClayContainer.claycoreCompute.SetInt("storeSolidId", 0);
				  		}
				  	}

					container.pickingThis = true;
				}
			}
			ClayContainer.pickedClayObjectId = -1;
	  		ClayContainer.pickedContainerId = -1;
			ClayContainer.pickingCommandBuffer.Clear();
			ClayContainer.pickingCommandBuffer.SetRenderTarget(ClayContainer.pickingRenderTextureId);
			ClayContainer.pickingCommandBuffer.ClearRenderTarget(true, true, Color.black, 1.0f);
			for(int i = 0; i < containers.Length; ++i){
				ClayContainer container = containers[i];
				if(container.enabled){
					container.drawClayxelPicking(i, ClayContainer.pickingCommandBuffer, shouldPickClayObjects);
				}
			}
			Graphics.ExecuteCommandBuffer(ClayContainer.pickingCommandBuffer);
			int rectWidth = (int)(1024.0f * ((float)mousePosX / (float)camera.pixelWidth ));
			int rectHeight = (int)(768.0f * ((float)mousePosY / (float)camera.pixelHeight));
			if(invertVerticalMouseCoords){
				rectHeight = 768 - rectHeight;
			}
			ClayContainer.pickingRect.Set(
				rectWidth, 
				rectHeight, 
				1, 1);
			RenderTexture oldRT = RenderTexture.active;
			RenderTexture.active = ClayContainer.pickingRenderTexture;
			ClayContainer.pickingTextureResult.ReadPixels(ClayContainer.pickingRect, 0, 0);
			ClayContainer.pickingTextureResult.Apply();
			RenderTexture.active = oldRT;
			Color pickCol = ClayContainer.pickingTextureResult.GetPixel(0, 0);
			
			int pickId = (int)((pickCol.r + pickCol.g * 255.0f + pickCol.b * 255.0f) * 255.0f);
	  		ClayContainer.pickedClayObjectId = pickId - 1;
	  		ClayContainer.pickedContainerId = (int)(pickCol.a * 256.0f);
	  		if(ClayContainer.pickedContainerId >= 255){
	  			ClayContainer.pickedContainerId = -1;
	  		}
	  		pickedContainerId = ClayContainer.pickedContainerId;
			pickedClayObjectId = ClayContainer.pickedClayObjectId;
		}

		public static void pickingMicrovoxel(Camera camera, float mousePosX, float mousePosY, out int pickedContainerId, out int pickedClayObjectId, bool invertVerticalMouseCoords = false){
			pickedContainerId = -1;
			pickedClayObjectId = -1;

			if(camera == null){
				return;
			}

			int rectWidth = (int)(ClayContainer.microvoxelRTSize.x * ((float)mousePosX / (float)camera.pixelWidth));
			int rectHeight = (int)(ClayContainer.microvoxelRTSize.y * ((float)mousePosY / (float)camera.pixelHeight));

			if(invertVerticalMouseCoords){
				rectHeight = (int)ClayContainer.microvoxelRTSize.y - rectHeight;
			}

			if(ClayContainer.pickingTextureResult == null){
				ClayContainer.pickingTextureResult = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				ClayContainer.pickingRect = new Rect(0, 0, 1, 1);
			}
			
			ClayContainer.pickingRect.Set(
				rectWidth, 
				rectHeight, 
				1, 1);

			RenderTexture oldRT = RenderTexture.active;

			RenderTexture.active = ClayContainer.mvRenderTexture0;
			ClayContainer.pickingTextureResult.ReadPixels(ClayContainer.pickingRect, 0, 0);
			ClayContainer.pickingTextureResult.Apply();
			Color pickCol1 = ClayContainer.pickingTextureResult.GetPixel(0, 0);

			RenderTexture.active = oldRT;
			
			pickedContainerId = (int)((pickCol1.r * 255) + (pickCol1.g * 255.0f) * 256.0f + (pickCol1.b * 255.0f) * 256.0f * 256.0f) - 1;
			
			if(pickedContainerId == -1){
				return;
			}

			pickedContainerId = pickedContainerId / ClayContainer.totalMaxChunks;

			RenderTexture.active = ClayContainer.mvRenderTexture1;
			ClayContainer.pickingTextureResult.ReadPixels(ClayContainer.pickingRect, 0, 0);
			ClayContainer.pickingTextureResult.Apply();
			Color pickCol2 = ClayContainer.pickingTextureResult.GetPixel(0, 0);
			
			RenderTexture.active = ClayContainer.mvRenderTexture2;
			ClayContainer.pickingTextureResult.ReadPixels(ClayContainer.pickingRect, 0, 0);
			ClayContainer.pickingTextureResult.Apply();
			Color pickCol3 = ClayContainer.pickingTextureResult.GetPixel(0, 0);

			pickedClayObjectId = (int)((pickCol1.a + pickCol2.a * 255.0f + pickCol3.a * 255.0f) * 255.0f) - 1;
		}

		public int getRenderMode(){
			return (int)this.renderMode;
		}

		public void setRenderMode(int renderMode){
			this.renderMode = (ClayContainer.RenderModes)renderMode;
			this.needsInit = true;
		}

		public static string getRenderPipe(){
			return ClayContainer.renderPipe;
		}

		public bool needsUpdate = true;
		public float meshNormalSmooth = 60.0f;
		public static string defaultAssetsPath = "clayxelsFrozen";

		// end of public interface, following functions are for internal use //

		static ComputeBuffer solidsUpdatedBuffer = null;
		static ComputeBuffer solidsPerChunkBuffer = null;
		static ComputeBuffer meshIndicesBuffer = null;
		static ComputeBuffer meshVertsBuffer = null;
		static ComputeBuffer meshColorsBuffer = null;
		static ComputeBuffer pointCloudDataToSolidIdBuffer = null;
		static ComputeBuffer chunkPointCloudDataBuffer = null;
		static ComputeBuffer chunkPointCloudDataToSolidIdBuffer = null;
		static ComputeBuffer gridDataBuffer = null;
		static ComputeBuffer triangleConnectionTable = null;
		static ComputeBuffer prefilteredSolidIdsBuffer = null;
		static ComputeBuffer solidsFilterBuffer = null;
		static ComputeBuffer numSolidsPerChunkBuffer = null;
		static ComputeBuffer gridDataLowResBuffer = null;
		static ComputeShader claycoreCompute = null;
		static int maxSolids = 512;
		static int maxSolidsPerVoxel = 128;
		static int maxPointCount = (256 * 256 * 256) / 5;
		static int inspectorUpdated;
		static public bool globalDataNeedsInit = true;
		static List<string> solidsCatalogueLabels = new List<string>();
		static List<List<string[]>> solidsCatalogueParameters = new List<List<string[]>>();
		static List<ComputeBuffer> globalCompBuffers = new List<ComputeBuffer>();
		static int lastUpdatedContainerId = -1;
		static int maxThreads = 8;
		static int[] solidsInSingleChunkArray;
		static int frameSkip = 0;
		static string renderPipe = "";
		static RenderTexture pickingRenderTexture = null;
		static RenderTargetIdentifier pickingRenderTextureId;
		static CommandBuffer pickingCommandBuffer;
		static Texture2D pickingTextureResult;
		static Rect pickingRect;
		static int pickedClayObjectId = -1;
		static int pickedContainerId = -1;
		static int pickedClayObjectIdMV = -1;
		static int pickedContainerIdMV = -1;
		static GameObject pickedObj = null;
		static bool pickingMode = false;
		static bool pickingShiftPressed = false;
		static int maxChunkX = 3;
		static int maxChunkY = 3;
		static int maxChunkZ = 3;
		static float globalBlend = 1.0f;
		static int totalMaxChunks = 27;
		static int[] indirectArgsData = new int[]{0, 1, 0, 0};
		static int[] microvoxelDrawData = new int[]{0, 0, 0, 0, 0, 0, 0, 0};
		static int[] microvoxelBoundingBoxData;
		static Material pickingMaterialPolySplat = null;
		static MaterialPropertyBlock pickingMaterialPropertiesPolySplat;
		static MaterialPropertyBlock pickingMaterialPropertiesMicroVoxel;
		static int totalChunksInScene = 0;
		static bool vramLimitEnabled = false;
		static int numThreadsComputeStartRes;
		static int numThreadsComputeFullRes;
		static int[] updateChunksDefaultValues;
		static int[] indirectChunk1DefaultValues;
		static int[] indirectChunk2DefaultValues;
		static int[] pointsInChunkDefaultValues;
		static float autoLODNear = 0.0f;
		static float autoLODFar = 0.0f;
		static int lastPickedContainerId = -1;
		static Mesh microVoxelMesh = null;
		static Dictionary<int, ClayContainer> containersInScene = new Dictionary<int, ClayContainer>();
		static List<ClayContainer> containersToRender = new List<ClayContainer>();
		static Material microvoxelPassMat = null;
		static RenderTexture mvRenderTexture0 = null;
		static RenderTexture mvRenderTexture1 = null;
		static RenderTexture mvRenderTexture2 = null;
		static RenderTexture mvRenderTexture3 = null;
		static RenderTexture mvRenderTexture4 = null;
		static RenderTexture mvRenderTexture5 = null;
		static RenderTargetIdentifier[] mvRenderBuffers;
		static int[] chunkIdOffsetDefaultData;
		static Vector2 microvoxelRTSize;
		static Vector2Int microvoxelRTSizeOverride = new Vector2Int(1024, 512);
		static float bufferSizeReduceFactor = 0.5f;
		static bool directPick = false;
		static bool directPickEnabled = false;
		static string globalUserWarning = "";
		static bool prefsOverridden = false;
		
		public delegate void RenderPipelineInitCallback();
		static public RenderPipelineInitCallback renderPipelineInitCallback = null;

		public enum RenderModes{
			polySplat,
			microVoxel
		}
		
		[SerializeField] int unityInstanceId = 0;
		[SerializeField] int clayxelDetail = 88;
		[SerializeField] int chunksX = 1;
		[SerializeField] int chunksY = 1;
		[SerializeField] int chunksZ = 1;
		[SerializeField] Material material = null;
		[SerializeField] ShadowCastingMode castShadows = ShadowCastingMode.On;
		[SerializeField] bool receiveShadows = true;
		[SerializeField] public string storeAssetPath = "";
		[SerializeField] bool frozen = false;
		[SerializeField] bool clayObjectsOrderLocked = true;
		[SerializeField] bool autoBounds = false;
		[SerializeField] RenderModes renderMode = RenderModes.microVoxel;
		[SerializeField] ClayContainer instanceOf = null;
		[SerializeField] float splatsLOD = 30.0f;
		
		int containerId = -1;
		bool invalidated = false;
		int chunkSize = 8;
		bool memoryOptimized = false;
		float globalSmoothing = 0.0f;
		Dictionary<int, int> solidsUpdatedDict = new Dictionary<int, int>();
		List<ComputeBuffer> compBuffers = new List<ComputeBuffer>();
		bool needsInit = true;
		int[] genericIntBufferArray = new int[1]{0};
		List<Vector3> solidsPos;
		List<Quaternion> solidsRot;
		List<Vector3> solidsScale;
		List<float> solidsBlend;
		List<int> solidsType;
		List<Vector3> solidsColor;
		List<Vector4> solidsAttrs;
		List<Vector4> solidsAttrs2;
		List<int> solidsClayObjectId;
		ComputeBuffer solidsPosBuffer = null;
		ComputeBuffer solidsRotBuffer = null;
		ComputeBuffer solidsScaleBuffer = null;
		ComputeBuffer solidsBlendBuffer = null;
		ComputeBuffer solidsTypeBuffer = null;
		ComputeBuffer solidsColorBuffer = null;
		ComputeBuffer solidsAttrsBuffer = null;
		ComputeBuffer solidsAttrs2Buffer = null;
		ComputeBuffer solidsClayObjectIdBuffer = null;
		ComputeBuffer genericNumberBuffer = null;
		ComputeBuffer indirectChunkArgs1Buffer = null;
		ComputeBuffer indirectChunkArgs2Buffer = null;
		ComputeBuffer indirectChunkArgs3Buffer = null;
		ComputeBuffer updateChunksBuffer = null;
		ComputeBuffer chunksCenterBuffer = null;
		ComputeBuffer indirectDrawArgsBuffer = null;
		ComputeBuffer indirectDrawArgsBuffer2 = null;
		ComputeBuffer pointCloudDataMip3Buffer = null;
		ComputeBuffer gridPointersMip2Buffer = null;
		ComputeBuffer gridPointersMip3Buffer = null;
		ComputeBuffer boundingBoxBuffer = null;
		ComputeBuffer numPointsInChunkBuffer = null;
		ComputeBuffer renderIndirectDrawArgsBuffer = null;
		ComputeBuffer pointToChunkIdBuffer = null;
		ComputeBuffer volumetricDrawBuffer = null;
		ComputeBuffer chunkIdOffsetBuffer = null;
		Vector3 boundsScale = new Vector3(0.0f, 0.0f, 0.0f);
		Vector3 boundsCenter = new Vector3(0.0f, 0.0f, 0.0f);
		Bounds renderBounds = new Bounds();
		bool solidsHierarchyNeedsScan = false;
		List<ClayObject> clayObjects = new List<ClayObject>();
		List<Solid> solids = new List<Solid>();
		int numChunks = 0;
		float deltaTime = 0.0f;
		float voxelSize = 0.0f;
		int updateFrame = 0;
		float splatRadius = 1.0f;
		bool editingThisContainer = false;
		int autoBoundsChunkSize = 0;
		int autoFrameSkip = 0;
		string userWarning = "";
		MaterialPropertyBlock materialProperties;
		int LODLevel = 0;
		int pointCount = 0;
		bool pickingThis = false;
		bool microvoxelsEditorDelayedOptimize = false;
		bool visible = true;
		
		// instances data
		List<ClayContainer> instances = new List<ClayContainer>();
		Matrix4x4[] instancesMatrix;
		Matrix4x4[] instancesMatrixInv;
		int[] instancesId;
		ComputeBuffer instancesMatrixBuffer = null;
		ComputeBuffer instancesMatrixInvBuffer = null;
		ComputeBuffer instancesIdBuffer = null;
		bool instancingOtherContainer = false;

		enum Kernels{
			computeGrid,
			generatePointCloud,
			debugDisplayGridPoints,
			computeGridForMesh,
			computeMesh,
			filterSolidsPerChunk,
			compactPointCloud,
			optimizePointCloud,
			generatePointCloudMicroVoxels,
			optimizeMicrovoxels,
			computeGridMip3
		}

		static void initMicroVoxelGlobal(){
			if(ClayContainer.microVoxelMesh != null){
				DestroyImmediate(ClayContainer.microVoxelMesh);
			}

			ClayContainer.microVoxelMesh = new Mesh();
			ClayContainer.microVoxelMesh.indexFormat = IndexFormat.UInt32;

			/* cube topology
		       5---6
			 / |     |
			/  4    |
			1---2  7
			|    |  /
			0---3/

			y  z
			|/
			---x
			*/

			Vector3[] cubeVerts = new Vector3[]{
				new Vector3(-0.5f, -0.5f, -0.5f),// 0
				new Vector3(-0.5f, 0.5f, -0.5f),// 1
				new Vector3(0.5f, 0.5f, -0.5f),// 2
				new Vector3(0.5f, -0.5f, -0.5f),// 3
				new Vector3(-0.5f, -0.5f, 0.5f),// 4
				new Vector3(-0.5f, 0.5f, 0.5f),// 5
				new Vector3(0.5f, 0.5f, 0.5f),// 6
				new Vector3(0.5f, -0.5f, 0.5f)// 7
				};

			Vector2[] cubeUV = new Vector2[]{
				new Vector2(0.0f, 0.0f),
				new Vector2(0.0f, 1.0f),
				new Vector2(1.0f, 1.0f),
				new Vector2(1.0f, 0.0f),
				new Vector2(0.0f, 1.0f),
				new Vector2(0.0f, 1.0f),
				new Vector2(1.0f, 1.0f),
				new Vector2(1.0f, 0.0f)
			};
			
			int[] cubeIndices = new int[]{
				3, 1, 0, 
				2, 1, 3, 

				2, 5, 1,
				6, 5, 2,

				7, 2, 3,
				6, 2, 7,

				4, 5, 7,
				7, 5, 6,

				0, 1, 4,
				4, 1, 5,

				0, 4, 3,
				3, 4, 7};
			
			int gridSize = ClayContainer.totalMaxChunks;

			List<Vector3> meshVerts = new List<Vector3>(cubeVerts.Length * gridSize);
			List<Vector2> meshUVs = new List<Vector2>(cubeVerts.Length * gridSize);
			List<int> meshIndices = new List<int>(cubeIndices.Length * gridSize);

			for(int i = 0; i < gridSize; ++i){
				meshVerts.AddRange(cubeVerts);
				meshIndices.AddRange(cubeIndices);
				meshUVs.AddRange(cubeUV);

				for(int j = 0; j < cubeIndices.Length; ++j){
					cubeIndices[j] += cubeVerts.Length;
				}
			}

			ClayContainer.chunkIdOffsetDefaultData = new int[ClayContainer.totalMaxChunks];
			for(int i = 0; i < ClayContainer.totalMaxChunks; ++i){
				ClayContainer.chunkIdOffsetDefaultData[i] = i;
			}

			ClayContainer.microVoxelMesh.vertices = meshVerts.ToArray();
			ClayContainer.microVoxelMesh.uv = meshUVs.ToArray();
			ClayContainer.microVoxelMesh.triangles = meshIndices.ToArray();

			// editor RT size
			int resX = 1024;
			int resY = 1024;

			if(Application.isPlaying){
				resX = ClayContainer.microvoxelRTSizeOverride.x;
				resY = ClayContainer.microvoxelRTSizeOverride.y;
			}
			
			ClayContainer.microvoxelRTSize = new Vector2(resX, resY);
			
			ClayContainer.mvRenderTexture0 = new RenderTexture(resX, resY, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        	ClayContainer.mvRenderTexture1 = new RenderTexture(resX, resY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        	ClayContainer.mvRenderTexture2 = new RenderTexture(resX, resY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        	ClayContainer.mvRenderTexture3 = new RenderTexture(resX, resY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        	ClayContainer.mvRenderTexture4 = new RenderTexture(resX, resY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        	ClayContainer.mvRenderTexture5 = new RenderTexture(resX, resY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        	
        	ClayContainer.mvRenderTexture0.autoGenerateMips = false;
			ClayContainer.mvRenderTexture0.useMipMap = false;
			ClayContainer.mvRenderTexture0.antiAliasing = 1;
			ClayContainer.mvRenderTexture0.anisoLevel = 0;
			ClayContainer.mvRenderTexture0.filterMode = FilterMode.Point;
			ClayContainer.mvRenderTexture0.Create();

        	ClayContainer.mvRenderTexture1.autoGenerateMips = false;
			ClayContainer.mvRenderTexture1.useMipMap = false;
			ClayContainer.mvRenderTexture1.antiAliasing = 1;
			ClayContainer.mvRenderTexture1.anisoLevel = 0;
			ClayContainer.mvRenderTexture1.filterMode = FilterMode.Point;
			ClayContainer.mvRenderTexture1.Create();

			ClayContainer.mvRenderTexture2.autoGenerateMips = false;
			ClayContainer.mvRenderTexture2.useMipMap = false;
			ClayContainer.mvRenderTexture2.antiAliasing = 1;
			ClayContainer.mvRenderTexture2.anisoLevel = 0;
			ClayContainer.mvRenderTexture2.filterMode = FilterMode.Point;
			ClayContainer.mvRenderTexture2.Create();

			ClayContainer.mvRenderTexture3.autoGenerateMips = false;
			ClayContainer.mvRenderTexture3.useMipMap = false;
			ClayContainer.mvRenderTexture3.antiAliasing = 1;
			ClayContainer.mvRenderTexture3.anisoLevel = 0;
			ClayContainer.mvRenderTexture3.filterMode = FilterMode.Point;
			ClayContainer.mvRenderTexture3.Create();

			ClayContainer.mvRenderTexture4.autoGenerateMips = false;
			ClayContainer.mvRenderTexture4.useMipMap = false;
			ClayContainer.mvRenderTexture4.antiAliasing = 1;
			ClayContainer.mvRenderTexture4.anisoLevel = 0;
			ClayContainer.mvRenderTexture4.filterMode = FilterMode.Point;
			ClayContainer.mvRenderTexture4.Create();

			ClayContainer.mvRenderTexture5.autoGenerateMips = false;
			ClayContainer.mvRenderTexture5.useMipMap = false;
			ClayContainer.mvRenderTexture5.antiAliasing = 1;
			ClayContainer.mvRenderTexture5.anisoLevel = 0;
			ClayContainer.mvRenderTexture5.filterMode = FilterMode.Point;
			ClayContainer.mvRenderTexture5.Create();

        	ClayContainer.mvRenderBuffers = new RenderTargetIdentifier[5];
        	ClayContainer.mvRenderBuffers[0] = new RenderTargetIdentifier(ClayContainer.mvRenderTexture0);
        	ClayContainer.mvRenderBuffers[1] = new RenderTargetIdentifier(ClayContainer.mvRenderTexture1);
        	ClayContainer.mvRenderBuffers[2] = new RenderTargetIdentifier(ClayContainer.mvRenderTexture2);
        	ClayContainer.mvRenderBuffers[3] = new RenderTargetIdentifier(ClayContainer.mvRenderTexture3);
        	ClayContainer.mvRenderBuffers[4] = new RenderTargetIdentifier(ClayContainer.mvRenderTexture5);
        	
        	// Shader.SetGlobalFloat("_nan", System.Single.NaN);

        	Shader.SetGlobalInt("maxChunks", ClayContainer.totalMaxChunks);

        	if(ClayContainer.renderPipe == "urp"){
        		ClayContainer.microvoxelPassMat = new Material(Shader.Find("Clayxels/ClayxelMicroVoxelPassURP"));
        		ClayContainer.microvoxelPassMat.EnableKeyword("CLAYXELS_URP");
        	}
        	else if(ClayContainer.renderPipe == "hdrp"){
        		ClayContainer.microvoxelPassMat = new Material(Shader.Find("Clayxels/ClayxelMicroVoxelPassHDRP"));
        		ClayContainer.microvoxelPassMat.EnableKeyword("CLAYXELS_HDRP");
        	}

        	// whie sculpting we need these flags to be able to get inside a container with the camera
        	ClayContainer.microvoxelPassMat.DisableKeyword("CLAYXEL_EARLY_Z_OPTIMIZE_ON");
        	ClayContainer.microvoxelPassMat.EnableKeyword("CLAYXEL_EARLY_Z_OPTIMIZE_OFF");
        	ClayContainer.microvoxelPassMat.SetFloat("_Cull", (float)CullMode.Front);

        	if(Application.isPlaying){
        		// at play time, we enable this to optimize rendering
				ClayContainer.microvoxelPassMat.DisableKeyword("CLAYXEL_EARLY_Z_OPTIMIZE_OFF");
        		ClayContainer.microvoxelPassMat.EnableKeyword("CLAYXEL_EARLY_Z_OPTIMIZE_ON");
        		ClayContainer.microvoxelPassMat.SetFloat("_Cull", (float)CullMode.Back);
        	}

        	#if UNITY_EDITOR
	        	if(ClayContainer.renderPipelineInitCallback != null){
	        		ClayContainer.renderPipelineInitCallback();
	        	}
	        #endif
		}

		static void reassignContainerIds(){
			#if UNITY_EDITOR
				EditorPrefs.SetInt("clayxelsUniqueId", 0);
			#else
				PlayerPrefs.SetInt("clayxelsUniqueId", 0);
			#endif

			ClayContainer[] containers = UnityEngine.Object.FindObjectsOfType<ClayContainer>();

			for(int i = 0; i < containers.Length; ++i){
				containers[i].containerId = -1;
			}
		}

		void checkContainerId(){
			if(this.containerId == -1){
				this.assignContainerId();
			}
		}

		void assignContainerId(){
			#if UNITY_EDITOR
				this.containerId = EditorPrefs.GetInt("clayxelsUniqueId");
				ClayContainer.containersInScene[this.containerId] = this;

				EditorPrefs.SetInt("clayxelsUniqueId", this.containerId + 1);
			#else
				this.containerId = PlayerPrefs.GetInt("clayxelsUniqueId");
				ClayContainer.containersInScene[this.containerId] = this;

				PlayerPrefs.SetInt("clayxelsUniqueId", this.containerId + 1);
			#endif
		}

		void checkContainerIdIsUnique(){
			if(this.unityInstanceId != this.GetInstanceID()){
				if(this.unityInstanceId == 0){
					this.unityInstanceId = this.GetInstanceID();
				}
				else{
					this.unityInstanceId = this.GetInstanceID();
					if (this.unityInstanceId < 0){
						// this container just got duplicated
						this.containerId = -1;
					}
				}
			}
		}

		static void checkNeedsGlobalInit(){
			if(ClayContainer.globalDataNeedsInit){
				return;
			}

			if(ClayContainer.claycoreCompute == null && !ReferenceEquals(ClayContainer.claycoreCompute , null)){
				ClayContainer.globalDataNeedsInit = true;
			}
		}

		void removeFromScene(){
			ClayContainer.containersInScene[this.containerId] = null;
			ClayContainer.containersToRender.Remove(this);
		}

		void addToScene(){
			this.checkContainerId();
			
			ClayContainer.containersInScene[this.containerId] = this;
			
			if(this.instanceOf == null){
				if(!ClayContainer.containersToRender.Contains(this)){
					ClayContainer.containersToRender.Add(this);
				}
			}
		}

		void initInstance(){
			if(this.instanceOf == this || this.instanceOf.instanceOf != null){
				this.instanceOf = null;
				this.needsInit = true;

				return;
			}

			this.instancingOtherContainer = true;

			if(!this.instanceOf.instances.Contains(this)){
				this.instanceOf.instances.Add(this);
			}

			this.instanceOf.initInstancesData();

			this.needsInit = false;

			if(Application.isPlaying){
				this.enabled = false;
			}
		}

		void initInstancesData(){
			if(this.needsInit){
				return;
			}

			try{// unity will error on exiting play mode 
				int numInstances = this.instances.Count + 1;

				ClayContainer.microvoxelDrawData[0] = 36 * this.numChunks;
				ClayContainer.microvoxelDrawData[1] = numInstances;
				this.volumetricDrawBuffer.SetData(ClayContainer.microvoxelDrawData);

				this.instancesMatrix = new Matrix4x4[numInstances];
				this.instancesMatrixInv = new Matrix4x4[numInstances];
				this.instancesId = new int[numInstances];

				if(this.compBuffers.Contains(this.instancesMatrixBuffer)){
					this.compBuffers.Remove(this.instancesMatrixBuffer);
					this.instancesMatrixBuffer.Release();

					this.compBuffers.Remove(this.instancesMatrixInvBuffer);
					this.instancesMatrixInvBuffer.Release();

					this.compBuffers.Remove(this.instancesIdBuffer);
					this.instancesIdBuffer.Release();
				}

				this.instancesMatrixBuffer = new ComputeBuffer(numInstances, sizeof(float) * 16);
				this.compBuffers.Add(this.instancesMatrixBuffer);

				this.instancesMatrixInvBuffer = new ComputeBuffer(numInstances, sizeof(float) * 16);
				this.compBuffers.Add(this.instancesMatrixInvBuffer);

				this.instancesIdBuffer = new ComputeBuffer(numInstances, sizeof(int));
				this.compBuffers.Add(this.instancesIdBuffer);
				
				this.checkContainerId();

				this.instancesId[0] = this.containerId;
				for(int i = 0; i < this.instances.Count; ++i){
					this.instances[i].checkContainerId();

					this.instancesId[i + 1] = this.instances[i].containerId;
				}
				
				this.instancesIdBuffer.SetData(this.instancesId);
			}
			catch{}
		}
		
		void initMicroVoxelBuffer(){
        	this.materialProperties.SetTexture("microVoxRenderTex0", ClayContainer.mvRenderTexture0, RenderTextureSubElement.Color);
			this.materialProperties.SetTexture("microVoxRenderTex1", ClayContainer.mvRenderTexture1, RenderTextureSubElement.Color);
			this.materialProperties.SetTexture("microVoxRenderTex2", ClayContainer.mvRenderTexture2, RenderTextureSubElement.Color);
			this.materialProperties.SetTexture("microVoxRenderTex3", ClayContainer.mvRenderTexture3, RenderTextureSubElement.Color);
			this.materialProperties.SetTexture("microVoxRenderTex4", ClayContainer.mvRenderTexture5, RenderTextureSubElement.Color);
			this.materialProperties.SetTexture("microVoxRenderTexDepth", ClayContainer.mvRenderTexture4, RenderTextureSubElement.Color);
			
			this.materialProperties.SetInt("memoryOptimized", 0);

			this.material.enableInstancing = true;
			this.material.EnableKeyword("UNITY_INSTANCING_ENABLED");

			this.initInstancesData();
		}

		void drawClayxelsPolySplat(ClayContainer instance){
			instance.renderBounds.center = instance.transform.position;

			instance.splatRadius = this.voxelSize * ((instance.transform.lossyScale.x + instance.transform.lossyScale.y + instance.transform.lossyScale.z) / 3.0f);
			
			this.materialProperties.SetMatrix("objectMatrix", instance.transform.localToWorldMatrix);
			this.materialProperties.SetFloat("chunkSize", (float)this.chunkSize);

			this.materialProperties.SetFloat("splatRadius", instance.splatRadius);

			this.materialProperties.SetInt("solidHighlightId", -1);

			#if UNITY_EDITOR
				if(!Application.isPlaying){
					this.updateMaterialInEditor(instance);
				}
			#endif
			
			Graphics.DrawProceduralIndirect(this.material, 
				instance.renderBounds,
				MeshTopology.Triangles, instance.renderIndirectDrawArgsBuffer, 0,
				null, this.materialProperties,
				this.castShadows, this.receiveShadows, this.gameObject.layer);
		}

		void drawClayxelsMicroVoxel(){
			this.updateInstances();

			this.materialProperties.SetFloat("chunkSize", (float)this.chunkSize);
			this.materialProperties.SetBuffer("chunksCenter", this.chunksCenterBuffer);
			this.materialProperties.SetBuffer("boundingBox", this.boundingBoxBuffer);
			this.materialProperties.SetInt("containerId", this.containerId);

			this.materialProperties.SetInt("containerHighlightId", ClayContainer.pickedContainerIdMV);
			this.materialProperties.SetInt("solidHighlightId", ClayContainer.pickedClayObjectIdMV);

			this.materialProperties.SetBuffer("instancesObjectMatrix", this.instancesMatrixBuffer);
			this.materialProperties.SetBuffer("instancesObjectMatrixInv", this.instancesMatrixInvBuffer);
			this.materialProperties.SetBuffer("instancesId", this.instancesIdBuffer);

			Graphics.DrawMeshInstancedIndirect(
				ClayContainer.microVoxelMesh, 0, this.material, 
				this.renderBounds,
				this.volumetricDrawBuffer,
				0, this.materialProperties,
				this.castShadows, this.receiveShadows, this.gameObject.layer);

			#if UNITY_EDITOR
				// this will fix an editor issue causing the draw call to happen before the pre-pass
				if(this.microvoxelsEditorDelayedOptimize){
					this.microvoxelsEditorDelayedOptimize = false;

					this.optimizeMemory();

					if(Application.isPlaying){
						this.enableAllClayObjects(false);
					}
				}
			#endif
		}

		public static RenderTargetIdentifier[] getMicroVoxelRenderBuffers(){
			return ClayContainer.mvRenderBuffers;
		}

		public static RenderBuffer getMicroVoxelDepthBuffer(){
			return ClayContainer.mvRenderTexture0.depthBuffer;
		}

		public static void drawMicroVoxelPrePass(CommandBuffer cmd){
			int renderStage = 0;
			for(int i = 0; i < ClayContainer.containersToRender.Count; ++i){
				ClayContainer.containersToRender[i].drawMicroVoxelsPass(cmd, renderStage);
			}

			cmd.CopyTexture(ClayContainer.mvRenderTexture3, ClayContainer.mvRenderTexture4);
		}

		void drawMicroVoxelsPass(CommandBuffer cmd, int renderStage){
			if(this.invalidated){
				return;
			}

			if(!this.visible || 
				this.renderMode != ClayContainer.RenderModes.microVoxel || 
				this.needsInit || 
				this.frozen || 
				!this.gameObject.activeSelf){

				return;
			}

			this.materialProperties.SetInt("renderStage", renderStage);

			if(renderStage == 0){
				this.materialProperties.SetFloat("chunkSize", (float)this.chunkSize);
				this.materialProperties.SetBuffer("chunksCenter", this.chunksCenterBuffer);
				this.materialProperties.SetBuffer("boundingBox", this.boundingBoxBuffer);
				this.materialProperties.SetBuffer("pointCloudDataMip3", this.pointCloudDataMip3Buffer);
				this.materialProperties.SetBuffer("gridPointersMip2", this.gridPointersMip2Buffer);
				this.materialProperties.SetBuffer("gridPointersMip3", this.gridPointersMip3Buffer);
				this.materialProperties.SetBuffer("chunkIdOffset", this.chunkIdOffsetBuffer);
				this.materialProperties.SetInt("containerId", this.containerId);
				this.materialProperties.SetFloat("bufferSizeReduceFactor", ClayContainer.bufferSizeReduceFactor);

				this.materialProperties.SetBuffer("instancesObjectMatrix", this.instancesMatrixBuffer);
				this.materialProperties.SetBuffer("instancesObjectMatrixInv", this.instancesMatrixInvBuffer);
				this.materialProperties.SetBuffer("instancesId", this.instancesIdBuffer);

				if(this.memoryOptimized){
					this.materialProperties.SetInt("memoryOptimized", 1);
				}
				else{
					this.materialProperties.SetInt("memoryOptimized", 0);
				}

				this.materialProperties.SetTexture("_MainTex", this.material.GetTexture("_MainTex"));
				this.materialProperties.SetFloat("_backFillDark", this.material.GetFloat("_backFillDark"));
				this.materialProperties.SetFloat("_splatSizeMult", this.material.GetFloat("_splatSizeMult"));
				this.materialProperties.SetFloat("_roughSize", this.material.GetFloat("_roughSize"));
				this.materialProperties.SetFloat("_alphaCutout", this.material.GetFloat("_alphaCutout"));
				this.materialProperties.SetFloat("_backFillAlpha", this.material.GetFloat("_backFillAlpha"));
				this.materialProperties.SetFloat("_roughPos", this.material.GetFloat("_roughPos"));
				this.materialProperties.SetFloat("_roughTwist", this.material.GetFloat("_roughTwist"));
				this.materialProperties.SetFloat("_roughOrientX", this.material.GetFloat("_roughOrientX"));
				this.materialProperties.SetFloat("_roughOrientY", this.material.GetFloat("_roughOrientY"));
				this.materialProperties.SetFloat("_roughOrientZ", this.material.GetFloat("_roughOrientZ"));
				this.materialProperties.SetFloat("_splatBillboard", this.material.GetFloat("_splatBillboard"));
				this.materialProperties.SetFloat("_roughColor", this.material.GetFloat("_roughColor"));
				this.materialProperties.SetFloat("_splatPixelWidthThreshold", this.splatsLOD);
			}

			int passId = 0;
			cmd.DrawMeshInstancedIndirect(
				ClayContainer.microVoxelMesh, 0, 
				ClayContainer.microvoxelPassMat, 
				passId, 
				this.volumetricDrawBuffer, 0, this.materialProperties);
		}

		void computeClayPolySplat(){
			ClayContainer.claycoreCompute.SetFloat("seamOffsetMultiplier", 4.0f);

			ClayContainer.indirectArgsData[0] = 0;
			this.indirectDrawArgsBuffer.SetData(ClayContainer.indirectArgsData);

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "chunksCenter", this.chunksCenterBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "numPointsInChunk", this.numPointsInChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridMip3, "chunksCenter", this.chunksCenterBuffer);

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridMip3, "chunksCenter", this.chunksCenterBuffer);

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloud, "numPointsInChunk", this.numPointsInChunkBuffer);

			for(int chunkIt = 0; chunkIt < this.numChunks; ++chunkIt){
				uint indirectChunkId = sizeof(int) * ((uint)chunkIt * 3);

				ClayContainer.claycoreCompute.SetInt("chunkId", chunkIt);
				ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.computeGrid, this.indirectChunkArgs1Buffer, indirectChunkId);
				ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.computeGridMip3, this.indirectChunkArgs2Buffer, indirectChunkId);
				ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.generatePointCloud, this.indirectChunkArgs2Buffer, indirectChunkId);
			}

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.compactPointCloud, "numPointsInChunk", this.numPointsInChunkBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.compactPointCloud, "pointCloudDataMip3", this.pointCloudDataMip3Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.compactPointCloud, "indirectDrawArgs", this.indirectDrawArgsBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.compactPointCloud, "pointToChunkId", this.pointToChunkIdBuffer);
			ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.compactPointCloud, this.indirectChunkArgs3Buffer, 0);

			this.materialProperties.SetBuffer("chunkPoints", this.pointCloudDataMip3Buffer);
			this.materialProperties.SetBuffer("chunksCenter", this.chunksCenterBuffer);
			this.materialProperties.SetBuffer("pointToChunkId", this.pointToChunkIdBuffer);
			
			this.splatRadius = (this.voxelSize * ((this.transform.lossyScale.x + this.transform.lossyScale.y + this.transform.lossyScale.z) / 3.0f));
		}

		void computeClayMicroVoxel(){
			ClayContainer.claycoreCompute.SetFloat("seamOffsetMultiplier", 8.0f);
			ClayContainer.claycoreCompute.SetFloat("bufferSizeReduceFactor", ClayContainer.bufferSizeReduceFactor);

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGrid, "chunksCenter", this.chunksCenterBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.computeGridMip3, "chunksCenter", this.chunksCenterBuffer);
			
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloudMicroVoxels, "boundingBox", this.boundingBoxBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloudMicroVoxels, "gridPointersMip3", this.gridPointersMip3Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloudMicroVoxels, "gridPointersMip2", this.gridPointersMip2Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloudMicroVoxels, "pointCloudDataMip3", this.pointCloudDataMip3Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.generatePointCloudMicroVoxels, "volumetricDraw", this.volumetricDrawBuffer);
			
			ClayContainer.microvoxelDrawData[0] = 36 * this.numChunks;
			ClayContainer.microvoxelDrawData[1] = this.instances.Count + 1;
			this.volumetricDrawBuffer.SetData(ClayContainer.microvoxelDrawData);

			if(this.numChunks == 1){
				this.boundingBoxBuffer.SetData(ClayContainer.microvoxelBoundingBoxData, 0, 0, 6 * ClayContainer.totalMaxChunks);
			}

			for(int chunkIt = 0; chunkIt < this.numChunks; ++chunkIt){
				uint indirectChunkId = sizeof(int) * ((uint)chunkIt * 3);

				ClayContainer.claycoreCompute.SetInt("chunkId", chunkIt);
				ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.computeGrid, this.indirectChunkArgs1Buffer, indirectChunkId);
				ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.computeGridMip3, this.indirectChunkArgs2Buffer, indirectChunkId);
				ClayContainer.claycoreCompute.DispatchIndirect((int)Kernels.generatePointCloudMicroVoxels, this.indirectChunkArgs1Buffer, indirectChunkId);
			}

			this.renderBounds.center = this.transform.position;

			this.materialProperties.SetFloat("chunkSize", (float)this.chunkSize);
			this.materialProperties.SetBuffer("chunksCenter", this.chunksCenterBuffer);
			this.materialProperties.SetBuffer("boundingBox", this.boundingBoxBuffer);
			this.materialProperties.SetBuffer("pointCloudDataMip3", this.pointCloudDataMip3Buffer);
			this.materialProperties.SetBuffer("gridPointersMip2", this.gridPointersMip2Buffer);
			this.materialProperties.SetBuffer("gridPointersMip3", this.gridPointersMip3Buffer);
			this.materialProperties.SetInt("solidHighlightId", -1);
		}

		void updateInternalBounds(){
			this.chunkSize = (int)Mathf.Lerp(40.0f, 4.0f, (float)this.clayxelDetail / 100.0f);
			
			if(this.autoBoundsChunkSize > this.chunkSize){
				this.chunkSize = this.autoBoundsChunkSize;
			}
			
			float voxelSize = (float)this.chunkSize / 256;

			this.voxelSize = voxelSize;
			this.splatRadius = this.voxelSize * ((this.transform.lossyScale.x + this.transform.lossyScale.y + this.transform.lossyScale.z) / 3.0f);

			this.globalSmoothing = this.voxelSize;
			ClayContainer.claycoreCompute.SetFloat("globalRoundCornerValue", this.globalSmoothing);

			this.boundsScale.x = (float)this.chunkSize * this.chunksX;
			this.boundsScale.y = (float)this.chunkSize * this.chunksY;
			this.boundsScale.z = (float)this.chunkSize * this.chunksZ;
			this.renderBounds.size = this.boundsScale * this.transform.lossyScale.x;

			float gridCenterOffset = (this.chunkSize * 0.5f);
			this.boundsCenter.x = ((this.chunkSize * (this.chunksX - 1)) * 0.5f) - (gridCenterOffset*(this.chunksX-1));
			this.boundsCenter.y = ((this.chunkSize * (this.chunksY - 1)) * 0.5f) - (gridCenterOffset*(this.chunksY-1));
			this.boundsCenter.z = ((this.chunkSize * (this.chunksZ - 1)) * 0.5f) - (gridCenterOffset*(this.chunksZ-1));

			float chunkOffset = this.chunkSize - voxelSize; // removes the seam between chunks

			if(this.autoBounds){
				this.needsUpdate = true;
			}
		}

		void drawClayxelPicking(int containerId, CommandBuffer pickingCommandBuffer, bool pickClayObjects){
			if(this.needsInit && this.instanceOf == null){
				return;
			}

			ClayContainer container = this;
			if(this.instanceOf != null){
				container = this.instanceOf;
			}

			if(pickClayObjects){
				ClayContainer.pickingMaterialPropertiesPolySplat.SetInt("selectMode", 1);
			}
			else{
				ClayContainer.pickingMaterialPropertiesPolySplat.SetInt("selectMode", 0);
			}

			if(this.renderMode == ClayContainer.RenderModes.polySplat){
				ClayContainer.pickingMaterialPropertiesPolySplat.SetFloat("chunkSize", (float)container.chunkSize);
				ClayContainer.pickingMaterialPropertiesPolySplat.SetBuffer("chunksCenter", container.chunksCenterBuffer);
				ClayContainer.pickingMaterialPropertiesPolySplat.SetInt("containerId", containerId);
				ClayContainer.pickingMaterialPropertiesPolySplat.SetMatrix("objectMatrix", this.transform.localToWorldMatrix);
				ClayContainer.pickingMaterialPropertiesPolySplat.SetBuffer("pointCloudDataToSolidId", ClayContainer.pointCloudDataToSolidIdBuffer);
				ClayContainer.pickingMaterialPropertiesPolySplat.SetFloat("splatRadius",  container.splatRadius);
				ClayContainer.pickingMaterialPropertiesPolySplat.SetBuffer("chunkPoints", container.pointCloudDataMip3Buffer);
				ClayContainer.pickingMaterialPropertiesPolySplat.SetBuffer("pointToChunkId", container.pointToChunkIdBuffer);
				
				pickingCommandBuffer.DrawProceduralIndirect(Matrix4x4.identity, ClayContainer.pickingMaterialPolySplat, -1, 
					MeshTopology.Triangles, container.indirectDrawArgsBuffer, 0, ClayContainer.pickingMaterialPropertiesPolySplat);
			}
		}

		static void collectClayObjectsRecursive(GameObject obj, ref List<GameObject> collection){
			if(obj.GetComponent<ClayObject>() != null){
				collection.Add(obj);
			}

			for(int i = 0; i < obj.transform.childCount; ++i){
				GameObject childObj = obj.transform.GetChild(i).gameObject;

				if(childObj.GetComponent<ClayContainer>() == null){
					ClayContainer.collectClayObjectsRecursive(childObj, ref collection);
				}
			}
		}

		public static int[] getMemoryStats(){
			int[] memStats = new int[]{0, 0, 0};
			// memStats[0] = ClayContainer.maxPointCount * ClayContainer.maxChunks
			// int mbPerContainer = (64 * ClayContainer.maxPointCount) / 8000000;

			int upfrontMem = 0;
			for(int i = 0; i < ClayContainer.globalCompBuffers.Count; ++i){
				long size = ((long)ClayContainer.globalCompBuffers[i].stride * (long)ClayContainer.globalCompBuffers[i].count);
				int sizeMb = (int)(size * 0.000001f);
				upfrontMem += sizeMb;
			}

			memStats[0] = upfrontMem;

			int chunksInScene = 0;
			ClayContainer[] clayxelObjs = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
			for(int i = 0; i < clayxelObjs.Length; ++i){
				ClayContainer container = clayxelObjs[i];
				if(container.enabled && container.instanceOf == null){
					chunksInScene += container.numChunks;
				}
			}

			int mbPerContainer = (64 * ClayContainer.maxPointCount) / 8000000;// bit to mb
			int sceneContainersMb = mbPerContainer * chunksInScene;
			memStats[1] = sceneContainersMb;

			return memStats;
		}

		public static void applyPrefs(){
			ClayxelsPrefs prefs = ClayContainer.loadPrefs();

			if(prefs == null){
				Debug.Log("Clayxels: invalid prefs file detected!");
				return;
			}

			ClayContainer.directPickEnabled = prefs.directPickEnabled;
			ClayContainer.directPick = ClayContainer.directPickEnabled;

			ClayContainer.boundsColor = new Color32((byte)prefs.boundsColor[0], (byte)prefs.boundsColor[1], (byte)prefs.boundsColor[2], (byte)prefs.boundsColor[3]);
			ClayContainer.pickingKey = prefs.pickingKey;
			ClayContainer.mirrorDuplicateKey = prefs.mirrorDuplicateKey;
			
			int[] pointCountPreset = new int[]{300000, 900000, 2000000};
			ClayContainer.maxPointCount = pointCountPreset[prefs.maxPointCount];

			float[] reduction = new float[]{0.5f, 0.7f, 1.0f};
			ClayContainer.bufferSizeReduceFactor = reduction[prefs.maxPointCount];
			
			int[] solidsCountPreset = new int[]{512, 4096, 16384};
			ClayContainer.maxSolids = solidsCountPreset[prefs.maxSolidsCount];

			int[] solidsPerVoxelPreset = new int[]{128, 512, 2048};
			ClayContainer.maxSolidsPerVoxel = solidsPerVoxelPreset[prefs.maxSolidsPerVoxel];

			ClayContainer.frameSkip = prefs.frameSkip;
			ClayContainer.vramLimitEnabled = prefs.vramLimitEnabled;
			ClayContainer.setMaxBounds(prefs.maxBounds);

			ClayContainer.globalBlend = prefs.globalBlend;

			ClayContainer.defaultAssetsPath = prefs.defaultAssetsPath;

			int cameraWidth = 2048;
			int cameraHeight = 2048;
			if(Camera.main != null){
				cameraWidth = Camera.main.pixelWidth;
				cameraHeight = Camera.main.pixelHeight;
			}

			if(prefs.renderSize.x < 512){
				prefs.renderSize.x = 512;
			}
			else if(prefs.renderSize.x > cameraWidth){
				prefs.renderSize.x = cameraWidth;	
			}

			if(prefs.renderSize.y < 512){
				prefs.renderSize.y = 512;
			}
			else if(prefs.renderSize.y > cameraHeight){
				prefs.renderSize.y = cameraHeight;	
			}
			
			ClayContainer.microvoxelRTSizeOverride = prefs.renderSize;

			#if UNITY_EDITOR
				if(!AssetDatabase.IsValidFolder("Assets/" + ClayContainer.defaultAssetsPath)){
					AssetDatabase.CreateFolder("Assets", ClayContainer.defaultAssetsPath);
				}
			#endif
		}

		public static ClayxelsPrefs loadPrefs(){
			ClayxelsPrefs prefs = null;

			try{
	    		TextAsset configTextAsset = (TextAsset)Resources.Load("clayxelsPrefs", typeof(TextAsset));
	    		prefs = JsonUtility.FromJson<ClayxelsPrefs>(configTextAsset.text);
	    	}
	    	catch{
	    		#if UNITY_EDITOR
		    		ClayContainer.checkPrefsIntegrity();

		    		TextAsset configTextAsset = (TextAsset)Resources.Load("clayxelsPrefs", typeof(TextAsset));
		    		prefs = JsonUtility.FromJson<ClayxelsPrefs>(configTextAsset.text);
		    	#endif
	    	}

	    	return prefs;
		}

		void autoLOD(ClayContainer instance){
			// float distFromCamera = (Camera.main.transform.position - instance.transform.position).magnitude;
			// float focusPoint = (distFromCamera - ClayContainer.autoLODNear) / (ClayContainer.autoLODFar - ClayContainer.autoLODNear);
			// int lod = (int)(focusPoint * 100.0f);
			
			// this.setLOD(instance, lod);
		}

		void optimizeMemory(){
			if(this.memoryOptimized || this.forceUpdate){
				return;
			}

			if(this.renderMode == ClayContainer.RenderModes.polySplat){
				this.optimizeMemoryPolySplat();
			}
			else{
				this.optimizeMemoryMicroVoxel();
			}
		}

		void optimizeMemoryMicroVoxel(){
			if(this.solids.Count == 0){
				return;
			}

			this.memoryOptimized = true;
			this.userWarning = "";
			this.updateFrame = 0;

			ComputeBuffer microvoxelCountersBuffer = new ComputeBuffer(3, sizeof(int));

			int[] counters = new int[]{0, 0, 0};
			microvoxelCountersBuffer.SetData(new int[]{0, 0, 0});

			int reducedMip3BufferSize = (int)(((float)(256 * 256 * 256) * this.numChunks) * ClayContainer.bufferSizeReduceFactor);

			ComputeBuffer gridPointersMip3OptBuffer = new ComputeBuffer(reducedMip3BufferSize, sizeof(int));
			ComputeBuffer gridPointersMip2OptBuffer = new ComputeBuffer((64 * 64 * 64) * this.numChunks, sizeof(int));
			ComputeBuffer pointCloudDataMip3OptBuffer = new ComputeBuffer(reducedMip3BufferSize, sizeof(int) * 2);

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizeMicrovoxels, "microvoxelCounters", microvoxelCountersBuffer);

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizeMicrovoxels, "gridPointersMip3", this.gridPointersMip3Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizeMicrovoxels, "gridPointersMip2", this.gridPointersMip2Buffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizeMicrovoxels, "pointCloudDataMip3", this.pointCloudDataMip3Buffer);

			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizeMicrovoxels, "chunkIdOffset", this.chunkIdOffsetBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizeMicrovoxels, "gridPointersMip3Opt", gridPointersMip3OptBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizeMicrovoxels, "gridPointersMip2Opt", gridPointersMip2OptBuffer);
			ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizeMicrovoxels, "pointCloudDataMip3Opt", pointCloudDataMip3OptBuffer);

			ClayContainer.claycoreCompute.Dispatch((int)Kernels.optimizeMicrovoxels, this.chunksX, this.chunksY, this.chunksZ);

			// remove expanded buffers
			this.compBuffers.Remove(this.gridPointersMip3Buffer);
			this.compBuffers.Remove(this.gridPointersMip2Buffer);
			this.compBuffers.Remove(this.pointCloudDataMip3Buffer);

			this.gridPointersMip3Buffer.Release();
			this.gridPointersMip2Buffer.Release();
			this.pointCloudDataMip3Buffer.Release();

			// extract counters of optimized buffers
			microvoxelCountersBuffer.GetData(counters);
			int numMip2Elements = counters[0]; 
			int numMip3Elements = counters[1];
			int numPointCloudMip3Elements = counters[2];

			if(numMip3Elements == 0){
				microvoxelCountersBuffer.Release();
				gridPointersMip3OptBuffer.Release();
				gridPointersMip2OptBuffer.Release();
				pointCloudDataMip3OptBuffer.Release();

				return;
			}
			
			if(numMip3Elements >= reducedMip3BufferSize){
				this.userWarning = "max point count exceeded, increase limit from Global Config window";
				Debug.Log("Clayxels: container " + this.gameObject.name + " has exceeded the limit of points allowed, increase limit from Global Config window");
				
				microvoxelCountersBuffer.Release();
				gridPointersMip3OptBuffer.Release();
				gridPointersMip2OptBuffer.Release();
				pointCloudDataMip3OptBuffer.Release();

				return;
			}

			int[] tmpPointCloudDataStorage = new int[reducedMip3BufferSize * 2];

			microvoxelCountersBuffer.Release();

			// trim compacted buffers
			gridPointersMip2OptBuffer.GetData(tmpPointCloudDataStorage, 0, 0, numMip2Elements);
			this.gridPointersMip2Buffer = new ComputeBuffer(numMip2Elements, sizeof(int));
			this.gridPointersMip2Buffer.SetData(tmpPointCloudDataStorage, 0, 0, numMip2Elements);
			this.compBuffers.Add(this.gridPointersMip2Buffer);
			gridPointersMip2OptBuffer.Release();

			gridPointersMip3OptBuffer.GetData(tmpPointCloudDataStorage, 0, 0, numMip3Elements);
			this.gridPointersMip3Buffer = new ComputeBuffer(numMip3Elements, sizeof(int));
			this.gridPointersMip3Buffer.SetData(tmpPointCloudDataStorage, 0, 0, numMip3Elements);
			this.compBuffers.Add(this.gridPointersMip3Buffer);
			gridPointersMip3OptBuffer.Release();

			pointCloudDataMip3OptBuffer.GetData(tmpPointCloudDataStorage, 0, 0, numPointCloudMip3Elements * 2);
			this.pointCloudDataMip3Buffer = new ComputeBuffer(numPointCloudMip3Elements, sizeof(int) * 2);
			this.pointCloudDataMip3Buffer.SetData(tmpPointCloudDataStorage, 0, 0, numPointCloudMip3Elements * 2);
			this.compBuffers.Add(this.pointCloudDataMip3Buffer);
			pointCloudDataMip3OptBuffer.Release();

			this.materialProperties.SetInt("memoryOptimized", 1);
		}

		void expandMemoryMicroVoxel(){
			this.memoryOptimized = false;

			int numChunks = this.numChunks;
			if(this.autoBounds){
				numChunks = ClayContainer.totalMaxChunks;
			}

			this.compBuffers.Remove(this.gridPointersMip3Buffer);
			this.compBuffers.Remove(this.gridPointersMip2Buffer);
			this.compBuffers.Remove(this.pointCloudDataMip3Buffer);

			this.gridPointersMip3Buffer.Release();
			this.gridPointersMip2Buffer.Release();
			this.pointCloudDataMip3Buffer.Release();

			this.materialProperties.SetInt("memoryOptimized", 0);

			int reducedMip3BufferSize = (int)(((float)(256 * 256 * 256) * numChunks) * ClayContainer.bufferSizeReduceFactor);

			this.chunkIdOffsetBuffer.SetData(ClayContainer.chunkIdOffsetDefaultData);

			this.gridPointersMip3Buffer = new ComputeBuffer(reducedMip3BufferSize, sizeof(int));
			this.compBuffers.Add(this.gridPointersMip3Buffer);

			this.gridPointersMip2Buffer = new ComputeBuffer((64 * 64 * 64) * numChunks, sizeof(int));
			this.compBuffers.Add(this.gridPointersMip2Buffer);

			this.pointCloudDataMip3Buffer = new ComputeBuffer(reducedMip3BufferSize, sizeof(int) * 2);
			this.compBuffers.Add(this.pointCloudDataMip3Buffer);

			this.chunkIdOffsetBuffer.SetData(ClayContainer.chunkIdOffsetDefaultData);

			ClayContainer.microvoxelDrawData[0] = 36 * numChunks;
			ClayContainer.microvoxelDrawData[1] = this.instances.Count + 1;
			this.volumetricDrawBuffer.SetData(ClayContainer.microvoxelDrawData);

			this.updateChunksBuffer.SetData(ClayContainer.updateChunksDefaultValues, 0, 0, numChunks);
			this.indirectChunkArgs1Buffer.SetData(ClayContainer.indirectChunk1DefaultValues, 0, 0, numChunks * 3);
			this.indirectChunkArgs2Buffer.SetData(ClayContainer.indirectChunk2DefaultValues, 0, 0, numChunks * 3);
			this.indirectChunkArgs3Buffer.SetData(new int[]{this.chunksX, this.chunksY, this.chunksZ});

			this.forceUpdateAllSolids();
		}

		void optimizeMemoryPolySplat(){
			this.memoryOptimized = true;
			this.userWarning = "";
			this.updateFrame = 0;

			int reducedMip3BufferSize = (int)(((float)(256 * 256 * 256) * this.numChunks) * ClayContainer.bufferSizeReduceFactor);
			int[] tmpPointCloudDataStorage = new int[reducedMip3BufferSize * 2];

			this.indirectDrawArgsBuffer.GetData(ClayContainer.indirectArgsData);	
			
			this.pointCount = ClayContainer.indirectArgsData[0] / 3;

			if(this.pointCount > ClayContainer.maxPointCount){
				this.pointCount = ClayContainer.maxPointCount;

				this.userWarning = "max point count exceeded, increase limit from Global Config window";

				Debug.Log("Clayxels: container " + this.gameObject.name + " has exceeded the limit of points allowed, increase limit from Global Config window");
			}
			
			if(this.pointCount > 0){
				float accurateBoundsSize = this.computeBoundsSize() * this.transform.lossyScale.x;
				this.renderBounds.size = new Vector3(accurateBoundsSize, accurateBoundsSize, accurateBoundsSize);

				int bufferId = this.compBuffers.IndexOf(this.pointCloudDataMip3Buffer);
				this.pointCloudDataMip3Buffer.GetData(tmpPointCloudDataStorage, 0, 0, this.pointCount * 2);
				this.pointCloudDataMip3Buffer.Release();
				this.pointCloudDataMip3Buffer = new ComputeBuffer(this.pointCount, sizeof(int) * 2);
				this.compBuffers[bufferId] = this.pointCloudDataMip3Buffer;
				this.pointCloudDataMip3Buffer.SetData(tmpPointCloudDataStorage, 0, 0, this.pointCount * 2);
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizePointCloud, "pointCloudDataMip3", this.pointCloudDataMip3Buffer);
				this.materialProperties.SetBuffer("chunkPoints", this.pointCloudDataMip3Buffer);

				bufferId = this.compBuffers.IndexOf(this.pointToChunkIdBuffer);
				int pointToChunkBufferSize = (this.pointCount / 5) + 1;
				this.pointToChunkIdBuffer.GetData(tmpPointCloudDataStorage, 0, 0, pointToChunkBufferSize);
				this.pointToChunkIdBuffer.Release();
				this.pointToChunkIdBuffer = new ComputeBuffer(pointToChunkBufferSize, sizeof(int));
				this.compBuffers[bufferId] = this.pointToChunkIdBuffer;
				this.pointToChunkIdBuffer.SetData(tmpPointCloudDataStorage, 0, 0, pointToChunkBufferSize);
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizePointCloud, "pointToChunkId", this.pointToChunkIdBuffer);
				this.materialProperties.SetBuffer("pointToChunkId", this.pointToChunkIdBuffer);

				this.materialProperties.SetInt("solidHighlightId", -1);

				this.indirectDrawArgsBuffer2.SetData(ClayContainer.indirectArgsData);

				this.renderIndirectDrawArgsBuffer = this.indirectDrawArgsBuffer2;

				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.optimizePointCloud, "indirectDrawArgs", this.indirectDrawArgsBuffer);
					
				ClayContainer.claycoreCompute.Dispatch((int)Kernels.optimizePointCloud, 1, 1, 1);
			}
			
			ClayContainer.indirectArgsData[0] = 0;
		}

		void expandMemory(){
			if(!this.memoryOptimized){
				return;
			}

			if(this.renderMode == ClayContainer.RenderModes.polySplat){
				this.expandMemoryPolySplat();
			}
			else{
				this.expandMemoryMicroVoxel();
			}
		}

		void expandMemoryPolySplat(){
			this.memoryOptimized = false;

			int numChunks = this.numChunks;
			if(this.autoBounds){
				numChunks = ClayContainer.totalMaxChunks;
			}

			int bufferId = this.compBuffers.IndexOf(this.pointCloudDataMip3Buffer);
			this.pointCloudDataMip3Buffer.Release();
			this.pointCloudDataMip3Buffer = new ComputeBuffer(ClayContainer.maxPointCount * this.numChunks, sizeof(int) * 2);
			this.compBuffers[bufferId] = this.pointCloudDataMip3Buffer;

			this.materialProperties.SetBuffer("chunkPoints", this.pointCloudDataMip3Buffer);

			bufferId = this.compBuffers.IndexOf(this.pointToChunkIdBuffer);
			this.pointToChunkIdBuffer.Release();
			this.pointToChunkIdBuffer = new ComputeBuffer((ClayContainer.maxPointCount / 5) * this.numChunks, sizeof(int));
			this.compBuffers[bufferId] = this.pointToChunkIdBuffer;

			this.renderIndirectDrawArgsBuffer = this.indirectDrawArgsBuffer;

			this.LODLevel = 0;
			
			this.forceUpdateAllSolids();
		}

		static void parseSolidsAttrs(string content, ref int lastParsed){
			string[] lines = content.Split(new[]{ "\r\n", "\r", "\n" }, StringSplitOptions.None);
			for(int i = 0; i < lines.Length; ++i){
				string line = lines[i];
				if(line.Contains("label: ")){
					if(line.Split('/').Length == 3){// if too many comment slashes, it's a commented out solid,
						lastParsed += 1;

						string[] parameters = line.Split(new[]{"label:"}, StringSplitOptions.None)[1].Split(',');
						string label = parameters[0].Trim();
						
						ClayContainer.solidsCatalogueLabels.Add(label);

						List<string[]> paramList = new List<string[]>();

						for(int paramIt = 1; paramIt < parameters.Length; ++paramIt){
							string param = parameters[paramIt];
							string[] attrs = param.Split(':');
							string paramId = attrs[0];
							string[] paramLabelValue = attrs[1].Split(' ');
							string paramLabel = paramLabelValue[1];
							string paramValue = paramLabelValue[2];

							paramList.Add(new string[]{paramId.Trim(), paramLabel.Trim(), paramValue.Trim()});
						}

						ClayContainer.solidsCatalogueParameters.Add(paramList);
					}
				}
			}
		}

		void initSolidsData(){
			this.solidsPosBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 3);
			this.compBuffers.Add(this.solidsPosBuffer);
			this.solidsRotBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 4);
			this.compBuffers.Add(this.solidsRotBuffer);
			this.solidsScaleBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 3);
			this.compBuffers.Add(this.solidsScaleBuffer);
			this.solidsBlendBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float));
			this.compBuffers.Add(this.solidsBlendBuffer);
			this.solidsTypeBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(int));
			this.compBuffers.Add(this.solidsTypeBuffer);
			this.solidsColorBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 3);
			this.compBuffers.Add(this.solidsColorBuffer);
			this.solidsAttrsBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 4);
			this.compBuffers.Add(this.solidsAttrsBuffer);
			this.solidsAttrs2Buffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(float) * 4);
			this.compBuffers.Add(this.solidsAttrs2Buffer);
			this.solidsClayObjectIdBuffer = new ComputeBuffer(ClayContainer.maxSolids, sizeof(int));
			this.compBuffers.Add(this.solidsClayObjectIdBuffer);

			this.solidsPos = new List<Vector3>(new Vector3[ClayContainer.maxSolids]);
			this.solidsRot = new List<Quaternion>(new Quaternion[ClayContainer.maxSolids]);
			this.solidsScale = new List<Vector3>(new Vector3[ClayContainer.maxSolids]);
			this.solidsBlend = new List<float>(new float[ClayContainer.maxSolids]);
			this.solidsType = new List<int>(new int[ClayContainer.maxSolids]);
			this.solidsColor = new List<Vector3>(new Vector3[ClayContainer.maxSolids]);
			this.solidsAttrs = new List<Vector4>(new Vector4[ClayContainer.maxSolids]);
			this.solidsAttrs2 = new List<Vector4>(new Vector4[ClayContainer.maxSolids]);
			this.solidsClayObjectId = new List<int>(new int[ClayContainer.maxSolids]);
		}

		void OnDisable(){
			this.visible = false;
		}

		void OnEnable(){
			this.visible = true;
		}

		void OnDestroy(){
			this.invalidated = true;

			ClayContainer.totalChunksInScene = 0;

			this.releaseBuffers();
			
			this.removeFromScene();

			if(ClayContainer.containersInScene.Count == 0){
				ClayContainer.releaseGlobalBuffers();
			}

			this.instances.Clear();
			
			if(this.instanceOf != null){
				this.instanceOf.instances.Remove(this);
				this.instanceOf.initInstancesData();
			}

			#if UNITY_EDITOR
				this.removeEditorEvents();
				if(!Application.isPlaying){
					if(ClayContainer.containersToRender.Count == 0){
						ClayContainer.globalDataNeedsInit = true;
					}
				}
			#endif
		}

		void releaseBuffers(){
			for(int i = 0; i < this.compBuffers.Count; ++i){
				this.compBuffers[i].Release();
			}

			this.compBuffers.Clear();
		}

		static void releaseGlobalBuffers(){
			for(int i = 0; i < ClayContainer.globalCompBuffers.Count; ++i){
				ClayContainer.globalCompBuffers[i].Release();
			}

			ClayContainer.globalCompBuffers.Clear();

			ClayContainer.globalDataNeedsInit = true;
		}

		void limitChunkValues(){
			if(this.chunksX > ClayContainer.maxChunkX){
				this.chunksX = ClayContainer.maxChunkX;
			}
			if(this.chunksY > ClayContainer.maxChunkY){
				this.chunksY = ClayContainer.maxChunkY;
			}
			if(this.chunksZ > ClayContainer.maxChunkZ){
				this.chunksZ = ClayContainer.maxChunkZ;
			}
			if(this.chunksX < 1){
				this.chunksX = 1;
			}
			if(this.chunksY < 1){
				this.chunksY = 1;
			}
			if(this.chunksZ < 1){
				this.chunksZ = 1;
			}

			if(this.chunkSize < 4){
				this.chunkSize = 4;
			}
			else if(this.chunkSize > 255){
				this.chunkSize = 255;
			}
		}

		void initChunks(){
			if(this.autoBounds){
				this.chunksX = ClayContainer.maxChunkX;
				this.chunksY = ClayContainer.maxChunkY;
				this.chunksZ = ClayContainer.maxChunkZ;
			}

			this.numChunks = this.chunksX * this.chunksY * this.chunksZ;

			this.boundsScale.x = (float)this.chunkSize * this.chunksX;
			this.boundsScale.y = (float)this.chunkSize * this.chunksY;
			this.boundsScale.z = (float)this.chunkSize * this.chunksZ;
			this.renderBounds.size = this.boundsScale * this.transform.lossyScale.x;

			float gridCenterOffset = (this.chunkSize * 0.5f);
			this.boundsCenter.x = ((this.chunkSize * (this.chunksX - 1)) * 0.5f) - (gridCenterOffset*(this.chunksX-1));
			this.boundsCenter.y = ((this.chunkSize * (this.chunksY - 1)) * 0.5f) - (gridCenterOffset*(this.chunksY-1));
			this.boundsCenter.z = ((this.chunkSize * (this.chunksZ - 1)) * 0.5f) - (gridCenterOffset*(this.chunksZ-1));

			this.materialProperties = new MaterialPropertyBlock();

			this.numPointsInChunkBuffer = new ComputeBuffer(this.numChunks, sizeof(int));
			this.compBuffers.Add(this.numPointsInChunkBuffer);

			int reducedMip3BufferSize = (int)(((float)(256 * 256 * 256) * this.numChunks) * ClayContainer.bufferSizeReduceFactor);

			if(this.renderMode == ClayContainer.RenderModes.polySplat){
				this.pointToChunkIdBuffer = new ComputeBuffer((ClayContainer.maxPointCount / 5) * this.numChunks, sizeof(int));
				this.compBuffers.Add(this.pointToChunkIdBuffer);

				ClayContainer.claycoreCompute.SetFloat("seamOffsetMultiplier", 4.0f);
			}
			else{ // init microVoxel renderer buffers
				this.gridPointersMip3Buffer = new ComputeBuffer(reducedMip3BufferSize, sizeof(int));
				this.compBuffers.Add(this.gridPointersMip3Buffer);

				this.gridPointersMip2Buffer = new ComputeBuffer((64 * 64 * 64) * this.numChunks, sizeof(int));
				this.compBuffers.Add(this.gridPointersMip2Buffer);

				this.chunkIdOffsetBuffer = new ComputeBuffer(ClayContainer.totalMaxChunks, sizeof(int));
				this.compBuffers.Add(this.chunkIdOffsetBuffer);

				this.chunkIdOffsetBuffer.SetData(ClayContainer.chunkIdOffsetDefaultData);

				ClayContainer.claycoreCompute.SetFloat("seamOffsetMultiplier", 8.0f);
			}

			this.boundingBoxBuffer = new ComputeBuffer(ClayContainer.totalMaxChunks * 6, sizeof(int)); 
			this.compBuffers.Add(this.boundingBoxBuffer);

			this.pointCloudDataMip3Buffer = new ComputeBuffer(reducedMip3BufferSize, sizeof(int) * 2);
			this.compBuffers.Add(this.pointCloudDataMip3Buffer);

			this.chunksCenterBuffer = new ComputeBuffer(this.numChunks, sizeof(float) * 3);
			this.compBuffers.Add(this.chunksCenterBuffer);

			if(this.numChunks == 1){
				this.chunksCenterBuffer.SetData(new float[]{0.0f, 0.0f, 0.0f});
			}

			this.indirectDrawArgsBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(this.indirectDrawArgsBuffer);

			this.indirectDrawArgsBuffer2 = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(this.indirectDrawArgsBuffer2);

			this.renderIndirectDrawArgsBuffer = this.indirectDrawArgsBuffer;

			ClayContainer.indirectArgsData[0] = 0;
			this.indirectDrawArgsBuffer.SetData(ClayContainer.indirectArgsData);
			this.indirectDrawArgsBuffer2.SetData(ClayContainer.indirectArgsData);

			this.updateChunksBuffer = new ComputeBuffer(this.numChunks, sizeof(int));
			this.compBuffers.Add(this.updateChunksBuffer);

			this.indirectChunkArgs1Buffer = new ComputeBuffer(this.numChunks * 3, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(this.indirectChunkArgs1Buffer);

			this.indirectChunkArgs2Buffer = new ComputeBuffer(this.numChunks * 3, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(this.indirectChunkArgs2Buffer);

			this.indirectChunkArgs3Buffer = new ComputeBuffer(3, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(this.indirectChunkArgs3Buffer);

			this.updateChunksBuffer.SetData(ClayContainer.updateChunksDefaultValues, 0, 0, this.numChunks);
			this.indirectChunkArgs1Buffer.SetData(ClayContainer.indirectChunk1DefaultValues, 0, 0, this.numChunks * 3);
			this.indirectChunkArgs2Buffer.SetData(ClayContainer.indirectChunk2DefaultValues, 0, 0, this.numChunks * 3);
			this.indirectChunkArgs3Buffer.SetData(new int[]{this.chunksX, this.chunksY, this.chunksZ});

			this.volumetricDrawBuffer = new ComputeBuffer(8, sizeof(int), ComputeBufferType.IndirectArguments);
			this.compBuffers.Add(this.volumetricDrawBuffer);

			ClayContainer.microvoxelDrawData[0] = 36 * this.numChunks;
			ClayContainer.microvoxelDrawData[1] = this.instances.Count + 1;
			this.volumetricDrawBuffer.SetData(ClayContainer.microvoxelDrawData);
		}

		void initMaterialProperties(){
			bool initDefaults = false;

			if(this.customMaterial != null){
				this.material = this.customMaterial;
			}
			else{
				string renderModeMatSuffix = "";
				if(this.renderMode == ClayContainer.RenderModes.microVoxel){
					renderModeMatSuffix = "MicroVoxelASE";
				}

				if(ClayContainer.renderPipe == "hdrp"){
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
	            		renderModeMatSuffix += "_2020_2";
	            	}
	            }
				
				if(this.material != null && this.customMaterial == null){// validate default shader
					if(ClayContainer.renderPipe == "hdrp" && this.material.shader.name != "Clayxels/ClayxelHDRPShader" + renderModeMatSuffix){
						this.material = null;
					}
					else if(ClayContainer.renderPipe == "urp" && this.material.shader.name != "Clayxels/ClayxelURPShader" + renderModeMatSuffix){
						this.material = null;
					}
					else if(ClayContainer.renderPipe == "builtin" && this.material.shader.name != "Clayxels/ClayxelBuiltInShader"){
						this.material = null;
					}
				}
				
				if(this.material != null && this.customMaterial == null){
					// if material is still not null, means it's a valid shader,
					// probably this container got duplicated in scene
					this.material = new Material(this.material);
				}
				else{
					// brand new container, lets create a new material
					if(ClayContainer.renderPipe == "hdrp"){
						this.material = new Material(Shader.Find("Clayxels/ClayxelHDRPShader" + renderModeMatSuffix));
					}
					else if(ClayContainer.renderPipe == "urp"){
						this.material = new Material(Shader.Find("Clayxels/ClayxelURPShader" + renderModeMatSuffix));
					}
					else{
						this.material = new Material(Shader.Find("Clayxels/ClayxelBuiltInShader"));
					}

					initDefaults = true;
				}
			}

			if(this.customMaterial == null && initDefaults){
				// set the default clayxel texture to a dot on the standard material
				Texture texture = this.material.GetTexture("_MainTex");
				if(texture == null){
					this.material.SetTexture("_MainTex", (Texture)Resources.Load("clayxelDot"));
				}

				if(this.renderMode == ClayContainer.RenderModes.microVoxel){
					this.material.SetTexture("_MainTex", (Texture)Resources.Load("clayxelDotBlur"));

					this.material.SetFloat("_splatSizeMult", 0.5f);
					this.material.SetFloat("_roughSize", 0.3f);
					this.material.SetFloat("_alphaCutout", 0.5f);
				}
			}
			
			this.material.SetFloat("chunkSize", (float)this.chunkSize);
		}

		int _compoundLastId = -1;
		void scanRecursive(Transform trn, List<ClayObject> collectedClayObjs){
			bool insideCompound = false;

			ClayObject clayObj = trn.gameObject.GetComponent<ClayObject>();
			if(clayObj != null){
				if(clayObj.isValid() && trn.gameObject.activeSelf){
					clayObj._setGroupEnd(-1);

					if(clayObj.getMode() == ClayObject.ClayObjectMode.clayGroup){
						insideCompound = true;
					}

					if(this.clayObjectsOrderLocked){
						clayObj.clayObjectId = collectedClayObjs.Count;
						collectedClayObjs.Add(clayObj);
					}
					else{
						int id = clayObj.clayObjectId;
						if(id < 0){
							id = 0;
						}

						if(id > collectedClayObjs.Count - 1){
							collectedClayObjs.Add(clayObj);
						}
						else{
							collectedClayObjs.Insert(id, clayObj);
						}
					}

					this._compoundLastId = clayObj.clayObjectId;
				}
			}

			for(int i = 0; i < trn.childCount; ++i){
				GameObject childObj = trn.GetChild(i).gameObject;
				if(childObj.activeSelf && childObj.GetComponent<ClayContainer>() == null){
					this.scanRecursive(childObj.transform, collectedClayObjs);
				}
			}

			if(insideCompound){
				int compoundClayObjId = clayObj.clayObjectId;
				collectedClayObjs[this._compoundLastId]._setGroupEnd(compoundClayObjId);
			}
		}

		void collectClayObject(ClayObject clayObj){
			if(clayObj.getNumSolids() == 0){
				clayObj.init();
			}

			clayObj.clayObjectId = this.clayObjects.Count;
			this.clayObjects.Add(clayObj);

			int numSolids = clayObj.getNumSolids();
			if(clayObj.getMode() == ClayObject.ClayObjectMode.clayGroup){
				numSolids = 1;
			}

			for(int i = 0; i < numSolids; ++i){
				Solid solid = clayObj.getSolid(i);
				solid.id = this.solids.Count;
				solid.clayObjectId = clayObj.clayObjectId;
				this.solids.Add(solid);

				if(solid.id < ClayContainer.maxSolids){
					this.solidsUpdatedDict[solid.id] = 1;
				}
				else{
					break;
				}
			}

			if(clayObj._isGroupEnd()){
				int compoundClayObjId = clayObj._getGroupClayObjectId();
				ClayObject compoundClayObj = this.clayObjects[compoundClayObjId];
				
				Solid solid = compoundClayObj._getGroupEndSolid();
				solid.id = this.solids.Count;
				solid.clayObjectId = compoundClayObj.clayObjectId;
				this.solids.Add(solid);
				
				if(solid.id < ClayContainer.maxSolids){
					this.solidsUpdatedDict[solid.id] = 1;
				}
			}

			clayObj.transform.hasChanged = true;
			clayObj.setClayxelContainer(this);
		}

		int getBufferCount(ComputeBuffer buffer){
			ComputeBuffer.CopyCount(buffer, this.genericNumberBuffer, 0);
			this.genericNumberBuffer.GetData(this.genericIntBufferArray);
			int count = this.genericIntBufferArray[0];

			return count;
		}

		void updateSolids(){
			foreach(int i in this.solidsUpdatedDict.Keys){
				if(i > this.solids.Count - 1){
					continue;
				}
				
				Solid solid = this.solids[i];

				int clayObjId = solid.clayObjectId;
				if(solid.clayObjectId > -1){
					ClayObject clayObj = this.clayObjects[solid.clayObjectId];
					clayObj.pullUpdate();
				}
				else{
					clayObjId = 0;
				}

				this.solidsPos[i] = solid.position;
				this.solidsRot[i] = solid.rotation;
				this.solidsScale[i] = solid.scale;
				this.solidsBlend[i] = solid.blend * ClayContainer.globalBlend;
				this.solidsType[i] = solid.primitiveType;
				this.solidsColor[i] = solid.color;
				this.solidsAttrs[i] = solid.attrs;
				this.solidsAttrs2[i] = solid.attrs2;
				this.solidsClayObjectId[i] = clayObjId;
			}

			if(this.solids.Count > 0){
				this.solidsPosBuffer.SetData(this.solidsPos);
				this.solidsRotBuffer.SetData(this.solidsRot);
				this.solidsScaleBuffer.SetData(this.solidsScale);
				this.solidsBlendBuffer.SetData(this.solidsBlend);
				this.solidsTypeBuffer.SetData(this.solidsType);
				this.solidsColorBuffer.SetData(this.solidsColor);
				this.solidsAttrsBuffer.SetData(this.solidsAttrs);
				this.solidsAttrs2Buffer.SetData(this.solidsAttrs2);
				this.solidsClayObjectIdBuffer.SetData(this.solidsClayObjectId);
			}

			ClayContainer.claycoreCompute.SetInt("numSolids", this.solids.Count);
			ClayContainer.claycoreCompute.SetFloat("chunkSize", (float)this.chunkSize);

			if(this.numChunks > 1 || this.autoBounds){
				ClayContainer.claycoreCompute.SetInt("numSolidsUpdated", this.solidsUpdatedDict.Count);
				int[] solidsUpdatedArray = new int[this.solidsUpdatedDict.Count];
				this.solidsUpdatedDict.Keys.CopyTo(solidsUpdatedArray, 0);
				ClayContainer.solidsUpdatedBuffer.SetData(solidsUpdatedArray);
				
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "numPointsInChunk", this.numPointsInChunkBuffer);
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "chunksCenter", this.chunksCenterBuffer);
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "boundingBox", this.boundingBoxBuffer);

				ClayContainer.claycoreCompute.Dispatch((int)Kernels.filterSolidsPerChunk, this.chunksX, this.chunksY, this.chunksZ);
			}
			else if(this.renderMode == ClayContainer.RenderModes.polySplat){
				// reset point cloud to render zero number of points
				this.numPointsInChunkBuffer.SetData(ClayContainer.pointsInChunkDefaultValues, 0, 0, this.numChunks);
			}

			this.solidsUpdatedDict.Clear();
		}

		float _debugAutoBounds = 0.0f;
		void updateAutoBounds(float boundsSize){
			this._debugAutoBounds = boundsSize;

			int newChunkSize = Mathf.CeilToInt(boundsSize / ClayContainer.maxChunkX);

			int detailChunkSize = (int)Mathf.Lerp(40.0f, 4.0f, (float)this.clayxelDetail / 100.0f);

			int estimatedNumChunks = Mathf.CeilToInt(boundsSize / detailChunkSize);

			if(estimatedNumChunks < 1){
				estimatedNumChunks = 1;
			}
			else if(estimatedNumChunks > ClayContainer.maxChunkX){
				estimatedNumChunks = ClayContainer.maxChunkX;
			}

			this.autoFrameSkip = estimatedNumChunks;
			
			if(estimatedNumChunks != this.chunksX){
				this.autoBoundsChunkSize = newChunkSize;

				this.resizeChunks(estimatedNumChunks);
				this.updateInternalBounds();
				this.forceUpdateAllSolids();
			}
			else if(newChunkSize != this.autoBoundsChunkSize){
				this.autoBoundsChunkSize = newChunkSize;

				if(this.autoBoundsChunkSize > detailChunkSize){
					this.updateInternalBounds();
					this.forceUpdateAllSolids();
				}
			}
		}

		void resizeChunks(int estimatedNumChunks){
			this.chunksX = estimatedNumChunks;
			this.chunksY = estimatedNumChunks;
			this.chunksZ = estimatedNumChunks;
			
			if(this.autoBoundsChunkSize > this.chunkSize){
				this.chunkSize = this.autoBoundsChunkSize;
			}

			this.boundsScale.x = (float)this.chunkSize * this.chunksX;
			this.boundsScale.y = (float)this.chunkSize * this.chunksY;
			this.boundsScale.z = (float)this.chunkSize * this.chunksZ;
			this.renderBounds.size = this.boundsScale * this.transform.lossyScale.x;

			float gridCenterOffset = (this.chunkSize * 0.5f);
			this.boundsCenter.x = ((this.chunkSize * (this.chunksX - 1)) * 0.5f) - (gridCenterOffset*(this.chunksX-1));
			this.boundsCenter.y = ((this.chunkSize * (this.chunksY - 1)) * 0.5f) - (gridCenterOffset*(this.chunksY-1));
			this.boundsCenter.z = ((this.chunkSize * (this.chunksZ - 1)) * 0.5f) - (gridCenterOffset*(this.chunksZ-1));

			float seamOffset = (this.chunkSize / 256.0f);
			float chunkOffset = this.chunkSize - seamOffset;

			this.numChunks = this.chunksX * this.chunksY * this.chunksZ;

			ClayContainer.claycoreCompute.SetInt("numChunksX", this.chunksX);
			ClayContainer.claycoreCompute.SetInt("numChunksY", this.chunksY);
			ClayContainer.claycoreCompute.SetInt("numChunksZ", this.chunksZ);
			
			this.updateChunksBuffer.SetData(ClayContainer.updateChunksDefaultValues, 0, 0, this.numChunks);

			this.indirectChunkArgs3Buffer.SetData(new int[]{this.chunksX, this.chunksY, this.chunksZ});
		}

		void initAutoBounds(){
			float boundsSize = this.computeBoundsSize();
			this.updateAutoBounds(boundsSize);
			this.updateInternalBounds();
		}

		float computeBoundsSize(){
			Vector3 autoBoundsScale = Vector3.zero;

			int solidCount = this.solids.Count;
			if(solidCount > ClayContainer.maxSolids){
				solidCount = ClayContainer.maxSolids;
			}

			float cellSizeOffset = ((float)this.chunkSize / 256.0f) * (16.0f * this.chunksX);

			for(int i = 0; i < solidCount; ++i){
				Solid solid = this.solids[i];

				if(solid.clayObjectId > -1){
					ClayObject clayObj = this.clayObjects[solid.clayObjectId];
					clayObj.pullUpdate();
				}
				
				float boundingSphere = Mathf.Sqrt(Vector3.Dot(solid.scale, solid.scale)) * 1.732f;
				float autoBoundsPosX = Mathf.Abs(solid.position.x * 2.0f) + boundingSphere + cellSizeOffset;
				float autoBoundsPosY = Mathf.Abs(solid.position.y * 2.0f) + boundingSphere + cellSizeOffset;
				float autoBoundsPosZ = Mathf.Abs(solid.position.z * 2.0f) + boundingSphere + cellSizeOffset;

				if(autoBoundsPosX > autoBoundsScale.x){
					autoBoundsScale.x = autoBoundsPosX;
				}
				if(autoBoundsPosY > autoBoundsScale.y){
					autoBoundsScale.y = autoBoundsPosY;
				}
				if(autoBoundsPosZ > autoBoundsScale.z){
					autoBoundsScale.z = autoBoundsPosZ;
				}
			}

			float autoBoundsChunkSize = Mathf.Max(autoBoundsScale.x, Mathf.Max(autoBoundsScale.y, autoBoundsScale.z));

			return autoBoundsChunkSize;
		}

		void logFPS(){
			this.deltaTime += (Time.unscaledDeltaTime - this.deltaTime) * 0.1f;
			float fps = 1.0f / this.deltaTime;
			Debug.Log(fps);
		}

		void switchComputeData(){
			ClayContainer.lastUpdatedContainerId = this.GetInstanceID();
			
			ClayContainer.claycoreCompute.SetFloat("globalRoundCornerValue", this.globalSmoothing);

			ClayContainer.claycoreCompute.SetInt("numChunksX", this.chunksX);
			ClayContainer.claycoreCompute.SetInt("numChunksY", this.chunksY);
			ClayContainer.claycoreCompute.SetInt("numChunksZ", this.chunksZ);

			this.bindSolidsBuffers((int)Kernels.computeGrid);
			this.bindSolidsBuffers((int)Kernels.computeGridMip3);
			
			if(this.numChunks == 1 && !this.autoBounds){
				this.genericIntBufferArray[0] = this.solids.Count;
				ClayContainer.numSolidsPerChunkBuffer.SetData(this.genericIntBufferArray);

				ClayContainer.solidsPerChunkBuffer.SetData(ClayContainer.solidsInSingleChunkArray);
			}
			else{
				this.bindSolidsBuffers((int)Kernels.filterSolidsPerChunk);

				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "updateChunks", this.updateChunksBuffer);
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "indirectChunkArgs1", this.indirectChunkArgs1Buffer);
				ClayContainer.claycoreCompute.SetBuffer((int)Kernels.filterSolidsPerChunk, "indirectChunkArgs2", this.indirectChunkArgs2Buffer);
			}
		}

		void bindSolidsBuffers(int kernId){
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsPos", this.solidsPosBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsRot", this.solidsRotBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsScale", this.solidsScaleBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsBlend", this.solidsBlendBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsType", this.solidsTypeBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsColor", this.solidsColorBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsAttrs", this.solidsAttrsBuffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsAttrs2", this.solidsAttrs2Buffer);
			ClayContainer.claycoreCompute.SetBuffer(kernId, "solidsClayObjectId", this.solidsClayObjectIdBuffer);
		}

		void Start(){
			this.checkContainerIdIsUnique();
			
			if(this.needsInit){
				this.init();
			}
		}

		bool checkNeedsInit(){
			// we need to perform these checks because prefabs will reset some of these attributes upon instancing
			if(this.needsInit || this.numChunks == 0 || this.material == null){
				return true;
			}

			return false;
		}

		void updateClay(){
			if(this.checkNeedsInit()){
				this.init();
				this.updateFrame = 0;
			}
			else{
				// inhibit updates if this transform is the trigger
				if(this.transform.hasChanged){
					this.needsUpdate = false;
					this.transform.hasChanged = false;

					// if this transform moved and also one of the solids moved, then we still need to update
					if(this.forceUpdate){
						this.needsUpdate = true;
					}
				}
			}
			
			if(this.needsUpdate && this.updateFrame == 0){
				this.computeClay();
			}

			int updateCounter = ClayContainer.frameSkip + this.autoFrameSkip;
			if(updateCounter > 0){
				this.updateFrame = (this.updateFrame + 1) % updateCounter;
			}
		}

		void updateInstances(){
			this.instancesMatrix[0] = this.transform.localToWorldMatrix;
			this.instancesMatrixInv[0] = this.transform.worldToLocalMatrix;

			this.renderBounds.center = this.transform.position;

			for(int i = 0; i < this.instances.Count; ++i){
				ClayContainer instance = this.instances[i];
				
				this.instancesMatrix[i + 1] = instance.transform.localToWorldMatrix;
				this.instancesMatrixInv[i + 1] = instance.transform.worldToLocalMatrix;

				instance.renderBounds = this.renderBounds;
				instance.renderBounds.center = instance.transform.position;
				this.renderBounds.Encapsulate(instance.renderBounds);
			}

			this.instancesMatrixBuffer.SetData(this.instancesMatrix);
			this.instancesMatrixInvBuffer.SetData(this.instancesMatrixInv);

			this.materialProperties.SetVector("renderBoundsCenter", this.renderBounds.center);
		}

		void Update(){
			if(this.instanceOf != null){
				return;
			}

			if(this.clayObjects.Count == 0){
				if(!this.solidsHierarchyNeedsScan){
					return;
				}
			}

			// lets make sure we're not referring to an instance that got deleted
			#if UNITY_EDITOR
				if(!Application.isPlaying){
					if(this.instanceOf == null && this.instancingOtherContainer){
						this.needsInit = true;
					}
				}
			#endif
			
			this.updateClay();
			
			if(this.visible){
				this.drawClayxels(this);
			}
		}

		// All functions past this point are used only in editor
		
		public static Color boundsColor = new Color(0.5f, 0.5f, 1.0f, 0.1f);
		public static string pickingKey = "p";
		public static string mirrorDuplicateKey = "m";

		#if UNITY_EDITOR

		void Awake(){
			if(!Application.isPlaying){
				this.needsInit = true;
			}
		}

		public static void linkAllPrefabInstances(ClayContainer clayContainer){
			GameObject thisPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(clayContainer.gameObject);

			if(!thisPrefab.name.EndsWith("_clayPrefab")){
				if(thisPrefab.name.Contains("_clayPrefab")){
					string[] tokens = thisPrefab.name.Split(new string[]{"_clayPrefab"}, StringSplitOptions.None);
					thisPrefab.name = tokens[0];
				}

				thisPrefab.name += "_clayPrefab";
			}

			ClayContainer[] sourceContainers = thisPrefab.GetComponentsInChildren<ClayContainer>();
			for(int i = 0; i < sourceContainers.Length; ++i){
				ClayContainer container = sourceContainers[i];
				container.setIsInstanceOf(null);
				container.enableAllClayObjects(true);
				clayContainer.needsInit = true;
				clayContainer.init();

				PrefabUtility.RecordPrefabInstancePropertyModifications(clayContainer);
			}

			PrefabUtility.RecordPrefabInstancePropertyModifications(thisPrefab);

			string thisPrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(thisPrefab);

			ClayContainer[] containers = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
			for(int i = 0; i < containers.Length; ++i){
				ClayContainer container = containers[i];

				if(container != clayContainer && PrefabUtility.IsPartOfAnyPrefab(container.gameObject)){
					
					GameObject otherPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(container.gameObject);

					if(otherPrefab != thisPrefab){
						string otherPrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(otherPrefab);

						if(otherPrefabPath == thisPrefabPath){

							if(otherPrefab.name.EndsWith("_clayPrefab")){
								otherPrefab.name = otherPrefab.name.Replace("_clayPrefab", "");
							}

							container.setIsInstanceOf(null);
							container.enableAllClayObjects(false);

							ClayContainer.linkNestedInstances(thisPrefab, otherPrefab);
							
							container.needsInit = true;
							container.init();

							PrefabUtility.RecordPrefabInstancePropertyModifications(container);
							PrefabUtility.RecordPrefabInstancePropertyModifications(otherPrefab);
						}
					}
				}
			}
		}

		void setupPrefab(){			
			if(this.instanceOf != null){
				return;
			}
			
			if(!PrefabUtility.IsPartOfAnyPrefab(this.gameObject)){
				return;
			}
			
			GameObject thisPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(this.gameObject);
			
			if(thisPrefab.name.EndsWith("_clayPrefab")){
				return;
			}

			string thisPrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(thisPrefab);
			
			GameObject sourcePrefab = null;

			ClayContainer[] containers = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
			for(int i = 0; i < containers.Length; ++i){
				ClayContainer container = containers[i];

				if(PrefabUtility.IsPartOfAnyPrefab(container.gameObject)){
					GameObject otherPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(container.gameObject);
					
					string otherPrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(otherPrefab);

					if(otherPrefab.name.EndsWith("_clayPrefab") &&
						otherPrefab != thisPrefab &&
						otherPrefabPath == thisPrefabPath){

						sourcePrefab = otherPrefab;
						break;
					}
				}
			}

			if(sourcePrefab != null){
				ClayContainer.linkNestedInstances(sourcePrefab, thisPrefab);

				PrefabUtility.RecordPrefabInstancePropertyModifications(thisPrefab);
			}
		}

		static void linkNestedInstances(GameObject source, GameObject dest){
			ClayContainer[] sourceContainers = source.GetComponentsInChildren<ClayContainer>();
			ClayContainer[] destContainers = dest.GetComponentsInChildren<ClayContainer>();

			if(sourceContainers.Length != destContainers.Length){
				return;
			}
			
			for(int i = 0; i < sourceContainers.Length; ++i){
				ClayContainer sourceContainer = sourceContainers[i];
				ClayContainer destContainer = destContainers[i];

				if(sourceContainer != destContainer){
					destContainer.enableAllClayObjects(false);
					destContainer.setIsInstanceOf(sourceContainer);

					PrefabUtility.RecordPrefabInstancePropertyModifications(destContainer);
				}
			}
		}

		static void checkPrefsIntegrity(){
			string configFileName = "";

			string[] assets = AssetDatabase.FindAssets("clayxelsPrefs t:TextAsset");
			for(int i = 0; i < assets.Length; ++i){
	    		string filename = AssetDatabase.GUIDToAssetPath(assets[i]);
	    		string[] tokens = filename.Split('.');
	    		if(tokens[tokens.Length - 1] == "json"){
	    			configFileName = filename;
	    			break;
	    		}
	    	}

	    	TextAsset configTextAsset = (TextAsset)Resources.Load("clayxelsPrefs", typeof(TextAsset));
    		
			if(configFileName == "" || configTextAsset.text == ""){
				ClayxelsPrefs prefs = new ClayxelsPrefs();
				
				string jsonText = JsonUtility.ToJson(prefs);
    			File.WriteAllText("Assets/Clayxels/Resources/clayxelsPrefs.json" , jsonText);
    			AssetDatabase.Refresh();
			}
		}

		public static void savePrefs(ClayxelsPrefs prefs){
			ClayContainer.prefsOverridden = false;

			string[] assets = AssetDatabase.FindAssets("clayxelsPrefs t:TextAsset");
	    	string configFileName = "";
	    	for(int i = 0; i < assets.Length; ++i){
	    		string filename = AssetDatabase.GUIDToAssetPath(assets[i]);
	    		string[] tokens = filename.Split('.');
	    		if(tokens[tokens.Length - 1] == "json"){
	    			configFileName = filename;
	    			break;
	    		}
	    	}

	    	string jsonText = JsonUtility.ToJson(prefs);
	    	
    		File.WriteAllText(configFileName , jsonText);
    		AssetDatabase.Refresh();
		}

		public void autoRenameClayObject(ClayObject clayObj){
			 List<string> solidsLabels = ClayContainer.solidsCatalogueLabels;

			string blendSign = "+";
			if(clayObj.blend < 0.0f){
				blendSign = "-";
			}

			string isColoring = "";
			if(clayObj.attrs.w == 1.0f){
				blendSign = "";
				isColoring = "[paint]";
			}

			string typeStr = "";

			if(clayObj.getMode() == ClayObject.ClayObjectMode.clayGroup){
				typeStr = "group";
			}
			else{
				typeStr = solidsLabels[clayObj.primitiveType];
			}

			clayObj.gameObject.name = "clay_" + typeStr + blendSign + isColoring;
		}

		public static void shortcutMirrorDuplicate(){
			for(int i = 0; i < UnityEditor.Selection.gameObjects.Length; ++i){
				GameObject gameObj = UnityEditor.Selection.gameObjects[i];
				if(gameObj.GetComponent<ClayObject>()){
					gameObj.GetComponent<ClayObject>().mirrorDuplicate();
				}
			}
		}

		static void shortcutAddClay(){
			if(UnityEditor.Selection.gameObjects.Length > 0){
				if(UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>()){
					ClayContainer container = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>().getClayContainer();
					ClayObject clayObj = container.addClayObject();
					UnityEditor.Selection.objects = new GameObject[]{clayObj.gameObject};
				}
				else if(UnityEditor.Selection.gameObjects[0].GetComponent<ClayContainer>() != null){
					ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayContainer>().addClayObject();
					UnityEditor.Selection.objects = new GameObject[]{clayObj.gameObject};
				}
			}
		}

		public static float getEditorUIScale(){
			PropertyInfo p =
				typeof(GUIUtility).GetProperty("pixelsPerPoint", BindingFlags.Static | BindingFlags.NonPublic);

			float editorUiScaling = 1.0f;
			if(p != null){
				editorUiScaling = (float)p.GetValue(null, null);
			}

			return editorUiScaling;
		}

		void updateMaterialInEditor(ClayContainer instance){
			if(instance.pickingThis && ClayContainer.pickedObj == null){
				if(this.renderMode == ClayContainer.RenderModes.polySplat){
					this.materialProperties.SetBuffer("pointCloudDataToSolidId", ClayContainer.pointCloudDataToSolidIdBuffer);
				}

				if(!instance.editingThisContainer){
					this.materialProperties.SetInt("solidHighlightId", -2);
				}
				else{
					this.materialProperties.SetInt("solidHighlightId", ClayContainer.pickedClayObjectId);
				}
			}
			else{
				this.materialProperties.SetInt("solidHighlightId", -1);
			}

			if(this.renderMode == ClayContainer.RenderModes.polySplat){
				this.materialProperties.SetBuffer("chunkPoints", this.pointCloudDataMip3Buffer);
				this.materialProperties.SetBuffer("chunksCenter", this.chunksCenterBuffer);
				this.materialProperties.SetBuffer("pointToChunkId", this.pointToChunkIdBuffer);
			}
		}

		[MenuItem("GameObject/3D Object/Clayxel Container" )]
		public static ClayContainer createNewContainer(){
			 GameObject newObj = new GameObject("ClayxelContainer");
			 ClayContainer newClayContainer = newObj.AddComponent<ClayContainer>();

			 UnityEditor.Selection.objects = new GameObject[]{newObj};

			 return newClayContainer;
		}

		void OnValidate(){
			// called on a few (annoying) occasions, like every time any script recompiles
			this.needsInit = true;
			this.numChunks = 0;
		}

		void removeEditorEvents(){
			AssemblyReloadEvents.beforeAssemblyReload -= this.onBeforeAssemblyReload;

			EditorApplication.hierarchyChanged -= this.onHierarchyChanged;

			UnityEditor.Selection.selectionChanged -= this.onSelectionChanged;

			Undo.undoRedoPerformed -= this.onUndoPerformed;
		}

		void reinstallEditorEvents(){
			this.removeEditorEvents();

			AssemblyReloadEvents.beforeAssemblyReload += this.onBeforeAssemblyReload;

			EditorApplication.hierarchyChanged += this.onHierarchyChanged;

			UnityEditor.Selection.selectionChanged += this.onSelectionChanged;

			Undo.undoRedoPerformed += this.onUndoPerformed;

			PrefabUtility.prefabInstanceUpdated -= ClayContainer.onPrefabUpdate;
			PrefabUtility.prefabInstanceUpdated += ClayContainer.onPrefabUpdate;
		}

		static void onPrefabUpdate(GameObject obj){
			if(Application.isPlaying){
				return;
			}

			// called when storing a new prefab

			string prefabFile = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
			string prefabPath = Path.GetDirectoryName(prefabFile);

			ClayContainer[] containers = obj.GetComponentsInChildren<ClayContainer>();
			
			for(int i = 0; i < containers.Length; ++i){
				ClayContainer container = containers[i];
				
				if(container.customMaterial != null){
					return;
				}
				
				if(container.material != null){
					if(!AssetDatabase.Contains(container.material)){
						Material storedMat = new Material(container.material);

						string assetNameUnique = obj.name + "_" + container.name + "_" + obj.GetInstanceID();
						string materialFile = Path.Combine(prefabPath, assetNameUnique + ".mat");
						AssetDatabase.CreateAsset(storedMat, materialFile);

						container.customMaterial = (Material)AssetDatabase.LoadAssetAtPath(materialFile, typeof(Material));
						container.material = container.customMaterial;
					}
				}
			}

			PrefabUtility.ApplyPrefabInstance(obj, InteractionMode.AutomatedAction);

			GameObject thisPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(obj);
			if(!thisPrefab.name.EndsWith("_clayPrefab")){
				thisPrefab.name += "_clayPrefab";
			}
		}

		void onBeforeAssemblyReload(){
			// called when this script recompiles

			if(Application.isPlaying){
				return;
			}

			this.releaseBuffers();
			ClayContainer.releaseGlobalBuffers();

			ClayContainer.globalDataNeedsInit = true;
			this.needsInit = true;

			ClayContainer[] containers = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
    		for(int i = 0; i < containers.Length; ++i){
    			containers[i].needsInit = true;
    		}
		}

		void onUndoPerformed(){
			if(this.invalidated){
				return;
			}

			this.updateFrame = 0;

			if(Undo.GetCurrentGroupName() == "changed clayobject" ||
				Undo.GetCurrentGroupName() == "changed clayxel container"){
				this.needsUpdate = true;
			}
			else if(Undo.GetCurrentGroupName() == "added clayxel solid"){
				this.scheduleClayObjectsScan();
			}
			else if(Undo.GetCurrentGroupName() == "Selection Change"){
				if(!UnityEditor.Selection.Contains(this.gameObject)){
					if(UnityEditor.Selection.gameObjects.Length > 0){
						ClayObject clayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
						if(clayObj != null){
							if(clayObj.getClayContainer() == this){
								this.needsUpdate = true;
							}
						}
					}
				}
			}

			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			ClayContainer.getSceneView().Repaint();
		}

		public static bool _skipHierarchyChanges = false;
		void onHierarchyChanged(){
			if(this.invalidated){
				return;
			}
			
			if(this.frozen){
				return;
			}

			if(!this.enabled){
				return;
			}

			if(this.instanceOf != null){
				return;
			}

			if(ClayContainer._skipHierarchyChanges){
				ClayContainer._skipHierarchyChanges = false;
				return;
			}

			// the order of operations here is important to successfully move clayOjects from one container to another
			if(this.editingThisContainer){
				this.solidsHierarchyNeedsScan = true;
			}

			this.onSelectionChanged();
			
			if(this.editingThisContainer){
				this.forceUpdateAllSolids();
				this.computeClay();
			}
		}

		public static void _inspectorUpdate(){
			ClayContainer.inspectorUpdated = UnityEngine.Object.FindObjectsOfType<ClayContainer>().Length;
		}

		static ClayContainer selectionReoderContainer = null;
		static int selectionReorderId = -1;
		static int selectionReorderIdOffset = 0;

		public void selectToReorder(ClayObject clayObjToReorder, int reorderOffset){
			ClayContainer.selectionReoderContainer = this;
			ClayContainer.selectionReorderId = clayObjToReorder.clayObjectId;
			ClayContainer.selectionReorderIdOffset = reorderOffset;
		}

		void reorderSelected(){
			if(ClayContainer.selectionReoderContainer != this){
				ClayContainer.selectionReoderContainer = null;
				return;
			}

			if(UnityEditor.Selection.gameObjects.Length == 0){
				return;
			}

			ClayObject selectedClayObj = UnityEditor.Selection.gameObjects[0].GetComponent<ClayObject>();
			if(selectedClayObj == null){
				return;
			}

			if(selectedClayObj.getClayContainer() != ClayContainer.selectionReoderContainer){
				return;
			}

			ClayObject reoderedClayObj = this.clayObjects[ClayContainer.selectionReorderId];

			int idOffset = selectedClayObj.clayObjectId - ClayContainer.selectionReorderId; 
			this.reorderClayObject(ClayContainer.selectionReorderId, idOffset + ClayContainer.selectionReorderIdOffset);

			ClayContainer.pickedObj = reoderedClayObj.gameObject;
			ClayContainer.pickingMode = true;

			ClayContainer.selectionReoderContainer = null;
		}

		void onSelectionChanged(){
			// for some reason this callback is also triggered by the inspector
			// so we first have to check if this is really a selection change or an inspector update. wtf. 
			if(ClayContainer.inspectorUpdated > 0){
				ClayContainer.inspectorUpdated -= 1;
				return;
			}

			if(this.invalidated){
				return;
			}

			if(this.needsInit){
				return;
			}

			if(this.frozen){
				return;
			}

			if(this.instanceOf != null){
				return;
			}

			if(ClayContainer.selectionReoderContainer != null){
				this.reorderSelected();
			}

			if(!this.enabled){
				return;
			}
			
			bool wasEditingThis = this.editingThisContainer;
			this.editingThisContainer = false;
			if(UnityEditor.Selection.Contains(this.gameObject)){
				// check if this container got selected
				this.editingThisContainer = true;
				this.needsUpdate = true;
			}

			if(!this.editingThisContainer){
				// check if one of this container's clayObject got selected
				for(int i = 0; i < UnityEditor.Selection.gameObjects.Length; ++i){
					GameObject sel = UnityEditor.Selection.gameObjects[i];
					ClayObject clayObj = sel.GetComponent<ClayObject>();
					if(clayObj != null){
						if(clayObj.getClayContainer() == this){
							this.editingThisContainer = true;
							this.needsUpdate = true;
							this.updateFrame = 0;
							return;
						}
					}
				}

				if(wasEditingThis){// if we're changing selection, optimize the buffers of this container
					if(this.renderMode == ClayContainer.RenderModes.polySplat){
						this.optimizeMemory();
						this.drawClayxels(this);
						
						UnityEditor.EditorApplication.QueuePlayerLoopUpdate();// fix instances disappearing
					}
					else{
						this.microvoxelsEditorDelayedOptimize = true;
					}
				}
			}
		}

		static void finalizeSculptAction(){
			for(int i = 0; i < UnityEditor.Selection.gameObjects.Length; ++i){
				GameObject sel = UnityEditor.Selection.gameObjects[i];
				ClayObject clayObj = sel.GetComponent<ClayObject>();
				if(clayObj != null){
					ClayContainer container = clayObj.getClayContainer();
					if(container != null){
						if(container.needsUpdate){
							container.computeClay();
						}
					}
				}
			}
		}

		static void onSceneGUI(SceneView sceneView){
			if(Application.isPlaying){
				return;
			}

			if(!UnityEditorInternal.InternalEditorUtility.isApplicationActive){
				// this callback keeps running even in the background
				return;
			}

			if(ClayContainer.containersToRender.Count == 0 || ClayContainer.globalDataNeedsInit){
				return;
			}

			Event ev = Event.current;

			if(ev.type == EventType.MouseUp){
				ClayContainer.finalizeSculptAction();
			}
				
			if(ev.isKey){
				ClayContainer.clearPicking();

				if(ev.keyCode.ToString().ToLower() == ClayContainer.pickingKey){
					ClayContainer.startScenePicking();
				}
				else if(ev.keyCode.ToString().ToLower() == ClayContainer.mirrorDuplicateKey){
					ClayContainer.shortcutMirrorDuplicate();
				}
				// else if(ev.keyCode.ToString().ToLower() == "f"){
				// 	int unityMajorVersion = int.Parse(Application.unityVersion.Split('.')[0]);
				// 	if(unityMajorVersion < 2020){// unity 2020 fixes the need for this custom framing
				// 		ClayContainer.frameSelected(sceneView);
				// 	}
				// }

				return;
			}

			float uiScale = ClayContainer.getEditorUIScale();
			int pickingMousePosX = (int)(ev.mousePosition.x * uiScale);
			int pickingMousePosY = (int)(ev.mousePosition.y * uiScale);

			if(pickingMousePosX < 0 || pickingMousePosX >= sceneView.camera.pixelWidth || 
				pickingMousePosY < 0 || pickingMousePosY >= sceneView.camera.pixelHeight){

				return;
			}
			
			if(ClayContainer.directPick){
				if(ev.type == EventType.Used && !ev.alt){
					ClayContainer.mouseClickMicrovoxel(pickingMousePosX, pickingMousePosY);
				}
				
				if(ev.type == EventType.MouseMove && !ev.alt){
					ClayContainer.mouseMoveMicrovoxel(pickingMousePosX, pickingMousePosY);
				}
			}

			if(!ClayContainer.pickingMode){
				return;
			}

			if(ClayContainer.pickedObj != null){
				if(ClayContainer.pickingShiftPressed){
					List<UnityEngine.Object> sel = new List<UnityEngine.Object>();
		   			for(int i = 0; i < UnityEditor.Selection.objects.Length; ++i){
		   				sel.Add(UnityEditor.Selection.objects[i]);
		   			}
		   			sel.Add(ClayContainer.pickedObj);
		   			UnityEditor.Selection.objects = sel.ToArray();
	   			}
	   			else{
					UnityEditor.Selection.objects = new GameObject[]{ClayContainer.pickedObj};
				}
			}
			
			if(ev.type == EventType.MouseMove){
				if(ClayContainer.pickedObj != null){
					ClayContainer.clearPicking();
				}
			}
			else if(ev.type == EventType.MouseDown && !ev.alt){
				if(pickingMousePosX < 0 || pickingMousePosX >= sceneView.camera.pixelWidth || 
					pickingMousePosY < 0 || pickingMousePosY >= sceneView.camera.pixelHeight){
					clearPicking();
					return;
				}

				ev.Use();

				ClayContainer.finalizePicking(sceneView);
			}
			else if((int)ev.type == 7){ // on repaint
				ClayContainer.performScenePicking(sceneView.camera, pickingMousePosX, pickingMousePosY);
			}

			sceneView.Repaint();
		}	

		static void setupScenePicking(){
			SceneView.duringSceneGui -= ClayContainer.onSceneGUI;
			SceneView.duringSceneGui += ClayContainer.onSceneGUI;

			ClayContainer.setupPicking();
		}

		public static void startScenePicking(){
			ClayContainer[] containers = GameObject.FindObjectsOfType<ClayContainer>();

			for(int i = 0; i < containers.Length; ++i){
				ClayContainer container = containers[i];
				container.pickingThis = false;
			}

			ClayContainer.pickingMode = true;
			ClayContainer.pickedObj = null;

			ClayContainer.pickedClayObjectId = -1;
	  		ClayContainer.pickedContainerId = -1;
			ClayContainer.lastPickedContainerId = -1;

			for(int i = 0; i < SceneView.sceneViews.Count; ++i){
				((SceneView)(SceneView.sceneViews[i])).Repaint();
			}

			if(!ClayContainer.directPickEnabled){
				ClayContainer.directPick = true;
			}
		}

		static void performScenePicking(Camera camera, float mousePosX, float mousePosY){
			if(mousePosX < 0 || mousePosX >= camera.pixelWidth || 
				mousePosY < 0 || mousePosY >= camera.pixelHeight){

				pickedContainerId = -1;
				pickedClayObjectId = -1;
				return;
			}

			ClayContainer[] containers = GameObject.FindObjectsOfType<ClayContainer>();

			if(ClayContainer.lastPickedContainerId > -1 && ClayContainer.pickedContainerId != ClayContainer.lastPickedContainerId && ClayContainer.lastPickedContainerId < containers.Length){
				ClayContainer lastContainer = containers[ClayContainer.lastPickedContainerId];
				lastContainer.pickingThis = false;
				ClayContainer.lastPickedContainerId = -1;
			}
				
			if(ClayContainer.pickedContainerId > -1 && ClayContainer.pickedContainerId < containers.Length){
				ClayContainer container = containers[ClayContainer.pickedContainerId];
				ClayContainer.lastPickedContainerId = ClayContainer.pickedContainerId;
				
				if(container.renderMode == ClayContainer.RenderModes.polySplat){
					if(container.editingThisContainer && !container.pickingThis && container.instanceOf == null){
						ClayContainer.claycoreCompute.SetInt("storeSolidId", 1);
						container.forceUpdateAllSolids();
			  			container.computeClay();
			  			ClayContainer.claycoreCompute.SetInt("storeSolidId", 0);
			  		}
			  	}
				
				container.pickingThis = true;
			}
			
			ClayContainer.pickedClayObjectId = -1;
	  		ClayContainer.pickedContainerId = -1;

			ClayContainer.pickingCommandBuffer.Clear();
			ClayContainer.pickingCommandBuffer.SetRenderTarget(ClayContainer.pickingRenderTextureId);
			ClayContainer.pickingCommandBuffer.ClearRenderTarget(true, true, Color.black, 1.0f);

			for(int i = 0; i < containers.Length; ++i){
				ClayContainer container = containers[i];
				if(container.enabled){
					container.drawClayxelPicking(i, ClayContainer.pickingCommandBuffer, container.pickingThis);
				}
			}
			
			Graphics.ExecuteCommandBuffer(ClayContainer.pickingCommandBuffer);
			
			int rectWidth = (int)(1024.0f * ((float)mousePosX / (float)camera.pixelWidth));
			int rectHeight = (int)(768.0f * ((float)mousePosY / (float)camera.pixelHeight));
			
			#if UNITY_EDITOR_OSX
				rectHeight = 768 - rectHeight;
			#endif

			ClayContainer.pickingRect.Set(
				rectWidth, 
				rectHeight, 
				1, 1);

			RenderTexture oldRT = RenderTexture.active;
			RenderTexture.active = ClayContainer.pickingRenderTexture;
			ClayContainer.pickingTextureResult.ReadPixels(ClayContainer.pickingRect, 0, 0);
			ClayContainer.pickingTextureResult.Apply();
			RenderTexture.active = oldRT;
			
			Color pickCol = ClayContainer.pickingTextureResult.GetPixel(0, 0);
			int pickId = (int)((pickCol.r + pickCol.g * 255.0f + pickCol.b * 255.0f) * 255.0f);
	  		ClayContainer.pickedClayObjectId = pickId - 1;
	  		ClayContainer.pickedContainerId = (int)(pickCol.a * 256.0f);
	  		
	  		if(ClayContainer.pickedContainerId >= 255){
	  			ClayContainer.pickedContainerId = -1;
	  		}
		}

		static void clearPicking(){
			ClayContainer[] containers = GameObject.FindObjectsOfType<ClayContainer>();

			for(int i = 0; i < containers.Length; ++i){
				ClayContainer container = containers[i];
				container.pickingThis = false;
			}

			bool continuePicking = false;
			if(ClayContainer.pickedContainerId > -1 && ClayContainer.pickedContainerId < containers.Length){
				if(containers[ClayContainer.pickedContainerId].instanceOf == null){
					if(ClayContainer.pickedObj != null){
						if(ClayContainer.pickedObj.GetComponent<ClayContainer>() != null){
							ClayContainer.pickedObj = null;
							ClayContainer.pickingMode = true;

							continuePicking = true;
						}
					}
				}
			}

			if(!continuePicking){
				ClayContainer.pickingMode = false;
				ClayContainer.pickedObj = null;
				ClayContainer.pickedContainerId = -1;
				ClayContainer.pickedClayObjectId = -1;
				ClayContainer.lastPickedContainerId = -1;
			}

			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
		}

		static void finalizePicking(SceneView sceneView){
	  		if(ClayContainer.pickedContainerId > -1){
	  			ClayContainer container = ClayContainer.containersInScene[ClayContainer.pickedContainerId];

	  			GameObject newSel = null;
	  			if(container.instanceOf == null && ClayContainer.pickedClayObjectId > -1 && container.editingThisContainer){
		  			newSel = container.getClayObject(ClayContainer.pickedClayObjectId).gameObject;
		  		}
		  		else{
		  			newSel = container.gameObject;
		  		}

	  			UnityEditor.Selection.objects = new GameObject[]{newSel};
	  			ClayContainer.pickedObj = newSel;
	  			ClayContainer.pickingShiftPressed = Event.current.shift;
	  			
	  			return;
	  		}
			
			ClayContainer.clearPicking();
		}

		void OnDrawGizmos(){
			if(Application.isPlaying){
				return;
			}

			if(!this.editingThisContainer){
				return;
			}

			// debug auto bounds
			// if(this.autoBounds){
			// 	Gizmos.color = Color.red;
			// 	Gizmos.matrix = this.transform.localToWorldMatrix;
			// 	Gizmos.DrawWireCube(Vector3.zero, new Vector3(this._debugAutoBounds, this._debugAutoBounds, this._debugAutoBounds));
			// }

			Gizmos.color = ClayContainer.boundsColor;
			Gizmos.matrix = this.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(this.boundsCenter, this.boundsScale);
		}

		static public void reloadAll(){
			ClayContainer.globalDataNeedsInit = true;
			ClayContainer.initGlobalData();

			ClayContainer[] containers = UnityEngine.Object.FindObjectsOfType<ClayContainer>();
			for(int i = 0; i < containers.Length; ++i){
				ClayContainer container = containers[i];
				container.needsInit = true;
				container.init();

				container.needsUpdate = true;
			}

			UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
			((SceneView)SceneView.sceneViews[0]).Repaint();

			if(ClayContainer.globalUserWarning != ""){
				Debug.Log("Clayxels Warning: " + ClayContainer.globalUserWarning);
			}
		}

		public static SceneView getSceneView(){
			SceneView sceneView = SceneView.currentDrawingSceneView;
			if(sceneView != null){
				return sceneView;
			}

			sceneView = SceneView.lastActiveSceneView;
			if(sceneView != null){
				return sceneView;
			}

			return (SceneView)SceneView.sceneViews[0];
		}
		
		public int retopoMaxVerts = -1;
		public bool retopoApplied = false;
		
		public void storeMesh(string assetName){
			if(this.gameObject.GetComponent<MeshFilter>().sharedMesh == null){
				return;
			}

			string assetNameUnique = this.storeAssetPath + "_" + this.GetInstanceID();

			if(!AssetDatabase.Contains(this.gameObject.GetComponent<MeshRenderer>().sharedMaterial)){
				if(this.gameObject.GetComponent<MeshRenderer>() != null){
					if(this.gameObject.GetComponent<MeshRenderer>().sharedMaterial != null){
						AssetDatabase.CreateAsset(this.gameObject.GetComponent<MeshRenderer>().sharedMaterial, "Assets/" + ClayContainer.defaultAssetsPath + "/" + assetNameUnique + ".mat");
					}
				}
			}

			AssetDatabase.CreateAsset(this.gameObject.GetComponent<MeshFilter>().sharedMesh, "Assets/" + ClayContainer.defaultAssetsPath + "/" + assetNameUnique + ".mesh");
			AssetDatabase.SaveAssets();
		}

		public AnimationClip claymationAnimClip = null;
		public int claymationStartFrame = 0;
		public int claymationEndFrame = 0;

		public delegate void AnimUpdateCallback(int frame);

		/* Freeze this container to a claymation, a compact data format that retains the same shader as live clayxels. */
		public void freezeClaymation(){
			if(this.needsInit){
				this.init();
			}

			bool success = true;
			string assetNameUnique = ClayContainer.defaultAssetsPath + "/" + this.storeAssetPath + "_" + this.GetInstanceID();
			if(this.claymationAnimClip == null){
				success = this.bakeClaymationFile(assetNameUnique, 0, 0);
			}
			else{
				success = this.bakeClaymationFile(assetNameUnique, this.claymationStartFrame, this.claymationEndFrame, this.animClipCallback);
			}

			if(!success){
				return;
			}
			
			Claymation claymation = this.gameObject.GetComponent<Claymation>();
			if(claymation == null){
				claymation = this.gameObject.AddComponent<Claymation>();
			}

			string materialUniqueName = assetNameUnique + "_claymation";

			Material storedMat = new Material(this.material);
			AssetDatabase.CreateAsset(storedMat, "Assets/" + materialUniqueName + ".mat");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			claymation.claymationFile = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/" + assetNameUnique + ".clay.bytes", typeof(TextAsset));

			claymation.material = storedMat;

			if(this.claymationAnimClip != null){
				claymation.frameRate = (int)this.claymationAnimClip.frameRate;
			}

			claymation.enabled = true;
			claymation.init();

			this.frozen = true;
			this.enabled = false;

			this.releaseBuffers();

			this.enableAllClayObjects(false);
		}

		public void freezeContainersHierarchyToClaymation(){
			ClayContainer[] containers = this.GetComponentsInChildren<ClayContainer>();
			for(int i = 0; i < containers.Length; ++i){
				containers[i].freezeClaymation();
			}
		}

		/* Use this method when you want to provide a custom animCallback in order to bake procedural motion coming from your code.
		To simply bake models and animationClips use ClayContainer.freezeClaymation().*/
		public bool bakeClaymationFile(string assetName, int startFrame = 0, int endFrame = 0, AnimUpdateCallback animCallback = null){
			int numFrames = (endFrame - startFrame) + 1;

			// claymation file format starts here
			BinaryWriter writer = new BinaryWriter(File.Open(Application.dataPath + "/" + assetName + ".clay.bytes", FileMode.Create));

			int fileFormat = 0;
			writer.Write(fileFormat);
			writer.Write(this.chunkSize);
			writer.Write(this.chunksX);
			writer.Write(this.chunksY);
			writer.Write(this.chunksZ);
			writer.Write(numFrames);

			if(ClayContainer.lastUpdatedContainerId != this.GetInstanceID()){
				this.switchComputeData();
			}

			this.userWarning = "";

			int[] pointToChunkData = new int[(ClayContainer.maxPointCount / 5) * this.numChunks];

			int reducedMip3BufferSize = (int)(((float)(256 * 256 * 256) * this.numChunks) * ClayContainer.bufferSizeReduceFactor);
			int[] tmpPointCloudDataStorage = new int[reducedMip3BufferSize * 2];
			
			for(int frameIt = startFrame; frameIt < (endFrame + 1); ++frameIt){
				if(animCallback != null){
					animCallback(frameIt);
				}
				
				this.forceUpdateAllSolids();
				this.updateSolids();
				this.computeClay();
				this.optimizeMemory();

				int numPoints = 0;
				bool success = this.bakeClaymationFrame(ref numPoints, ref tmpPointCloudDataStorage, ref pointToChunkData);
				if(!success){
					writer.Close();

					return false;
				}

				writer.Write(numPoints);
					
				for(int pointIt = 0; pointIt < numPoints; ++pointIt){
					int dataIt = pointIt * 2;
					writer.Write(tmpPointCloudDataStorage[dataIt]);
					writer.Write(tmpPointCloudDataStorage[dataIt + 1]);
				}

				for(int pointIt = 0; pointIt < numPoints / 5; ++pointIt){
					writer.Write(pointToChunkData[pointIt]);
				}
			}
			
			writer.Close();
			
			AssetDatabase.Refresh();

			return true;
		}

		void animClipCallback(int frame){
			if(this.claymationAnimClip != null){
				this.claymationAnimClip.SampleAnimation(this.gameObject, (float)frame / this.claymationAnimClip.frameRate);
			}
		}

		bool bakeClaymationFrame(ref int numPoints, ref int[] pointsData, ref int[] pointToChunkData){
			this.indirectDrawArgsBuffer.GetData(ClayContainer.indirectArgsData);

			numPoints = ClayContainer.indirectArgsData[0] / 3;

			if(numPoints > ClayContainer.maxPointCount){
				this.userWarning = "max point count exceeded, increase limit from Global Config window";
				Debug.Log("Clayxels: container " + this.gameObject.name + " has exceeded the limit of points allowed, increase limit from Global Config window");
				
				return false;
			}
			
			this.pointCloudDataMip3Buffer.GetData(pointsData, 0, 0, numPoints * 2);
			this.pointToChunkIdBuffer.GetData(pointToChunkData, 0, 0, numPoints / 5);

			return true;
		}

		static void frameSelected(SceneView sceneView){
			int numClayxelsObjs = 0;
			Bounds bounds = new Bounds();

			for(int i = 0; i < UnityEditor.Selection.gameObjects.Length; ++i){
				GameObject selObj = UnityEditor.Selection.gameObjects[i];

				ClayContainer container = selObj.GetComponent<ClayContainer>();
				if(container != null){
					// bounds.Encapsulate(container.renderBounds);
					// numClayxelsObjs += 1;
				}
				else{
					ClayObject clayObj = selObj.GetComponent<ClayObject>();
					if(clayObj != null){
						bounds.Encapsulate(new Bounds(clayObj.transform.position, clayObj.transform.lossyScale));
						numClayxelsObjs += 1;
					}
				}
			}

			if(numClayxelsObjs > 0){
				sceneView.Frame(bounds, false);
			}
		}

		static void mouseClickMicrovoxel(int mouseX, int mouseY){
			if(ClayContainer.globalDataNeedsInit){
				return;
			}

			if(ClayContainer.containersInScene.Count == 0){
				return;
			}

			int pickedContainerId = -1;
			int pickedClayObjectId = -1;

			bool invertVerticalMouseCoords = false;

			#if UNITY_EDITOR_OSX
				invertVerticalMouseCoords = true;
			#endif

			ClayContainer.pickingMicrovoxel(Camera.current, mouseX, mouseY, out pickedContainerId, out pickedClayObjectId, invertVerticalMouseCoords);

			GameObject pickedObj = null;
			bool pickComplete = false;
			if(pickedContainerId > -1 && pickedContainerId < ClayContainer.containersInScene.Count){
				ClayContainer container = ClayContainer.containersInScene[pickedContainerId];

				if(container.instanceOf == null && container.editingThisContainer && pickedClayObjectId > -1){
					pickedObj = container.getClayObject(pickedClayObjectId).gameObject;
					pickComplete = true;
				}
				else{
					pickedObj = container.gameObject;
				}
			}
			
			if(pickedObj != null){
				if(Event.current.shift){
					List<UnityEngine.Object> sel = new List<UnityEngine.Object>();
		   			for(int i = 0; i < UnityEditor.Selection.objects.Length; ++i){
		   				sel.Add(UnityEditor.Selection.objects[i]);
		   			}
		   			sel.Add(pickedObj);
		   			UnityEditor.Selection.objects = sel.ToArray();
	   			}
	   			else{
					UnityEditor.Selection.objects = new GameObject[]{pickedObj};
				}
			}

			if(!ClayContainer.directPickEnabled){
				if(ClayContainer.pickedContainerIdMV == -1 || pickComplete){
					ClayContainer.directPick = false;
				}

				ClayContainer.pickedContainerIdMV = -1;
				ClayContainer.pickedClayObjectIdMV = -1;
			}
		}

		static void mouseMoveMicrovoxel(int mouseX, int mouseY){
			if(ClayContainer.globalDataNeedsInit){
				return;
			}

			if(ClayContainer.containersToRender.Count == 0){
				return;
			}

			int pickedContainerId = -1;
			int pickedClayObjectId = -1;

			bool invertVerticalMouseCoords = false;

			#if UNITY_EDITOR_OSX
				invertVerticalMouseCoords = true;
			#endif

			ClayContainer.pickingMicrovoxel(Camera.current, mouseX, mouseY, out pickedContainerId, out pickedClayObjectId, invertVerticalMouseCoords);
			
			ClayContainer container = null;
			if(pickedContainerId > -1 && pickedContainerId < ClayContainer.containersInScene.Count){
				container = ClayContainer.containersInScene[pickedContainerId];
			}
			
			ClayContainer.pickedContainerIdMV = -1;
			ClayContainer.pickedClayObjectIdMV = -1;

			if(container != null){
				ClayContainer.pickedContainerIdMV = pickedContainerId;

				if(!container.editingThisContainer || container.instanceOf != null){
					ClayContainer.pickedClayObjectIdMV = -2;
				}
				else{
					if(pickedClayObjectId > -1 && pickedClayObjectId < container.getNumClayObjects()){
						ClayObject clay = container.getClayObject(pickedClayObjectId);
						if(clay != null){
							GameObject clayObj = clay.gameObject;
							if(!UnityEditor.Selection.Contains(clayObj)){
								ClayContainer.pickedClayObjectIdMV = pickedClayObjectId;
							}
						}
					}
				}
			}
		}

		static public void _displayGlobalUserWarning(string msg){
			ClayContainer.globalUserWarning = "";

			if(msg != ""){
				ClayContainer.globalUserWarning = msg + "\n";

				Debug.Log("Clayxels Warning: " + ClayContainer.globalUserWarning);
			}
		}

		#endif// end if UNITY_EDITOR
	}
}
