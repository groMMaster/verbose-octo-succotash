using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject levelsParent;
    [SerializeField] private GameObject popup;
    [SerializeField] private GameObject stepText;
    private static int _step;
    private static LineRenderer _lineRenderer;
    private static Dictionary<int, List<int>> _paths;

    public void Awake()
    {
        _step = 1;
        stepText.GetComponent<Text>().text = "Ход: " + _step;
        _lineRenderer = GetComponent<LineRenderer>();
        _paths = new Dictionary<int, List<int>>
        {
            {0, new List<int>{1, 2, 3}},
            {1, new List<int>{0, 2, 4}},
            {2, new List<int>{0, 1, 4}},
            {3, new List<int>{0, 5}},
            {4, new List<int>{1, 2, 5, 6}},
            {5, new List<int>{3, 6}},
            {6, new List<int>{4, 5}},
        };
    }

    public void Start()
    {
        var firstLevel = levelsParent.transform.GetChild(0).gameObject;
        UpdateLevels(levelsParent);
    }
    
    public void OnClick()
    {
        var level = EventSystem.current.currentSelectedGameObject;
        var currentLevelIndex = GetCurrentLevel();
        var currentSquad = SquadsManager.GetCurrentSquad();
        // if level is available from current level 
        if (_paths[currentLevelIndex].Contains(Int32.Parse(level.name)) && !SquadsManager.GetSquadsState()[currentSquad])
        {
            SquadsManager.MoveSquad( SquadsManager.GetCurrentSquad(), Int32.Parse(level.name), false);
            // if squad moved to another level  
            if (SquadsManager.GetSquadsState()[currentSquad])
            {
                UpdateLevelsWithoutAviable(levelsParent, _lineRenderer);
                OpenPopup();
            }
            else
            {
                UpdateLevels(levelsParent);
            }
        }
    }

    public static int GetCurrentLevel()
    {
        return SquadsManager.GetSquadsLocation()[SquadsManager.GetCurrentSquad()];
    }
    
    public static void UpdateLevels(GameObject levelsParent)
    {
        var drawingList = new List<GameObject>();
        var currentLevel = levelsParent.transform.GetChild(GetCurrentLevel()).gameObject;
        var currentLevelIndex = GetCurrentLevel();

        foreach (Transform level in levelsParent.transform)
        {
            // if level is not current
            if (level.name != currentLevelIndex.ToString())
            {
                level.Find("OnActive").gameObject.SetActive(false);
                level.Find("Squad_"+SquadsManager.GetCurrentSquad()).gameObject.SetActive(false);
            }
            else
            {
                level.Find("OnActive").gameObject.SetActive(true);
                level.Find("Squad_"+SquadsManager.GetCurrentSquad()).gameObject.SetActive(true);
            }
            
            // if level available
            if (_paths[currentLevelIndex].Contains(Int32.Parse(level.name)))
            {
                level.Find("OnAvailable").gameObject.SetActive(true);
                drawingList.Add(currentLevel);
                drawingList.Add(level.gameObject);
            }
            else
            {
                level.Find("OnAvailable").gameObject.SetActive(false);
            }
        }
        
        DrawLine(_lineRenderer, drawingList.ToArray());
    }

    public static void UpdateLevelsWithoutAviable(GameObject parent, LineRenderer lineRenderer)
    {
        var currentLevelIndex = GetCurrentLevel();
        lineRenderer.positionCount = 0;

        foreach (Transform level in parent.transform)
        {
            // if level is not current
            if (level.name != currentLevelIndex.ToString())
            {
                level.Find("OnActive").gameObject.SetActive(false);
                level.Find("Squad_"+SquadsManager.GetCurrentSquad()).gameObject.SetActive(false);
            }
            else
            {
                level.Find("OnActive").gameObject.SetActive(true);
                level.Find("Squad_"+SquadsManager.GetCurrentSquad()).gameObject.SetActive(true);
            }
            level.Find("OnAvailable").gameObject.SetActive(false);
        }
    }

    public void OpenPopup()
    {
        popup.SetActive(true);
    }

    public void EndStep()
    {
        _step++;
        stepText.GetComponent<Text>().text = "Ход: " + _step;
        SquadsManager.RefreshSquadsState();
        UpdateLevels(levelsParent);
    }

    public static void DrawLine(LineRenderer lineRenderer, params GameObject[] objectsList)
    {
        lineRenderer.positionCount = objectsList.Length;
        for (int i = 0; i< objectsList.Length; i+=2)
        {
            var point1 = objectsList[i].transform.position;
            var point2 = objectsList[i+1].transform.position;
            var centedObject1 = new Vector3(point1.x + 0.4f,point1.y + 0.4f);
            var centedObject2 = new Vector3(point2.x + 0.4f,point2.y + 0.4f);
            lineRenderer.SetPosition(i, centedObject1);
            lineRenderer.SetPosition (i+1, centedObject2);
        }
    }
}
