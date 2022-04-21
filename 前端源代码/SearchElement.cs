using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.UI;
public class SearchElement : MonoBehaviour
{
    public TMP_Text title;
    public GameObject prefabSearchText;
    public Transform SearchTextParent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Create(string titleName,List<string> content)
    {
        title.text = titleName;
        TitleColorSet.SearchActionType buttonActionType = TitleColorSet.SearchActionType.None;
        if (SearchManager.Instance._mapTitleColor.ContainsKey(titleName))
        {
            var set = SearchManager.Instance._mapTitleColor[titleName];
            buttonActionType = set.actionType;
            title.text = titleName + "  " + content.Count; 
            title.color = set.color;
        }
        foreach (var tex in content)
        {
            if (string.IsNullOrWhiteSpace(tex))
                continue;

            var obj= Instantiate(prefabSearchText);
            obj.transform.SetParent(SearchTextParent);
            obj.GetComponent<TMP_Text>().text = "  " + tex;
            
            if(buttonActionType == TitleColorSet.SearchActionType.Article)
            {
                obj.GetComponent<Button>().enabled = true;
                obj.GetComponent<Button>().onClick.AddListener(delegate ()
                {
                    SearchManager.Instance.Search(tex, 0);
                });
            }
            if (buttonActionType == TitleColorSet.SearchActionType.Author)
            {
                obj.GetComponent<Button>().enabled = true;
                obj.GetComponent<Button>().onClick.AddListener(delegate ()
                {
                    SearchManager.Instance.Search(tex, 1);
                });
            }
        }
    }
    


}
