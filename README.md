# Defra BTMS Backend Stub

This service provides stubbed responses for specific BTMS requests.

## Prerequisites

The solution requires:

- .NET 9

  ```bash
  brew tap isen-ng/dotnet-sdk-versions
  brew install --cask dotnet-sdk9
  ```

- Docker

## Installation

1. Clone this repository
2. Install the required tools with `dotnet tool restore`
3. Check the solution builds with `dotnet build`
4. Check the service builds with `docker build .`

## Running

1. Run the application via Docker:
   ```
   docker build -t btms-backend-stub .
   docker run -p 8085:8085 btms-backend-stub
   ```
2. Navigate to http://localhost:8085

## Responses

See the Scenarios folder for all available responses. Examples are as follows:

Get import notification updates:
```http request
http://localhost:8085/api/import-notifications
```

Get individual import notification:
```http request
http://localhost:8085/api/import-notifications/CHEDA.GB.2024.4792831
```

Get individual movement:
```http request
http://localhost:8085/api/movements/24GBCUDNXBN1JNRAR5
```

Get individual goods movement:
```http request
http://localhost:8085/api/gmrs/GMRA00KBHFE0
```

## Development

Any new scenarios should be added to the Scenarios folder and they will be included automatically.

Scenarios requiring specific configuration in code should be registered in the `WireMockHostedService`.

## Testing

Run the service via Docker and test the output is as expected.

# Linting

We use [CSharpier](https://csharpier.com) to lint our code.

You can run the linter with `dotnet csharpier .`

## License

This project is licensed under The Open Government Licence (OGL) Version 3.  
See the [LICENSE](./LICENSE) for more details.