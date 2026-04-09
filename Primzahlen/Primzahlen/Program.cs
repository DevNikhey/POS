using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace Primzahlen
{
    class Program
    {
        private static List<int> prims = new List<int>();
        private static int tests = 0;
        private static int MAX_NUMBER = 1600000;
        private static int _NumberofThreads = 4;
        private static int THREAD_NUM
        {
            get
            {
                return _NumberofThreads;
            }

            set
            {
                _NumberofThreads = value;
                progress = new int[_NumberofThreads];
                Array.Fill(progress, 4);
            }
        }
        private static int[] progress { get; set; } = new int[4] {4, 10, 100, 200};

        static void Main(string[] args)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            prims.Add(2);
            prims.Add(3);

            watch.Restart();
            List<Thread> threads = new List<Thread>();
            for (int i=0; i < THREAD_NUM; i++)
            {
                WorkerThread workerthread = new WorkerThread(i);
                Thread t = new Thread(workerthread.Prim);
                t.Start();
                threads.Add(t);
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }

            watch.Stop();

            Console.WriteLine("Es wurden {0} Primzahlen gefunden", prims.Count);
            Console.WriteLine("Die höchste gefundene Primzahl ist {0}", prims.ElementAt(prims.Count - 1));
            Console.WriteLine("Die Laufzeit betrug {0:F0} Millisekungen", watch.ElapsedMilliseconds);
            Console.WriteLine("Es wurden {0} Vergleiche durchgeführt", tests);
        }

        class WorkerThread
        {
            private int id;

            public WorkerThread(int id)
            {
                this.id = id;
                Console.WriteLine("Fired up thread: " + id);
            }

            public void Prim()
            {
                int currentIndex = 5 + id * THREAD_NUM;
                while (currentIndex < MAX_NUMBER)
                {
                    int maxTeiler = (int)Math.Sqrt(currentIndex) + 1;
                    while(progress.Min() < maxTeiler)
                    {
                        Thread.Sleep(1);
                    }

                    int j = 0;  
                    while (true)
                    {
                        int n = 0;

                        lock (prims)
                        {
                            n = prims[j];
                        }

                        int rest = (currentIndex % n);
                        ++tests;
                        if (rest == 0)
                            break; //keine Primzahl
                        if (n >= maxTeiler)
                        {
                            lock(prims)
                            {
                                int k = prims.Count - 1;
                                while (prims[k] > currentIndex)
                                {
                                    --k;
                                }

                                prims.Insert(++k, currentIndex);
                                break;

                            }
                        }
                        ++j;
                    }
                    progress[id] = currentIndex;
                    currentIndex += (2 * THREAD_NUM);
                }
            }
        }
    }
}