using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;

public class BookRoationMaker : EditorWindow
{
    private float BookWidth=1500f, BookHeight=800f;
    private float PageWidth = 750f, PageHeight = 800f;
    private int pageCount=5;
    private float flipSpeed=0.2f;
    private bool showBackGround,showSetSize;
    private bool hasBaseBookTag,hasPageTag,hasContextTag, hasPageFrontTag,hasPageBackTag;
    private bool setBookSize;
    private bool useBookTexture,usePageTexture,customSetPage;
    private Texture2D BookTexture, PageTexture,customPageTexture,backgroundTexture;
  
  

    [MenuItem("Window/OpenMyWindow/BaseBookQuickMaker", false, 0)]
   static void WindowShow()
    {
        EditorWindow.GetWindow<BookRoationMaker>("Base Book  QuickMaker", false);
    }
    private void OnGUI()
    {
        pageCount = EditorGUILayout.IntField("PageCount", pageCount);   
        flipSpeed = EditorGUILayout.Slider("FlipSpeed", flipSpeed,0,1f);

        showBackGround = EditorGUILayout.BeginToggleGroup("ShowBookBackGround", showBackGround);
        backgroundTexture = (Texture2D)EditorGUILayout.ObjectField("BackGroundTexture", backgroundTexture, typeof(Texture2D),true);
        EditorGUILayout.EndToggleGroup();

        GUILayout.Space(10f);

        customSetPage = EditorGUILayout.BeginToggleGroup("CustomPageSize", customSetPage);
        customPageTexture = (Texture2D)EditorGUILayout.ObjectField("PageTexture", customPageTexture, typeof(Texture2D), true);
        EditorGUILayout.EndToggleGroup();

        EditorGUI.BeginDisabledGroup(customSetPage);
        showSetSize = EditorGUILayout.Foldout(showSetSize, "Show Set Size Method");
        if (showSetSize)
        {
            setBookSize = EditorGUILayout.BeginToggleGroup("SetBookSize", setBookSize);
            useBookTexture = EditorGUILayout.BeginToggleGroup("UseTextureToSize", useBookTexture);
            BookTexture = (Texture2D)EditorGUILayout.ObjectField("BookTexture", BookTexture, typeof(Texture2D), true);
            EditorGUILayout.EndToggleGroup();
            EditorGUI.BeginDisabledGroup(useBookTexture);
            EditorGUILayout.LabelField("CustomSetSize");
            BookWidth = EditorGUILayout.FloatField("BookWidth", BookWidth);
            BookHeight = EditorGUILayout.FloatField("BookHeight", BookHeight);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space(20f);

            EditorGUI.BeginDisabledGroup(setBookSize);
            EditorGUILayout.LabelField("SetPageSize");
            EditorGUILayout.Space(5f);

            usePageTexture = EditorGUILayout.BeginToggleGroup("UseTextureToSize", usePageTexture);
            PageTexture = (Texture2D)EditorGUILayout.ObjectField("PageTexture", PageTexture, typeof(Texture2D), true);
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space(10f);

            EditorGUI.BeginDisabledGroup(usePageTexture);

            EditorGUILayout.LabelField("CustomSetSize");
            EditorGUILayout.Space(5f);
            PageWidth = EditorGUILayout.FloatField("PageWidth", PageWidth);
            PageHeight = EditorGUILayout.FloatField("PageHeight", PageHeight);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10f);
        }
        EditorGUI.EndDisabledGroup();
       
        EditorGUILayout.Space(10f);

