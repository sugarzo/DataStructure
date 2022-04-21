using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;
public class TestScript : MonoBehaviour
{
    [DllImport("dlltest")]
    private static extern bool initial_readers(int Max_thread, int TOTAL_THREAD, bool ShowText);

    void Start()
    {
        ThreadManager.StartThread(() => { initial_readers(8, 2, false); }, null);
    }


}
