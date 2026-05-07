{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=XXXX;Database=dbname;Username=username;Password=password"
  },
  "Jwt": {
    "Issuer": "https://localhost:7170",
    "ExpireInMinutes": 480,
    "Key": "very-secret-key"
  }
}
