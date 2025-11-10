# Customer API Instructions designed to guide users on how to interact with the customer-related endpoints of the API.

## Controller design
 - Create new Controller named CustomerController in Controllers folder.
 - I need to perform all CRUD operations (Create, Read, Update, Delete) on Customer entities.
 - Create Services and Repository layers to handle business logic and data access.
 - I need to use In-memory as the database.
 - Create  POSCustomer with following fields in model class:
   - Id (int, primary key)
   - Name (string, required)
   - Email (string, required, unique)
   - Phone (string, optional)
	- billing_address (string,required)
   - CreatedAt (DateTime, default to current timestamp)
   - UpdatedAt (DateTime, nullable) 
 - Based on above model class, create a DbContext class named ApplicationDbContext in Data folder.
 - Create a model class named POSCustomer in Models folder.
 - Create Model POSViewCustomer to load inputs from controller and then convert to POSCustomer entity.
 - Implement the following endpoints in CustomerController:
   - GET /api/customers - Retrieve a list of all customers.
   - GET /api/customers/{id} - Retrieve a specific customer by ID.
   - POST /api/customers - Create a new customer based on input fields once added return 201 with customer object created.
   - PUT /api/customers/{id} - Update an existing customer and return customer object updated.
   - DELETE /api/customers/{id} - Delete a customer by ID and return 204 with customer id which has been deleted.
 - In POSViewCustomer model, apply data annotations for validation:
   - FirstName, LastName, and Email are required.
   - Email should be in a valid email format.
   - PhoneNumber is optional but if provided, should be in a valid phone number format.
	- Write a method ValidateInput and throw user readable exception if validation fails.
 - Create new XUnit Project named CustomerApi.Tests to write unit tests for CustomerController and CustomerService.
 - Use Moq to mock dependencies in unit tests.
 - Write unit tests to cover all CRUD operations in CustomerController and CustomerService.
 - Cover all possible scenarios including success and failure cases.
 - Look for edge cases and handle exceptions gracefully.
 


 ## Code Style
  - All method names, class name, variable names should be as per C# naming conventions.
 - All public methods and classes should have XML comments.
 - Methods should be readable and should not exceed 30 lines.
 - Please do all null checks to avoid null reference exceptions.
 - Add comments in methods for other developers to understand the code better.
 - Include Try/Catch blocks where necessary and log exceptions using Serilog.
 - Use Dependency Injection for services and repositories.
	- Register your services and repositories in the Startup.cs file.

