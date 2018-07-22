using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using DatabaseAccessLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DatabaseAccessLibrary
{
    public class Data
    {
        public List<FullModel> DataList = new List<FullModel>();
        public void ParseData(string dataSource){
            using (StreamReader sr = new StreamReader(dataSource))
            {
                Random r = new Random();
                string currentLine;
                currentLine = sr.ReadLine();
                // currentLine will be null when the StreamReader reaches the end of file
                while((currentLine = sr.ReadLine()) != null)
                {
                    var split = currentLine.Split(',');
                    FullModel item = new FullModel();
                    item.Name = split[1];
                    item.Surname = split[2];
                    item.Birthdate = DateTime.Parse(split[3]);
                    item.Age = Int32.Parse(split[4]);
                    item.Married = Boolean.Parse(split[5]);
                    item.Height = Double.Parse(split[6]);
                    item.Salary = long.Parse(split[7]);
                    item.Abc = (ABC)r.Next(0, 2);

                    if (split[8] == "")
                        item.NullSurname = "";
                    else
                        item.NullSurname = split[8];
                    DateTime outDateTime;
                    DateTime.TryParse(split[9], out outDateTime);
                    item.NullBirthdate = outDateTime;
                    int outInt;
                    Int32.TryParse(split[10], out outInt);
                    item.NullAge = outInt;
                    bool outBool;
                    Boolean.TryParse(split[11], out outBool);
                    item.NullMarried = outBool;
                    double outDouble;
                    Double.TryParse(split[12], out outDouble);
                    item.NullHeight = outDouble;
                    long outLong;
                    long.TryParse(split[13], out outLong);
                    item.NullSalary = outLong;
                    if (r.Next(0, 1) == 0)
                        item.NullAbc = (ABC)r.Next(0, 2);
                    else 
                        item.NullAbc = null;

                    DataList.Add(item);
                }
            }
        }
    }
}