if ($(git rev-parse --abbrev-ref HEAD) -eq "develop")
{
  $fileContent = "-----BEGIN RSA PRIVATE KEY-----`n"
  $fileContent += $env:priv_key.Replace(' ', "`n")
  $fileContent += "`n-----END RSA PRIVATE KEY-----`n"
  Set-Content c:\users\appveyor\.ssh\id_rsa $fileContent
  $TempDir = $env:temp + "\" + [System.Guid]::NewGuid().ToString()
  git clone git@github.com:Pomona/Pomona-gh-pages.git $TempDir -b gh-pages
  Write-Output "$TempDir"
  nuget restore
  .\packages\Storyteller.3.0.0.334-rc\tools\ST.exe doc-export $TempDir\ FileDump -c .\samples\ .\tests\
  pushd $TempDir
  git status
  git add -A .
  git commit -m "Updated ST doc preview"
  git push
  popd
  Remove-Item  $TempDir\ -Recurse -Force
  Write-Output "Updated documentation"
}
