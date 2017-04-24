using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;

public class Executor : MonoBehaviour
{
    private Thread thread;
    private Process myProcess = new Process();
    public static Queue<string> commands=new Queue<string>();
    string exeName = @"CSGTool.exe";
    string exePath = @"CSGTool\";

    // Use this for initialization
    void Start()
    {
        thread = new Thread(Call);
        thread.IsBackground = true;
        thread.Start();
        command("LOAD cube");
        command("COPY cube1 cube");
        command("WRITE cube1");
    }
    // Update is called once per frame
    void Update()
    {
    }

    static public void command(string line) {
        commands.Enqueue(line);
    }

    private void Call()
    {
        myProcess.StartInfo.FileName = exePath+exeName;
        myProcess.StartInfo.WorkingDirectory = exePath;
        myProcess.StartInfo.Arguments = "script.txt";
        myProcess.StartInfo.UseShellExecute = false;
        myProcess.StartInfo.CreateNoWindow = false;
        //myProcess.StartInfo.RedirectStandardOutput = true;
        myProcess.StartInfo.RedirectStandardInput = true;
        myProcess.Start();
        //string redirectedOutput = string.Empty;
        while (true)
        {
            Thread.Sleep(333);
            bool isempty=true;
            foreach (string x in commands)
            {
                isempty = false;
                break;
            }
            if (!isempty)
            {
                string line = commands.Dequeue();
                myProcess.StandardInput.WriteLine( line );

            }
        }
        //myProcess.WaitForExit();
    }
    private void OnDisable()
    {
        thread.Abort();//強制中斷當前執行緒
        //myProcess.CloseMainWindow();
        myProcess.Kill();
    }
    /*
    private void OnApplicationQuit()
    {
        thread.Abort();
    }
    */
}
