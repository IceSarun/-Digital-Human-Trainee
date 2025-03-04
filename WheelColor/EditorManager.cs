using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EditorManager : MonoBehaviour
{
    //public GameObject tutorialPanel; // ����Ѻ�Ǻ��� tutorial Panel
    public GameObject editorModePanel; //����Ѻ���͡��觷������
    public GameObject inModePanelFor2D; // ����Ѻ�͹��������������� 2d 
    public GameObject inModePanelFor3D; // ����Ѻ�͹��������������� 3d
    public bool checkColorModePanel = false;
    public bool checkColor3DModePanel = false;

    //���͹䢡�����������͵�ͧ����� default theme
    public GameObject color2D;
    public GameObject starPanel;
    public GameObject themePanel; // ����Ѻ����¹���
    public GameObject editThemeAgain;

    [SerializeField]
    private ThemeData themeName;
    public ThemeIconManager themManager;
    public TMP_Text themNameShow;

    //control panel in EditorMode
    [SerializeField]
    private PanelManager PM;
    [SerializeField]
    private PanelManagerFor2D PM2D;
    [SerializeField]
    private TutorialManager tm;

    public void Start()
    {
        editorModePanel.SetActive(false);
        inModePanelFor2D.SetActive(false);
        inModePanelFor3D.SetActive(false);
        starPanel.SetActive(false);
        themePanel.SetActive(false);
        editThemeAgain.SetActive(false);
    }

    public void Update()
    {
        themNameShow.text = themeName.selectedThemeName;
    }

    public void OpenEditorPanel()
    {
        //�����Դ ����ѧ����� edit mode
        if (checkColorModePanel == false && checkColor3DModePanel == false)
        {
            editorModePanel.SetActive(true);
            //�������͹���� �ҡ����� Defualt Theme ���������ö��������¹����
            if (themeName.selectedThemeName != "Default")
            {
                color2D.SetActive(false);
            }
            else {
                color2D.SetActive(true);
            }
        }
        else {
            IgnoreOnclick();
        }

    }

    public void OpenTutorialPanel() {
        //�����Դ ����ѧ����� edit mode
        if (checkColorModePanel == false && checkColor3DModePanel == false)
        {
            tm.OpenPanel();
        }
        else
        {
            IgnoreOnclick();
        }
    }

    public void ColorMode()
    {
        editorModePanel.SetActive(false);
        inModePanelFor2D.SetActive(true);
        checkColorModePanel = true;

    }

    public void ThemeMode() {
        editThemeAgain.SetActive(false);
        editorModePanel.SetActive(false);
        inModePanelFor2D.SetActive(true);
        themePanel.SetActive(true);

    }
    public void OpenStarPanel()
    {
        starPanel.SetActive(true);
        themePanel.SetActive(false);
    }
    public void CloseStarPanel()
    {
        themePanel.SetActive(true);
        starPanel.SetActive(false);
    }

    public void ShowEditThemeAgain() {
        editThemeAgain.SetActive(true);
        themePanel.SetActive(false);
        starPanel.SetActive(false);
    }

    public void ExitPreveiwMode() {
        if (themManager.previousTheme != null && (themManager.previousTheme.themeName != themManager.selectedTheme.themeName))
        {
            Debug.Log(themManager.previousTheme.themeName);
            themManager.RestorePreviousTheme();
            ThemeMode();
        }
        themManager.InPreveiwMode.SetActive(false);
    }

    public void ColorObjectMode()
    {
        editorModePanel.SetActive(false);
        inModePanelFor3D.SetActive(true);
        checkColor3DModePanel = true;
    }

    public void changeScene() {
        if (checkColorModePanel == false && checkColor3DModePanel == false)
        {
            SceneManager.LoadScene("SampleScene");
        }
        else { 
            IgnoreOnclick();
        }
            
    }
    

    public void CanclePanel() {
        editorModePanel.SetActive(false);
        inModePanelFor2D.SetActive(false);
        inModePanelFor3D.SetActive(false);
        themePanel.SetActive(false);
        starPanel.SetActive(false);
        PM.ClosePanels();
        PM2D.ClosePanels();
        checkColorModePanel = false;
        checkColor3DModePanel = false;
    }
    public void IgnoreOnclick() {
        return;    
    }

}
