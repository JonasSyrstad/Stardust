using System;
using System.Threading;

namespace ThreadSynchronizationTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var StartMultiThreads = new MyThreadTest();
            StartMultiThreads.CreateThreads();
        }
    }
    public class MyThreadTest
    {
        static readonly AutoResetEvent thread1Step = new AutoResetEvent(false);
        static readonly AutoResetEvent thread2Step = new AutoResetEvent(true);

        void DisplayThread1()
        {
            while (true)
            {
                thread2Step.WaitOne();
                Console.WriteLine("Display Thread 1");
                Thread.Sleep(1000);
                thread1Step.Set();
            }
        }

        void DisplayThread2()
        {
            while (true)
            {
                thread1Step.WaitOne();
                Console.WriteLine("Display Thread 2");
                Thread.Sleep(1000);
                thread2Step.Set();
            }
        }

        internal void CreateThreads()
        {
            // construct two threads for our demonstration;
            Thread thread1 = new Thread(new ThreadStart(DisplayThread1));
            Thread thread2 = new Thread(new ThreadStart(DisplayThread2));

            // start them
            thread1.Start();
            thread2.Start();
        }

       
    }
}
