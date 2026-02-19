IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ParameterResource> orgUrl = builder.AddParameter("azure-devops-org-url");
IResourceBuilder<ParameterResource> pat = builder.AddParameter("azure-devops-pat", secret: true);

_ = builder.AddProject<Projects.CabaVS_AzureDevOpsHelper_API>("azure-devops-helper-api")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("AzureDevOps__OrgUrl", orgUrl)
    .WithEnvironment("AzureDevOps__Pat", pat);

await builder.Build().RunAsync();