dotnet-run:
	dotnet run --project ./src/CluedIn.Contrib.Submitter

dotnet-watch:
	dotnet watch --project ./src/CluedIn.Contrib.Submitter

docker-build:
	docker build -t cluedin-contrib-submitter .

docker-run:
	docker run -it --rm -p 3000:8080 cluedin-contrib-submitter

docker-compose-build:
	docker-compose build

docker-compose-up:
	docker-compose up
