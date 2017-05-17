﻿using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class UVInspectorWindow : EditorWindow
{
    [MenuItem("Window/UV Inspector")]
    public static void ShowWindow()
    {
        GetWindow<UVInspectorWindow>("UV Inspector", true, GetInspectorWindowType());
    }

    static Type GetInspectorWindowType()
    {
		return typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
    }

    PreviewRenderUtility m_PreviewRenderUtility;
    Material m_UvMaterial;
    GameObject m_SelectedObject;
    bool m_MultipleObjectsSelected;
    int m_TextureWidth;
    int m_TextureHeight;

    void Update()
    {
        var selectedObject = Selection.gameObjects.Length == 1 ? Selection.gameObjects[0] : null;
        var multipleObjectsSelected = Selection.gameObjects.Length > 1;

        if (m_SelectedObject != selectedObject || m_MultipleObjectsSelected != multipleObjectsSelected)
        {
            m_SelectedObject = selectedObject;
            m_MultipleObjectsSelected = multipleObjectsSelected;
            Repaint();
        }
    }

	readonly string k_TextureWidthKey = typeof(UVInspectorWindow).FullName + "/" + nameof(m_TextureWidth);
    readonly string k_TextureHeightKey = typeof(UVInspectorWindow).FullName + "/" + nameof(m_TextureHeight);

    void OnEnable()
    {
        m_PreviewRenderUtility = new PreviewRenderUtility();

        m_UvMaterial = new Material(Shader.Find("Unlit/UVShader")) { hideFlags = HideFlags.HideAndDontSave };

        m_TextureWidth = EditorPrefs.HasKey(k_TextureWidthKey) ? EditorPrefs.GetInt(k_TextureWidthKey) : 256;
        m_TextureHeight = EditorPrefs.HasKey(k_TextureHeightKey) ? EditorPrefs.GetInt(k_TextureHeightKey) : 256;
    }

	void OnDisable()
	{
        m_PreviewRenderUtility.Cleanup();
        m_PreviewRenderUtility = null;

        DestroyImmediate(m_UvMaterial);

        EditorPrefs.SetInt(k_TextureWidthKey, m_TextureWidth);
        EditorPrefs.SetInt(k_TextureHeightKey, m_TextureHeight);
	}

    void OnGUI()
	{
        EditorGUILayout.Space();
        m_TextureWidth = EditorGUILayout.IntField("Width", m_TextureWidth);
		m_TextureHeight = EditorGUILayout.IntField("Height", m_TextureHeight);

        if (m_MultipleObjectsSelected)
        {
            GUILayout.Label("Only a single object can be inspected.");
        }

        if (m_SelectedObject == null)
        {
            return;
        }

        var renderer = m_SelectedObject.GetComponent<Renderer>();
        var mesh = renderer != null ? m_SelectedObject.GetComponent<MeshFilter>()?.sharedMesh : null;
        if (mesh == null)
        {
            GUILayout.Label("Selected object has no mesh.");
            return;
        }

        var saveTexture = GUILayout.Button("Save texture");

        var bounds = m_SelectedObject.CalculateBounds();
		m_UvMaterial.SetVector("_Center", bounds.center);
		m_UvMaterial.SetVector("_Extents", bounds.extents);

        var renderRect = new Rect(Vector2.zero, new Vector2(m_TextureWidth, m_TextureHeight));
        m_PreviewRenderUtility.BeginPreview(renderRect, GUIStyle.none);
        m_PreviewRenderUtility.DrawMesh(mesh, Matrix4x4.identity, m_UvMaterial, 0);
		m_PreviewRenderUtility.camera.transform.position = Vector3.forward * -10f;
        m_PreviewRenderUtility.camera.Render();
        var renderTexture = m_PreviewRenderUtility.EndPreview() as RenderTexture;

        var previewRect = GUILayoutUtility.GetRect(Mathf.Min(m_TextureWidth, position.width), m_TextureHeight);
        EditorGUI.DrawPreviewTexture(previewRect, renderTexture, null, ScaleMode.ScaleToFit);

        if (!saveTexture)
            return;
        
        var path = EditorUtility.SaveFilePanel("Save texture as PNG", "", m_SelectedObject.name + ".png", "png");
        if (path.Length != 0)
        {
            RenderTexture.active = renderTexture;
            var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            var pngData = texture.EncodeToPNG();
            if (pngData != null)
                File.WriteAllBytes(path, pngData);
        }
    }
}