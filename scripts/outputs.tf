output "cosmos_db_primary_key" {
  description = "Cosmos db primary key"
  value = azurerm_cosmosdb_account.cosmos_db.primary_key
  sensitive   = true
}

output "cosmos_db_endpoint" {
  description = "Cosmos db primary key"
  value = azurerm_cosmosdb_account.cosmos_db.endpoint
}