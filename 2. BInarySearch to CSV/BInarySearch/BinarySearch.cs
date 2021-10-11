using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BInarySearch
{
    public class BinarySearch
    {
        private int[] _jumpTable;
        private byte[] _pattern;
        private int _patternLength;
        public BinarySearch()
        {
        }
        public BinarySearch(byte[] pattern)
        {
            _pattern = pattern;
            _jumpTable = new int[256];
            _patternLength = _pattern.Length;
            for (var index = 0; index < 256; index++)
                _jumpTable[index] = _patternLength;
            for (var index = 0; index < _patternLength - 1; index++)
                _jumpTable[_pattern[index]] = _patternLength - index - 1;
        }
        public void SetPattern(byte[] pattern)
        {
            _pattern = pattern;
            _jumpTable = new int[256];
            _patternLength = _pattern.Length;
            for (var index = 0; index < 256; index++)
                _jumpTable[index] = _patternLength;
            for (var index = 0; index < _patternLength - 1; index++)
                _jumpTable[_pattern[index]] = _patternLength - index - 1;
        }
        public unsafe int Search(byte[] searchArray, int startIndex = 0)
        {
            if (_pattern == null)
                throw new Exception("Pattern has not been set.");
            if (_patternLength > searchArray.Length)
                throw new Exception("Search Pattern length exceeds search array length.");
            var index = startIndex;
            var limit = searchArray.Length - _patternLength;
            var patternLengthMinusOne = _patternLength - 1;
            fixed (byte* pointerToByteArray = searchArray)
            {
                var pointerToByteArrayStartingIndex = pointerToByteArray + startIndex;
                fixed (byte* pointerToPattern = _pattern)
                {
                    while (index <= limit)
                    {
                        var j = patternLengthMinusOne;
                        while (
                            j >= 0 && pointerToPattern[j] == pointerToByteArrayStartingIndex[index + j])
                            j--;
                        if (j < 0)
                            return index;
                        index += Math.Max(_jumpTable[pointerToByteArrayStartingIndex[index + j]] - _patternLength + 1 + j, 1);
                    }
                }
            }
            return -1;
        }
        public static unsafe FindPosition[] SearchPattern(List<int> searchPattern, byte[] searchArray, int x86 = 1)
        {
            int startIndex = 0;
            var index = startIndex;
            var limit = (searchArray.Length  - searchPattern.Count* x86);
            var patternLengthMinusOne = searchPattern.Count - 1;
            var list = new List<int>();


            List<FindPosition> retFindResults = new List<FindPosition>();
            List<int> retList = new List<int>();


            fixed (byte* pointerToByteArray = searchArray)
            {
                void* pointerToSearchArray;
                if (x86 == 1)
                    pointerToSearchArray = (byte*)pointerToByteArray;
                else
                    pointerToSearchArray = (char*)pointerToByteArray;
                while (index * x86 <= limit)
                {
                    List<int> newData = new List<int>();
                    var j = index;
                    bool bFound = false;
                    while (true)
                    {
                        if (j * x86 >= searchArray.Length)
                        {
                            break;
                        }
                        int val = -1;
                        
                        if (x86 == 1)
                            val = ((byte*)pointerToSearchArray)[j];
                        else
                        {
                            // ######## endian exchange 
                            val = ((char*)pointerToSearchArray)[j];
                            byte[] bytes = BitConverter.GetBytes(val);
                            byte[] new_bytes = new byte[2];
                            new_bytes[0] = bytes[1];
                            new_bytes[1] = bytes[0];
                            val = BitConverter.ToUInt16(new_bytes);
                            // <---------------#
                        }

                        if (!newData.Contains(val))
                        {
                            newData.Add(val);

                            // this is search end and no match start
                            if (newData.Count > searchPattern.Count)
                                break;
                        }
                        
                        int[] tmpArray = new int[newData.Count];
                        searchPattern.CopyTo(0, tmpArray, 0, newData.Count);
                        bool bEqual = true; int idx = 0;
                        foreach(int tmp in tmpArray)
                        {
                            if(tmp!= newData[idx++])
                            {
                                bEqual = false;
                                break;
                            }
                        }
                        if (!bEqual)
                        {
                            index += (retList.Count);
                            retList = new List<int>();
                            break;
                        }
                        else
                        {
                            retList.Add(val);
                            //if (retList.Count > 10)
                            //{
                            //    System.Diagnostics.Debug.WriteLine(val);
                            //}
                            if (searchPattern.Count== newData.Count)
                            {
                                //retList = new List<int>();
                                //for (int i = index; i < j; i++)
                                //{
                                //    int value = -1;
                                //    if (x86 == 1)
                                //        value = (int)(((byte*)pointerToSearchArray)[i]);
                                //    else
                                //        value = (int)(((short*)pointerToSearchArray)[i]);
                                //    retList.Add(value);
                                //}
                                if (retList.Count > 0)
                                {
                                    FindPosition FindPosResult = new FindPosition();
                                    FindPosResult.no = retFindResults.Count;
                                    FindPosResult.byteLen = x86;
                                    FindPosResult.position = index * x86;
                                    FindPosResult.data = retList;
                                    retFindResults.Add(FindPosResult);
                                }
                                index += (retList.Count); index--;
                                bFound = true;
                            }
                        }
                        //if (newData.Count >= searchPattern.Count) 
                        //    break;
                        if (bFound) 
                            break;
                        j++;

                    }
                    index++;
                    //if(index>934000) 
                    //    System.Diagnostics.Debug.Write(index + ",");
                }
                //System.Diagnostics.Debug.WriteLine(",");
            }

            FindPosition[] ret = retFindResults.ToArray();
            return ret;
        }
        public unsafe List<int> SearchAll(byte[] searchArray, int startIndex = 0)
        {
            var index = startIndex;
            var limit = searchArray.Length - _patternLength;
            var patternLengthMinusOne = _patternLength - 1;
            var list = new List<int>();
            fixed (byte* pointerToByteArray = searchArray)
            {
                var pointerToByteArrayStartingIndex = pointerToByteArray + startIndex;
                fixed (byte* pointerToPattern = _pattern)
                {
                    while (index <= limit)
                    {
                        var j = patternLengthMinusOne;
                        while (j >= 0 && pointerToPattern[j] == pointerToByteArrayStartingIndex[index + j])
                            j--;
                        if (j < 0)
                            list.Add(index);
                        index += Math.Max(_jumpTable[pointerToByteArrayStartingIndex[index + j]] - _patternLength + 1 + j, 1);
                    }
                }
            }
            return list;
        }
        public int SuperSearch(byte[] searchArray, int nth, int start = 0)
        {
            var e = start;
            var c = 0;
            do
            {
                e = Search(searchArray, e);
                if (e == -1)
                    return -1;
                c++;
                e++;
            } while (c < nth);
            return e - 1;
        }
    }
}
