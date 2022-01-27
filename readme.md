# Image Gallery

This sample uses the following core technologies to deliver an image gallery experience:

- [ASP.NET Core](https://dot.net)
- [Marten](https://martendb.io)
- [ImageSharp.Web](https://sixlabors.com)
- [HTMX](https://htmx.org)

This allows you to upload imaged directly into PostgreSQL and serve them. ImageSharp.Web will process
initial images, but cache them to disk so that they are only processed once per unique call. That's pretty
cool.

## Getting Started

You will need an instance of PostgreSQL in order to get started. I recommend using Docker Desktop. You'll
also need .NET SDK 6+.

## License

This is a demo, don't publish this demo to production as-is, or do, I'm not your mom, just don't sue me.