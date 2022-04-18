using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

public class SimpleTimer
{
    private int mID = 100;
    private Dictionary<int, Clock> mClockDict;
    private List<int> mToBeRemoveClock;

    public SimpleTimer()
    {
        mID = 100;
        mClockDict = new Dictionary<int, Clock>();
        mToBeRemoveClock = new List<int>();
    }

    /// <summary>
    /// ��Ӽ�ʱ����
    /// </summary>
    /// <param name="delay">��ʱ������</param>
    /// <param name="taskCB">��ʱ�ص�</param>
    /// <param name="cancelCB">ȡ���ص�</param>
    /// <param name="RepeatTime">�ظ�����</param>
    /// <returns></returns>
    public int AddTask(uint delay, Action taskCB, Action cancelCB, int RepeatTime = 1)
    {
        int id = GenerateID();
        Clock clock = new Clock(id, delay, new Stopwatch(), taskCB, cancelCB, RepeatTime);
        mClockDict.Add(id, clock);

        clock.Start();
        return id;
    }

    /// <summary>
    /// ����ȡ��������ȡ���ص�
    /// </summary>
    /// <param name="taskID">����ID</param>
    public void CancelTask(int taskID)
    {
        if (mClockDict.ContainsKey(taskID)) {
            mClockDict[taskID].OnCancel();
            mClockDict[taskID].Reset();
            mClockDict.Remove(taskID);
        }
    }

    /// <summary>
    /// ֱ��ɾ������
    /// </summary>
    /// <param name="taskID">����ID</param>
    public void DeleteTask(int taskID)
    {
        if (mClockDict.ContainsKey(taskID)) {
            mClockDict[taskID].Reset();
            mClockDict.Remove(taskID);
        }
        else {
            Debug.LogError($"��Ҫ�Ƴ��ļ�ʱ��ID{taskID}������");
        }
    }

    /// <summary>
    /// ��ͣ���м�ʱ����
    /// </summary>
    public void Pause()
    {
        foreach (Clock clock in mClockDict.Values) {
            clock.Pause();
        }
    }

    /// <summary>
    /// ������ʱ
    /// </summary>
    public void Resume()
    {
        foreach (Clock clock in mClockDict.Values) {
            clock.Resume();
        }
    }

    /// <summary>
    /// ��ͣĳ������
    /// </summary>
    /// <param name="taskID"></param>
    public void Pause(int taskID)
    {
        if (mClockDict.ContainsKey(taskID)) {
            mClockDict[taskID].Pause();
        }
        else {
            Debug.LogError($"��Ҫ�Ƴ��ļ�ʱ��ID{taskID}������");
        }
    }

    /// <summary>
    /// ����ĳ������
    /// </summary>
    /// <param name="taskID"></param>
    public void Resume(int taskID)
    {
        if (mClockDict.ContainsKey(taskID)) {
            mClockDict[taskID].Resume();
        }
        else {
            Debug.LogError($"��Ҫ�Ƴ��ļ�ʱ��ID{taskID}������");
        }
    }

    private int GenerateID()
    {
        if (mID >= int.MaxValue)
            mID = 0;

        return mID++;
    }

    public void FixedUpdate()
    {
        mToBeRemoveClock.Clear();
        if (mClockDict != null && mClockDict.Count > 0) {
            var clockList = mClockDict.Values.ToList();
            for (int i = 0; i < clockList.Count; i++) {
                clockList[i].CountDown(out bool isRemove);
                if (isRemove) {
                    mToBeRemoveClock.Add(clockList[i].GetClockID());
                }
            }

            if (mToBeRemoveClock.Count > 0) {
                for (int i = 0; i < mToBeRemoveClock.Count; i++) {
                    DeleteTask(mToBeRemoveClock[i]);
                }
            }
            mToBeRemoveClock.Clear();
        }


    }
}

public class Clock
{
    private int ID;
    private uint Delay;
    private Stopwatch StopWatch;
    private Action TaskCB;
    private Action CancelCB;
    private int RepeatTime;

    private bool mIsPermanent;
    private uint NextCBTime;

    public Clock(int id, uint delay, Stopwatch stopWatch, Action taskCB, Action cancelCB, int repeatTime)
    {
        ID = id;
        Delay = delay;
        StopWatch = stopWatch;
        TaskCB = taskCB;
        CancelCB = cancelCB;
        RepeatTime = repeatTime;

        mIsPermanent = repeatTime < 0;
        NextCBTime = delay;
    }

    public int GetClockID()
    {
        return ID;
    }

    public void Start()
    {
        StopWatch.Start();
    }

    public void CountDown(out bool isRemove)
    {
        isRemove = false;
        if (StopWatch.ElapsedMilliseconds >= NextCBTime) {
            TaskCB?.Invoke();
            if (mIsPermanent) NextCBTime += Delay;
            else {
                RepeatTime -= 1;
                if (RepeatTime > 0) NextCBTime += Delay;
                else isRemove = true;
            }
        }
    }

    public void Pause()
    {
        StopWatch.Stop();
    }

    public void Resume()
    {
        StopWatch.Start();
    }

    public void OnCancel()
    {
        CancelCB?.Invoke();
    }

    public void Reset()
    {
        ID = -1;
        Delay = 0;
        StopWatch.Stop();
        StopWatch.Reset();
        StopWatch = null;
        TaskCB = null;
        CancelCB = null;
        RepeatTime = -1;
        mIsPermanent = false;
        NextCBTime = 0;
    }
}
