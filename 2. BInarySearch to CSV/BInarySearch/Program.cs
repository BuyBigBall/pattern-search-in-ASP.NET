using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BInarySearch
{
    struct CsvPattern
    {
        public string name;
        public int byteLen;
        public List<int> data;
    }
    public struct FindPosition
    {
        public int no;
        public int position;
        public int byteLen;
        public List<int> data;
    }
    class Program
    {
        const int MAX_BINARY_FILESIZE = 1024 * 1024 * 10;
        const int MAX_PATTERN_BYTELENGTH = 1024 * 100;
        const int MAX_FILE_COUNT = 1024 * 100;
        const string DATA_DIRECTORY = @"../../../../SampleData";
        const string JSON_OUTPUT_FILE = @"../../../../Sample.json";

        public static List<CsvPattern> patternList = new List<CsvPattern>();
        //public static List<CsvPattern> pattern = new List<CsvPattern>();
        public static Dictionary<string, CsvPattern>  pattern = new Dictionary<string, CsvPattern>();
        //public static Dictionary<string, FindPosition> ResultDictionary = new Dictionary<string, FindPosition>();
        static CsvPattern getPatternObject(CsvRow csvRow)
        {
            int iNo = Int16.Parse(csvRow[0]);
            string strPosition = csvRow[1];
            string[] strData = csvRow[2].Split(" ", 0);
            //byte[] btArray = CsvReader.ConvertToByteArray(strData);
            CsvPattern onePattern = new CsvPattern();
            onePattern.name = iNo + "_" + strPosition;
            onePattern.byteLen = strData.Length;
            //onePattern.data = strData;
            return onePattern;
        }

        public static void ProcessCsvDirectory(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessCsvFile(fileName);

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessCsvDirectory(subdirectory);
        }
        public static void ProcessCsvFile(string path)
        {
            int len = path.Length;
            if (path.Substring(len - 4).ToLower() == ".csv")
            {
                String cvsFilePath = path;
                var streamCSV = new FileStream(cvsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
                bool ret = false;
                CsvRow row = new CsvRow();
                CsvReader csvStream = new CsvReader(streamCSV);
                while ((ret = csvStream.ReadRow(row)))
                {
                    if (!Regex.IsMatch(row[0], @"^\d+$")) continue;

                    string Key = row[0] + "_" + row[1];
                    // Add data
                    //if (pattern.ContainsKey(Key))
                    //{
                    //    // Key already 
                    //    CsvPattern tmpPattern = pattern.GetValueOrDefault(row[0]);
                    //    List<int> newData = new List<int>();
                    //    bool x86 = false;
                    //    foreach (var val in row[2].Split(' '))
                    //    {
                    //        int intValue = int.Parse(val, System.Globalization.NumberStyles.HexNumber);
                    //        if (!tmpPattern.data.Contains(intValue)) tmpPattern.data.Add(intValue);
                    //        if(intValue > byte.MaxValue) x86 = true;
                    //    }
                    //    tmpPattern.byteLen = x86 || tmpPattern.byteLen==2 ? 2 : 1;

                    //    pattern.Remove(row[0]);
                    //    pattern.Add(row[0], tmpPattern);
                    //}


                    bool x86 = false;
                    bool isExist = false;
                    List<int> newData = new List<int>();
                    foreach (string val in row[2].Split(' '))
                    {
                        int intValue = int.Parse(val, System.Globalization.NumberStyles.HexNumber);
                        if (intValue > byte.MaxValue) x86 = true;
                        if (!newData.Contains(intValue))
                        {
                            newData.Add(intValue);
                        }
                        bool isEqual = false;
                        foreach (CsvPattern tmpPattern in pattern.Values)
                        {
                            if (tmpPattern.data.Equals ( newData))
                            {
                                isEqual = true;
                                break;
                            }
                            
                        }
                        if(isEqual)
                        {
                            isExist = true;
                            break;
                        }
                    }
                    
                    if(!isExist)
                    {
                        CsvPattern tmpPattern = new CsvPattern();
                        tmpPattern.name = row[1];
                        tmpPattern.data = newData;
                        tmpPattern.byteLen = x86 ? 2 : 1;
                        pattern.Add(Key, tmpPattern);
                    }
                }
            }
        }

        public static void ProcessBinaryDirectory(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessBinaryFile(fileName);

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessBinaryDirectory(subdirectory);
        }
        public static void ProcessBinaryFile(string path)
        {
            int len = path.Length;
            if (path.Substring(len - 4).ToLower() == ".bin")
            {
                byte[] baBinaryData = null;
                String binaryFilePath = path;
                var stream = new FileStream(binaryFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
                baBinaryData = new byte[stream.Length];
                stream.BeginRead(baBinaryData, 0, baBinaryData.Length, null, null);
                stream.Close();

                var stream_w = new FileStream(binaryFilePath.Replace(".bin", "_result.csv"), FileMode.Create, FileAccess.Write, FileShare.Write, 4096, true);
                CsvWriter csvWriteHandler = new CsvWriter(stream_w);
                CsvRow row = new CsvRow();
                row.Add("Name");                row.Add("Position");                row.Add("Data"); csvWriteHandler.WriteRow(row);

                int startIndex = 0;
                BinarySearch searchObj = new BinarySearch();
                foreach (var a_pattern in pattern)
                {
                    //string strSearchValues = "";
                    //foreach (var iTemp in a_pattern.Value.data)
                    //{
                    //    if(a_pattern.Value.byteLen==2)
                    //        strSearchValues += iTemp.ToString("X4");
                    //    else
                    //        strSearchValues += iTemp.ToString("X2");
                    //}
                    //byte[] baSearchPattern = CsvReader.ConvertToByteArray(strSearchValues);
                    //searchObj.SetPattern(baSearchPattern);
                    //List<int> iIndexArray = searchObj.SearchAll(baBinaryData, startIndex);

                    //if (iIndexArray.Count > 0)
                    //{
                    //    for (int i = 0; i < iIndexArray.Count; i++)
                    //    {
                    //        FindPosition result = new FindPosition();
                    //        result.no = i + 1;
                    //        result.byteLen = a_pattern.Value.byteLen;
                    //        result.data = iIndexArray;
                    //        result.position = iIndexArray[i];

                    //        row = new CsvRow();
                    //        row.Add(result.no.ToString());
                    //        row.Add(result.position.ToString("X8"));
                    //        if (a_pattern.Value.byteLen == 2)
                    //            row.Add(String.Join(" ", new List<int>(result.data).ConvertAll(i => i.ToString("X4")).ToArray()));
                    //        else
                    //            row.Add(String.Join(" ", new List<int>(result.data).ConvertAll(i => i.ToString("X2")).ToArray()));

                    //        csvWriteHandler.WriteRow(row);
                    //    }
                    //}

                    FindPosition[] results = BinarySearch.SearchPattern(a_pattern.Value.data, baBinaryData, a_pattern.Value.byteLen);
                    
                    for (int i = 0; i < results.Length; i++)
                    {
                        row = new CsvRow();
                        FindPosition result = results[i];
                        row.Add(result.no.ToString());
                        row.Add(result.position.ToString("X8"));
                        if (a_pattern.Value.byteLen == 2)
                            row.Add(String.Join(" ", new List<int>(result.data).ConvertAll(i => i.ToString("X4")).ToArray()));
                        else
                            row.Add(String.Join(" ", new List<int>(result.data).ConvertAll(i => i.ToString("X2")).ToArray()));

                        csvWriteHandler.WriteRow(row);
                    }
                    //System.Diagnostics.Debug.WriteLine(strSearchValues);
                }

                System.Diagnostics.Debug.WriteLine(path);
                csvWriteHandler.Close();
            }
        }

        static void Main(string[] args)
        {
            //#region For Csv File Processing and get Find Patterns.
            //File.Delete(JSON_OUTPUT_FILE);
            //// get Find Patterns
            //ProcessCsvDirectory(DATA_DIRECTORY);
            //System.Diagnostics.Debug.WriteLine(pattern);

            //// save Find Patterns to JSON
            //string JSONresult = Newtonsoft.Json.JsonConvert.SerializeObject(pattern.Values);
            //using (var tw = new StreamWriter(JSON_OUTPUT_FILE, true))
            //{
            //    tw.WriteLine(JSONresult.ToString());
            //    tw.Close();
            //}
            //#endregion

            //return;

            #region For getting Patterns from JSON file.
            if (File.Exists(JSON_OUTPUT_FILE))
            {
                pattern = new Dictionary<string, CsvPattern>();
                StreamReader r = new StreamReader(JSON_OUTPUT_FILE);
                while (!r.EndOfStream)
                {
                    int i = 1;
                    string jsonString = r.ReadLine();
                    List<CsvPattern> temp = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CsvPattern>>(jsonString);
                    foreach (CsvPattern a_pattern in temp)
                    {
                        pattern.Add((i++).ToString(), a_pattern);
                    }
                    int patternCount = pattern.Count;
                    //System.Diagnostics.Debug.WriteLine(patternCount);
                }
            }
            else
            {
                Console.WriteLine("Json files no existance");
                return;
            }

            ProcessBinaryDirectory(DATA_DIRECTORY);
            #endregion
        }

    }
}
