# Welcome to Conectify!

Hi! This packages is meant to be used as a simple HTTP Server with callbacks for simple thinks.
Do not mistake this as a full fledged HTTP Web Server, this is meant as a small server that can pass data from a browser or another application.

# What can you do with this

- You can listen to multiple endpoints
- You can catch Headers,Query and the Body[as string] of requests to the endpoint
- Set the listening address and port
- Customise the endpoints you want to receive data to.
- Set the response type to JSON,,HTML or plain text.
- Set the HTTP Response code

## Disclaimer

While possible to set up a simple web site using this package it highly not recomment it, because as the package is only one request can be help at a time. Use this package at your own rick.

## Example

```
const int port = 3000;
const string ip = "http://localhost"; // Note! DO NOT ADD '/' AT THE END

var instance = new JamPaul.Toolbox.Conectify.Instance(port, ip);

//Subscribe to the events
instance.OnStart += (sender, arg) => Console.WriteLine(arg);
instance.OnStop += (sender, arg) => Console.WriteLine(arg);
instance.OnError += (sender, exception) =>
Console.WriteLine(exception.Message);

//Create the endpoints
var endpoint1 = new JamPaul.Toolbox.Conectify.Endpoint(HttpMethod.Get, "/test", true, true);
var endpoint2 = new JamPaul.Toolbox.Conectify.Endpoint(HttpMethod.Post, "/test", true, true);

//Add the endpoints and create the callback functions
instance.AddEndpoint(endpoint1,
    (s, a) =>
    {

        return new JamPaul.Toolbox.Conectify.Response(200, "GET Request Works!");

    });
instance.AddEndpoint(endpoint2,
    (s, a) =>
    {
        return new JamPaul.Toolbox.Conectify.Response(200, "Post Request Works!");
    });

//Start the HTTP Server
bool instanceStarted = instance.Start();
```
