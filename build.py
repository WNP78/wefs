# This script is a little jank, but will take your project's data and package it up
# with the game build of the engine distributed with the editor.
# The path to that is set below. As you can see, it's hardcoded in when the project is created,
# so you will probably want to change that if working with others!
import os, shutil, sys, json

worlds_game_build_path = "C:\Users\nicky\Misc\WorldsEngine\out\build\x64-Debug\BuildOutput\GameBuild"

exe_test_path = os.path.join(worlds_game_build_path, "WorldsEngine.exe")

if not os.path.exists(worlds_game_build_path) or not os.path.exists(exe_test_path):
    print("Could't find engine game build!")
    print("If you've moved the editor or have used someone else's project, you will need to modify this script.")
    sys.exit(1)

project_dict = {}

with open("WorldsProject.json", "r") as f:
    project_dict = json.load(f)
    
print(f"Building {project_dict['projectName']}")

if os.path.exists("Build"):
    shutil.rmtree("Build")

shutil.copytree(worlds_build_out_path, "Build")
shutil.copytree("Data", "Build/GameData")

for copy_dir in project_dict["copyDirectories"]:
    if not os.path.exists("SourceData/" + copy_dir):
        print(f"Warning: Copy directory {copy_dir} doesn't exist.")
        continue
    shutil.copytree("SourceData/" + copy_dir, "Build/GameData/" + copy_dir)
    
shutil.copytree("CompiledCode", "Build/GameAssemblies")

# There are some files produced by the C# project build that we wouldn't want to distribute,
# so clear those out!
os.remove("Build/GameAssemblies/Game.pdb")
os.remove("Build/GameAssemblies/WorldsEngineManaged.dll")
os.remove("Build/GameAssemblies/WorldsEngineManaged.pdb")
os.remove("Build/NetAssemblies/WorldsEngineManaged.pdb")

os.rename("Build/WorldsEngine.exe", "Build/" + project_dict['projectName'] + ".exe")

print("Build finished")
