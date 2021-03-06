﻿using System;
using System.Configuration;
using System.IO;
using DatabaseMigraine;

namespace DatabaseCreator
{
	class Program
	{
		//TODO: change this to use DbParams instead of an app.config file:
		static readonly string DbCreationPath = ConfigurationManager.AppSettings["DbCreationPath"];
		static readonly string DisposableDbConnString = ConfigurationManager.AppSettings["DisposableDbConnString"];
		static readonly string DisposableDbHostname = ConfigurationManager.AppSettings["DisposableDbHostname"];

		static void Main(string[] args)
		{
			try
			{

				if (args.Length < 1 || args.Length > 2 || (args.Length == 2 && !args[1].Contains("--config:") && !args[1].Contains("--databaseName:")))
				{
					Console.WriteLine("Please supply the name of database(s) you wish to create.");
					Console.WriteLine();
					Console.WriteLine("Optionally in the 2nd argument, specify a config file to sabotage with:");
					Console.WriteLine("--config:\"/path/to/first.config.file{0}/path/to/second.config.file\"",
									  Path.PathSeparator);
					Console.WriteLine("To Exit Press Any Key");
					Console.Read();
					Environment.Exit(1);
				}

				string[] configFiles = null;
				if (args[1].Contains("--config:"))
				{
					string paths = args[1].Substring(args[1].IndexOf(":") + 1);

					configFiles = paths.Split(Path.PathSeparator);
					foreach (var configFile in configFiles)
					{
						Console.WriteLine(configFile);
						if (!File.Exists(configFile))
						{
							throw new FileNotFoundException(configFile);
						}
					}
				}


				if (args[1].Contains("--databaseName:"))
				{
					CreateDatabase(args[0], args[1].Substring(args[1].IndexOf(":") + 1));
				}
				else
				{
					string disposableDbName = CreateDatabase(args[0],string.Empty);

					if (args[1].Contains("--config:"))
					{
						foreach (var configFile in configFiles)
						{
							ConfigFileSaboteur.Sabotage(configFile, args[0], disposableDbName);
						}
					}
				}

				Environment.Exit(0);

			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				Environment.Exit(1);
			}
		}

		private static string CreateDatabase(string dbNameInVcs,string fixedDatabaseName)
		{
			var disposableDbServer = ConnectionHelper.Connect(DisposableDbHostname, DisposableDbConnString);

			var disposableDbCreator = new DisposableDbManager(DbCreationPath, disposableDbServer, dbNameInVcs);
			if(!String.IsNullOrEmpty(fixedDatabaseName))
			{
				DisposableDbManager.KillDb(disposableDbServer,fixedDatabaseName);
				disposableDbCreator.FixedDatabaseName = fixedDatabaseName;	
			}

			

			string disposableDbName = disposableDbCreator.CreateCompleteDisposableDb();
			Console.WriteLine(disposableDbName);
			return disposableDbName;
		}
	}
}
