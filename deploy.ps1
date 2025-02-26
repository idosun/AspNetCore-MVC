$sentry_org="testorg-az"
$sentry_project="idos-aspnet-core"
$version=sentry-cli releases propose-version

#Create Release
sentry-cli releases -o $sentry_org new -p $sentry_project $version

#Associate Commits
sentry-cli releases -o $sentry_org -p $sentry_project set-commits --auto $version

#Set new value for release version environment variable
$Env:SENTRY_RELEASE=$version

#Launch app
dotnet run
