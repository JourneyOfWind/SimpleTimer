using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerTest:MonoBehaviour
{
    int mTimerTaskID = -1;
    SimpleTimer mTimer;
    private void Start() {
        
        mTimer = new SimpleTimer();
        CreateTimerTask();
    }

    private void CreateTimerTask() {
        int count = 0;
        mTimerTaskID = mTimer.AddTask(1000, () => {
            count++;
            Debug.Log(count);
        }, () => {
            Debug.Log("任务取消成功");
        }, -1);
    }

    private void FixedUpdate() {
        mTimer.FixedUpdate();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            mTimer.Pause(mTimerTaskID);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            mTimer.Resume(mTimerTaskID);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            mTimer.CancelTask(mTimerTaskID);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            mTimer.DeleteTask(mTimerTaskID);
        }
    }
}
