# GitRecursive
GitRecursive is used to pull the most recent versions of master or any other branch you specify in the subdirectories of a given directory. It only goes one level deep.

You needn't worry about the existing work that you have done on the subdirectories. 

## What the tool does:
1. Get the current branch
2. Stash the local changes
3. Clean everything else using git reset --hard
4. Checks out master or the branch you specify
5. Pulls the branch specified.
6. Checks out the old branch
7. Pops the stashed changes.

## Usage of the tool:
```
gitRecursive <arg1> <*arg2>
```

arg1 - root directory to pull on all subdirectories

arg2 - (optional) the branch to checkout before pulling