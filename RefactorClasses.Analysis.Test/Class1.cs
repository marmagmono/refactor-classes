using System;

namespace RefactorClasses.Analysis.Test
{
    public static class Test
    {
        public static void Something()
        {
            Console.Out.WriteLine("dswwww");
        }
    }

    public class Test1
    {
        public void MyMethod(int a, int b)
        {
            int u = a;
            u = a;
        }
    }
}
