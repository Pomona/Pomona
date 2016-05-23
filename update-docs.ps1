$ErrorActionPreference = "Stop"

if ($(git rev-parse --abbrev-ref HEAD) -eq "develop")
{
  $TempDir = $env:temp + "\" + [System.Guid]::NewGuid().ToString()
  git clone git@github.com:Pomona/Pomona-gh-pages.git $TempDir -b gh-pages
  Write-Output "$TempDir"
  nuget restore
  .\packages\Storyteller.3.0.0.334-rc\tools\ST.exe doc-export $TempDir\preview\ FileDump -c .\samples\ .\tests\
  pushd $TempDir
  git status
  git add -A .
  git commit -m "Updated ST doc preview"
  git push
  popd
  Remove-Item  $TempDir\ -Recurse -Force
  Write-Output "Updated documentation"
}
