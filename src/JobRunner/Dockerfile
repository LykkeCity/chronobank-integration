FROM microsoft/dotnet:1.1-runtime
ENTRYPOINT ["dotnet", "JobRunner.dll"]
ARG source=.
WORKDIR /app
COPY $source .
