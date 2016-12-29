using System;

namespace Character
{
    class Program
    {
        static void Main(string[] args)
        {
            Func<long,long> progressionFunc = new Func<long, long>(l => l/100);
            Progression progression = new Progression(progressionFunc);

            var xp = progression.GetExperienceForLevel(3);

            Console.WriteLine(xp);
            string x =Console.ReadLine();
        }
    }
}
