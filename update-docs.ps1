$ErrorActionPreference = "Stop"

Write-Output "What branch is this? "

Write-Output "$(git rev-parse --abbrev-ref HEAD)"

Get-ChildItem Env:

if ($(git rev-parse --abbrev-ref HEAD) -eq "develop")
{
  git remote -v
  git fetch
  $TempDir = $env:temp + "\" + [System.Guid]::NewGuid().ToString()
  git worktree add $TempDir gh-pages
  pushd $TempDir
  git reset --hard origin/gh-pages
  popd
  nuget restore
  .\packages\Storyteller.3.0.0.334-rc\tools\ST.exe doc-export $TempDir\preview\ FileDump -c .\samples\ .\tests\
  pushd $TempDir
  git status
  git add -A .
  git commit -m "Updated ST doc preview"
  git push
  popd
  Remove-Item  $TempDir\ -Recurse -Force
  git worktree prune
  Write-Output "Updated documentation"
}
