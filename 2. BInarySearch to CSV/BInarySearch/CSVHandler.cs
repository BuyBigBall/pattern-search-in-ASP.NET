using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Text.RegularExpressions;

namespace BInarySearch
{

    
    public class CsvRow : List<string>
    {
        public string LineText { get; set; }
    }

    /// <summary>
    /// Class to write data to a CSV file
    /// </summary>
    public class CsvWriter : StreamWriter
    {
        public const Char chDeliminator = ';';
        public CsvWriter(Stream stream)
            : base(stream)
        {
        }

        public CsvWriter(string filename)
            : base(filename)
        {
        }

        /// <summary>
        /// Writes a single row to a CSV file.
        /// </summary>
        /// <param name="row">The row to be written</param>
        public void WriteRow(CsvRow row)
        {
            StringBuilder builder = new StringBuilder();
            bool firstColumn = true;
            foreach (string value in row)
            {
                // Add separator if this isn't the first value
                if (!firstColumn)
                    builder.Append(chDeliminator);
                // Implement special handling for values that contain comma or quote
                // Enclose in quotes and double up any double quotes
                if (value.IndexOfAny(new char[] { '"', chDeliminator }) != -1)
                    builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                else
                    builder.Append(value);
                firstColumn = false;
            }
            row.LineText = builder.ToString();
            WriteLine(row.LineText);
        }
    }

    /// <summary>
    /// Class to read data from a CSV file
    /// </summary>
    public class CsvReader : StreamReader
    {
        public const Char chDeliminator = ';';
        public CsvReader(Stream stream)
            : base(stream)
        {
        }

        public CsvReader(string filename)
            : base(filename)
        {
        }

        /// <summary>
        /// Reads a row of data from a CSV file
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool ReadRow(CsvRow row)
        {
            row.LineText = ReadLine();
            if (String.IsNullOrEmpty(row.LineText))
                return false;

            int pos = 0;
            int rows = 0;

            while (pos < row.LineText.Length)
            {
                string value;

                // Special handling for quoted field
                if (row.LineText[pos] == '"')
                {
                    // Skip initial quote
                    pos++;

                    // Parse quoted value
                    int start = pos;
                    while (pos < row.LineText.Length)
                    {
                        // Test for quote character
                        if (row.LineText[pos] == '"')
                        {
                            // Found one
                            pos++;

                            // If two quotes together, keep one
                            // Otherwise, indicates end of value
                            if (pos >= row.LineText.Length || row.LineText[pos] != '"')
                            {
                                pos--;
                                break;
                            }
                        }
                        pos++;
                    }
                    value = row.LineText.Substring(start, pos - start);
                    value = value.Replace("\"\"", "\"");
                }
                else
                {
                    // Parse unquoted value
                    int start = pos;
                    while (pos < row.LineText.Length && row.LineText[pos] != chDeliminator)
                        pos++;
                    value = row.LineText.Substring(start, pos - start);
                }

                // Add field to list
                if (rows < row.Count)
                    row[rows] = value;
                else
                    row.Add(value);
                rows++;

                // Eat up to and including next comma
                while (pos < row.LineText.Length && row.LineText[pos] != chDeliminator)
                    pos++;
                if (pos < row.LineText.Length)
                    pos++;
            }
            // Delete any unused items
            while (row.Count > rows)
                row.RemoveAt(rows);

            // Return true if any columns read
            return (row.Count > 0);
        }

        private static byte FromCharacterToByte(char character, int index, int shift = 0)
        {
            byte value = (byte)character;
            if (((0x40 < value) && (0x47 > value)) || ((0x60 < value) && (0x67 > value)))
            {
                if (0x40 == (0x40 & value))
                {
                    if (0x20 == (0x20 & value))
                        value = (byte)(((value + 0xA) - 0x61) << shift);
                    else
                        value = (byte)(((value + 0xA) - 0x41) << shift);
                }
            }
            else if ((0x29 < value) && (0x40 > value))
                value = (byte)((value - 0x30) << shift);
            else
                throw new InvalidOperationException(String.Format("Character '{0}' at index '{1}' is not valid alphanumeric character.", character, index));

            return value;
        }

