using System;
using System.Collections.Generic;
using Football.Bot.Infrastructure;
using Pulumi;
using Pulumi.Azure.AppInsights;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.Azure.Authorization;
using Pulumi.Azure.CosmosDB.Inputs;
using Pulumi.Azure.KeyVault;
using Pulumi.Azure.ServiceBus;
using Pulumi.AzureNative.Resources;
using CosmosDB = Pulumi.Azure.CosmosDB;
using Storage = Pulumi.Azure.Storage;

await Pulumi.Deployment.RunAsync(async () =>
{
    // Define client config
    var current = await Pulumi.Azure.Core.GetClientConfig.InvokeAsync();
    var stack = Pulumi.Deployment.Instance.StackName;

    // Create a resource group
    var resourceGroup = new ResourceGroup($"pl-rg-{stack}", new ResourceGroupArgs
    {
        Location = "West Europe",
        ResourceGroupName = $"pl-rg-{stack}"
    });

    // Create a resource group
    var cosmosDbAccount = new CosmosDB.Account($"pl-{stack}-cosmos-db-account", new CosmosDB.AccountArgs
    {
        Name = $"pl-{stack}-cosmos-db-account",
        ResourceGroupName = resourceGroup.Name,
        Location = resourceGroup.Location,
        OfferType = "Standard",
        Kind = "GlobalDocumentDB",
        ConsistencyPolicy = new AccountConsistencyPolicyArgs {ConsistencyLevel = "Session"},
        GeoLocations = new AccountGeoLocationArgs
        {
            Location = resourceGroup.Location,
            FailoverPriority = 0
        }
    });

    // Create an Application insight
    var appInsight = new Insights($"pl-{stack}-app-insights", new InsightsArgs
    {
        Name = $"pl-{stack}-app-insights",
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        ApplicationType = "web"
    });

    // Create an app service plan
    var servicePlan = new ServicePlan($"pl-{stack}-service-plan", new ServicePlanArgs
    {
        Name = $"pl-{stack}-service-plan",
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        OsType = "Linux",
        SkuName = "Y1"
    });

    // Create a storage account
    var storageAccount = new Storage.Account($"pl{stack}storageaccountpl", new Storage.AccountArgs
    {
        Name = $"pl{stack}storageaccountpl",
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        AccountTier = "Standard",
        AccountReplicationType = "LRS"
    });

    // Create a key vault
    var keyVault = new KeyVault($"pl-{stack}-key-vault", new KeyVaultArgs
    {
        Name = $"pl-{stack}-key-vault",
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        EnabledForDiskEncryption = true,
        TenantId = current.TenantId,
        EnableRbacAuthorization = true,
        PurgeProtectionEnabled = false,
        SoftDeleteRetentionDays = 7,
        SkuName = "standard"
    });

    // Create service bus example
    var serviceBusNamespace = new Namespace($"pl-{stack}-service-bus", new NamespaceArgs
    {
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        Sku = "Standard",
        Name = $"pl-{stack}-service-bus",
    });

    // Create service bus queue
    var queue = new Queue("pl-queue", new QueueArgs
    {
        NamespaceId = serviceBusNamespace.Id,
        Name = "pl-queue"
    });

    // Create an Azure Function app
    var functionApp = new LinuxFunctionApp("pl-function-app", new LinuxFunctionAppArgs
    {
        Name = $"pl-{stack}-function-app",
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        ServicePlanId = servicePlan.Id,
        StorageAccountName = storageAccount.Name,
        StorageAccountAccessKey = storageAccount.PrimaryAccessKey,
        AppSettings = new InputMap<string>
        {
            {"FUNCTIONS_WORKER_RUNTIME", "dotnet"},
            {"APPINSIGHTS_INSTRUMENTATIONKEY", appInsight.InstrumentationKey},
            {"Cosmos__Container", "matches"},
            {"Cosmos__Database", "football"},
            {"Cosmos__Token", cosmosDbAccount.PrimaryKey},
            {"Cosmos__Endpoint", cosmosDbAccount.Endpoint},
            {"KeyVaultEndpoint", keyVault.VaultUri},
            {"ServiceBusConnection__fullyQualifiedNamespace", serviceBusNamespace.Endpoint.Apply(s => new Uri(s).Host)}
        },
        Identity = new LinuxFunctionAppIdentityArgs
        {
            Type = "SystemAssigned"
        },
        SiteConfig = new LinuxFunctionAppSiteConfigArgs()
    }, new CustomResourceOptions
    {
        DependsOn = {appInsight, keyVault, cosmosDbAccount, serviceBusNamespace}
    });
    
    // Create Key Vault assigment metadata reader for azure function 
    var readerRoleAssignment = new Assignment("4574adae-d926-11ed-afa1-0242ac120002", new AssignmentArgs
    {
        PrincipalId = functionApp.Identity.Apply(identity => identity.PrincipalId),
        RoleDefinitionId = Roles.KeyVaultReader,
        Scope = keyVault.Id,
        Name = "4574adae-d926-11ed-afa1-0242ac120002"
    }, new CustomResourceOptions
    {
        DependsOn = {keyVault, functionApp}
    });

    // Create Key Vault assigment secret reader for azure function 
    var secretRoleAssignment = new Assignment("539cfc2e-d926-11ed-afa1-0242ac120002", new AssignmentArgs
    {
        PrincipalId = functionApp.Identity.Apply(identity => identity.PrincipalId),
        RoleDefinitionId = Roles.KeyVaultSecretsUser,
        Scope = keyVault.Id,
        Name = "539cfc2e-d926-11ed-afa1-0242ac120002"
    }, new CustomResourceOptions
    {
        DependsOn = {keyVault, functionApp}
    });

    // Create Key Vault assigment admin reader for current user 
    var adminRoleAssignment = new Assignment("801e9dc8-d923-11ed-afa1-0242ac120002", new AssignmentArgs
    {
        PrincipalId = current.ObjectId,
        RoleDefinitionId = Roles.KeyVaultAdministrator,
        Scope = keyVault.Id,
        Name = "801e9dc8-d923-11ed-afa1-0242ac120002"
    }, new CustomResourceOptions
    {
        DependsOn = {keyVault, functionApp}
    });
    
    // Create Key Vault assigment admin reader for current user 
    var serviceBusRoleAssignment = new Assignment("801e9dc8-d923-11ed-afa1-0242ac120002", new AssignmentArgs
    {
        PrincipalId = functionApp.Identity.Apply(identity => identity.PrincipalId),
        RoleDefinitionId = Roles.ServiceBusOwner,
        Scope = queue.Id,
        Name = "801e9dc8-d923-11ed-afa1-0242ac120002"
    }, new CustomResourceOptions
    {
        DependsOn = {queue, functionApp}
    });
    
    return new Dictionary<string, object>
    {
        {"resource_group_name", resourceGroup.Name},
        {"cosmos_db_account_name", cosmosDbAccount.Name},
        {"app_insight_name", appInsight.Name},
        {"service_plan_name", servicePlan.Name},
        {"storage_account_name", storageAccount.Name},
        {"function_app_name", functionApp.Name},
        {"function_app_hostname", functionApp.DefaultHostname},
        {"keyVault_name", keyVault.Name},
        {"reader_assignment", readerRoleAssignment.Id},
        {"secret_assignment", secretRoleAssignment.Id},
        {"admin_assignment", adminRoleAssignment.Id},
        {"service-bus", serviceBusNamespace.Name},
        {"queue", queue.Name},
        {"queue_assigment", serviceBusRoleAssignment.Id}
    };
});