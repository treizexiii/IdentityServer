# Identity Server

## Introduction

This is a simple identity server that is used to authenticate users and provide them with a JWT token. This token can then be used to access other services.

## Getting Started

To get started, you will need to have the following installed:

- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)

Once you have these installed, you can create a `.env` file in the deployment folder of the project. This file should contain the following:

```env
ASPNETCORE_ENVIRONMENT=
DB_HOST=
DB_PORT=
DB_USER=
DB_PASSWORD=
DB_NAME=
DB_VOLUME=
SECRET_HASHING_SALT=
SU_FILE=
```

You can then run the following command to start the identity server:

```bash
docker compose up -d
```

## Usage

### Migrator

To run the migrator, you can use the following command:

```bash
 docker exec -it identity-database-migrator ./Identity.Migrator 
```

Database could be seeded with the following command with the `datas/su.json` file:

```bash
 docker exec -it identity-database-migrator ./Identity.Migrator 3
```
File could rewritten with docker volume (see `docker-compose.yml`).


### API

The API can be accessed at `http://localhost:9090`.
The API swagger documentation can be accessed at `http://localhost:9090/swagger`.


