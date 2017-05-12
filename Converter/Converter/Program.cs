using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Converter
{
    /// <summary>
    /// Class to hold information on individual conversions
    /// </summary>
    class Conversion
    {
        public Conversion Next { get; set; }   //Creates a pathway between each conversion
        public string From { get; set; }       //Input unit
        public string To { get; set; }         //Output unit
        public double Multiplier { get; set; } //Multiplier to convert input to output
    }//end Conversion class

    /// <summary>
    /// List class to make it easy to cycle through conversions
    /// </summary>
    class List
    {
        public Conversion firstEntry { get; set; } //Start of list
    }//end List class

    class Program
    {
        /// <summary>
        /// Loads a file containing conversions and allows user to convert between various units
        /// </summary>
        static void Main( string[] args )
        {
            char[] delimiter = { ',', ' ' };        //The characters required for formatting the conversions
            string fileName = "convert.txt";        //The name of the conversion file. Store in same folder as .exe and no filepath is required.
            bool contConverting = true;             //Whether the user wants to use the program more
            List conversionRates = new List();      //The various conversions in the file
            string[] testInputs = { "0,ounce,gram", "-1,pound,kilogram", " 4 , p I N t, l iT re ", "inch,1,mile",
                                      "pound,ounce , 1", "1,ounce,mile", "1,0unce,pound" }; //Various inputs that may throw errors on unchecked systems - feel free to add more here

            fileName = CheckDirectory( fileName ) + fileName;                            //Adds any directory specified by the user before the filename
            
            contConverting = LoadConversionFile( fileName, conversionRates, delimiter ); //Populate the conversion list on startup, if it fails signal the program to close

            BasicTesting( conversionRates ); //Test all conversions in list

            for (int i = 0; i < testInputs.Length ; i++ )           //Tests various error prevention features
            {
                AdvancedTesting( conversionRates, delimiter, testInputs[i] );
            }

            while ( contConverting == true )
            {
                RunProgram( conversionRates, delimiter );        //Run conversion
                contConverting = CheckContinue();              //Asks the user whether they wish to continue using the program
            }

            Output( "Press any key to exit..." );
            Console.ReadKey();
        }

        static string CheckDirectory( string fileName )
        {
            string input;       //The user's response
            Output( "Would you like to specify a different directory for the conversion text file? Y/N" );

            for ( ; ; )          //Infinite loop to ensure that the user answers yes or no
            {
                input = Console.ReadLine();
                if ( input == "Y" || input == "y" ) //Return the directory or nothing to indicate default path is acceptable
                {
                    Output( "Please input the directory (e.g. C:\\SOFT153\\Converter\\)" );
                    return Console.ReadLine();
                }
                else if ( input == "N" || input == "n" )
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Calls various methods to convert user's input into requested output
        /// </summary>
        /// <param name="conversionRates">List of conversion rates on the file</param>
        static void RunProgram( List conversionRates, char[] delim )
        {
            string input;                            //The conversion requested by the user in its raw form
            string[] userConversion = new string[3]; //The user's conversion separated out into it's 3 parts (Multiplier, From and To)
            string answer;                           //The answer to the user's request

            input = UserInput();                            //Take user input
            userConversion = input.Split( delim[0] );       //Convert it to a format that matches the format in the conversion list

            userConversion[0] = RemoveSpaces (userConversion, delim[1], 0 );

            if ( EnsureNumeric( userConversion[0] ) )
            {
                for ( int i = 1; i < userConversion.Length; i++ ) //Removes any spaces mid-word
                {
                    userConversion[i] = RemoveSpaces( userConversion, delim[1], i );
                }

                try
                {
                    userConversion[1] = LowerCase( userConversion[1] ); //Convert each word
                    userConversion[2] = LowerCase( userConversion[2] ); //To lower case
                }
                catch ( IndexOutOfRangeException ex )  //Ensures that the array being too small (if the user doesn't use enough commas) doesn't crash the program
                {
                    Output( "Error! Please ensure that you input in the correct format." );
                }

                answer = Convert.ToString( ConvertUnit( userConversion, conversionRates ) ); //Stores the answer
                PickOutput( answer, userConversion ); //Chooses the relevant message to output to the user
            }
            else if ( userConversion[1] == null || userConversion[2] == null )
            {
                Output( "Please check the format of your input and try again." );
            }
            else
            {
                Output( "Please check 0-9 and \".\" characters only before first comma and that it is greater than zero" );
            }
        }

        /// <summary>
        /// Loads conversion rules from a text file
        /// </summary>
        /// <param name="locFile">The file path & name of the conversion rules text file</param>
        /// <param name="conversionRates">List to populate with various conversion rates stored in a text file</param>
        static bool LoadConversionFile( string locFile, List conversionRates, char[] delim )
        {
            string lineFromFile; //The line read from the text file
            string[] separated;  //The line broken down into it's 3 parts
            Conversion conv;     //It is then stored as a conversion

            try
            {
                using ( StreamReader reader = new StreamReader( locFile ) ) //Reads through the file line by line
                    while ( !reader.EndOfStream )                         //Until it runs out of lines to read
                    {
                        lineFromFile = reader.ReadLine();               //Copy the line to a variable
                        if ( lineFromFile != "" )                         //And if it isn't empty
                        {
                            conv = new Conversion();                    //Create a new conversion
                            separated = lineFromFile.Split( delim[0] );    //Separate out the parts of the conversion

                            for ( int i = 0; i < separated.Length; i++ )  //Remove any blank spaces
                            {
                                separated[i] = RemoveSpaces( separated, delim[1], i );
                            }

                            if ( EnsureNumeric( separated[2] ) ) //If multiplier is numeric then continue
                            {
                                conv.From = LowerCase(separated[0]);                //Convert the words to lower case
                                conv.To = LowerCase(separated[1]);                  //And store them in their relevant variable
                                conv.Multiplier = Convert.ToDouble(separated[2]);   //Along with the multiplier to convert from one to the other

                                AddToBeginning(conversionRates, conv);              //Then add the conversion to the beginning of the conversion list
                            }
                            else
                            {
                                Output( separated[0] +  " to " + separated[1] + " conversion cannot be added because of invalid characters in the multiplier part of the file." );
                            }
                        }
                    }

                //I did look into the possibility of coding if a->b, and b->c both work then surely a->c can be found.
                //However, whilst knowing the conversions in the text file and making sure they look for the right combinations is easy...
                //If looking for every single combination of conversions and checking whether they are a covertible combination
                //(which would need to be the case if the user could add their own conversions to the text file later) then you quickly end up
                //with a problem similar to the travelling salesperson problem. You must compare (n-1)! lines in the text file to find
                //any relational conversions then create these as conversions in their own right, then compare these with all the lines in the
                //original list except for the two that made them. You must also be very careful to not allow any duplicates - very easy to
                //create infinite loops.

                //Here is a part of what was tried so you can see the approach that I looked at:

            //      Conversion AtoC;

            //      conv = conversionRates.firstEntry;              //from start of list

            //      while (conv != null && conv.Next != null)       //whilst there are entries to compare
            //      {
            //          AtoC = new Conversion();
            //          if (conv.To == conv.Next.From)              //If 2 of the units are the same then converting between the other 2 is possible
            //          {
            //              AtoC.From = conv.From;
            //              AtoC.To = conv.Next.To;
            //              AtoC.Multiplier = conv.Multiplier * conv.Next.Multiplier;
            //              AddToBeginning(conversionRates, AtoC);                       //Add it to the conversion list and check the next adjacent pair
            //          }
            //          conv = conv.Next;
            //      }

                return true;
            }
            catch (FileNotFoundException ex) //Prevents a crash if the conversion file is missing and also warns the user
            {
                Output("Conversion file not found, please check the directory and try again.");
                return false;
            }
        }

        /// <summary>
        /// Removes spaces from the text (whether at the end of or in the middle of words)
        /// </summary>
        /// <param name="lineToSplit">The line of text to be split</param>
        /// <param name="delimiter">The character to know where to split the text</param>
        /// <param name="i">The number in the array to currently work on</param>
        /// <returns>Returns the part of the conversion without spaces</returns>
        static string RemoveSpaces( string[] lineToSplit, char delimiter, int i )
        {
            string[] removeSpaces = lineToSplit[i].Split( delimiter ); //Picks out the word/number to work on and splits it into an array
            string merged = "";                                      //Rebuilt word

            for ( int j = 0; j < removeSpaces.Length; j++ ) //Cycle through the array
            {
                if ( removeSpaces[j] != null ) //If the array part isn't empty
                {
                    if ( removeSpaces[j] != " " ) //And isn't a space
                    {
                        merged += removeSpaces[j]; //Add it to the rebuilt word
                    }
                }
            }
            return merged; //And send it back to be stored
        }

        /// <summary>
        /// Adds a conversion to the beginning the list of conversion
        /// </summary>
        /// <param name="convList">The list of conversions</param>
        /// <param name="newConv">The conversion to be added</param>
        static void AddToBeginning( List convList, Conversion newConv )
        {
            newConv.Next = convList.firstEntry; //Makes next point to the old first entry
            convList.firstEntry = newConv;      //And makes the new entry the new first entry
        }

        /// <summary>
        /// Convert all characters to lower case
        /// </summary>
        /// <param name="stringToLower">The string to be converted to lower case</param>
        static string LowerCase( string stringToLower )
        {
            string lowered = "";        //Rebuilt string in lower case
            int toLower;                //Unicode/ASCII value of char
            char letter;                //Character extracted from string

            for ( int i = 0; i < stringToLower.Length; i++ ) 
            {
                letter = Convert.ToChar( stringToLower[i] );    //String letter extracted as single character
                toLower = Convert.ToInt32( letter );            //Converted to Unicode/ASCII numerical value

                if ( toLower >= 65 && toLower <= 90 )           //If UPPERCASE add 32 to convert to lowercase
                {
                    toLower += 32; 
                    letter = Convert.ToChar( toLower ); 
                }
                else if ( toLower >= 48 && toLower <= 57 )        //If numeric character warn the user that the conversion won't work
                {
                    Output( "Numeric character detected in word - please check conversion file and your own input for errors." );
                }

                lowered += Convert.ToString( letter );          //rebuild string of lower case letters
            }

            return lowered; 
        }

        /// <summary>
        /// Ensures that every character in the string is 0-9 or .
        /// </summary>
        /// <param name="checkNumeric">number input by user/in text file</param>
        /// <returns>True for numeric, false for invalid character</returns>
        static bool EnsureNumeric( string checkNumeric )
        {
            char digit;     //Character extracted from string
            int unicode;    //Unicode/ASCII value of character

            if (checkNumeric == "0")
            {
                return false;
            }

            for (int i = 0; i < checkNumeric.Length; i++)
            {

                digit = Convert.ToChar( checkNumeric[i] );  //String digit extracted as single character
                unicode = Convert.ToInt32( digit );         //Converted to Unicode/ASCII numerical value

                if ( ( digit != 46 && digit < 48 ) || digit > 57 )           //If character isn't 0-9 or . then stop converting & warn the user
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Asks the User what number of one unit they'd like converted into a different unit
        /// </summary>
        /// <returns>User request</returns>
        static string UserInput()
        {
            Output( "Please input an amount and units to convert from and to (i.e. \"1,ounce,gram\" = 1 ounce in grams)" );
            return Console.ReadLine();
        }

        /// <summary>
        /// Convert from unit x to unit y
        /// </summary>
        /// <param name="userConversion">The conversion that the user has requested</param>
        /// <param name="convList">The list of conversions loaded from the text file</param>
        static double ConvertUnit( string[] userConversion, List convList )
        {
            Conversion conv = convList.firstEntry; //Start at the beginning of the list

            while ( conv != null ) //If the current conversion isn't empty
            {
                if ( userConversion[1] == conv.From ) //Check if the convert from matches the user's request
                {
                    if ( userConversion[2] == conv.To ) //Check if the convert to matches the user's request
                    {
                        return ( Convert.ToDouble( userConversion[0] ) * Convert.ToDouble( conv.Multiplier ) ); //Carry out the conversion
                    }
                }
                else if ( userConversion[1] == conv.To ) //Check if the convert to matches the user's convert from
                {
                    if ( userConversion[2] == conv.From ) //Check if the convert from matches the user's convert to
                    {
                        return ( Convert.ToDouble( userConversion[0] ) / Convert.ToDouble( conv.Multiplier ) ); //Carry out the conversion
                    }
                }
                conv = conv.Next; //Move to the next conversion to check that
            }
            return -1; //Returns an impossible value to signal that the conversion has not been found
        }

        /// <summary>
        /// Asks the user if they wish to continue converting
        /// </summary>
        /// <returns>Their response</returns>
        static bool CheckContinue()
        {
            string answer; //The user's response
            Output( "Convert another measurement? Y/N" );

            for ( ; ; ) //Infinite loop to ensure that the user answers yes or no
            {
                answer = Console.ReadLine(); //Read what the user has typed
                if ( answer == "y" || answer == "Y" ) //If it's yes return true
                {
                    return true;
                }
                else if ( answer == "n" || answer == "N" ) //If it's no return false
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Picks whether to output the completed or conversion or whether to tell the user that the conversion wasn't possible
        /// </summary>
        /// <param name="answer">The conversion answer</param>
        /// <param name="userConv">The user's request</param>
        static void PickOutput( string answer, string[] userConv )
        {
            if ( answer == "-1" )//If an impossible value has been sent tell the user that there has been an issue
            {
                Output( "The specified conversion is either missing from the file or impossible (check unit types or update the file)." );
            }
            else //Otherwise tell them the answer
            {
                Output( userConv, answer );
            }
        }

        /// <summary>
        /// Output "a of x is the same as b of y"
        /// </summary>
        /// <param name="input">The user's request</param>
        /// <param name="answer">The value after the conversion</param>
        static void Output( string[] input, string answer )
        {
            Console.WriteLine();
            Console.WriteLine( input[0] + " " + input[1] + "s converts to " + answer + " " + input[2] + "s." );
        }

        /// <summary>
        /// Used to output various messages to the user
        /// </summary>
        /// <param name="message">The message</param>
        static void Output( string message )
        {
            Console.WriteLine( message );
        }

        /// <summary>
        /// Tests each conversion with random numbers
        /// </summary>
        /// <param name="conversionRates">The conversions loaded from the file</param>
        static void BasicTesting( List conversionRates )
        {
            Conversion testConv = conversionRates.firstEntry;
            string[] testUser = new string[3];
            string answer;

            Output( "Basic testing..." );

            while ( testConv != null ) //Cycles through all entities in list and converts each way
            {
                testUser[0] = "5";
                testUser[1] = testConv.From;
                testUser[2] = testConv.To;

                answer = Convert.ToString( ConvertUnit( testUser, conversionRates ) );

                PickOutput( answer, testUser );

                testUser[0] = "10";
                testUser[1] = testConv.To;
                testUser[2] = testConv.From;

                answer = Convert.ToString( ConvertUnit( testUser, conversionRates ) );

                PickOutput( answer, testUser );

                testConv = testConv.Next;
            }

            Output( "End of basic testing" );
        }

        /// <summary>
        /// Basic conversion subroutine but with predefined input
        /// </summary>
        /// <param name="conversionRates">list of conversion rates in text file</param>
        /// <param name="delimiter">The character to know where to split the text</param>
        /// <param name="input">pre-defined input (replaces user input)</param>
        static void AdvancedTesting( List conversionRates, char[] delim, string input )
        {
            string[] userConversion = new string[3]; //The user's conversion separated out into it's 3 parts (Multiplier, From and To)
            string answer;                           //The answer to the user's request

            userConversion = input.Split( delim[0] );       //Convert it to a format that matches the format in the conversion list

            userConversion[0] = RemoveSpaces( userConversion, delim[1], 0 );

            if ( EnsureNumeric( userConversion[0] ) )
            {
                for ( int i = 1; i < userConversion.Length; i++ ) //Removes any spaces mid-word
                {
                    userConversion[i] = RemoveSpaces( userConversion, delim[1], i );
                }

                try
                {
                    userConversion[1] = LowerCase( userConversion[1] ); //Convert each word
                    userConversion[2] = LowerCase( userConversion[2] ); //To lower case
                }
                catch (IndexOutOfRangeException ex)  //Ensures that the array being too small (if the user doesn't use enough commas) doesn't crash the program
                {
                    Output( "Error! Please ensure that you input in the correct format." );
                }

                answer = Convert.ToString( ConvertUnit( userConversion, conversionRates ) ); //Stores the answer
                PickOutput( answer, userConversion ); //Chooses the relevant message to output to the user
            }
            else if ( userConversion[1] == null || userConversion[2] == null )
            {
                Output( "Please check the format of your input and try again." );
            }
            else
            {
                Output( "Please check 0-9 and \".\" characters only before first comma and that it is greater than zero" );
            }
        }
    }//end class
}//end namespace