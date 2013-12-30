using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Two threads. Bug if T1(i) executes before T2(j)
// parameters: 
//  -- K: the number of instructions per thread, K = 100
//  -- p: the probability of a context switch at any point
//  -- f: For a 1-variable bug, the variable is accessed fK times

namespace StressTesting
{
    class Program
    {
        public static int contextSwitchProbability = 100; // in 1000

        static void Main(string[] args)
        {
            ExperimentIJ(args);
        }

        static void ExperimentIJK(string[] args)
        {
            ProgramState.rand = new Random(DateTime.Now.Millisecond);

            var max = 10000;

            var K = 10000;
            float f = 0;
            foreach(var s in args) 
                float.TryParse(s, out f);
            if (f == 0) f = 0.1F;
            //Console.WriteLine("using f = {0}", f);

            var start = DateTime.Now;
            for (int a = 0; a < 5000; a++)
            {
                var i = ProgramState.rand.Next(K);
                var j = ProgramState.rand.Next(K);
                var k = ProgramState.rand.Next(K);
                if (k < i)
                {
                    var tmp = i;
                    i = k;
                    k = tmp;
                }

                var cnt = 0;
                for (int c = 0; c < max; c++)
                {
                    var r = Run(i, j, k, K);
                    if (r) cnt++;
                }

                var p1 = (float)cnt / max * 100;

                cnt = 0;
                for (int c = 0; c < max; c++)
                {
                    var r = Run((int)(f * i), (int)(f * j), (int)(f * k), (int)(f * K));
                    if (r) cnt++;
                }

                var p2 = (float)cnt / max * 100;

                Console.WriteLine("{0} {1} {2} {3} {4} {5}", (p2 > p1) ? "Pass" : (p1 > p2) ? "Fail" : "Equl", i, j, k, p1, p2);
                // Console.Error.WriteLine("{0} {1}", a, (DateTime.Now - start).TotalSeconds);
            }

        }

        static void ExperimentIJ(string [] args)
        {
            ProgramState.rand = new Random(DateTime.Now.Millisecond);

            var max = 10000;

            var K = 10000;
            float f = 0;
            foreach (var s in args)
                float.TryParse(s, out f);
            if (f == 0) f = 0.1F;

            var start = DateTime.Now;
            for (int a = 0; a < 5000; a++)
            {
                var i = ProgramState.rand.Next(K);
                var j = ProgramState.rand.Next(K);

                var cnt = 0;
                for (int c = 0; c < max; c++)
                {
                    var r = Run(i, j, K);
                    if (r) cnt++;
                }

                var p1 = (float)cnt / max * 100;

                cnt = 0;
                for (int c = 0; c < max; c++)
                {
                    var r = Run((int)(f * i), (int)(f * j), (int)(f * K));
                    if (r) cnt++;
                }

                var p2 = (float)cnt / max * 100;

                Console.WriteLine("{0} {1} {2} {3} {4}", i, j, K, p1, p2);

                //Console.WriteLine("{0} {1} {2} {3}", (int)(f * i), (int)(f * j), (int)(f * K), (float)cnt / max * 100);
                //Console.Error.WriteLine("{0} {1}", a, (DateTime.Now - start).TotalSeconds);
            }
        }

        public static bool Run(int i, int j, int K)
        {
            var state = new ProgramState(K);
            while (!state.Finished())
            {
                if (state.At(0, i))
                    return true;

                if (state.At(1, j))
                    return false;

                state.Step();
            }

            System.Diagnostics.Debug.Assert(false);
            return false;
        }

        public static bool Run(int i, int j, int k, int K)
        {
            var state = new ProgramState(K);
            bool ihit = false;
            
            while (!state.Finished())
            {
                if (state.At(0, i))
                    ihit = true;

                if (state.At(1, j))
                {
                    if (!ihit) return false;
                    return true;
                }

                if (state.At(0, k))
                        return false;

                state.Step();
            }

            System.Diagnostics.Debug.Assert(false);
            return false;
        }

    }

    class ProgramState
    {
        public int [] pc;
        int currThread;
        public static Random rand = null;
        int K;

        public ProgramState(int K)
        {
            pc = new int[2];
            pc[0] = 0;
            pc[1] = 0;
            currThread = rand.Next(2);
            System.Diagnostics.Debug.Assert(currThread == 0 || currThread == 1);
            this.K = K;
        }

        public void Step()
        {
            var choice = rand.Next(1000);
            if (choice < Program.contextSwitchProbability && pc[(currThread + 1) % 2] < K)
            {
                currThread = (currThread + 1) % 2;
            }
            if(pc[currThread] < K)
                pc[currThread]++;
        }

        public bool At(int tid, int p)
        {
            return pc[tid] == p;
        }

        public bool Finished()
        {
            return pc[0] == K && pc[1] == K;
        }
    }
}
