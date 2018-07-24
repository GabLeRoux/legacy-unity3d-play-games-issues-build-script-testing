#!/usr/bin/env bash
set -x # adds verbosity

unity_project_path=${1:-"../.."} # 1st script param, defaults to "../.."
ios_build_path=${2:-"."} # 2nd script param, defaults to "."

# cocoapods
cp ${unity_project_path}/Assets/Editor/Podfile ${ios_build_path}/Podfile
pod install

# Removes GoogleSignIn CFBundleExecutable because wtf
# http://stackoverflow.com/questions/32622899/itms-90535-unable-to-publish-ios-app-with-latest-google-signin-sdk
/usr/libexec/PlistBuddy -c "Delete :CFBundleExecutable" ${ios_build_path}/Pods/GoogleSignIn/Resources/GoogleSignIn.bundle/Info.plist
