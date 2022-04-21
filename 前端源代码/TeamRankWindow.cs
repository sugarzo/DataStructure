using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TeamRankWindow : MonoBehaviour
{
    public TMP_Text content;
    List<string> aList = new List<string>();
    public Button button;
    public StateIcon StateIcon;
    private int index = 0;
    string extra = string.Empty;
    public Text ex;
    private void OnEnable()
    {
        index = 0;
        extra = string.Empty;
        ex.text = "";
        content.text = "";
        StateIcon.SetState(StateIcon.State.Waiting);
    }
    private void Start()
    {

        button.onClick.AddListener(delegate ()
        {
            button.interactable = false;

            StateIcon.SetState(StateIcon.State.Loading);
            ThreadManager.StartThread(delegate ()
            {
                ReadAuaRank(ref aList,ref extra);

            }, delegate ()
            {
                StateIcon.SetState(StateIcon.State.Finish);
                button.interactable = true;
                content.text = "";
                ex.text = extra;
                for (int i = 0; i < aList.Count; i++)
                {
                    content.text += "Top" + (i + 1) + "  " +aList[i] + "\n";
                }
                button.interactable = true;
            });
        });
    }

    public void ReadAuaRank(ref List<string> aList,ref string extra)
    {
        aList.Clear();

        var state = System.IO.File.ReadAllLines(Program.filePath + "database//aua_result.dat");
        var rank = System.IO.File.ReadAllLines(Program.filePath + "database//aua_rank_result.dat");

        extra = state[0] + " " + rank[0];
        for (int i = 1; i < rank.Length; i++)
        {
            aList.Add(rank[i]);
        }
    }
}
