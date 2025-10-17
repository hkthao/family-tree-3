gemini -m gemini-2.5-flash
dotnet run --project src/Web
npm run dev

docker build -t face-service-local .
docker run -p 8000:8000 --name face-service-container face-service-local