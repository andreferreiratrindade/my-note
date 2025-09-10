# my-note

dotnet publish src/MyNote -c Release -p:PublishReadyToRun=false

dotnet lambda package -c Release -o ./publish

cdk synth > template.yaml


dotnet lambda-test-tool-8.0
