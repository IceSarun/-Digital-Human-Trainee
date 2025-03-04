using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class Tutorial
    {
        public GameObject panelForTeach;
        public int startNumber;
        public int endNumber; 
    }

    public GameObject overlayPanel;    // Panel �մ�����ʧ���Դ˹�Ҩ�
    private int currentStep = 0;       // ��鹵͹�Ѩ�غѹ
    [Header("Set Tutorial")]
    public List<Tutorial> tutorials= new List<Tutorial>();// ����Ѻ�� Tutorial panel (Panel ��ԧ� ����������)


    // find panel for dynamic
    public List<GameObject> popUpPanel = new List<GameObject>(); // Panel �ʴ���ͤ��� Pop-up
    public Transform tutorialParent;

    private void Awake()
    {
        if (tutorialParent != null)
        {
            // �֧ Transform �ͧ�١������� iconParent
            Transform[] allChildren = tutorialParent.GetComponentsInChildren<Transform>(true);
            foreach (var child in allChildren)
            {
                // ��Ǩ�ͺ��� GameObject ������� "TutorialPopUp" �������
                if (child.gameObject.CompareTag("TutorialPopUp"))
                {
                    popUpPanel.Add(child.gameObject);
                }
            }

        }
        
    }

    void Start()
    {
        overlayPanel.SetActive(true);
        for (int i = 0; i < popUpPanel.Count; i++)
        {
            //Debug.Log("Hello" + i.ToString());
            popUpPanel[i].SetActive(false);
            if (i == 0)
            {
                popUpPanel[i].SetActive(true);
            }
            
        }
        FalseAllTeachPanel();
    }

    public void FalseAllTeachPanel() {
        for (int i = 0; i < tutorials.Count; i++)
        {
            tutorials[i].panelForTeach.SetActive(false);


        }
    }

    public void ShowStep(int stepIndex)
    {
        Debug.Log("stepIndex = "+ stepIndex);
        //Debug.Log("popUpPanel = "+ popUpPanel.Count);
        if (stepIndex >= popUpPanel.Count)
        {
            EndTutorial();
            return;
        }
        for (int i = 0; i < popUpPanel.Count; i++)
        {
            popUpPanel[i].SetActive(false);
            if (i == stepIndex)
            {
                popUpPanel[i].SetActive(true);
            }

        }
        for (int i = 0; i < tutorials.Count; i++) {
            Debug.Log("start" + tutorials[i].startNumber);

            /*if (tutorials[i].endNumber < stepIndex) {
                tutorials[i].panelForTeach.SetActive(false);
            }

            if (tutorials[i].startNumber > stepIndex && tutorials[i].endNumber >= stepIndex) {
                tutorials[i].panelForTeach.SetActive(false);
            }*/
            
            if (tutorials[i].startNumber == stepIndex)
            {
                tutorials[i].panelForTeach.SetActive(true);
                return;
            }
            else if (tutorials[i].startNumber < stepIndex && tutorials[i].endNumber >= stepIndex) {
                tutorials[i].panelForTeach.SetActive(true);
                return;
            }
        }
    }

    public void NextStep()
    {
        currentStep++;
        FalseAllTeachPanel();
        //Debug.Log(currentStep);
        ShowStep(currentStep);

    }

    public void BackStep() 
    {

        currentStep--;
        FalseAllTeachPanel();
        //Debug.Log(currentStep);
        ShowStep(currentStep);
    }

    public void SkipTutorial()
    {
        EndTutorial();
    }

    public void EndTutorial()
    {
        overlayPanel.SetActive(false);
        currentStep = 0;
        FalseAllTeachPanel();
        Debug.Log(currentStep);
        Debug.Log("Tutorial Ended");
    }

    public void OpenPanel() {
        overlayPanel.SetActive(true);
        ShowStep(0);
    }
}