        if (GUILayout.Button("Create Book"))
        {

           foreach(var tagName in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (tagName == "BaseBook")
                {
                    hasBaseBookTag = true;
                    break;
                }
                else
                {
                    hasBaseBookTag = false;
                    continue;
                }             
            }
            foreach (var tagName in UnityEditorInternal.InternalEditorUtility.tags)
            {
                 if (tagName == "Book")
                {
                    hasPageTag = true;
                    break;
                }

                else
                {
                    hasPageTag = false;
                    continue;
                }
            }
            foreach (var tagName in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (tagName == "Context")
                {
                    hasContextTag = true;
                    break;
                }

                else
                {
                    hasContextTag = false;
                    continue;
                }
            }
            foreach (var tagName in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (tagName == "PageFront")
                {
                    hasPageFrontTag = true;
                    break;
                }

                else
                {
                    hasPageFrontTag = false;
                    continue;
                }
            }
            foreach (var tagName in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (tagName == "PageBack")
                {
                    hasPageBackTag = true;
                    break;
                }

                else
                {
                    hasPageBackTag = false;
                    continue;
                }
            }



            switch (hasBaseBookTag)
            {
                case true:
                    break;
                case false:
                    UnityEditorInternal.InternalEditorUtility.AddTag("BaseBook");
                    break;
            }
            switch (hasPageTag)
            {
                case true:
                    break;
                case false:
                    UnityEditorInternal.InternalEditorUtility.AddTag("Page");
                    break;
            }
            switch (hasContextTag)
            {
                case true:
                    break;
                case false:
                    UnityEditorInternal.InternalEditorUtility.AddTag("Context");
                    break;
            }
            switch (hasPageFrontTag)
            {
                case true:
                    break;
                case false:
                    UnityEditorInternal.InternalEditorUtility.AddTag("PageFront");
                    break;
            }
            switch (hasPageBackTag)
            {
                case true:
                    break;
                case false:
                    UnityEditorInternal.InternalEditorUtility.AddTag("PageBack");
                    break;
            }


            GameObject canvas = new GameObject("Book", typeof(Canvas));
            canvas.tag = "BaseBook";
            canvas.AddComponent<BookRoate>();
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();
            canvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            canvas.GetComponent<CanvasScaler>().scaleFactor = 1;
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.GetComponent<BookRoate>().flipSpeed = flipSpeed;


            GameObject backGround = new GameObject("BackGround", typeof(Image));
            backGround.transform.SetParent(canvas.transform);
            backGround.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            switch (showBackGround)
            {
                case true:
                    backGround.GetComponent<Image>().color = Color.white;
                    Sprite backgroundSprite= Sprite.Create(backgroundTexture, new Rect(0,0,backgroundTexture.width, backgroundTexture.height), new Vector2(0, 0));
                    backGround.GetComponent<RectTransform>().sizeDelta = new Vector2(backgroundTexture.width, backgroundTexture.height);
                    backGround.GetComponent<Image>().sprite = backgroundSprite;
                    break;
                case false:                
                    backGround.GetComponent<Image>().color = Color.black;
                    checkInBook(backGround);
                    break;
            }         
            backGround.SetActive(showBackGround);

            GameObject nextPageButton = new GameObject("NextPageButton", typeof(Image));
            nextPageButton.AddComponent<Button>();
            nextPageButton.transform.SetParent(canvas.transform);
            checkInButton(nextPageButton, 1);
      

            GameObject forwardPageButton = new GameObject("ForwardPageButton", typeof(Image));
            forwardPageButton.AddComponent<Button>();
            forwardPageButton.transform.SetParent(canvas.transform);
            checkInButton(forwardPageButton, -1);
         

            canvas.GetComponent<BookRoate>().nextPageButton = nextPageButton;
            canvas.GetComponent<BookRoate>().forwardPageButton=forwardPageButton;



                    for (int i = 0; i < pageCount; i++)
                    {
                        GameObject pagePoint = new GameObject("Page" + (i + 1) + "Point");
                        pagePoint.transform.SetParent(canvas.transform);
                        pagePoint.AddComponent<RectTransform>();
                        pagePoint.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                        canvas.GetComponent<BookRoate>().PageList.Add(pagePoint.transform);
                pagePoint.tag = "Page";

                        GameObject page = new GameObject("Page" + (i + 1), typeof(Image));
                        page.transform.SetParent(backGround.transform);
                page.tag = "Context";
                      
                        page.transform.SetParent(pagePoint.transform);
                         checkInPage(page);
                        page.GetComponent<Image>().raycastTarget = false;

                GameObject pageFront = new GameObject("PageFront");
                pageFront.transform.SetParent(page.transform);
                pageFront.AddComponent<RectTransform>();
                pageFront.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                pageFront.tag = "PageFront";

                GameObject pageBack = new GameObject("PageBack");
                pageBack.transform.SetParent(page.transform);
                pageBack.AddComponent<RectTransform>();
                pageBack.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                pageBack.tag = "PageBack";
            }
          
        }

