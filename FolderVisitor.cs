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

		private async Task<bool> ExecuteGitCommand(string arguments, string subDirectory, bool redirectStandardOutput = false)
		{
			ProcessStartInfo gitCommand = new ProcessStartInfo()
			{
				FileName = "git",
				WorkingDirectory = subDirectory,
				Arguments = arguments,
				RedirectStandardOutput = redirectStandardOutput
			};

			var process = Process.Start(gitCommand);
			process.WaitForExit();
			return process.ExitCode == 0;
		}

		private async Task<string> GetCurrentBranch(string subDirectory)
		{
			var processStartInfo = new ProcessStartInfo()
			{
				Arguments = "branch",
				FileName = "git",
				WorkingDirectory = subDirectory,
				RedirectStandardOutput = true,
			};

			var branchProcess = Process.Start(processStartInfo);
			branchProcess.WaitForExit(TimeSpan.FromMilliseconds(1_500).Milliseconds);
			if(!branchProcess.HasExited || branchProcess.ExitCode == 128)
			{
				return string.Empty;
			}
			using (var streamReader = branchProcess.StandardOutput)
			{
				string line;

				while ((line = await streamReader.ReadLineAsync()) != null)
				{
					if (line.StartsWith('*'))
					{
						//we've got our branch name
						return line.Replace("* ", "");
					}
				}
			}
			return string.Empty; 
		}

		private async Task<bool> Visit(string subDirectory)
		{

			//set pager.branch to false
			var exitCode = await ExecuteGitCommand("config --global pager.branch false", subDirectory);

			//get branch name
			var currentBranch = await GetCurrentBranch(subDirectory);

			if(currentBranch.Equals(string.Empty))
			{
				return false;
			}
			//set pager.branch to true
			exitCode = await ExecuteGitCommand("config --global pager.branch true", subDirectory);

			//stash
			exitCode = await ExecuteGitCommand("stash", subDirectory);

			//Reset
			exitCode = await ExecuteGitCommand("reset --hard", subDirectory);

			//Checkout
			exitCode = await ExecuteGitCommand($"checkout {_gitBranch}", subDirectory);

			//pull
			exitCode = await ExecuteGitCommand("pull", subDirectory);

			//checkout current branch
			exitCode = await ExecuteGitCommand($"checkout {currentBranch}", subDirectory);

			//pop
			exitCode = await ExecuteGitCommand("stash pop", subDirectory);

			return true;
		}
	}
}