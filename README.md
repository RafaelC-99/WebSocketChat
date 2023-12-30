# WebSocketChat_Application
<p>This repository contains a simple WebSocket chat application with a .NET server and an HTML client.</p>

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)

## USAGE

### Server:

Use the .NET CLI to run the server side project.

```bash
cd  WebSocketChat_Server/
dotnet run
```

### Client:

Open the `index.html` file in a web browser.
 <p> 
   <ul>
     <li>The HTML client supports multiple tabs, allowing you to simulate interactions between multiple users in the chat.</li>
   </ul>
 </p>
 
<ol>
    <li>Enter your username when prompted.</li>
    <li>Start chatting! You can type messages in the input field and press Enter to send them.</li>
    <li>Click the "Leave chat" button to disconnect from the chat.</li>
</ol>

## Important Notes
* Make sure the server is running before trying to connect with the client.
* The server and client should be accessible from the same machine.
