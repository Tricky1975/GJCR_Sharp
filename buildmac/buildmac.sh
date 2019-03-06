#!/bin/sh

MACBUNDLE=GJCR.app
PUREMACAPP=GJCR

echo Mac application creation script
echo ===============================

if [ -d "$MACBUNDLE" ]; then
   echo "Destroying old version";
   rm -Rv "$MACBUNDLE"
fi

if [ "$1" != "skipcompile" ]; then
   echo "Compiling $PUREMACAPP"
   msbuild "-p:Configuration=Release;WarningLevel=0" ../$PUREMACAPP.sln
fi


echo "Creating bundle folders"
mkdir "$MACBUNDLE"
mkdir "$MACBUNDLE/Contents"
mkdir "$MACBUNDLE/Contents/MacOS"
mkdir "$MACBUNDLE/Contents/Resources"


echo "Copying info"
cp -v "Needed/Info.plist" "$MACBUNDLE/Contents"

echo "Copying icon"
cp -v "../Icons/GJCR.icns" "$MACBUNDLE/Contents/Resources/GJCR.icns"

echo "Copying binaries"
cp -Rv "../bin/Release/"* "$MACBUNDLE/Contents/MacOS"

echo "Copying startup file"
cp -v "Needed/RunShell.sh" "$MACBUNDLE/Contents/MacOS/$PUREMACAPP"
chmod +x "$MACBUNDLE/Contents/MacOS/$PUREMACAPP"

echo "If no errors (I said ERRORS not WARNINGS!!!) everything SHOULD be in order"
echo "In that case there should be a MyData.app bundle in this very folder."
echo "You can drag it into your /Applications folder, if you like ;)"
