# TTS-Content-Downloader

Downloads TTS workshop OBJ assets based on TTS JSON save game file

# Usage

Copy the desired TTS JSON save game file, typically names something like *TS_Save_86.json*, to the same folder as this application EXE. Rename the file to *Minis.json* (or update the code to refelect the correct file name). Run the application. The application will do nothing for a while while it collects all of the mesh references in the file. Be patient. Then a line will be printed out for each mesh OBJ file that the application tries to download. Downloaded assets are place in an Assets sub-folder (created automatically if not present). 

# Limitation

The application assumes that all mesh references are to OBJ file and that all references have the Nickname proeprty. The application does some minor OBJ file adjustment to make the file names more friendly and thus projects which use assetBundles are currently not supported by this downloader.
