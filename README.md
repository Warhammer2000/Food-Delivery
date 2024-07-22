# Food Delivery

Welcome to the FoodDelivery project! This is a web application designed to simplify the process of ordering food from various restaurants and having it delivered right to your doorstep.

## Table of Contents

- [Features](#features)
- [Technologies](#technologies)
- [Installation](#installation)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## Features

- **User Authentication:** Secure login and registration for users, managers, couriers, and administrators.
- **Restaurant Management:** Admins and managers can add, update, and delete restaurant information.
- **Menu Management:** Managers can manage the menu items for their restaurants.
- **Order Management:** Users can place orders, which can then be managed by couriers and admins.
- **Payment Integration:** Simulated payment processing with credit card management.
- **Cart Functionality:** Users can add items to a cart, view their cart, and proceed to checkout.
- **Responsive Design:** The application is responsive and can be used on various devices.

## Technologies

This project uses the following technologies and frameworks:

- **ASP.NET Core:** Backend framework for building the web application.
- **MongoDB:** NoSQL database for storing data.
- **Entity Framework Core:** ORM for interacting with the MongoDB database.
- **Bootstrap:** Frontend framework for responsive design.
- **jQuery:** JavaScript library for DOM manipulation.
- **Razor Pages:** Templating engine for building dynamic web pages.
- **RabbitMQ:** Messaging broker for handling order updates and notifications.

## Installation

To get started with the FoodDelivery project, follow these steps:

1. **Clone the repository:**

   ```
   git clone https://github.com/yourusername/FoodDelivery.git
   cd FoodDelivery
   ```

2. **Set up the MongoDB database:**

   - Install MongoDB from [here](https://www.mongodb.com/try/download/community).
   - Start the MongoDB service.
   - Create the necessary databases and collections as per the application requirements.

3. **Configure the application:**

   - Update the `appsettings.json` file with your MongoDB connection string and other necessary configurations.

4. **Install dependencies:**

   ```
   dotnet restore
   ```

5. **Build the project:**

   ```
   dotnet build
   ```

6. **Run the application:**

   ```
   dotnet run
   ```

7. **Open your browser and navigate to:**

   ```
   http://localhost:5000
   ```

## Usage

- **Registration and Login:**
  - Register a new user account or log in with existing credentials.
  - Admins and managers can access additional features for managing restaurants and orders.
- **Restaurant and Menu Management:**
  - Admins can add new restaurants and assign managers.
  - Managers can manage menu items for their assigned restaurants.
- **Placing an Order:**
  - Browse restaurants and their menus.
  - Add items to your cart and proceed to checkout.
  - Select a saved credit card or add a new one for payment.
- **Order Management:**
  - Users can view their order history.
  - Couriers can update the status of orders they are delivering.

## Project Structure

The project structure is organized as follows:

```
graph FoodDelivery/
│
├── Controllers/            # Contains the MVC controllers
├── Models/                 # Data models and entity classes
├── Views/                  # Razor views for the frontend
├── ViewModels/             # View models for passing data to views
├── Helpers/                # Helper classes and extension methods
├── Services/               # Service classes for business logic
├── wwwroot/                # Static files (CSS, JS, images)
│
├── Program.cs              # Entry point of the application
├── Startup.cs              # Configures services and the app's request pipeline
├── appsettings.json        # Application configuration settings
│
└── README.md               # This file
```

## Contributing

I welcome contributions from the community! If you have any ideas, bug reports, or pull requests, please feel free to contribute.

1. Fork the repository.
2. Create a new branch (`git checkout -b feature/YourFeature`).
3. Commit your changes (`git commit -m 'Add some feature'`).
4. Push to the branch (`git push origin feature/YourFeature`).
5. Open a pull request.

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Contact

If you have any questions or feedback, feel free to contact me at rustampulatovwh@gmail.com

Thank you for using FoodDelivery! Enjoy your meal!

