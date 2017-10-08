/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bam.Net;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.Data.Sql;
using System.Data.SqlClient;
using Bam.Net.Testing;
using System.IO;
using Bam.Net.Dust;
using Bam.Net.CommandLine;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.ComponentModel;
using Bam.Net;
using Bam.Net.Testing.Unit;

namespace CommandLineTests
{
    public class TestProgram : CommandLineTestInterface
    {
        // Add optional code here to be run before initialization/argument parsing.
        public static void PreInit()
        {
            #region expand for PreInit help
            // To accept custom command line arguments you may use            
            /*
             * AddValidArgument(string argumentName, bool allowNull)
            */

            // All arguments are assumed to be name value pairs in the format
            // /name:value unless allowNull is true.

            // to access arguments and values you may use the protected member
            // arguments. Example:

            /*
             * arguments.Contains(argName); // returns true if the specified argument name was passed in on the command line
             * arguments[argName]; // returns the specified value associated with the named argument
             */

            // the arguments protected member is not available in PreInit() (this method)
            #endregion
			AddValidArgument("t", true, description: "run all tests");
			DefaultMethod = typeof(TestProgram).GetMethod("Start");
		}

		public static void Start()
		{
			if (Arguments.Contains("t"))
			{
				RunAllUnitTests(typeof(TestProgram).Assembly);
			}
			else
			{
				Interactive();
			}
		}

        /*
          * Methods addorned with the ConsoleAction attribute can be run
          * interactively from the command line while methods addorned with
          * the TestMethod attribute will be run automatically when the
          * compiled executable is run.  To run ConsoleAction methods use
          * the command line argument /i.
          * 
          * All methods addorned with ConsoleAction and TestMethod attributes 
          * must be static for the purposes of extending CommandLineTestInterface
          * or an exception will be thrown.
          * 
          */

        // To run ConsoleAction methods use the command line argument /i.        
        
        [ConsoleAction("Long max")]
        public static void LongMax()
        {
            OutLineFormat("Int: {0}", Int32.MaxValue);
            OutLineFormat("Logn: {0}", Int64.MaxValue);
        }

        
        #region do not modify
        static void Main(string[] args)
        {
            PreInit();
            Initialize(args);
        }


        #endregion
    }
}
