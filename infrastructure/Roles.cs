namespace Football.Bot.Infrastructure;

public static class Roles
{
    public const string KeyVaultReader =
        "/providers/Microsoft.Authorization/roleDefinitions/21090545-7ca7-4776-b22c-e363652d74d2";

    public const string KeyVaultSecretsUser =
        "/providers/Microsoft.Authorization/roleDefinitions/4633458b-17de-408a-b874-0445c86b69e6";

    public const string KeyVaultAdministrator =
        "/providers/Microsoft.Authorization/roleDefinitions/00482a5a-887f-4fb3-b363-3b7fe8e74483";
    
    public const string ServiceBusOwner =
        "/providers/Microsoft.Authorization/roleDefinitions/090c5cfd-751d-490a-894a-3ce6f1109419";
}