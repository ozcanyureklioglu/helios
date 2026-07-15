var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Vectomera_Api>("api");

var worker = builder.AddProject<Projects.Vectomera_Worker>("worker");

builder.Build().Run();

