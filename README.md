Question 2 was attempted.

Environment: IIS 
C# Restful WebAPI
Backend DB : Microsoft SQL Server 

0) source code can be found in the GrantDisimburesment folder.

1) Spin up a MSSQLServer on port 60666 on Docker on Windows running with WSL2 by using the Dockerfile by calling the below commands:

docker build -t nicholasmssql .
docker run -d -p 60666:1433 --name nicholasInstance nicholasmssql

2) Connect to DB using Azure Data Studio or any other tool on localhost,60666

3) Run Schema found in section 1 of SchemaAndQueries.sql

4) Set Up IIS App Service instance to ./PublishedInstance  
a) if the port of the database is changed, please update 	  correspondingly in the web.config file. 

5) A swagger.json file is provided for ease of usage using Postman.

