#!/usr/bin/env bash
log_file=postbuild.log
exec | tee $log_file 2>&1 # log everything (output + stderr) to log_file
set -x # adds verbosity

unity_project_path=${1:-"../.."} # scrpit param or default to "../.." ;)
ios_build_path=${2:-"."}

# handles cocoapods
cp ${unity_project_path}/Assets/Editor/Podfile ${ios_build_path}/Podfile
pod install

# Remove GoogleSignIn CFBundleExecutable because wtf
# http://stackoverflow.com/questions/32622899/itms-90535-unable-to-publish-ios-app-with-latest-google-signin-sdk
/usr/libexec/PlistBuddy -c "Delete :CFBundleExecutable" ${ios_build_path}/Pods/GoogleSignIn/Resources/GoogleSignIn.bundle/Info.plist
