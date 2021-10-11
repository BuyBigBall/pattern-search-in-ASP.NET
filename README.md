# pattern-search-in-ASP.NET

the zip file contains binary files with it definition
those examples are just some samples, we have a lot more.
About our architecture:
We have a Server which is sending data via json to a client application
What we need:
We need 2 applications (designed as library) written in C# .NetCore 3.1 / .Net5.0 (you can choose)
Application 1: Extract Data from delivered CSV file and make json file 
Application 2: Take one Binary File and the json file, and calculate the position and length of extracted data in binary file

So you have 3 entries here not that much you have
That dict will contain the csv data
  Name = dict key
  Data = dict list values (splitted by ‚ ‚)
  Make a dictionary<string,List<int>>

So all sample csv files will be there
And your dict will have 3 lists of its values binded to the column „name“
So list values are each single value of the data
Then we have the problem that 1 and 2 has integer values, 3 has bytes. We have to save that for searching it later
Don‘t add any value of that data to list if already existing
(List.Contains)
Thats the algo of how to import that csv
Maybe you need to make class or struct for that, thats up to you how to do
So we have column name
If we search bytes or integers
And a list/array of that values

Now about detecting
Go through whole file and check if you have a sequence of exactly that lists until it doesn‘t match it
Now we have to declare if multiple findings are allowed
As 3 is allowed to, i would write that fix to the json file and 1 and 2 is not
But allowing multiple times the 0000

Algo Application 2:
Read the JSON-File and the Binary File
Search inside the Binary File for the Data you saved in Application 1 (JSON-File) to find the position and length
(like you got from example for each "Name")
You should declare a minimum amount of int matches and need to check if it´s allowed to have same value multiple times (declared in Application 1)
Output should be CSV File exactly like inside the example
Performance of that application is very important and needs to be fast (time <1s)

