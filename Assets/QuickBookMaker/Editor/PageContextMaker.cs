using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

public class PageContextMaker : EditorWindow
{
    private SerializedProperty serializedProperty;
    private SerializedObject serializedObject;
    private bool useShaderToCull;

    public List<PageContextData> pageContextLists = new List<PageContextData>();

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        serializedProperty = serializedObject.FindProperty("pageContextLists");
    }

    
    [MenuItem("Window/OpenMyWindow/PageContextMaker",false,1)]
    static void WindowShow()
    {
        EditorWindow.GetWindow<PageContextMaker>("Page Context Maker",false);
    }

    private void OnGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedProperty, new GUIContent("PageContext"), true);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(10f);

        useShaderToCull = EditorGUILayout.Toggle("UseShaderToCull", useShaderToCull);
   
        EditorGUILayout.Space(30f);

        if(GUILayout.Button("Create Page Context"))
        {
            if (Selection.gameObjects.Length == 0)
                throw new Exception("Haven't select any GameObjects");
            else
            {
                foreach(var item in Selection.gameObjects)
                {
                    if (item.tag == "PageFront" || item.tag == "PageBack")
                    {
                        switch (useShaderToCull)
                        {
                            case true:
                                if (AssetDatabase.LoadAssetAtPath("Assets/QuickBookMaker/Shader/PageShowFront.mat", typeof(Material)) == null)
                                {
                                    Material matBack = new Material(Shader.Find("Unlit/UnlitCardShow"));
                                    matBack.SetFloat("_Cull", 2);
                                    AssetDatabase.CreateAsset(matBack, "Assets/QuickBookMaker/Shader/PageShowFront.mat");
                                }
                                if (AssetDatabase.LoadAssetAtPath("Assets/QuickBookMaker/Shader/PageShowBack.mat", typeof(Material)) == null)
                                {
                                    Material matBack = new Material(Shader.Find("Unlit/UnlitCardShow"));
                                    matBack.SetFloat("_Cull", 1);
                                    AssetDatabase.CreateAsset(matBack, "Assets/QuickBookMaker/Shader/PageShowBack.mat");
                                }
                                foreach (var context in Selection.gameObjects)
                                {
                                    for (int i = 0; i < pageContextLists.Count; i++)
                                    {
                                                                         
                                        switch (pageContextLists[i].side)
                                        {

                                            case pageFrontOrBackContext.Back:                        
                                                    foreach(var temp in Selection.gameObjects)
                                                    {
                                                        if (temp.tag == "PageBack")
                                                        {
                                                            GameObject pages = new GameObject("PageContext" + i, typeof(Image));
                                                            pages.transform.SetParent(item.transform);
                                                        if (AssetDatabase.LoadAssetAtPath("Assets/QuickBookMaker/Shader/PageShowBack.mat", typeof(Material)) == null)
                                                            throw new Exception("Material : PageShowBack doesn't exist in the path : Assets/QuickBookMaker/Shader ");
                                                        Material matBack = (Material)AssetDatabase.LoadAssetAtPath("Assets/QuickBookMaker/Shader/PageShowBack.mat", typeof(Material));
                                                        pages.GetComponent<Image>().material = matBack;
                                                        pages.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                                                        pages.GetComponent<Image>().sprite = pageContextLists[i].pageContext;
                                                        pages.GetComponent<Image>().SetNativeSize();
                                                        pages.GetComponent<Image>().raycastTarget = false;
                                                    }                                                         
                                                    }                                               
                                                break;

                                            case pageFrontOrBackContext.Front:
                                                foreach (var temp in Selection.gameObjects)
                                                {
                                                    if (temp.tag == "PageFront")
                                                    {
                                                        GameObject pages = new GameObject("PageContext" + i, typeof(Image));
                                                        pages.transform.SetParent(item.transform);
                                                        if (AssetDatabase.LoadAssetAtPath("Assets/QuickBookMaker/Shader/PageShowFront.mat", typeof(Material)) == null)
                                                            throw new Exception("Material : PageShowFront doesn't exist in the path : Assets/QuickBookMaker/Shader ");
                                                        Material matFront = (Material)AssetDatabase.LoadAssetAtPath("Assets/QuickBookMaker/Shader/PageShowFront.mat", typeof(Material));
                                                        pages.GetComponent<Image>().material = matFront;

                                                        pages.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                                                        pages.GetComponent<Image>().sprite = pageContextLists[i].pageContext;
                                                        pages.GetComponent<Image>().SetNativeSize();
                                                        pages.GetComponent<Image>().raycastTarget = false;
                                                    }

                                                }                                            
                                               break;
                                        }
                                      
                                    }
                                }
                                break;
                            case false:
                                foreach (var context in Selection.gameObjects)
                                {
                                    for (int i = 0; i < pageContextLists.Count; i++)
                                    {
                                                                   
                                        switch (pageContextLists[i].side)
                                        {

                                            case pageFrontOrBackContext.Back:
                                                    foreach (var temp in Selection.gameObjects)
                                                    {
                                                        if (temp.tag == "PageBack")
                                                        {
                                                            GameObject pages = new GameObject("PageContext" + i, typeof(Image));
                                                            pages.transform.SetParent(item.transform);
                                                            pages.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                                                            pages.GetComponent<Image>().sprite = pageContextLists[i].pageContext;
                                                            pages.GetComponent<Image>().SetNativeSize();
                                                        pages.GetComponent<Image>().raycastTarget = false;
                                                    }

                                                    }
                                                break;

                                            case pageFrontOrBackContext.Front:
                                                    foreach (var temp in Selection.gameObjects)
                                                    {
                                                        if (temp.tag == "PageFront")
                                                        {
                                                            GameObject pages = new GameObject("PageContext" + i, typeof(Image));
                                                            pages.transform.SetParent(item.transform);
                                                            pages.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                                                            pages.GetComponent<Image>().sprite = pageContextLists[i].pageContext;
                                                            pages.GetComponent<Image>().SetNativeSize();
                                                        pages.GetComponent<Image>().raycastTarget = false;
                                                    }
                                                    }
                                                break;
                                        }
                                      
                                    }
                                }
                                break;
                        }
                       
                    }         
                    
                    else
                    {
                        throw new Exception(@"Some selected Gameobjects' tags aren't ""PageFront"" or ""PageBack""");
                    }
                }
               
               
            }
         
        }

        EditorGUILayout.Space(5f);

        if(GUILayout.Button("Clear All FrontPage Contexts"))
        {
            if (EditorUtility.DisplayDialog("Confirm Your Choice", "Do you want to destroy all FrontPage Contexts which active in the hierarchy?", "Yes", "No"))
            {
                if (GameObject.FindGameObjectsWithTag("PageFront").Length== 0)
                {
                    Debug.LogWarning("There are no PageFront in the Hierarchy");
                    return;
                }
                else
                foreach (var item in GameObject.FindGameObjectsWithTag("PageFront"))
                {
                    for (int i = 0; i < item.transform.childCount; i++)
                        DestroyImmediate(item.transform.GetChild(i).gameObject);
                }
            }
        }

        EditorGUILayout.Space(5f);

        if (GUILayout.Button("Clear All BackPage Contexts"))
        {
            if (EditorUtility.DisplayDialog("Confirm Your Choice", "Do you want to destroy all BackPage Contexts which active in the hierarchy?", "Yes", "No"))
            {
                if (GameObject.FindGameObjectsWithTag("PageBack").Length == 0)
                {
                    Debug.LogWarning("There are no PageBack in the Hierarchy");
                    return;
                }
                else
                foreach (var item in GameObject.FindGameObjectsWithTag("PageBack"))
                {
                    for (int i = 0; i < item.transform.childCount; i++)
                        DestroyImmediate(item.transform.GetChild(i).gameObject);
                }
            }
        }

        EditorGUILayout.Space(5f);

        if (GUILayout.Button("Clear All Lists"))
        {
            if (pageContextLists.Count == 0)
                Debug.LogWarning("There is no context in the pages");
            else
            if (EditorUtility.DisplayDialog("Confirm Your Choice", "Do you want to clear your contexts?", "Yes", "No"))
                pageContextLists.Clear();                           
        }
    }
}




[System.Serializable]
public class PageContextData
{
   public Sprite pageContext;
    public pageFrontOrBackContext side;
}


public enum pageFrontOrBackContext
{
    Front,Back
}

