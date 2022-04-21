using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class AuthorRankResultWindow : MonoBehaviour
{
    public TMP_Text content;
    List<string> aList = new List<string>();
    List<int> rank = new List<int>();
    public Button button;
    public StateIcon StateIcon;
    public Button btn_Next;
    private int index = 0;

    private void OnEnable()
    {
        index = 0;
        content.text = "";
        StateIcon.SetState(StateIcon.State.Waiting);
    }
    private void Start()
    {
        btn_Next.onClick.AddListener(NextPage);

        button.onClick.AddListener(delegate () 
        {
            button.interactable = false;
            
            StateIcon.SetState(StateIcon.State.Loading);
            ThreadManager.StartThread(delegate ()
            {
                Program.dllTool.ReadAuthorRank(ref rank, ref aList, index);
                
            },delegate() 
            {
                StateIcon.SetState(StateIcon.State.Finish);
                button.interactable = true;
                content.text = "";
                for (int i = 0; i < 100; i++)
                {
                    content.text += "Top" + (index * 100 + i + 1) + " " + aList[i] + " " + rank[i] + "\n";
                }
                button.interactable = true;
            });
        });
    }
    private void NextPage()
    {
        button.interactable = false;
        index++;
        button.onClick.Invoke();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
