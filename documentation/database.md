# Database documentation

## Create new migrations

Prerequisites:

- a local .env file in src/Identity.Migrator with the following content:
    - `ASPNETCORE_ENVIRONMENT`
    - `DB_USER`
    - `DB_PASSWORD`
    - `DB_NAME`
    - `DB_PORT`
    - `DB_HOST`

To create a new migration, run the following command in the root of the project:

```powershell
dotnet ef --project ./src/Identity.Persistence/Identity.Persistence.csproj --startup-project ./src/Identity.Migrator/Identity.Migrator.csproj migrations add <migration_name> 
```

To remove a migrations run the following command in the root of the project:

```powershell
dotnet ef --project ./src/Identity.Persistence/Identity.Persistence.csproj --startup-project ./src/Identity.Migrator/Identity.Migrator.csproj migrations remove
```

## Update/create database

To update or create the database, deploy the migration tool Idenity.Migrator (see docker-compose.yml).
Then run the following command:

```powershell
./Identity.Migrator create # Create the database
```

```powershell
./Identity.Migrator migrate # Update the database
```

Database could be seed with default roles and superadmin with the following command:

```powershell
./Identity.Migrator seed # Seed the database
```
