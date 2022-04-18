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
    /// 添加计时任务
    /// </summary>
    /// <param name="delay">计时任务间隔</param>
    /// <param name="taskCB">计时回调</param>
    /// <param name="cancelCB">取消回调</param>
    /// <param name="RepeatTime">重复次数</param>
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
    /// 任务取消，触发取消回调
    /// </summary>
    /// <param name="taskID">任务ID</param>
    public void CancelTask(int taskID)
    {
        if (mClockDict.ContainsKey(taskID)) {
            mClockDict[taskID].OnCancel();
            mClockDict[taskID].Reset();
            mClockDict.Remove(taskID);
        }
    }

    /// <summary>
    /// 直接删除任务
    /// </summary>
    /// <param name="taskID">任务ID</param>
    public void DeleteTask(int taskID)
    {
        if (mClockDict.ContainsKey(taskID)) {
            mClockDict[taskID].Reset();
            mClockDict.Remove(taskID);
        }
        else {
            Debug.LogError($"将要移除的计时器ID{taskID}不存在");
        }
    }

    /// <summary>
    /// 暂停所有计时任务
    /// </summary>
    public void Pause()
    {
        foreach (Clock clock in mClockDict.Values) {
            clock.Pause();
        }
    }

    /// <summary>
    /// 继续计时
    /// </summary>
    public void Resume()
    {
        foreach (Clock clock in mClockDict.Values) {
            clock.Resume();
        }
    }

    /// <summary>
    /// 暂停某个任务
    /// </summary>
    /// <param name="taskID"></param>
    public void Pause(int taskID)
    {
        if (mClockDict.ContainsKey(taskID)) {
            mClockDict[taskID].Pause();
        }
        else {
            Debug.LogError($"将要移除的计时器ID{taskID}不存在");
        }
    }

    /// <summary>
    /// 继续某个任务
    /// </summary>
    /// <param name="taskID"></param>
    public void Resume(int taskID)
    {
        if (mClockDict.ContainsKey(taskID)) {
            mClockDict[taskID].Resume();
        }
        else {
            Debug.LogError($"将要移除的计时器ID{taskID}不存在");
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
