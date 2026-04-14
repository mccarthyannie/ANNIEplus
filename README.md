ANNIE+ : Yoga Booking App 

Annie_Plus:
 Frontend from blazor framework. This is the UI and front-facing functionality of the app.

Annie_API:
 This is the backend of the app. This connects the PostgreSQL database to the frontend using API calls. This runs inside docker.  

How to run ANNIE+
 1. Ensure docker and dot net are installed in your working environment.
 2. in a terminal window, cd to Annie_API, and run 'docker compose build'.
 3. run 'docker compose up -d' and then 'docker ps' to check if API and db are running.
 4. in a second terminal window, cd to Annie_Plus and run 'dotnet build' then 'dotnet run' to initiate the frontend.
 5. open localhost:5031 in a browser tab and ANNIE+ should appear and be functional.

What you need to run ANNIE+:
 - .NET 8
 - docker

Migration concerns:
 - all migration conflicts have normally been solved with clearing the volume using 'docker compose down -v' and then rebuilding. 
