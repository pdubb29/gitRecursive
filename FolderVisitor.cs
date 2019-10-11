using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gitRecursive
{
	internal class FolderVisitor
	{
		private readonly string _gitBranch;

		private List<string> _subDirectories;

		public FolderVisitor(string path, string gitBranch)
		{
			_gitBranch = gitBranch ?? "master";
			_subDirectories = Directory.GetDirectories(path).ToList();
		}

		internal async Task<bool> VisitAll()
		{
			List<Task<bool>> listOfTasks = new List<Task<bool>>();
			foreach(var subDirectory in _subDirectories)
			{
				Task<bool> task = Visit(subDirectory);
				listOfTasks.Add(task);
			}
			Task.WaitAll(listOfTasks.ToArray());
			bool result = true;
			foreach(var item in listOfTasks)
			{
				result = result && item.Result;
			}
			return result;
		}

		private async Task<bool> Visit(string subDirectory)
		{
			ProcessStartInfo baseStartInfo = new ProcessStartInfo()
			{
				FileName = "git",
				WorkingDirectory = subDirectory
			};

			//stash
			baseStartInfo.Arguments = "stash";
			var stashProcess = Process.Start(baseStartInfo);
			stashProcess.WaitForExit();

			//Reset
			baseStartInfo.Arguments = "reset --hard";
			var resetProcess = Process.Start(baseStartInfo);
			resetProcess.WaitForExit();

			//Checkout
			baseStartInfo.Arguments = $"checkout {_gitBranch}";
			var checkoutProcess = Process.Start(baseStartInfo);
			checkoutProcess.WaitForExit();

			//pull
			baseStartInfo.Arguments = "pull";
			var pullProcess = Process.Start(baseStartInfo);
			pullProcess.WaitForExit();

			return checkoutProcess.ExitCode == 0 && resetProcess.ExitCode == 0 && pullProcess.ExitCode == 0;
		}
	}
}