        public static byte[] ConvertToByteArray(string value)
        {
            byte[] bytes = null;
            if (String.IsNullOrEmpty(value))
                bytes = null;
            else
            {
                int string_length = value.Length;
                int character_index = (value.StartsWith("0x", StringComparison.Ordinal)) ? 2 : 0; // Does the string define leading HEX indicator '0x'. Adjust starting index accordingly.               
                int number_of_characters = string_length - character_index;

                bool add_leading_zero = false;
                if (0 != (number_of_characters % 2))
                {
                    add_leading_zero = true;

                    number_of_characters += 1;  // Leading '0' has been striped from the string presentation.
                }

                bytes = new byte[number_of_characters / 2]; // Initialize our byte array to hold the converted string.

                int write_index = 0;
                if (add_leading_zero)
                {
                    bytes[write_index++] = FromCharacterToByte(value[character_index], character_index);
                    character_index += 1;
                }

                for (int read_index = character_index; read_index < value.Length; read_index += 2)
                {
                    byte upper = FromCharacterToByte(value[read_index], read_index, 4);
                    byte lower = FromCharacterToByte(value[read_index + 1], read_index + 1);

                    bytes[write_index++] = (byte)(upper | lower);
                }
            }

            return bytes;
        }
    }


    public class FileHelperEx
    {
        public int BufferSize = 1024 * 1024;
        public void ReadStream(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                ProcessRead(stream);
            }
        }
        public void WriteStream(string path, Action<byte[]> action)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, true))
            {
                ProcessStream(stream, action);
            }
        }
        public void Copy(string InPath, string OutPath)
        {
            using (var ins = new FileStream(InPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                using (var ops = new FileStream(OutPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                {
                    CopyStream(ins, ops);
                }
            }
        }

        public void ProcessStream(Stream stream, Action<byte[]> action)
        {
            var DataQueue = new ConcurrentQueue<byte[]>();
            var DataReady = new AutoResetEvent(false);
            var DataProcessed = new AutoResetEvent(false);
            var ReadDataTask = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    byte[] Data = new byte[BufferSize];
                    var BytesRead = stream.Read(Data, 0, Data.Length);
                    //if (BytesRead != BufferSize)
                    //    Data = Data.SubArray(0, BytesRead);
                    DataQueue.Enqueue(Data);
                    DataReady.Set();
                    if (BytesRead != BufferSize)
                        break;
                    DataProcessed.WaitOne();
                }
            });
            var ProcessDataTask = Task.Factory.StartNew(() =>
            {
                byte[] Data;
                do
                {
                    DataReady.WaitOne();
                    DataQueue.TryDequeue(out Data);
                    DataProcessed.Set();
                    action(Data);
                    if (Data.Length != BufferSize)
                        break;
                } while (Data.Length == BufferSize);
            });
            ReadDataTask.Wait();
            ProcessDataTask.Wait();
        }
        public void CopyStream(Stream InStream, Stream OutStream)
        {
            var DataQueue = new ConcurrentQueue<byte[]>();
            var DataReady = new AutoResetEvent(false);
            var DataProcessed = new AutoResetEvent(false);
            var ReadDataTask = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var Data = new byte[BufferSize];
                    var BytesRead = InStream.Read(Data, 0, Data.Length);
                    //if (BytesRead != BufferSize)
                    //    Data = Data.SubArray(0, BytesRead);
                    DataQueue.Enqueue(Data);
                    DataReady.Set();
                    if (BytesRead != BufferSize)
                        break;
                    DataProcessed.WaitOne();
                }
            });
            var ProcessDataTask = Task.Factory.StartNew(() =>
            {
                byte[] Data;
                do
                {
                    DataReady.WaitOne();
                    DataQueue.TryDequeue(out Data);
                    DataProcessed.Set();
                    OutStream.Write(Data, 0, Data.Length);
                    if (Data.Length != BufferSize)
                        break;
                } while (Data.Length == BufferSize);
            });
            ReadDataTask.Wait();
            ProcessDataTask.Wait();
        }

        public void ProcessRead(Stream stream)
        {
            System.Collections.Generic.Dictionary<string , string> dic = new System.Collections.Generic.Dictionary<string, string>();
            bool ret = false;
            CsvRow row = new CsvRow();
            CsvReader csvStream = new CsvReader(stream);
            while ((ret = csvStream.ReadRow(row)))
            {
                if (!Regex.IsMatch(row[0], @"^\d+$")) continue;

                int iNo = Int16.Parse(row[0]);
                string strPosition = row[1];
                string strData = row[2].Replace(" ", string.Empty );
                byte[] btArray = CsvReader.ConvertToByteArray(strData);
                System.Diagnostics.Debug.WriteLine(btArray.ToString());
            }
                
            System.Diagnostics.Debug.WriteLine(row.ToString());

        }

    }
}
