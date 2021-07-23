[![build](https://github.com/Cisien/paste/actions/workflows/main.yml/badge.svg)](https://github.com/Cisien/paste/actions/workflows/main.yml)

### Setup Instructions
First, run the container in interactive mode. The first token will be generated and printed in stdout. Copy this down as it's only displayed once.

```sh
docker run -it --rm -v ./paste-data:/app/data -p 80:80 ghcr.io/cisien/paste:latest
```

Then stop that instance, and start it again in daemon mode

```sh
docker run -d -v ./paste-data:/app/data -p 80:80 --restart always ghcr.io/cisien/paste:latest
```
