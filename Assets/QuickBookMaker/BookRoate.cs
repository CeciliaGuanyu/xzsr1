using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookRoate:MonoBehaviour
{
    public float flipSpeed;
    public List<Transform> PageList = new List<Transform>();
    public GameObject nextPageButton, forwardPageButton;
    private  int index=-1;
    private bool isRoate;


    private void Awake()
    {
        nextPageButton.SetActive(true);
        forwardPageButton.SetActive(false);
        PageList[0].SetAsLastSibling();
        foreach(var item in PageList)
        {
            item.GetChild(0).GetChild(0).gameObject.SetActive(true);
            item.GetChild(0).GetChild(1).gameObject.SetActive(false);
        }
    }
    private void Start()
    {
        nextPageButton.GetComponent<Button>().onClick.AddListener(RoateNext);
        forwardPageButton.GetComponent<Button>().onClick.AddListener(RoateBack);
       
    }
    public void RoateNext()
    {
        if (isRoate)
            return;
        index++;
        NextPageButtonAction();
        PageList[index].SetAsLastSibling();
        StartCoroutine(IERoatePage(-180,true));
    }

   private void NextPageButtonAction()
    {
        if (!forwardPageButton.activeInHierarchy)
            forwardPageButton.SetActive(true);
        if (index == PageList.Count - 1)
            nextPageButton.SetActive(false);
    }

    public void RoateBack()
    {
        if (isRoate)
            return;
        PageList[index].SetAsLastSibling();
        ForwardPageButtonAction();
        StartCoroutine(IERoatePage(0,false));
    }

    private void ForwardPageButtonAction()
    {
        if (!nextPageButton.activeInHierarchy)
           nextPageButton.SetActive(true);
        if (index-1 == -1)
            forwardPageButton.SetActive(false);
    }


    IEnumerator IERoatePage(float angle,bool forward)
    {
        float value = 0;
        while (true)
        {
            isRoate = true;
            value += Time.deltaTime * flipSpeed;
            PageList[index].rotation = Quaternion.Slerp(PageList[index].rotation, Quaternion.Euler(0, angle, 0), value);

            PageList[index].GetChild(0).GetChild(1).rotation = Quaternion.Slerp(PageList[index].GetChild(0).GetChild(1).rotation, Quaternion.Euler(0, angle*2, 0), value*2);
             
            if (Quaternion.Angle(PageList[index].rotation, Quaternion.Euler(0, angle, 0)) <90f)
            {
                switch(forward )
                {
                    case true:
                    PageList[index].GetChild(0).GetChild(1).gameObject.SetActive(true);
                        PageList[index].GetChild(0).GetChild(0).gameObject.SetActive(false);
                        break;
                    case false:
                        PageList[index].GetChild(0).GetChild(1).gameObject.SetActive(false);
                        PageList[index].GetChild(0).GetChild(0).gameObject.SetActive(true);
                        break;
                }
                  
            }
            if (Quaternion.Angle(PageList[index].rotation,Quaternion.Euler(0,angle,0)) < 0.1f)
            {
                if (forward == false)
                    index--;
                isRoate = false;
                break;              
            }

      
            yield return null;
        }
    }
}
