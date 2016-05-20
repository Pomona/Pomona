$ErrorActionPreference = "Stop"

Write-Output "What branch is this? "

Write-Output "$(git rev-parse --abbrev-ref HEAD)"

Get-ChildItem Env:

if ($(git rev-parse --abbrev-ref HEAD) -eq "develop")
{
  git remote -v
  git fetch
  git worktree add ..\Pomona-gh-pages gh-pages
  pushd ..\Pomona-gh-pages
  git reset --hard origin/gh-pages
  popd
  nuget restore
  .\packages\Storyteller.3.0.0.334-rc\tools\ST.exe doc-export ..\Pomona-gh-pages\preview\ FileDump -c .\samples\ .\tests\
  pushd ..\Pomona-gh-pages
  git status
  git add -A .
  git commit -m "Updated ST doc preview"
  git push
  popd
  Remove-Item  ..\Pomona-gh-pages\ -Recurse -Force
  git worktree prune
  Write-Output "Updated documentation"
}
