using System;
using System.Collections.Generic;
using Football.Bot.Infrastructure;
using Pulumi;
using Pulumi.AzureNative.Authorization;
using Pulumi.AzureNative.DocumentDB;
using Pulumi.AzureNative.DocumentDB.Inputs;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using Pulumi.AzureNative.ServiceBus;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.KeyVault.Inputs;

using Kind = Pulumi.AzureNative.Storage.Kind;
using SkuName = Pulumi.AzureNative.KeyVault.SkuName;
using ServiceBus = Pulumi.AzureNative.ServiceBus;
using Storage = Pulumi.AzureNative.Storage;
using Web = Pulumi.AzureNative.Web;

await Pulumi.Deployment.RunAsync(async () =>
{
    // Define client config
    var current = await GetClientConfig.InvokeAsync();
    var stack = Pulumi.Deployment.Instance.StackName;

    // Create a resource group
    var resourceGroup = new ResourceGroup($"pl-rg-{stack}", new ResourceGroupArgs
    {
        Location = "West Europe",
        ResourceGroupName = $"pl-rg-{stack}"
    });

    // Create a oosmos db account
    var cosmosDbAccount = new DatabaseAccount($"pl-{stack}-cosmos-db-account", new DatabaseAccountArgs
    {
        AccountName = $"pl-{stack}-cosmos-db-account",
        ResourceGroupName = resourceGroup.Name,
        Location = resourceGroup.Location,
        DatabaseAccountOfferType = DatabaseAccountOfferType.Standard,
        Kind = DatabaseAccountKind.GlobalDocumentDB,
        ConsistencyPolicy = new ConsistencyPolicyArgs {DefaultConsistencyLevel = DefaultConsistencyLevel.Session},
        Locations =
        {
            new LocationArgs
            {
                LocationName = resourceGroup.Location,
                FailoverPriority = 0
            }
        }
    });

    // Create a key vault
    var keyVault = new Vault($"pl-{stack}-key-vault", new VaultArgs
    {
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        VaultName = $"pl-{stack}-key-vault",
        Properties = new VaultPropertiesArgs
        {
            EnabledForDiskEncryption = true,
            EnableRbacAuthorization = true,
            EnableSoftDelete = true,
            TenantId = current.TenantId,
            SoftDeleteRetentionInDays = 7,
            Sku = new SkuArgs
            {
                Family = "A",
                Name = SkuName.Standard
            }
        }
    });

    // Create service bus example
    var serviceBusNamespace = new Namespace($"pl-{stack}-service-bus", new NamespaceArgs
    {
        NamespaceName = $"pl-{stack}-service-bus",
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        Sku = new ServiceBus.Inputs.SBSkuArgs
        {
            Name = ServiceBus.SkuName.Standard
        }
    });

    // Create service bus queue
    var queue = new ServiceBus.Queue("pl-queue", new ServiceBus.QueueArgs
    {
        NamespaceName = serviceBusNamespace.Name,
        ResourceGroupName = resourceGroup.Name,
        QueueName = "pl-queue",
        MaxDeliveryCount = 3
    });

    // Create an app service plan
    var servicePlan = new AppServicePlan($"pl-{stack}-service-plan", new AppServicePlanArgs
    {
        Name = $"pl-{stack}-service-plan",
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        Sku = new SkuDescriptionArgs
        {
            Tier = "Standard",
            Name = "Y1"
        },
        Reserved = true
    });


    // Create a storage account
    var storageAccount = new StorageAccount($"pl{stack}storageaccountpl", new StorageAccountArgs
    {
        AccountName = $"pl{stack}storageaccountpl",
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        Kind = Kind.Storage,
        Sku = new Storage.Inputs.SkuArgs
        {
            Name = Storage.SkuName.Standard_LRS
        }
    });

    // Create an Application insight
    var appInsight = new Component($"pl-{stack}-app-insights", new ComponentArgs
    {
        ResourceName = $"pl-{stack}-app-insights",
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        Kind = "web"
    });


    // Create an Azure Function app
    var cosmosDbToken = ListDatabaseAccountKeys.Invoke(new ListDatabaseAccountKeysInvokeArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = cosmosDbAccount.Name
        })
        .Apply(result => result.PrimaryMasterKey);

    var serviceBusNameSpace = serviceBusNamespace.ServiceBusEndpoint.Apply(s => new Uri(s).Host);
    var storageConnectionString = StorageConnectionString(resourceGroup, storageAccount);

    var functionApp = new WebApp("pl-function-app", new WebAppArgs
    {
        Name = $"pl-{stack}-function-app",
        Location = resourceGroup.Location,
        ResourceGroupName = resourceGroup.Name,
        ServerFarmId = servicePlan.Id,
        Kind = "FunctionApp",
        StorageAccountRequired = true,
        Identity = new Web.Inputs.ManagedServiceIdentityArgs
        {
            Type = Web.ManagedServiceIdentityType.SystemAssigned
        },
        SiteConfig = new SiteConfigArgs
        {
            AppSettings = new[]
            {
                new NameValuePairArgs {Name = "FUNCTIONS_EXTENSION_VERSION", Value = "~4"},
                new NameValuePairArgs {Name = "FUNCTIONS_WORKER_RUNTIME", Value = "dotnet"},
                new NameValuePairArgs {Name = "APPINSIGHTS_INSTRUMENTATIONKEY", Value = appInsight.InstrumentationKey},
                new NameValuePairArgs {Name = "Cosmos__Container", Value = "matches"},
                new NameValuePairArgs {Name = "Cosmos__Database", Value = "football"},
                new NameValuePairArgs {Name = "Cosmos__Endpoint", Value = cosmosDbAccount.DocumentEndpoint},
                new NameValuePairArgs {Name = "Cosmos__Token", Value = cosmosDbToken},
                new NameValuePairArgs {Name = "KeyVaultEndpoint", Value = keyVault.Properties.Apply(response => response.VaultUri)},
                new NameValuePairArgs {Name = "AzureWebJobsStorage", Value = storageConnectionString},
                new NameValuePairArgs {Name = "ServiceBusConnection__fullyQualifiedNamespace", Value = serviceBusNameSpace}
            }
        }
    }, new CustomResourceOptions
    {
        DependsOn = {appInsight, keyVault, cosmosDbAccount, serviceBusNamespace}
    });

    // Create Key Vault assigment metadata reader for azure function 
    var readerRoleAssignment = new RoleAssignment("4574adae-d926-11ed-afa1-0242ac120002", new RoleAssignmentArgs
    {
        PrincipalId = functionApp.Identity.Apply(identity => identity.PrincipalId),
        RoleDefinitionId = Roles.KeyVaultReader,
        Scope = keyVault.Id,
        RoleAssignmentName = "4574adae-d926-11ed-afa1-0242ac120002",
        PrincipalType = PrincipalType.User
    }, new CustomResourceOptions
    {
        DependsOn = {keyVault, functionApp}
    });

    // Create Key Vault assigment secret reader for azure function 
    var secretRoleAssignment = new RoleAssignment("539cfc2e-d926-11ed-afa1-0242ac120002", new RoleAssignmentArgs
    {
        PrincipalId = functionApp.Identity.Apply(identity => identity.PrincipalId),
        RoleDefinitionId = Roles.KeyVaultSecretsUser,
        Scope = keyVault.Id,
        RoleAssignmentName = "539cfc2e-d926-11ed-afa1-0242ac120002",
        PrincipalType = PrincipalType.User
    }, new CustomResourceOptions
    {
        DependsOn = {keyVault, functionApp}
    });

    // Create Key Vault assigment admin reader for current user 
    var adminRoleAssignment = new RoleAssignment("801e9dc8-d923-11ed-afa1-0242ac120002", new RoleAssignmentArgs
    {
        PrincipalId = current.ObjectId,
        RoleDefinitionId = Roles.KeyVaultAdministrator,
        Scope = keyVault.Id,
        RoleAssignmentName = "801e9dc8-d923-11ed-afa1-0242ac120002",
        PrincipalType = PrincipalType.User
    }, new CustomResourceOptions
    {
        DependsOn = {keyVault}
    });

    // Create Key Vault assigment admin reader for current user 
    var serviceBusRoleAssignment = new RoleAssignment("801e9dc8-d923-11ed-afa1-0242ac120000", new RoleAssignmentArgs
    {
        PrincipalId = functionApp.Identity.Apply(identity => identity.PrincipalId),
        RoleDefinitionId = Roles.ServiceBusOwner,
        Scope = queue.Id,
        RoleAssignmentName = "801e9dc8-d923-11ed-afa1-0242ac120000",
        PrincipalType = PrincipalType.User
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
        {"function_app_hostname", functionApp.DefaultHostName},
        {"keyVault_name", keyVault.Name},
        {"reader_assignment", readerRoleAssignment.Id},
        {"secret_assignment", secretRoleAssignment.Id},
        {"admin_assignment", adminRoleAssignment.Id},
        {"service-bus", serviceBusNamespace.Name},
        {"queue", queue.Name},
        {"queue_assigment", serviceBusRoleAssignment.Id}
    };
});

static Output<string> StorageConnectionString(ResourceGroup resourceGroup, StorageAccount storageAccount)
{
    var accessKey = ListStorageAccountKeys.Invoke(new ListStorageAccountKeysInvokeArgs
    {
        ResourceGroupName = resourceGroup.Name,
        AccountName = storageAccount.Name
    }).Apply(x => x.Keys[0].Value);

    return Output.Format(
        $"DefaultEndpointsProtocol=https;AccountName={storageAccount.Name};AccountKey={accessKey};EndpointSuffix=core.windows.net");
}