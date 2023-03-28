using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

public class FindTextureFormat : MonoBehaviour
{
    public List<Shader> shadersInScene;
    public List<Texture2D> texturesInScene;
    public List<Material> materialsInScene;
    public List<RenderTexture> rTexInScene;
    
    private void Start()
    {
        shadersInScene = GetAllObjectsOnlyInScene();
        texturesInScene = GetAllTexturesOnlyInScene();
        materialsInScene = GetAllMaterialsOnlyInScene();
        rTexInScene = GetAllRenderTextures();
        
        // Material material = new Material(Shader.Find("Hidden/InternalErrorShader"));
        // Debug.Log("error material: " + material.parent.name);
    }

    List<Shader> GetAllObjectsOnlyInScene()
    {
        Debug.Log("Checking all shaders");
        List<Shader> objectsInScene = new List<Shader>();
        
        foreach (var go in FindObjectsOfTypeAll(typeof(Shader)) as Shader[])
        {
            if (!go.isSupported) // format == TextureFormat.RGBAFloat
            {
                Debug.Log("SHADER NOT SUPPORTED: " + go.name);
                objectsInScene.Add(go);
            }
        }

        return objectsInScene;
    }
    
    List<Texture2D> GetAllTexturesOnlyInScene()
    {
        Debug.Log("Checking all TextureFormats");
        List<Texture2D> texturesInScene = new List<Texture2D>();
        
        foreach (var go in FindObjectsOfTypeAll(typeof(Texture2D)) as Texture2D[])
        {
            var b = SystemInfo.IsFormatSupported(GraphicsFormat.R32G32B32A32_SFloat, FormatUsage.Render);
            
            if (go.format == TextureFormat.RGBAFloat) //  format == TextureFormat.RGBAFloat
            {
                Debug.Log("TextureFormat NOT SUPPORTED: " + go.name + ", supported? " + b);
                texturesInScene.Add(go);
            }
        }

        return texturesInScene;
    }
    
    List<Material> GetAllMaterialsOnlyInScene()
    {
        Debug.Log("Checking all materials");
        List<Material> matsList = new List<Material>();
        
        foreach (var go in FindObjectsOfTypeAll(typeof(Material)) as Material[])
        {
            if (go.shader.name.Contains("InternalError"))
            {
                // Debug.Log("material shader NOT SUPPORTED: " + go.name + ", go.mainTexture.name: " + go.mainTexture.name);
                matsList.Add(go);
            }
        }

        return materialsInScene;
    }
    
    List<RenderTexture> GetAllRenderTextures()
    {
        Debug.Log("Checking all render textures");
        List<RenderTexture> list = new List<RenderTexture>();
        
        foreach (var obj in FindObjectsOfTypeAll(typeof(RenderTexture)) as RenderTexture[])
        {
            Debug.Log("render texture: " + obj.name);
            if (obj.graphicsFormat == GraphicsFormat.R32G32B32A32_SFloat ||
                obj.format == RenderTextureFormat.ARGBFloat)
            {
                list.Add(obj);
            }
        }
        
        foreach (var obj in Resources.FindObjectsOfTypeAll(typeof(RenderTexture)) as RenderTexture[])
        {
            Debug.Log("resource render texture: " + obj.name);
            if (obj.graphicsFormat == GraphicsFormat.R32G32B32A32_SFloat ||
                obj.format == RenderTextureFormat.ARGBFloat)
            {
                Debug.Log("BAD render texture: " + obj.name);
                list.Add(obj);
            }
        }

        return rTexInScene;
    }
}
