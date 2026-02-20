IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ParameterResource> postgresUsername = builder.AddParameter("postgres-username");
IResourceBuilder<ParameterResource> postgresPassword = builder.AddParameter("postgres-password", secret: true);

IResourceBuilder<ParameterResource> orgUrl = builder.AddParameter("azure-devops-org-url");
IResourceBuilder<ParameterResource> pat = builder.AddParameter("azure-devops-pat", secret: true);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("posgres", postgresUsername, postgresPassword, 5003);
IResourceBuilder<PostgresDatabaseResource> postgresdb = postgres.AddDatabase("postgresdb");

_ = builder.AddProject<Projects.CabaVS_AzureDevOpsHelper_API>("azure-devops-helper-api")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("AzureDevOps__OrgUrl", orgUrl)
    .WithEnvironment("AzureDevOps__Pat", pat)
    .WaitFor(postgresdb).WithReference(postgresdb);

await builder.Build().RunAsync();
