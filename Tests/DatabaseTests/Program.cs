using System;
using DatabaseAccessLibrary;

namespace DatabaseTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Data data = new Data();
            data.ParseData("../../DatabaseAccessLibrary/Data/data1.csv");
            Console.WriteLine(data.DataList[1].Name + data.DataList[1].Surname);
            
        }
    }
}
