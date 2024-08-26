Command to run Admin in isolated container at localhost:5050
	- `docker run -p 5050:80 -e PGADMIN_DEFAULT_EMAIL=admin@admin.com -e PGADMIN_DEFAULT_PASSWORD=ChangeMe -e PGADMIN_CONFIG_ENHANCED_COOKIE_PROTECTION=True -e PGADMIN_CONFIG_CONSOLE_LOG_LEVEL=10 -d --name my_pgadmin dpage/pgadmin4`
		- should be port 80 in container, or it won't work
	- to run container in host network instead of its own isolated network
		`docker run --network host -e PGADMIN_DEFAULT_EMAIL=admin@admin.com -e PGADMIN_DEFAULT_PASSWORD=ChangeMe -e PGADMIN_CONFIG_ENHANCED_COOKIE_PROTECTION=True -e PGADMIN_CONFIG_CONSOLE_LOG_LEVEL=10 -d --name my_pgadmin dpage/pgadmin4`

See which network a container is running by passing container name. Example with container name 'ecstatic_wozniak'
	- `docker inspect ecstatic_wozniak -f "{{json .NetworkSettings.Networks }}"`
	
Notes for `IAsyncLifeTime.InitializeAsync` for the WebApplicationFactory. This means for the method that would get executed once before all (not each) tests within a collection or class fixtures
    public async Task InitializeAsync()
    {
        _gitHubApiServer.Start();
        _gitHubApiServer.SetupUser(ValidGithubUser);

        // Must be before CreateClient() as CreateClient() will intialize an API instance in memory, therefore call its ConfigureServices method, which requires DB to already be up & running
        await _dbContainer.StartAsync();

        // Must be before Respawner as Respawner will execute DB Initialization, which will create tables, therefore Respawner won't throw error that there are no tables in DB
        _httpClient = CreateClient();

        // TODOCont: Respawner won't consider this.
        //      - Edit 24.08: It won't consider it because it has a DeleteSql = 'truncate "public"."customers" cascade', so it won't keep track of individual rows inserted in a table, it will keep track of the table and delete all from it,
        //          - So the line bellow creating a customer through API prior to any test is being run will stay there after the first test is execuded then it'll be deleted
        //      - if I want to actually keep some data in a table between tests, there is a SeedSql that can be provided, I'd imagine that will create rows after deletion or smth like that?
        // TODOCont: FIND OUT HOW TO CONNECT PGADMIN TO THOSE CONTAINER RUNNING DATABASES!!
        //      - Maybe there is some config in testcontainers to expose them to be opened by admin?
        //      - But API accesses it just by conn str. Maybe I just need to install PG ADMIN on Windows, and not run it in conatiner. Perhaps when I go through Docker course I'll be more aware what I'm missing when running PGadmin in conatiner and it CAN'T ACCESS this testconatainers throwaway postgresDBs
        //      - Edit 24.08: Did it as described here https://stackoverflow.com/questions/25540711/docker-postgres-pgadmin-local-connection
        //      - I run admin in a container netword but give it
        //          host: host.docker.internal (doesn't work with localhost)
        //          name: {DB_containername}
        //          ...
        //var user = CreateCustomerUtils.CustomerGenerator.Generate();
        //await _httpClient.PostAsJsonAsync("customers", user);

        var postgreDbConnectionString = _dbContainer.GetConnectionString();
        _dbConnection = new NpgsqlConnection(postgreDbConnectionString);
        // Must be opened prior to giving it to Respawner
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            SchemasToInclude = new string[] { "public" },
            DbAdapter = DbAdapter.Postgres,
        });
    }