# Setup

Symlink the output 

```psl
New-Item -Path 'C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program 2\BepInEx\plugins\ExampleMod\ExampleMod.dll' -ItemType SymbolicLink -Value 'C:\Users\Olly\source\repos\SpaceWarp\ExampleMod\bin\Release\ExampleMod.dll'
```