        EditorGUILayout.Space(5f);
        if (GUILayout.Button("Destroy Selected Book"))
        {
            if (EditorUtility.DisplayDialog("Confirm Your Choice","Do you want to destroy the selected book?","Yes","No"))
            {
                if (Selection.gameObjects.Length == 0)
                    throw new Exception("Haven't Selected GameObjects");
                else
                {
                    foreach (var selectObject in Selection.gameObjects)
                    {
                        if (selectObject.tag != "BaseBook")
                            throw new Exception(@"Some of these selected GameObjects' tags!=""BaseBook""");
                    }
                    foreach (var selectObject in Selection.gameObjects)
                    {
                        DestroyImmediate(selectObject);
                    }
                }
            }
        }
        EditorGUILayout.Space(5f);
        if (GUILayout.Button("Destroy All Book"))
        {
          if(EditorUtility.DisplayDialog("Confirm Your Choice","Do you want to destroy all books which active in the hierarchy?","Yes","No"))
            {
                if (GameObject.FindGameObjectsWithTag("BaseBook").Length == 0)
                {
                    Debug.LogWarning("There are no BaseBook in the Hierarchy");
                    return;
                }
                foreach (var item in GameObject.FindGameObjectsWithTag("BaseBook"))
                {
                    DestroyImmediate(item);
                }
            }
        }
    }

    private void checkInBook(GameObject obj)
    {
        switch (setBookSize)
        {
            case true:
                switch (useBookTexture)
                {
                    case true:
                        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(BookTexture.width, BookTexture.height);
                        break;
                    case false:
                       obj.GetComponent<RectTransform>().sizeDelta = new Vector2(BookWidth, BookHeight);
                        break;
                }
                break;
            case false:
                switch (usePageTexture)
                {
                    case true:
                        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(PageTexture.width*2, PageTexture.height);
                        break;
                    case false:
                        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(PageWidth * 2, PageHeight);
                        break;
                }
                break;
        }
    }
    private void checkInButton(GameObject obj,int temp)
    {
        switch (setBookSize)
        {
            case true:
                switch (useBookTexture)
                {
                    case true:
                        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(temp*(BookTexture.width / 2) +temp*
            (obj.GetComponent<RectTransform>().rect.width / 2) +temp*20, 0);
                        break;
                    case false:
                        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(temp * (BookWidth / 2) + temp *
            (obj.GetComponent<RectTransform>().rect.width / 2) + temp * 20, 0);
                        break;
                }
                break;
            case false:
                switch (usePageTexture)
                {
                    case true:
                        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(temp * PageTexture.width+ temp *
                     (obj.GetComponent<RectTransform>().rect.width / 2) + temp * 20, 0);
                        break;
                    case false:
                        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(temp*PageWidth +temp*
                     (obj.GetComponent<RectTransform>().rect.width / 2)+temp *20, 0);
                        break;
                }
                break;
        }
    }
    private void checkInPage(GameObject obj)
    {
        switch (customSetPage)
        {
            case false:
                switch (setBookSize)
                {
                    case true:
                        switch (useBookTexture)
                        {
                            case true:
                                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(BookTexture.width / 2, BookTexture.height);
                                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(BookTexture.width / 4, 0);
                                break;
                            case false:
                                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(BookWidth / 2, BookHeight);
                                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(BookWidth / 4, 0);
                                break;
                        }
                        break;
                    case false:
                        switch (usePageTexture)
                        {
                            case true:
                                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(PageTexture.width, PageTexture.height);
                                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(PageTexture.width / 2, 0);
                                break;
                            case false:
                                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(PageWidth, PageHeight);
                                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(PageWidth / 2, 0);
                                break;
                        }
                        break;
                }
                break;
            case true:
                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(customPageTexture.width,customPageTexture.height);
                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(customPageTexture.width,0);
                obj.GetComponent<Image>().sprite = Sprite.Create(customPageTexture, new Rect(0, 0, customPageTexture.width, customPageTexture.height), new Vector2(0.5f,0.5f));
                break;
        }
       
    }

   
}

