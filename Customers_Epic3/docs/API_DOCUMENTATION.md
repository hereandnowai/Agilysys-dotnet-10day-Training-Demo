# API Documentation

## Overview

This document provides detailed information about the API endpoints, request/response formats, and usage examples.

## Base URL

- **Development**: `https://localhost:5001`
- **Production**: `https://<your-domain>`

## Authentication

*(To be implemented)*

## Endpoints

### Weather Forecast

#### Get Weather Forecast

Returns a 5-day weather forecast.

**Endpoint**: `GET /weatherforecast`

**Response**: `200 OK`

```json
[
  {
    "date": "2024-01-15",
    "temperatureC": 25,
    "temperatureF": 76,
    "summary": "Warm"
  }
]
```

## Error Responses

### 400 Bad Request

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "field": ["Validation error message"]
  }
}
```

### 404 Not Found

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404
}
```

### 500 Internal Server Error

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500
}
```

## Rate Limiting

*(To be implemented)*

## Changelog

### v1.0.0 (Initial Release)
- Basic project structure
- Weather forecast endpoint
- Structured logging with Serilog
