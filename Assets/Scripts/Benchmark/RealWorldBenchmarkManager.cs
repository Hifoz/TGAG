using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class RealWorldBenchmarkManager : BenchmarkChunkManager {

    public ChunkManager chunkManager;
    public Transform dummyPlayer;

    private int duration = 60;

    private void Start() {
        chunkManager.gameObject.SetActive(false);
    }

    /// <summary>
    /// Returns progress of benchmark
    /// </summary>
    /// <returns></returns>
    override public float getProgress() {
        return (float)stopwatch.Elapsed.TotalSeconds / duration;
    }

    /// <summary>
    /// Starts a "real" benchmark
    /// </summary>
    /// <param name="duration">Duration of each run</param>
    /// <param name="startThreads">threadcount to start with</param>
    /// <param name="endThreads">threadcount to end with</param>
    /// <param name="step">step to increase threadcount by</param>
    /// <returns>bool success</returns>
    public bool startBenchmark(int duration, int startThreads, int endThreads, int step) {
        if (!inProgress) {
            this.duration = duration;
            StartCoroutine(realBenchmark(duration, startThreads, endThreads, step));
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Starts a "real" benchmark
    /// </summary>
    /// <param name="duration">Duration of each run</param>
    /// <param name="startThreads">threadcount to start with</param>
    /// <param name="endThreads">threadcount to end with</param>
    /// <param name="step">step to increase threadcount by</param>
    private IEnumerator realBenchmark(int duration, int startThreads, int endThreads, int step) {
        QualitySettings.vSyncCount = 0;
        inProgress = true;
        yield return 0; //This random yield i put in for debugging suddenly fixed all bugs.

        path = string.Format("C:/temp/TGAG_Real_Benchmark_{0}.txt", DateTime.Now.Ticks);
        Directory.CreateDirectory("C:/temp");
        StreamWriter file = File.CreateText(path);

        file.WriteLine(string.Format("Testing from {0} to {1} threads with a step of {2}. ({3}):", startThreads, endThreads, step, DateTime.Now.ToString()));
        file.WriteLine(string.Format("Duration of each run: {0} seconds", duration));
        chunkManager.gameObject.SetActive(true);
        for (int run = startThreads; run <= endThreads; run += step) {
            UnityEngine.Debug.Log(String.Format("Testing with {0} thread(s)!", run));
            chunkManager.Reset(run);
            currentThreads = run;

            double lastSample = 0;
            const float sampleFreq = 1f;
            int framesInSample = 0;
            List<float[]> fpsOverTime = new List<float[]>(); //To graph framrate over time

            int frameCount = 0;
            float playerSpeed = 20; //This is currently the max animal speed
            Vector3 dir = new Vector3(1, 0, 0);

            stopwatch.Start();
            while (stopwatch.Elapsed.TotalSeconds <= duration) {
                framesInSample++;
                if (stopwatch.Elapsed.TotalSeconds >= lastSample + sampleFreq) {
                    fpsOverTime.Add(new float[] { (float)stopwatch.Elapsed.TotalSeconds, framesInSample });
                    framesInSample = 0;
                    lastSample = stopwatch.Elapsed.TotalSeconds;
                }

                if (UnityEngine.Random.Range(0f, 1f) < 0.01f) {
                    dir = Quaternion.AngleAxis(UnityEngine.Random.Range(-45, 45), Vector3.up) * dir;
                }

                Vector3 playerPos = dummyPlayer.position;
                playerPos += dir * playerSpeed * Time.deltaTime;
                dummyPlayer.position = playerPos;
                
                LandAnimalPlayer.playerPos.set(playerPos);
                LandAnimalPlayer.playerRot.set(dir);
                LandAnimalPlayer.playerSpeed.set(dir * playerSpeed);

                frameCount++;
                yield return 0;
            }
            ChunkManagerStats stats = chunkManager.stats;
            double time = stopwatch.Elapsed.TotalSeconds;

            stopwatch.Stop();
            stopwatch.Reset();
            string result = String.Format(
                "Average fps: {0} | Generated chunks: {1} | Generated animals: {2} | Cancelled chunks: {3} | Threads: {4}",
                (frameCount / time).ToString("N2"), stats.generatedChunks, stats.generatedAnimals, stats.cancelledChunks, run
            );
            UnityEngine.Debug.Log(result);
            file.WriteLine(String.Format(result));
            file.WriteLine(fpsOverTimeToString(fpsOverTime));
        }
        file.Close();
        UnityEngine.Debug.Log("DONE TESTING!");
        inProgress = false;
    }

    private string fpsOverTimeToString(List<float[]> fpsOverTime) {
        string data = string.Format("[{0}", Environment.NewLine);
        foreach (float[] sample in fpsOverTime) {
            data += string.Format("x:{0}|y:{1}{2}", sample[0].ToString("N2"), sample[1], Environment.NewLine);
        }
        data += string.Format("]{0}", Environment.NewLine);
        return data;
    }
}
