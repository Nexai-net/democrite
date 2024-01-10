External Storages MongoDB
=====

This sample use a database to:
- Register definitions
- Expose definitions
- Connect cluster parts (nodes, clients)

In this sample the node part will increase a counter associate to a "name" every minute. <br/>
The client will expose through an api a way to get the counter value.

# Sample Steps:

## 1 - Start local Mongo DB

The simple way is to docker [Docker](https://www.docker.com/)

````Shell
mongo run -d -p 27017:27017 mongo:latest
````

## 2 - Register Definitions

Launch the project "[Builder](/samples/ExternalStorages/MongoDB/Builder)".<br />
It will saved deux definitions on the database **democrite** collection **Definitions**.

## 3 - Start Node/Client

Start as many "Nodes" as you want and use the client part to get the counter values.
