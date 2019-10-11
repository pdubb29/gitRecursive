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

			//set pager.branch to false
			baseStartInfo.Arguments = "config --global pager.branch false";
			var setConfigProcess = Process.Start(baseStartInfo);
			setConfigProcess.WaitForExit();

			//get branch name
			var currentBranch = "";
			baseStartInfo.RedirectStandardOutput = true;
			baseStartInfo.Arguments = "branch";
			var branchProcess = Process.Start(baseStartInfo);
			branchProcess.WaitForExit(TimeSpan.FromMilliseconds(1_500).Milliseconds);
			if(!branchProcess.HasExited || branchProcess.ExitCode == 128)
			{
				return false;
			}
			using (var streamReader = branchProcess.StandardOutput)
			{
				string line;

				while ((line = await streamReader.ReadLineAsync()) != null)
				{
					if (line.StartsWith('*'))
					{
						//we've got our branch name
						currentBranch = line.Replace("* ", "");
					}
				}
			}
			baseStartInfo.RedirectStandardOutput = false;

			//set pager.branch to true
			baseStartInfo.Arguments = "config --global pager.branch true";
			var unsetConfigProcess = Process.Start(baseStartInfo);
			unsetConfigProcess.WaitForExit();

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

			//checkout current branch
			baseStartInfo.Arguments = $"checkout {currentBranch}";
			var checkoutOriginalProcess = Process.Start(baseStartInfo);
			checkoutOriginalProcess.WaitForExit();

			//pop
			baseStartInfo.Arguments = "stash pop";
			var stashPopProcess = Process.Start(baseStartInfo);
			checkoutOriginalProcess.WaitForExit();

			return true;
		}
	}
}