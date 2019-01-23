# filefeed

A small web service to expose files as a podcast RSS feeds and download them. Use with a podcast client on your phone (tested with podcaster) to auto download the files as they become available. You'll have access to all the files in the image. Don't expose without proper security. Includes a web page to browse the filesystem and and get the RSS links.

## Usage

Mount a folder containing the files (any level of subdirectories will work) you want the files you want to generate an RSS for. Run the container and access it at http://hostname:port. Find the folder you want and copy the RSS link for that folder. Subscribe to this link in your podcast client. In podcaster, it's in Podcasts > + > + > RSS feed. Optionnaly, configure auto download options in your client.

## docker run

```sh
docker run -d \
   -p xxxx:80 \
   -v /your/files:/files \
   --name filefeed \
   dettrittus/filefeed
```

## docker-compose.yml

```yaml
version: "2"
services:
  filefeed:
    image: dettrittus/filefeed
    container_name: filefeed
    ports:
      - xxxx:80
    volumes:
      - /your/files:/files
```
