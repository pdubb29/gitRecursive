using System;
using System.IO;
using static System.Console;

namespace gitRecursive
{
    class Program
    {
		private static string _path;
		private static string _gitBranch;

		static void Main(string[] args)
		{
			if(args.Length < 1 || args.Length > 2)
			{
				PrintUsage();
				return;
			}

			var validArguments = CheckArguments(args);
			if(!validArguments)
			{
				return;
			}

			var folderVisitor = new FolderVisitor(_path, _gitBranch);	
			var result = folderVisitor.VisitAll().Result;
			if(result)
			{
				WriteLine("Completed Successfully");
			}
			else
			{
				WriteLine("Failed to complete");
			}
		}

		private static void PrintUsage()
		{
			WriteLine("##################");
			WriteLine("## GitRecursive ##");
			WriteLine("##################");
			WriteLine("gr <arg1> <*arg2>");
			WriteLine("arg1 - root directory to pull on all subdirectories");
			WriteLine("arg2 - (optional) the branch to checkout before pulling");
		}

		private static bool CheckArguments(string[] args)
		{
			if(!Directory.Exists(args[0]))
			{
				//could be a local path
				var path = Path.Combine(Environment.CurrentDirectory, args[0]);
				if(Directory.Exists(path))
				{
					_path = path;
					return true;
				}
				return false;
			}
			_path = args[0];
			return true;
		}
	}
}
