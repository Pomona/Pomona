@echo off

nuget restore
packages\Storyteller\tools\ST.exe doc-run
