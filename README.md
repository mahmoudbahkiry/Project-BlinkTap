# BlinkTap

BlinkTap is a reaction-time testing application that measures and tracks users' response times. The project consists of a backend server built with Node.js and Express, and a Unity-based frontend for interactive testing. BlinkTap allows users to create accounts, perform reaction time tests, and track their progress over time.

## Features

- User registration and authentication via Firebase
- Custom user profiles with profession tracking
- Reaction time testing and scoring
- Historical score tracking and analysis
- Cross-platform compatibility
- Personalized user dashboards
- Multi-device synchronization

## Detailed Features

### User Authentication

BlinkTap provides a secure authentication system powered by Firebase. Users can register with email and password, and their credentials are safely stored and managed through Firebase Authentication.

### Profile Management

Users can create and customize their profiles, including setting their profession.

### Reaction Time Testing

The core functionality of BlinkTap is its reaction time testing system. The Unity frontend delivers visual stimuli in the shape of circles that appear on random places of the screen to users, who must respond as quickly as possible. The application measures precise reaction times down to milliseconds.

### Performance Analytics

All test results are stored in the Firebase database.

## Technical Architecture

### Backend Architecture

The backend is built on Node.js with Express, providing RESTful API endpoints for all application features. It handles user authentication, data storage, and retrieval operations.

- **Authentication Service**: Handles user registration, login, and token validation
- **Profile Service**: Manages user profile data and updates
- **Scoring Service**: Processes and stores test results

### Frontend Architecture

The Unity implementation creates an engaging, responsive interface for users to interact with. It communicates with the backend via API calls to store and retrieve data.

- **User Interface Layer**: Handles all visual elements and user interactions
- **API Communication Layer**: Manages data exchange with the backend

### Database Schema

BlinkTap uses Firebase Firestore for its database needs, with the following collections:

- **Users**: Basic user information
- **Professions**: User profession data
- **Scores**: Historical test results

## Third-Party Dependencies

### Backend

- **Express**: Web server framework for Node.js
- **Firebase/Firebase Admin**: Authentication and database services
- **CORS**: Cross-Origin Resource Sharing middleware
- **SQLite3**: Local database for development and testing

### Frontend/Unity

- **Unity Engine**: Game development platform used for the interactive frontend
- **Firebase SDK for Unity**: Authentication and data persistence
- **TextMeshPro**: Advanced text rendering capabilities

## Project Structure

- `BlinkTap-Backend/`: Node.js server and API endpoints

  - `server.js`: Main server entry point
  - `firebase.js`: Firebase configuration and initialization
  - `routes/`: API endpoint definitions
  - `middleware/`: Custom middleware functions
  - `models/`: Data models and schemas

- `UnityImplementations/`: Unity-based frontend application
  - `Assets/`: Unity project assets
  - `Scripts/`: C# scripts for application logic
  - `Prefabs/`: Reusable Unity objects
  - `Scenes/`: Application screens and layouts

## Prerequisites

Before setting up the project, ensure you have the following installed:

- Node.js (v14.0.0 or higher)
- npm (v6.0.0 or higher)
- Unity 2020.3 LTS or newer
- Firebase account
- Git (for version control)

## Setup Instructions

### Backend Setup

1. Navigate to the backend directory:

   ```
   cd BlinkTap-Backend
   ```

2. Install dependencies:

   ```
   npm install
   ```

3. Set up Firebase:

   - Create a Firebase project in the Firebase Console
   - Download the service account key and save it as `firebaseKey.json` in the root directory
   - Update Firebase configuration in `firebase.js` file

4. Start the server:

   ```
   node server.js
   ```

   The server will run on port 3000 by default.

### Unity Setup

1. Open Unity Hub
2. Add the project by selecting the `UnityImplementations` directory
3. Open the project using a compatible Unity version
4. Configure Firebase for Unity:
   - Download the Firebase Unity SDK
   - Import the Firebase Authentication and Firestore packages
   - Configure the Firebase settings according to your Firebase project
5. Build and run the project for your target platform

## API Endpoints

### Authentication Endpoints

- `POST /register`: User registration with email and password
- `POST /login`: User authentication and token generation

### User Profile Endpoints

- `GET /profile`: Retrieve user profile information
- `POST /profile`: Create or update user profile details

### Score Endpoints

- `POST /scores`: Submit new test results
- `GET /scores`: Retrieve historical test results
- `GET /scores/average`: Get average scores for a user

### System Endpoints

- `GET /debug`: Server status check

## Development

To contribute to this project, please follow the standard Git workflow:

1. Fork the repository
2. Create a feature branch
3. Submit a pull request
